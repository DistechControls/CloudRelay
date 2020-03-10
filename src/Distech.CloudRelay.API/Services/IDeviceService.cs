using Distech.CloudRelay.API.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public interface IDeviceService
    {
        /// <summary>
        /// Creates a device request based on the incoming HTTP request.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<DeviceRequest> CreateRequestAsync(string deviceId, HttpRequest request);

        /// <summary>
        /// Invokes the specified device request.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<DevicePayload> InvokeRequestAsync(string deviceId, DeviceRequest request);
    }
}
