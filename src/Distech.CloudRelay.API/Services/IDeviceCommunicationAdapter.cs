using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public interface IDeviceCommunicationAdapter
    {
        /// <summary>
        /// Invokes a command on the remote device.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="jsonPayload"></param>
        /// <exception cref="Common.Exceptions.IdNotFoundException">The device ID was not found.</exception>
        /// <exception cref="Common.Exceptions.CommunicationException">Cannot communicate with the IoT Hub.</exception>
        /// <exception cref="Common.Exceptions.CommunicationException">The IoT Hub returned an error code.</exception>
        /// <exception cref="Common.Exceptions.OperationException">An error occured during a device client operation.</exception>
        /// <returns></returns>
        Task<InvocationResult> InvokeCommandAsync(string deviceId, string jsonPayload);

        /// <summary>
        /// Returns the maximum message size.
        /// Any message with a size above this limit can either be rejected or buffered to workaround the limitation.
        /// </summary>
        /// <returns></returns>
        int GetMaximumMessageSize();
    }
}
