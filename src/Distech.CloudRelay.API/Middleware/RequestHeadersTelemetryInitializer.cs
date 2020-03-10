using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Middleware
{
    public class RequestHeadersTelemetryInitializer : ITelemetryInitializer
    {
        #region Members

        private readonly IHttpContextAccessor m_HttpContextAccessor;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public RequestHeadersTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            m_HttpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Tracks custom HTTP headers as part as Application Insights telemetry.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (!(telemetry is RequestTelemetry requestTelemetry))
                return;

            var remoteQuery = m_HttpContextAccessor.HttpContext?.Request.Headers.FirstOrDefault(h => h.Key.Equals(Model.DeviceRequest.RemoteQueryHeaderName, StringComparison.OrdinalIgnoreCase));
            
            if (!remoteQuery.HasValue || remoteQuery.Value.Key == null)
                return;

            requestTelemetry.Properties.Add(Model.DeviceRequest.RemoteQueryHeaderName, remoteQuery.Value.Value.ToString());
        }

        #endregion 
    }
}
