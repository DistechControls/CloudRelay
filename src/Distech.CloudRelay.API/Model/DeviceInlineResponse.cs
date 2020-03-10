using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceInlineResponse : DevicePayload
    {
        #region Properties

        /// <summary>
        /// Gets or sets the body of the response.
        /// </summary>
        public string Body { get; set; }

        #endregion
    }
}
