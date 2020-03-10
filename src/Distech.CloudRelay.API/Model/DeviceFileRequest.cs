using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceFileRequest : DeviceRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the URL of the blob that contains the actual request body.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string BlobUrl { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="request"></param>
        public DeviceFileRequest(HttpRequest request) : base(request)
        {
        }

        #endregion
    }
}
