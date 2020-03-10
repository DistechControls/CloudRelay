using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents an RFC 7807 problem details defining a specific error code to identity an error.
    /// </summary>
    public class ApplicationProblemDetails
        : ProblemDetails
    {
        /// <summary>
        /// The error code identifying the application error.
        /// </summary>
        public ErrorCodes ErrorCode { get; set; }
    }
}
