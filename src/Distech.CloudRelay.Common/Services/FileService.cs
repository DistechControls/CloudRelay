using Distech.CloudRelay.Common.DAL;
using Distech.CloudRelay.Common.Exceptions;
using Distech.CloudRelay.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Services
{
    public class FileService : IFileService
    {
        #region Constants

        /// <summary>
        /// The number of seconds during which the file will be accessible for reading.
        /// </summary>
        /// <remarks>
        /// This delay takes in consideration up to potentially 15 minutes of clock skew + 5 minutes of communication timeout.
        /// </remarks>
        private const int ReadAccessExpiryDelay = 60 * 20;

        #endregion

        #region Members

        private readonly IBlobRepository m_BlobRepository;
        private readonly IOptionsSnapshot<FileStorageOptions> m_FileStorageOptions;
        private readonly ILogger<FileService> m_Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="blobRepository"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public FileService(IBlobRepository blobRepository, IOptionsSnapshot<FileStorageOptions> options, ILogger<FileService> logger)
        {
            m_BlobRepository = blobRepository;
            m_FileStorageOptions = options;
            m_Logger = logger;
        }

        #endregion

        #region IFileService Implementation

        /// <summary>
        /// Opens the specified file which contains the device response.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="filename"></param>
        /// <exception cref="FileNotFoundException">The filename specified in <paramref name="filename"/> does not exist.</exception>
        /// <returns></returns>
        public async Task<BlobStreamDecorator> OpenFileAsync(string deviceId, string filename)
        {
            var filePath = $"/{m_FileStorageOptions.Value.DeviceFileUploadFolder}/{deviceId}/{filename}";

            try
            {
                return await m_BlobRepository.OpenBlobAsync(filePath);
            }
            catch (IdNotFoundException ex)
            {
                // re-throw the exception as a non ApiException which will cause the error to be properly logged
                throw new FileNotFoundException($"File '{filePath}' cannot be found", ex);
            }
        }

        /// <summary>
        /// Writes the specified stream which contains the device request to the file storage.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="data"></param>
        /// <exception cref="InvalidOperationException">
        /// Write operation failed due to a conflict while creating the destination blob
        /// -or- The uploaded blob has been deleted prior to generate the SAS token enabled URL to access the blob.
        /// </exception>
        /// <returns></returns>
        public async Task<string> WriteFileAsync(string deviceId, BlobStreamDecorator data)
        {
            var filePath = $"/{m_FileStorageOptions.Value.ServerFileUploadFolder}/{deviceId}/{m_FileStorageOptions.Value.ServerFileUploadSubFolder}/{Guid.NewGuid()}";

            try
            {
                await m_BlobRepository.WriteBlobAsync(filePath, data);
                return await m_BlobRepository.GetDelegatedReadAccessAsync(filePath, ReadAccessExpiryDelay);
            }
            catch (ApiException ex)
            {
                //re-throw the exception as a non ApiException which will cause the error to be properly logged
                string error;
                switch (ex)
                {
                    case ConflictException _:
                        //we are the less fortunate people on earth and got a Guid conflict
                        error = $"File '{filePath}' cannot be written";
                        break;
                    case IdNotFoundException _:
                        //there is a concurrency issue somewhere and someone deleted the blob we just created
                        error = $"File '{filePath}' cannot be found";
                        break;
                    default:
                        //some case we did not think about yet but we might actually need to handle...
                        throw;
                }

                //InvalidOperationException as all those errors are transient and should not happen, retrying should be fine
                throw new InvalidOperationException(error, ex);
            }
        }

        /// <summary>
        /// Cleans-up the files used for device request/response.
        /// </summary>
        /// <param name="minutesCleanupExpirationDelay"></param>
        /// <returns></returns>
        public async Task<int> CleanUpFilesAsync(uint minutesCleanupExpirationDelay)
        {
            if (minutesCleanupExpirationDelay == uint.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minutesCleanupExpirationDelay));
            }

            List<BlobInfo> blobs;
            FileStorageOptions options = m_FileStorageOptions.Value;

            //collect blobs for relay api -> device requests - any blobs in that path have been uploaded for the relay API purpose
            blobs = await m_BlobRepository.ListBlobAsync(options.ServerFileUploadFolder);
            m_Logger.LogDebug("Found '{blobCount}' blob(s) for relay -> device requests", blobs.Count);

            //collect blobs for device -> relay api responses - filter blobs related to relay API context only
            List<BlobInfo> responseBlobs = await m_BlobRepository.ListBlobAsync(options.DeviceFileUploadFolder);
            List<BlobInfo> relayApiBlobs = responseBlobs.Where(b => b.Path.Contains(m_FileStorageOptions.Value.ServerFileUploadSubFolder)).ToList();
            m_Logger.LogDebug("Found '{relayApiBlobsCount}' blob(s) for device -> relay responses", relayApiBlobs.Count);
            blobs.AddRange(relayApiBlobs);

            //select expired blobs only
            DateTime expirationDate = DateTime.UtcNow.AddMinutes(-minutesCleanupExpirationDelay);
            Task<bool>[] deleteTasks = blobs.Where(b => b.LastModified < expirationDate)
                                            .Select(b => m_BlobRepository.DeleteBlobAsync(b.Path))
                                            .ToArray();
            m_Logger.LogDebug("Found '{deleteTasksCount}' expired blob(s)", deleteTasks.Length);

            //parrallel delete ends up degrading performances when dealing with large amount of blobs (longer latencies and throttling) and could also ends up reaching the host maximum number of outbound connections.
            //the current implementation does not allow to control the amount of HttpClient created by the CloudBlobClient: https://github.com/Azure/azure-storage-net/issues/580
            //consider using the Batch client at some point: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/storage/Azure.Storage.Blobs.Batch
            await Task.WhenAll(deleteTasks);
            
            return deleteTasks.Count(t => t.Result);
        }

        #endregion
    }
}
