using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceInlineResponse : DeviceResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the body of the response.
        /// </summary>
        public object Body { get; set; }

        #endregion

        #region GetRawBody

        /// <summary>
        /// Returns the raw version of the body.
        /// </summary>
        /// <returns></returns>
        public string GetRawBody()
        {
            if (this.Body == null)
                return null;

            // Body was received as JSON (JArray or JObject) and should be returned as an unformatted string
            if (typeof(JToken).IsAssignableFrom(this.Body.GetType()))
                return (this.Body as JToken).ToString(Formatting.None);

            // Body was received as raw string (ECLYPSE behavior) and should be returned as is
            return this.Body.ToString();
        }

        #endregion
    }
}
