using Distech.CloudRelay.Common.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Opens the specified file which contains the device response.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="filename"></param>
        /// <exception cref="FileNotFoundException">The filename specified in <paramref name="filename"/> does not exist.</exception>
        /// <returns></returns>
        Task<BlobStreamDecorator> OpenFileAsync(string deviceId, string filename);

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
        Task<string> WriteFileAsync(string deviceId, BlobStreamDecorator data);

        /// <summary>
        /// Cleans-up the files used for device request/response.
        /// </summary>
        /// <param name="minutesCleanupExpirationDelay"></param>
        /// <returns></returns>
        Task<int> CleanUpFilesAsync(uint minutesCleanupExpirationDelay);
    }
}
