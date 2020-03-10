using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when a request contains invalid parameters.
    /// </summary>
    [Serializable]
    public class BadRequestException
        : ApiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public BadRequestException(ErrorCodes errorCode, string message)
            : this(errorCode, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public BadRequestException(ErrorCodes errorCode, string message, Exception inner)
            : base(StatusCodes.Status400BadRequest, errorCode, message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected BadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
