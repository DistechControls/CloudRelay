using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents the error codes reported by the application.
    /// </summary>
    public enum ErrorCodes
    {
        #region Common

        /// <summary>
        /// Represents an uninitialized error. 
        /// </summary>
        /// <remarks>
        /// This error should not be directly used and returned to the client.
        /// Instead a dedicated error code should be preferred, even if still not explicit.
        /// </remarks>
        UnknownError = 0,

        /// <summary>
        /// `responseTimeout` specified in the request is invalid.
        /// </summary>
        InvalidResponseTimeout = 100,

        #endregion

        #region Device

        /// <summary>
        /// `deviceId` specified in the request URL does not exist.
        /// </summary>
        DeviceNotFound = 1000,

        /// <summary>
        /// An error occured during device client operation.
        /// </summary>
        DeviceOperationError = 1001,

        /// <summary>
        /// Invalid result received from the controller
        /// </summary>
        InvalidResult = 1002,

        /// <summary>
        /// Message too large received from the controller (greather than 131072 bytes)
        /// </summary>
        MessageTooLarge = 1003,

        #endregion

        #region Environment

        /// <summary>
        /// `environmentId` specified in the request does not exist.
        /// </summary>
        EnvironmentNotFound = 3000,

        #endregion

        #region Communication

        /// <summary>
        /// An error occured while communicating with underlying resources.
        /// </summary>
        CommunicationError = 5000,

        /// <summary>
        /// An error occured in the gateway which prevent communicating with the underlying resource.
        /// </summary>
        GatewayError = 5001,

        /// <summary>
        /// An error occured in the gateway because the maximum number of messages has been reached.
        /// </summary>
        QuotaExceeded = 5002,

        #endregion

        #region Blob

        BlobNotFound = 6000,
        BlobAlreadyExists = 6001,

        #endregion
    }
}
