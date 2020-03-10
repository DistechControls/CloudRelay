using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when the device returns a result that do not respect the data contract.
    /// </summary>
    [Serializable]
    public class InvalidResultException
        : ApiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public InvalidResultException(ErrorCodes errorCode, string message)
            : this(errorCode, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public InvalidResultException(ErrorCodes errorCode, string message, Exception inner)
            : base(StatusCodes.Status502BadGateway, errorCode, message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InvalidResultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
