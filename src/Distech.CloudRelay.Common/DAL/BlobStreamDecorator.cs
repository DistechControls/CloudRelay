using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.DAL
{
    /// <summary>
    /// Represents a simple decorator class for a blob stream to add blob related metadata.
    /// </summary>
    public class BlobStreamDecorator
        : Stream
    {
        #region Members

        private readonly Stream m_Stream;
        private bool m_Disposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the blob content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the metadata to associate to the blob.
        /// </summary>
        public BlobMetadata Metadata { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="stream"></param>
        public BlobStreamDecorator(Stream stream)
            : this(stream, new BlobMetadata())
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="metadata"></param>
        public BlobStreamDecorator(Stream stream, BlobMetadata metadata)
        {
            m_Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="blobInfo"></param>
        public BlobStreamDecorator(Stream stream, BlobInfo blobInfo)
            : this(stream, blobInfo.Metadata)
        {
            ContentType = blobInfo.ContentType;
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //dispose managed state (managed objects).
                    m_Stream.Dispose();
                }

                //free unmanaged resources (unmanaged objects) and override a finalizer below.
                //set large fields to null.

                m_Disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Stream Implementation

        public override bool CanRead => m_Stream.CanRead;

        public override bool CanSeek => m_Stream.CanSeek;

        public override bool CanWrite => m_Stream.CanWrite;

        public override long Length => m_Stream.Length;

        public override long Position { get => m_Stream.Position; set => m_Stream.Position = value; }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return m_Stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void Flush()
        {
            m_Stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return m_Stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_Stream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return m_Stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return m_Stream.ReadAsync(buffer, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            m_Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_Stream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return m_Stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return m_Stream.WriteAsync(buffer, cancellationToken);
        }

        #endregion

        #region Azure Blob Support

        /// <summary>
        /// Applies the decorator information to the specified <see cref="CloudBlob"/>.
        /// </summary>
        /// <param name="blob"></param>
        public void ApplyDecoratorTo(BlobUploadOptions blob)
        {
            blob.HttpHeaders ??= new BlobHttpHeaders();
            blob.HttpHeaders.ContentType = ContentType;
            Metadata?.ApplyTo(blob);
        }

        #endregion
    }
}
