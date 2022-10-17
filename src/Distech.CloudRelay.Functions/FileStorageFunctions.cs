using Distech.CloudRelay.Common.Services;
using Distech.CloudRelay.Functions.Options;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Functions
{
    public class FileStorageFunctions
    {
        #region Constants

        /// <summary>
        /// The default expiration delay applied if not configured, in minutes.
        /// </summary>
        private const uint DefaultMinutesExpirationDelay = 60 * 24 * 7; //7 days

        #endregion

        #region Members

        private readonly IFileService m_FileService;
        private readonly IOptionsSnapshot<BlobCleanupOptions> m_Options;
        private readonly ILogger<FileStorageFunctions> m_Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="fileService"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public FileStorageFunctions(IFileService fileService, IOptionsSnapshot<BlobCleanupOptions> options, ILogger<FileStorageFunctions> logger)
        {
            m_FileService = fileService;
            m_Options = options;
            m_Logger = logger;
        }

        #endregion

        #region CleanupExpiredBlobsAsync function

        [Disable(FunctionAppEnvironment.BlobCleanupDisabledKey)]
        [FunctionName("FileStorageFunctions_CleanupExpiredBlobsAsync")]
        public async Task CleanupExpiredBlobsAsync([TimerTrigger("0 0 0 * * sun")]TimerInfo timerInfo)
        {
            uint delay = m_Options.Value.MinutesExpirationDelay ?? DefaultMinutesExpirationDelay;
            string expirationDate = DateTime.UtcNow.AddMinutes(-delay).ToString();
            int deleted = await m_FileService.CleanUpFilesAsync(delay);
            m_Logger.LogInformation("Cleaned-up '{deleted}' expired blob(s) since {expirationDate}", deleted, expirationDate);
        }

        #endregion
    }
}
