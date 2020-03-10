using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Options
{
    /// <summary>
    /// Represents the options used to access files from the storage system.
    /// </summary>
    public class FileStorageOptions
    {
        /// <summary>
        /// Gets or sets the connection string to the Azure storage account.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the folder used by the device to upload files.
        /// </summary>
        public string DeviceFileUploadFolder { get; set; }

        /// <summary>
        /// Gets or sets the name of the folder used by the server to upload files.
        /// </summary>
        public string ServerFileUploadFolder { get; set; }

        /// <summary>
        /// Gets or sets the name of the sub folder used by the server to upload files.
        /// </summary>
        public string ServerFileUploadSubFolder { get; set; }
    }
}
