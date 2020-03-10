using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when communication cannot be established to the underlying resources.
    /// </summary>
    [Serializable]
    public class CommunicationException
        : ApiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public CommunicationException(ErrorCodes errorCode, string message)
            : this(errorCode, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public CommunicationException(ErrorCodes errorCode, string message, Exception inner)
            : base(StatusCodes.Status503ServiceUnavailable, errorCode, message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
