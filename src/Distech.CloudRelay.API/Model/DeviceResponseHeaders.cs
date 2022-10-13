using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceResponseHeaders
    {
        #region Properties

        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        /// <remarks>New device implementation should use <see cref="DeviceResponse.Status"/> instead.</remarks>
        public int? Status { get; set; }

        /// <summary>
        /// Gets or sets the content-type header.
        /// </summary>
        [JsonProperty(PropertyName = "Content-Type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content-disposition header.
        /// </summary>
        [JsonProperty(PropertyName = "Content-Disposition")]
        public string ContentDisposition { get; set; }

        #endregion
    }
}
