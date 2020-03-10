using Distech.CloudRelay.Common.Exceptions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Middleware
{
    /// <summary>
    /// Filters known exceptions from the information gathered by Application Insights.
    /// </summary>
    public class ExpectedExceptionTelemetryProcessor : ITelemetryProcessor
    {
        #region Properties

        private ITelemetryProcessor Next { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="next"></param>
        public ExpectedExceptionTelemetryProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        #endregion

        #region Process

        /// <summary>
        /// Processes a collected telemetry item.
        /// </summary>
        /// <param name="item"></param>
        public void Process(ITelemetry item)
        {
            // filters expected exceptions by breaking the processing chain
            var dependency = item as ExceptionTelemetry;

            if (dependency?.Exception is ApiException)
                return;

            this.Next.Process(item);
        }

        #endregion
    }
}
