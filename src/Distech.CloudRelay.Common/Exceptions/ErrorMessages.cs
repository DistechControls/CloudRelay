using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    public class ErrorMessages
    {
        #region Common messages

        /// <summary>
        /// Returns the detailed message associated to the <see cref="IdNotFoundException"/> exception.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetIdNotFoundMessage(string id)
        {
            return $"Reference '{id}' does not exist";
        }

        /// <summary>
        /// Returns the detailed message associated to the <see cref="BadRequestException"/> and <see cref="BadModelStateRequestException"/> exception.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetBadRequestMessage(string id = default(string))
        {
            if (string.IsNullOrEmpty(id))
            {
                return "Invalid request";
            }
            else
            {
                return $"Invalid request for entity '{id}'";
            }
        }

        /// <summary>
        /// Returns the detailed message associated to the <see cref="ConflictException"/> exception.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetConflictMessage(string id = default(string))
        {
            if (string.IsNullOrEmpty(id))
            {
                return $"Entity has been updated by another request";
            }
            else
            {
                return $"Entity '{id}' has been updated by another request";
            }
        }

        #endregion

        #region Device

        /// <summary>
        /// Returns the detailed message associated to the <see cref="InvalidResultException"/> exception.
        /// </summary>
        /// <returns></returns>
        public static string GetInvalidResultMessage()
        {
            return "Invalid result";
        }

        /// <summary>
        /// Returns a detailed message associated to the <see cref="InvalidResultException"/> exception.
        /// </summary>
        /// <returns></returns>
        public static string GetMessageTooLargeErrorMessage()
        {
            return "Message too large";
        }

        /// <summary>
        /// Returns a detailed message associated to the <see cref="CommunicationException"/> exception.
        /// </summary>
        /// <returns></returns>
        public static string GetOperationErrorMessage()
        {
            return "Operation error or timeout";
        }

        #endregion

        #region Communication

        /// <summary>
        /// Returns a detailed message associated to the <see cref="CommunicationException"/> exception.
        /// </summary>
        /// <returns></returns>
        public static string GetGatewayErrorMessage()
        {
            return "Gateway error or device interaction disabled";
        }

        /// <summary>
        /// Returns a detailed message associated to the <see cref="CommunicationException"/> exception.
        /// </summary>
        /// <returns></returns>
        public static string GetQuotaExceededErrorMessage()
        {
            return "Total number of messages exceeded the allocated quota";
        }

        #endregion
    }
}
