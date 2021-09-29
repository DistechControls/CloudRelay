using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceFileResponse : DeviceResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the URL of the blob that contains the actual response body.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string BlobUrl { get; set; }

        #endregion
    }
}
