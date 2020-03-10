using Distech.CloudRelay.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API
{
    public class ApiEnvironment
    {
        #region Resource Type

        public enum ResourceType
        {
            IoTHubService,
            StorageAccount
        }

        #endregion

        #region Environment Constants

        /// <summary>
        /// The app settings name for the authentication provider. 
        /// </summary>
        public const string AuthenticationProvider = "Authentication:Provider";

        /// <summary>
        /// The app settings name for the Azure AD options.
        /// </summary>
        public const string AzureADOptions = "Authentication:AzureAD";

        /// <summary>
        /// The app settings name for the Azure AD application permisions (aka roles).
        /// </summary>
        public const string AzureADRoles = "Authentication:AzureAD:Roles";

        /// <summary>
        /// The app settings name for the Azure AD user delegated permisions (aka scopes).
        /// </summary>
        public const string AzureADScopes = "Authentication:AzureAD:Scopes";

        /// <summary>
        /// The app settings name for the Azure IoTHub adapter options.
        /// </summary>
        public const string IoTHubDeviceCommunication = "DeviceCommunication:IoTHub";

        /// <summary>
        /// The key to retrieve the IoT hub connection string of the service endpoint.
        /// </summary>
        private const string IoTHubServiceConnectionStringKey = "IoTHub:Service";

        /// <summary>
        /// The key to retrieve the storage account connection string.
        /// </summary>
        private const string StorageAccountConnectionStringKey = "StorageAccount";

        /// <summary>
        /// The key to retrieve the configuration section related to the file storage.
        /// </summary>
        public const string FileStorageSectionKey = "FileStorage";

        #endregion

        #region GetConnectionString

        /// <summary>
        /// Returns the computed connection string based on the specified parameters.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="environmentId"></param>
        /// <param name="configuration"></param>
        /// <exception cref="IdNotFoundException">The environment ID was not found.</exception>
        /// <returns></returns>
        public static string GetConnectionString(ResourceType resourceType, string environmentId, IConfiguration configuration)
        {
            string connectionStringKey = null;

            switch (resourceType)
            {
                case ResourceType.IoTHubService:
                    connectionStringKey = string.IsNullOrEmpty(environmentId) ? IoTHubServiceConnectionStringKey : $"{environmentId}:{IoTHubServiceConnectionStringKey}";
                    break;

                case ResourceType.StorageAccount:
                    connectionStringKey = string.IsNullOrEmpty(environmentId) ? StorageAccountConnectionStringKey : $"{environmentId}:{StorageAccountConnectionStringKey}";
                    break;
            }

            string connectionString = configuration.GetConnectionString(connectionStringKey);

            // ensure the environment exists in configuration when specified
            if (!string.IsNullOrEmpty(environmentId) && string.IsNullOrEmpty(connectionString))
                throw new IdNotFoundException(ErrorCodes.EnvironmentNotFound, environmentId);

            return connectionString;
        }

        #endregion
    }
}
