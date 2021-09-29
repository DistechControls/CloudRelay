using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public abstract class DeviceRequest
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

        /// <summary>
        /// Gets or sets the headers of the request.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public DeviceRequestHeaders Headers { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="request"></param>
        protected DeviceRequest(HttpRequest request)
        {
            this.Path = request.Headers.Where(h => h.Key.Equals(RemoteQueryHeaderName, StringComparison.OrdinalIgnoreCase)).Select(h => h.Value.ToString()).FirstOrDefault();
            this.Method = request.Method;
            this.Headers = new DeviceRequestHeaders(request);
        }

        #endregion
    }
}
