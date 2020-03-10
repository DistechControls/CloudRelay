using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Options
{
    /// <summary>
    /// Represents a class that is invoked after all IConfigureOptions<FileStorageOptions> have run.
    /// </summary>
    internal class FileStoragePostConfigureOptions : IPostConfigureOptions<FileStorageOptions>
    {
        #region Members

        private readonly IOptionsSnapshot<AzureStorageAccountOptions> m_StorageOptions;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public FileStoragePostConfigureOptions(IOptionsSnapshot<AzureStorageAccountOptions> azureStorageOptions)
        {
            m_StorageOptions = azureStorageOptions;
        }

        #endregion

        #region PostConfigure

        /// <summary>
        /// Invoked to configure a TOptions instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public void PostConfigure(string name, FileStorageOptions options)
        {
            m_StorageOptions.Value.ConnectionString = options.ConnectionString;
        }

        #endregion
    }
}
