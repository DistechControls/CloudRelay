using System;
using System.Collections.Generic;
using System.Text;

namespace Distech.CloudRelay.Functions
{
    public static class FunctionAppEnvironment
    {
        /// <summary>
        /// The key to retrieve the configuration section related to the file storage.
        /// </summary>
        public const string FileStorageSectionKey = "FileStorage";

        /// <summary>
        /// The key to retrieve the configuration section related to the blob clean-up function.
        /// </summary>
        public const string BlobCleanupSectionKey = "FileStorage:Cleanup";

        /// <summary>
        /// The key to retrieve whether the blob function is disabled.
        /// </summary>
        public const string BlobCleanupDisabledKey = BlobCleanupSectionKey + ":Disabled";

        /// <summary>
        /// The key to retrieve the blob expiration delay when cleaning-up blobs related to the relay API.
        /// </summary>
        public const string BlobCleanupExpirationDelayKey = BlobCleanupSectionKey + ":MinutesExpirationDelay";
    }
}
