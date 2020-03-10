using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents an exception formattable to the RFC 7807 format.
    /// </summary>
    [Serializable]
    public abstract class ApiException
        : Exception
    {
        #region Properties

        /// <summary>
        /// Returns the HTTP status code associated to the exception.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Returns the application error code associated to the exception.
        /// </summary>
        public ErrorCodes ErrorCode { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ApiException(int statusCode, ErrorCodes errorCode, string message, Exception inner)
            : base(message, inner)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ApiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Format the exception as a problem details.
        /// </summary>
        /// <returns></returns>
        public virtual ProblemDetails ToProblemDetails()
        {
            var details = CreateProblemDetails();

            details.Type = "about:blank";
            details.Title = ReasonPhrases.GetReasonPhrase(StatusCode);
            details.Status = StatusCode;
            details.Detail = Message;

            ErrorCodeHelpAttribute attr = ErrorCode.GetType()
                .GetMember(ErrorCode.ToString())
                .Where(m => m.MemberType == MemberTypes.Field)
                .FirstOrDefault()
                ?.GetCustomAttribute<ErrorCodeHelpAttribute>();

            if (attr != null)
            {
                details.Title = attr.Title;
                details.Type = attr.ResolveUrl();
            }

            return details;
        }

        /// <summary>
        /// Creates a new problem details instance.
        /// </summary>
        /// <returns></returns>
        protected virtual ProblemDetails CreateProblemDetails()
        {
            return new ApplicationProblemDetails { ErrorCode = ErrorCode };
        }

        #endregion
    }
}
