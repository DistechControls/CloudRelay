using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public abstract class DeviceResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        /// <remarks>This value has priority over the <see cref="DeviceResponseHeaders.Status"/> value.</remarks>
        public int? Status { get; set; }

        /// <summary>
        /// Gets or sets the headers of the response.
        /// </summary>
        public DeviceResponseHeaders Headers { get; set; }

        #endregion
    }
}
