using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Options
{
    /// <summary>
    /// Represents the options to configure the Azure IotHub adapter.
    /// </summary>
    public class AzureIotHubAdapterOptions
    {
        /// <summary>
        /// Gets or sets the identifier of the environment where the device is located.
        /// </summary>
        public string EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the device method to invoke.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to wait for a response from the device method invocation.
        /// </summary>
        public int ResponseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes above which the message is buffered to blob storage.
        /// </summary>
        public int MessageSizeThreshold { get; set; }
    }
}
