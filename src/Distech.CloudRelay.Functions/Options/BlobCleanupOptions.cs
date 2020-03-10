namespace Distech.CloudRelay.Functions.Options
{
    public class BlobCleanupOptions
    {
        /// <summary>
        /// Gets or sets the delay, in minutes, for blobs since last modification to be considered expired and allow them to be reclaimed by the blob maintainer process.
        /// </summary>
        public uint? MinutesExpirationDelay { get; set; }
    }
}
