using Distech.CloudRelay.API.Options;
using Distech.CloudRelay.Common.Exceptions;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public class AzureIotHubAdapter
        : IDeviceCommunicationAdapter
    {
        #region Constants

        private const int DirectMethodMinResponseTimeout = 5;
        private const int DirectMethodMaxResponseTimeout = 300;

        #endregion

        #region Members

        private readonly ServiceClient m_ServiceClient;
        private readonly IOptionsSnapshot<AzureIotHubAdapterOptions> m_Options;
        private readonly ILogger<AzureIotHubAdapter> m_Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public AzureIotHubAdapter(ServiceClient client, IOptionsSnapshot<AzureIotHubAdapterOptions> options, ILogger<AzureIotHubAdapter> logger)
        {
            m_ServiceClient = client;
            m_Options = options;
            m_Logger = logger;
        }

        #endregion

        #region IDeviceCommunicationAdapter Implementation

        /// <summary>
        /// Invokes a command on the remote device using direct methods.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="jsonPayload"></param>
        /// <exception cref="IdNotFoundException">The device ID was not found.</exception>
        /// <exception cref="CommunicationException">Cannot communicate with the IoT Hub.</exception>
        /// <exception cref="CommunicationException">The IoT Hub returned an error code.</exception>
        /// <exception cref="OperationException">An error occured during a device client operation.</exception>
        /// <returns></returns>
        public async Task<InvocationResult> InvokeCommandAsync(string deviceId, string jsonPayload)
        {
            // ensure the ResponseTimeout is whitin valid range to prevent exception
            int responseTimeout = m_Options.Value.ResponseTimeout;
            responseTimeout = Math.Min(Math.Max(DirectMethodMinResponseTimeout, responseTimeout), DirectMethodMaxResponseTimeout);

            var deviceMethod = new CloudToDeviceMethod(m_Options.Value.MethodName, TimeSpan.FromSeconds(responseTimeout));
            deviceMethod.SetPayloadJson(jsonPayload);

            try
            {
                CloudToDeviceMethodResult result = await m_ServiceClient.InvokeDeviceMethodAsync(deviceId, deviceMethod);
                return result.ToInvocationResult();
            }
            catch (IotHubException ex) when (TryHandleIotHubException(deviceId, ex, out ApiException apiException))
            {
                throw apiException;
            }
        }

        /// <summary>
        /// Returns the maximum message size.
        /// Any message with a size above this limit can either be rejected or buffered to workaround the limitation.
        /// </summary>
        /// <returns></returns>
        public int GetMaximumMessageSize()
        {
            return m_Options.Value.MessageSizeThreshold;
        }

        #endregion

        #region IoT Hub Exceptions Handling

        /// <summary>
        /// Handles IoT Hub exception and transform it as an <see cref="ApiException"/> with a well-known application error code.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="iotHubException"></param>
        /// <param name="apiException"></param>
        /// <returns></returns>
        private bool TryHandleIotHubException(string deviceId, IotHubException iotHubException, out ApiException apiException)
        {
            switch (iotHubException)
            {
                // thrown when there's no IoT Hub device registration OR there's an enabled IoT Hub device registration but device did not establish connection yet
                case DeviceNotFoundException ex:
                    apiException = new IdNotFoundException(ErrorCodes.DeviceNotFound, deviceId, ex);
                    break;

                // thrown when an attempt to communicate with the IoT Hub fails
                case IotHubCommunicationException ex:
                    m_Logger.LogWarning(ex, $"An IotHubCommunicationException occurred");
                    apiException = new CommunicationException(ErrorCodes.CommunicationError, ex.Message, ex);
                    break;

                // thrown when the IoT Hub returns an error code (i.e. device registration is disable which prevent the actual device from establishing a connection)
                case ServerErrorException ex:
                    m_Logger.LogWarning(ex, "A ServerErrorException occurred");
                    apiException = new CommunicationException(ErrorCodes.GatewayError, ErrorMessages.GetGatewayErrorMessage(), ex);
                    break;

                // thrown when the maximum number of IoT Hub messages has been reached
                case QuotaExceededException ex:
                    apiException = new CommunicationException(ErrorCodes.QuotaExceeded, ErrorMessages.GetQuotaExceededErrorMessage(), ex);
                    break;

                // thrown when the message size is greater than the max size allowed (131072 bytes)
                case MessageTooLargeException ex:
                    apiException = new InvalidResultException(ErrorCodes.MessageTooLarge, ErrorMessages.GetMessageTooLargeErrorMessage(), ex);
                    break;

                // thrown when an error occurs during device client operation (i.e. device doesn't repond within the configured time out)
                // shall always be kept last
                case IotHubException ex:
                    m_Logger.LogWarning(ex, "An IotHubException occurred");
                    apiException = new OperationException(ErrorCodes.DeviceOperationError, ErrorMessages.GetOperationErrorMessage(), ex);
                    break;

                // exception won't be transformed and therefore will be logged accordingly
                default:
                    apiException = null;
                    break;
            }

            return apiException != null;
        }

        #endregion
    }
}
