using Azure.Storage.Blobs.Models;
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
        /// <param name="blobPath"></param>
        /// <param name="blobProperties"></param>
        /// <returns></returns>
        public static AzureBlob FromBlobProperties(string blobPath, BlobProperties blobProperties)
        {
            return new AzureBlob
            {
                Path = blobPath,
                Checksum = Convert.ToBase64String(blobProperties.ContentHash),
                ContentType = blobProperties.ContentType,
                LastModified = blobProperties.LastModified,
                Length = blobProperties.ContentLength,
                Metadata = BlobMetadata.FromDictionary(blobProperties.Metadata)
            };
        }

        /// <summary>
        /// Create a new <see cref="AzureBlob"/> from a <see cref="BlobItem"/> instance.
        /// </summary>
        /// <param name="blobPath"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static AzureBlob FromBlobItem(string blobPath, BlobItem item)
        {
            return new AzureBlob
            {
                Path = blobPath,
                Checksum = Convert.ToBase64String(item.Properties.ContentHash),
                ContentType = item.Properties.ContentType,
                LastModified = item.Properties.LastModified ?? DateTimeOffset.MinValue,
                Length = item.Properties.ContentLength ?? -1,
                Metadata = BlobMetadata.FromDictionary(item.Metadata)
            };
        }

        #endregion
    }
}
