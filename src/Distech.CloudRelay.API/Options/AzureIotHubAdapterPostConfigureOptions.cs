using Distech.CloudRelay.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Options
{
    /// <summary>
    /// Represents a class that is invoked after all IConfigureOptions<AzureIotHubAdapterOptions> have run.
    /// </summary>
    public class AzureIotHubAdapterPostConfigureOptions : IPostConfigureOptions<AzureIotHubAdapterOptions>
    {
        #region Members

        private readonly IHttpContextAccessor m_HttpContextAccessor;
        private readonly IConfiguration m_Configuration;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        public AzureIotHubAdapterPostConfigureOptions(IHttpContextAccessor httpContextAccessor , IConfiguration configuration)
        {
            m_HttpContextAccessor = httpContextAccessor;
            m_Configuration = configuration;
        }

        #endregion

        #region PostConfigure

        /// <summary>
        /// Invoked to configure a TOptions instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public void PostConfigure(string name, AzureIotHubAdapterOptions options)
        {
            // apply default values 
            m_Configuration.Bind(ApiEnvironment.IoTHubDeviceCommunication, options);

            // overwrite the device method invocation response timeout
            if (m_HttpContextAccessor.HttpContext.Request.Query.TryGetValue(nameof(AzureIotHubAdapterOptions.ResponseTimeout), out var responseTimeout))
            {
                if (!int.TryParse(responseTimeout, out var parsedResponseTimeout))
                    throw new BadRequestException(ErrorCodes.InvalidResponseTimeout, ErrorMessages.GetBadRequestMessage());

                options.ResponseTimeout = parsedResponseTimeout;
            }

            // configure the environment ID
            bool environmentIdProvided = m_HttpContextAccessor.HttpContext.Request.Query.TryGetValue(nameof(AzureIotHubAdapterOptions.EnvironmentId), out var environmentId);
            options.EnvironmentId = environmentIdProvided ? environmentId.ToString() : null;
        }

        #endregion
    }
}
