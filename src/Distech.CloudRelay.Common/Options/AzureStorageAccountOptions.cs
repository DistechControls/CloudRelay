using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Options
{
    /// <summary>
    /// Represents the Azure storage account related settings.
    /// </summary>
    internal class AzureStorageAccountOptions
    {
        /// <summary>
        /// Gets or sets the connection string to the Azure storage account.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
