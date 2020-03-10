using Distech.CloudRelay.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.DAL
{
    /// <summary>
    /// Represents the repository in charge of persisting file as blob.
    /// </summary>
    public interface IBlobRepository
    {
        /// <summary>
        /// Returns the information associated to a blob.
        /// </summary>
        /// <param name="blobPath">The relative path, from the storage root, to the blob.</param>
        /// <returns></returns>
        Task<BlobInfo> GetBlobInfoAsync(string blobPath);

        /// <summary>
        /// Returns the list of blobs matching the specified path.
        /// </summary>
        /// <param name="blobPathPrefix"></param>
        /// <returns></returns>
        Task<List<BlobInfo>> ListBlobAsync(string blobPathPrefix);

        /// <summary>
        /// Opens a stream for reading from the blob.
        /// </summary>
        /// <param name="blobPath">The relative path, from the storage root, to the blob.</param>
        /// <exception cref="IdNotFoundException">The blob specified in <paramref name="blobPath"/> does not exist (<see cref="ErrorCodes.BlobNotFound"/>).</exception>
        /// <returns></returns>
        Task<BlobStreamDecorator> OpenBlobAsync(string blobPath);

        /// <summary>
        /// Creates a new blob from the specified stream content.
        /// </summary>
        /// <param name="blobPath">The relative path, from the storage root, to the blob.</param>
        /// <param name="data">The stream to add to the blob.</param>
        /// <param name="overwrite">Whether blob should be overwritten if it already exists.</param>
        /// <exception cref="ConflictException">The blob specified in <paramref name="blobPath"/> already exists (<see cref="ErrorCodes.BlobAlreadyExists"/>).</exception>
        /// <returns></returns>
        Task WriteBlobAsync(string blobPath, BlobStreamDecorator data, bool overwrite = false);

        /// <summary>
        /// Deletes the specified blob.
        /// </summary>
        /// <param name="blobPath"></param>
        /// <returns>Returns true if the blob has been deleted or false if the blob did not exist.</returns>
        Task<bool> DeleteBlobAsync(string blobPath);

        /// <summary>
        /// Returns an URL containing authentication/authorization information for the specified blob.
        /// </summary>
        /// <param name="blobPath"></param>
        /// <param name="secondsAccessExipiryDelay"></param>
        /// <exception cref="IdNotFoundException">The blob specified in <paramref name="blobPath"/> does not exist (<see cref="ErrorCodes.BlobNotFound"/>).</exception>
        /// <returns></returns>
        Task<string> GetDelegatedReadAccessAsync(string blobPath, int secondsAccessExipiryDelay);
    }
}
