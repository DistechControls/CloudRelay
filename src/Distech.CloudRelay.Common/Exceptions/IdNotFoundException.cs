using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when an entity ID is not found in a service layer.
    /// </summary>
    [Serializable]
    public class IdNotFoundException
        : ApiException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="id"></param>
        public IdNotFoundException(ErrorCodes errorCode, string id)
            : this(errorCode, id, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="id"></param>
        /// <param name="inner"></param>
        public IdNotFoundException(ErrorCodes errorCode, string id, Exception inner)
            : base(StatusCodes.Status404NotFound, errorCode, ErrorMessages.GetIdNotFoundMessage(id), inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected IdNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
