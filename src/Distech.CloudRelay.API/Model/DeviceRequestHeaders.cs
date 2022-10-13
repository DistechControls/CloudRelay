using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceRequestHeaders
    {
        #region Properties

        /// <summary>
        /// Gets or sets the accept header.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Accept { get; set; }

        /// <summary>
        /// Gets or sets the content-type header.
        /// </summary>
        [JsonProperty(PropertyName = "Content-Type", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content-disposition header.
        /// </summary>
        [JsonProperty(PropertyName = "Content-Disposition", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentDisposition { get; set; }

        /// <summary>
        /// Gets or sets the content-length header.
        /// </summary>
        [JsonProperty(PropertyName = "Content-Length", NullValueHandling = NullValueHandling.Ignore)]
        public long? ContentLength { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DeviceRequestHeaders()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="request"></param>
        public DeviceRequestHeaders(HttpRequest request)
        {
            Accept = request.GetTypedHeaders().Accept
                .Select(a => a.ToString())
                .DefaultIfEmpty() //aggregate does not support empty sequence
                .Aggregate((accept1, accept2) => $"{accept1}, {accept2}");
            ContentType = request.GetTypedHeaders().ContentType?.ToString();
            ContentDisposition = request.GetTypedHeaders().ContentDisposition?.ToString();
            ContentLength = request.ContentLength;
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
