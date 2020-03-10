using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceHeaders
    {
        #region Properties

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Accept { get; set; }

        [JsonProperty(PropertyName = HeaderNames.ContentType, NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty(PropertyName = HeaderNames.ContentDisposition, NullValueHandling = NullValueHandling.Ignore)]
        public string ContentDisposition { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DeviceHeaders()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="request"></param>
        public DeviceHeaders(HttpRequest request)
        {
            Accept = request.GetTypedHeaders().Accept?.Select(a => a.ToString()).Aggregate((accept1, accept2) => $"{accept1}, {accept2}");
            ContentType = request.GetTypedHeaders().ContentType?.ToString();
            ContentDisposition = request.GetTypedHeaders().ContentDisposition?.ToString();
        }

        #endregion

        #region GetEncoding

        /// <summary>
        /// Returns the encoding associated with the charset header or default to UTF-8.
        /// </summary>
        /// <returns></returns>
        public Encoding GetEncoding()
        {
            if (string.IsNullOrEmpty(this.ContentType))
                return Encoding.UTF8;

            try
            {
                var contentType = new ContentType(this.ContentType);
                return Encoding.GetEncoding(contentType.CharSet);
            }
            catch
            {
                // always default to UTF-8 in case of an invalid/unsupported charset is provided
                return Encoding.UTF8;
            }
        }

        #endregion
    }
}
