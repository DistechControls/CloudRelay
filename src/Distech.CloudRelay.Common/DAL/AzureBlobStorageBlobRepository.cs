using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Distech.CloudRelay.Common.Exceptions;
using Distech.CloudRelay.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.DAL
{
    /// <summary>
    /// Represents the repository used to persist file in an Azure storage container.
    /// </summary>
    internal class AzureStorageBlobRepository
        : IBlobRepository
    {
        #region Members

        private readonly Lazy<BlobServiceClient> m_LazyBlobServiceClient;

        private readonly ILogger<AzureStorageBlobRepository> m_Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public AzureStorageBlobRepository(IOptionsSnapshot<AzureStorageAccountOptions> options, ILogger<AzureStorageBlobRepository> logger)
        {
            //Note: Continues to rely on injecting options and creating client locally instead of relying on Microsoft.Azure.Extension
            // to rely on DI instead of BlobServiceClient.
            // Why: such clients are registered as Singleton by default, which is not compatible with current IOptions implementation
            // reying on HttpContext to resolve proper storage account/connection strings based on environment associated to request/tenant.
            // We end-up having a singleton instance (the client) having a depencency on IOptionsShapshot, which needs
            // to remain scoped. Even if it worked, only the first connection string will be accounted for...
            // In that case we need to discover the supported storage accounts ahead of time on API side to register any possible client
            // instances as named instance and rely on some sort of locator/scoped instance instead on the service side.
            m_LazyBlobServiceClient = new Lazy<BlobServiceClient>(() =>
            {
                return new BlobServiceClient(options.Value.ConnectionString);
            });
            m_Logger = logger;
        }

        #endregion

        #region IBlobRepository Implementation

        /// <summary>
        /// Returns the information associated to a blob.
        /// </summary>
        /// <param name="blobPath"></param>
        /// <returns></returns>
        public async Task<BlobInfo> GetBlobInfoAsync(string blobPath)
        {
            BlobInfo result = null;
            BlobClient blob = GetBlobClient(blobPath);

            try
            {
                var properties = await blob.GetPropertiesAsync();
                result = AzureBlob.FromBlobProperties(GetBlobPath(blob), properties);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound || ex.ErrorCode == BlobErrorCode.ContainerNotFound)
            {
                //swallow does not exist error and return default null instead (as previous behavior before dotnet6 migration)
            }

            return result;
        }

        /// <summary>
        /// Returns the list of blobs matching the specified path.
        /// </summary>
        /// <param name="blobPathPrefix"></param>
        /// <returns></returns>
        public async Task<List<BlobInfo>> ListBlobAsync(string blobPathPrefix)
        {
            //$root container is not implicitly supported, at least an explicit container name is expected
            string prefix = blobPathPrefix ?? throw new ArgumentNullException(nameof(blobPathPrefix));
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException(nameof(blobPathPrefix));
            }

            BlobContainerClient containerClient = GetContainerClient(blobPathPrefix);
            List<BlobInfo> result = new();

            //resolve any virtual folder prefix remaining without container info
            prefix = Regex.Replace(blobPathPrefix, $"^[/]?{containerClient.Name}", string.Empty);

            try
            {
                var segment = containerClient.GetBlobsAsync(prefix: prefix, traits: BlobTraits.Metadata).AsPages();
                await foreach (Page<BlobItem> blobPage in segment)
                {
                    result.AddRange(blobPage.Values.Select(b => AzureBlob.FromBlobItem(GetBlobPath(containerClient, b), b)));
                }
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.ContainerNotFound)
            {
                //swallow container not found and let the default return the currently empty list instead
            }

            return result;
        }

        /// <summary>
        /// Opens a stream for reading from the blob.
        /// </summary>
        /// <param name="blobPath">The relative path, from the storage root, to the blob.</param>
        /// <exception cref="IdNotFoundException">The blob specified in <paramref name="blobPath"/> does not exist (<see cref="ErrorCodes.BlobNotFound"/>).</exception>
        /// <returns></returns>
        public async Task<BlobStreamDecorator> OpenBlobAsync(string blobPath)
        {
            BlobClient blob = GetBlobClient(blobPath);

            try
            {
                BlobProperties properties = await blob.GetPropertiesAsync();
                //blob properties and metadata are automatically fetched when opening the blob stream
                Stream blobStream = await blob.OpenReadAsync();
                BlobInfo blobInfo = AzureBlob.FromBlobProperties(GetBlobPath(blob), properties);
                return new BlobStreamDecorator(blobStream, blobInfo);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound || ex.ErrorCode == BlobErrorCode.ContainerNotFound)
            {
                if (ex.ErrorCode == BlobErrorCode.ContainerNotFound)
                {
                    m_Logger.LogWarning("Container '{BlobContainerName}' does not exist", blob.BlobContainerName);
                }

                throw new IdNotFoundException(ErrorCodes.BlobNotFound, blobPath);
            }
        }

        /// <summary>
        /// Creates a new blob from the specified stream content.
        /// </summary>
        /// <param name="blobPath">The relative path, from the storage root, to the blob.</param>
        /// <param name="data">The stream to add to the blob.</param>
        /// <param name="overwrite">Whether blob should be overwritten if it already exists.</param>
        /// <exception cref="ConflictException">The blob specified in <paramref name="blobPath"/> already exists (<see cref="ErrorCodes.BlobAlreadyExists"/>).</exception>
        /// <returns></returns>
        public async Task WriteBlobAsync(string blobPath, BlobStreamDecorator data, bool overwrite = false)
        {
            BlobClient blob = GetBlobClient(blobPath);

            try
            {
                //associate metadata to the blob, which will be saved by the next upload operation
                BlobUploadOptions options = new();
                data.ApplyDecoratorTo(options);

                //access condition to ensure blob does not exist yet to catch concurrency issues
                if (!overwrite)
                {
                    options.Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All };
                }

                await blob.UploadAsync(data, options);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                //blob already exists, overwriting is currently not implemented => concurrency error on the client side
                throw new ConflictException(ErrorCodes.BlobAlreadyExists);
            }
        }

        /// <summary>
        /// Deletes the specified blob.
        /// </summary>
        /// <param name="blobPath"></param>
        /// <returns>Returns true if the blob has been deleted or false if the blob did not exist.</returns>
        public async Task<bool> DeleteBlobAsync(string blobPath)
        {
            BlobClient blob = GetBlobClient(blobPath);

            try
            {
                await blob.DeleteAsync();
                return true;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                //swallow blob not found to ease delete operations
                return false;
            }
        }

        /// <summary>
        /// Returns an URL containing authentication/authorization information for the specified blob.
        /// </summary>
        /// <param name="blobPath"></param>
        /// <param name="secondsAccessExipiryDelay"></param>
        /// <exception cref="IdNotFoundException">The blob specified in <paramref name="blobPath"/> does not exist (<see cref="ErrorCodes.BlobNotFound"/>).</exception>
        /// <returns></returns>
        public async Task<string> GetDelegatedReadAccessAsync(string blobPath, int secondsAccessExipiryDelay)
        {
            BlobClient blob = GetBlobClient(blobPath);

            if (!await blob.ExistsAsync())
            {
                throw new IdNotFoundException(ErrorCodes.BlobNotFound, blobPath);
            }

            Uri sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddSeconds(secondsAccessExipiryDelay));
            return sasUri.ToString();
        }

        #endregion

        #region Azure Clients

        /// <summary>
        /// Returns a client to manage the storage container specified in blob path.
        /// </summary>
        /// <param name="blobRelativePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private BlobContainerClient GetContainerClient(string blobRelativePath)
        {
            var segments = blobRelativePath.TrimStart('/').Split('/', 2);
            if (segments.Length < 1)
            {
                throw new ArgumentException($"Invalid blob path", nameof(blobRelativePath));
            }

            return m_LazyBlobServiceClient.Value.GetBlobContainerClient(segments[0]);
        }

        /// <summary>
        /// Returns a client to manage the storage blob specified in blob path.
        /// </summary>
        /// <param name="blobRelativePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private BlobClient GetBlobClient(string blobRelativePath)
        {
            var segments = blobRelativePath.TrimStart('/').Split('/', 2);
            if (segments.Length != 2)
            {
                throw new ArgumentException($"Invalid blob path", nameof(blobRelativePath));
            }

            return m_LazyBlobServiceClient.Value.GetBlobContainerClient(segments[0]).GetBlobClient(segments[1]);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the relative path for the specified blob from the blob client base URI.
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private static string GetBlobPath(BlobClient blob)
        {
            return $"{blob.BlobContainerName}/{blob.Name}";
        }

        /// <summary>
        /// Returns the relative path for the specified blob from the blob client base URI.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string GetBlobPath(BlobContainerClient container, BlobItem item)
        {
            return $"{container.Name}/{item.Name}";
        }

        #endregion
    }
}
