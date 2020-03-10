using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when a conflict occurs while upserting an entity in a service layer.
    /// </summary>
    [Serializable]
    public class ConflictException
        : ApiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public ConflictException(ErrorCodes errorCode)
            : this(errorCode, ErrorMessages.GetConflictMessage())
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public ConflictException(ErrorCodes errorCode, string message)
            : this(errorCode, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ConflictException(ErrorCodes errorCode, string message, Exception inner)
            : base(StatusCodes.Status409Conflict, errorCode, message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
