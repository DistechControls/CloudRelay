using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public abstract class DevicePayload
    {
        #region Properties

        /// <summary>
        /// Gets or sets the headers of the payload.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public DeviceHeaders Headers { get; set; }

        #endregion
    }
}
