using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distech.CloudRelay.Common.DAL
{
    /// <summary>
    /// Represents the information related to an Azure storage blob.
    /// </summary>
    internal class AzureBlob
        : BlobInfo
    {
        #region Constructors

        /// <summary>
        /// Private constructor to ensure factory pattern.
        /// </summary>
        private AzureBlob()
        {
        }

        #endregion

        #region Factory

        /// <summary>
        /// Create a new <see cref="AzureBlob"/> from an Azure storage blob properties.
        /// </summary>
        /// <param name="blobProperties"></param>
        /// <returns></returns>
        public static AzureBlob FromBlobProperties(string blobPath, BlobProperties blobProperties, IDictionary<string, string> blobMetadata)
        {
            return new AzureBlob
            {
                Path = blobPath,
                Checksum = blobProperties.ContentMD5,
                ContentType = blobProperties.ContentType,
                LastModified = blobProperties.LastModified.Value,
                Length = blobProperties.Length,
                Metadata = BlobMetadata.FromDictionary(blobMetadata)
            };
        }

        #endregion
    }
}
