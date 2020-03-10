using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceInlineRequest : DeviceRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the body of the request.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Body { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="request"></param>
        public DeviceInlineRequest(HttpRequest request) : base(request)
        {
        }

        #endregion
    }
}
