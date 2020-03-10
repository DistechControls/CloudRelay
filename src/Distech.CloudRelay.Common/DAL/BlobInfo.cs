using System;
using System.Collections.Generic;
using System.Text;

namespace Distech.CloudRelay.Common.DAL
{
    /// <summary>
    /// Represents the information related to a blob.
    /// </summary>
    public abstract class BlobInfo
    {
        /// <summary>
        /// Returns the blob path.
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// Returns the blob checksum.
        /// </summary>
        public string Checksum { get; protected set; }

        /// <summary>
        /// Returns the blob content type.
        /// </summary>
        public string ContentType { get; protected set; }

        /// <summary>
        /// Returns the blob last modified time.
        /// </summary>
        public DateTimeOffset LastModified { get; protected set; }

        /// <summary>
        /// Returns the blob length.
        /// </summary>
        public long Length { get; protected set; }

        /// <summary>
        /// Returns the metadata associated to the blob.
        /// </summary>
        public BlobMetadata Metadata { get; protected set; }
    }
}
