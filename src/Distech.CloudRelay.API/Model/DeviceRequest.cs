using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Distech.CloudRelay.API.Model
{
    public abstract class DeviceRequest : DevicePayload
    {
        #region Constants

        public const string RemoteQueryHeaderName = "Remote-Query";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the device resource path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the method associated with the device resource path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Method { get; set; }

        #endregion

        #region Constructors

        protected DeviceRequest(HttpRequest request)
        {
            this.Path = request.Headers.Where(h => h.Key.Equals(RemoteQueryHeaderName, StringComparison.OrdinalIgnoreCase)).Select(h => h.Value.ToString()).FirstOrDefault();
            this.Method = request.Method;
            this.Headers = new DeviceHeaders(request);
        }

        #endregion
    }
}
