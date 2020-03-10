using Distech.CloudRelay.Common.Exceptions;
using Distech.CloudRelay.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private readonly Lazy<CloudBlobClient> m_LazyBlobClient;

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
            m_LazyBlobClient = new Lazy<CloudBlobClient>(() =>
            {
                CloudStorageAccount account = CloudStorageAccount.Parse(options.Value.ConnectionString);
                return account.CreateCloudBlobClient();
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

            CloudBlobClient blobClient = m_LazyBlobClient.Value;
            CloudBlob blob = new CloudBlob(GetAbsoluteBlobUri(blobPath), blobClient.Credentials);

            //`ExistsAsync` also refreshes properties and metadata at the same times
            if (await blob.ExistsAsync())
            {
                result = AzureBlob.FromBlobProperties(GetBlobPath(blob), blob.Properties, blob.Metadata);
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

            CloudBlobClient blobClient = m_LazyBlobClient.Value;
            BlobContinuationToken continuationToken = null;

            //pad prefix path for container only with trailing slash '/', otherwise storage SDK list in $root container only
            if (!prefix.Contains('/'))
            {
                prefix += '/';
            }

            var result = new List<BlobInfo>();

            try
            {
                do
                {
                    BlobResultSegment segment = await blobClient.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.Metadata, null, continuationToken, null, null);
                    continuationToken = segment.ContinuationToken;

                    result.AddRange(segment.Results.Cast<CloudBlob>().Select(b => AzureBlob.FromBlobProperties(GetBlobPath(b), b.Properties, b.Metadata)));
                } while (continuationToken != null);
            }
            catch (StorageException ex) when (ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.ContainerNotFound)
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
            CloudBlobClient blobClient = m_LazyBlobClient.Value;
            CloudBlob blob = new CloudBlob(GetAbsoluteBlobUri(blobPath), blobClient.Credentials);

            try
            {
                //blob properties and metadata are automatically fetched when opening the blob stream
                Stream blobStream = await blob.OpenReadAsync();
                BlobInfo blobInfo = AzureBlob.FromBlobProperties(GetBlobPath(blob), blob.Properties, blob.Metadata);
                return new BlobStreamDecorator(blobStream, blobInfo);
            }
            catch (StorageException ex) when (ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound || ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.ContainerNotFound)
            {
                if (ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.ContainerNotFound)
                {
                    m_Logger.LogWarning($"Container '{blob.Container.Name}' does not exist");
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
            CloudBlobClient blobClient = m_LazyBlobClient.Value;
            CloudBlockBlob blockBlob = new CloudBlockBlob(GetAbsoluteBlobUri(blobPath), blobClient.Credentials);

            try
            {
                //associate metadata to the blob, which will be saved by the next upload operation
                data.ApplyDecoratorTo(blockBlob);

                //use IfNotExistsCondition by default to avoid overriding an existing blob without knowing about it
                //access condition to ensure blob does not exist yet to catch concurrency issues
                AccessCondition condition = null;
                if (!overwrite)
                {
                    condition = AccessCondition.GenerateIfNotExistsCondition();
                }

                await blockBlob.UploadFromStreamAsync(data, condition, null, null);
            }
            catch (StorageException ex) when (ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.BlobAlreadyExists)
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
            CloudBlobClient blobClient = m_LazyBlobClient.Value;
            var blob = new CloudBlob(GetAbsoluteBlobUri(blobPath), blobClient.Credentials);

            try
            {
                await blob.DeleteAsync();
                return true;
            }
            catch (StorageException ex) when (ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound)
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
            CloudBlobClient blobClient = m_LazyBlobClient.Value;
            CloudBlob blob = new CloudBlob(GetAbsoluteBlobUri(blobPath), blobClient.Credentials);

            if (!await blob.ExistsAsync())
            {
                throw new IdNotFoundException(ErrorCodes.BlobNotFound, blobPath);
            }

            string sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddSeconds(secondsAccessExipiryDelay)
            });

            return $"{blob.Uri}{sas}";
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the aboslute URI for the specified relative blob path.
        /// </summary>
        /// <param name="blobRelativePath"></param>
        /// <returns></returns>
        private Uri GetAbsoluteBlobUri(string blobRelativePath)
        {
            //blob client abosulte uri contains '/' when using a real storage account, but does not for the emulator
            //  - Azure storage account: "https://{accountName}.blob.core.windows.net/"
            //  - Emulator: "http://127.0.0.1:10000/devstoreaccount1"
            return new Uri($"{m_LazyBlobClient.Value.BaseUri.AbsoluteUri.TrimEnd('/')}/{blobRelativePath.TrimStart('/')}");
        }

        /// <summary>
        /// Returns the relative path for the specified blob from the blob client base URI.
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private string GetBlobPath(CloudBlob blob)
        {
            return $"{blob.Container.Name}/{blob.Name}";
        }

        #endregion
    }
}
