using Distech.CloudRelay.API.Model;
using Distech.CloudRelay.Common.DAL;
using Distech.CloudRelay.Common.Exceptions;
using Distech.CloudRelay.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public class DeviceService
        : IDeviceService
    {
        #region Members

        private readonly IDeviceCommunicationAdapter m_DeviceCommunicationAdapter;
        private readonly IFileService m_FileService;
        private readonly ILogger<DeviceService> m_Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="deviceCommunicationAdapter"></param>
        /// <param name="fileService"></param>
        /// <param name="logger"></param>
        public DeviceService(IDeviceCommunicationAdapter deviceCommunicationAdapter, IFileService fileService, ILogger<DeviceService> logger)
        {
            m_DeviceCommunicationAdapter = deviceCommunicationAdapter;
            m_FileService = fileService;
            m_Logger = logger;
        }

        #endregion

        #region IDeviceService Implementation

        /// <summary>
        /// Creates a device request based on the incoming HTTP request.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DeviceRequest> CreateRequestAsync(string deviceId, HttpRequest request)
        {
            // discard any body that could have been provided when doing GET or DELETE requests
            if (HttpMethods.IsGet(request.Method) || HttpMethods.IsDelete(request.Method))
                return new DeviceInlineRequest(request);

            // determine whether the request body needs to be written to file storage or sent inline to the device.
            // a more accurate check could be done against the serialized DeviceRequest instead of the current request.ContentLength since additional info is added to the payload later on.
            // that would still not be perfectly accurate since the SDK adds its own data to the final payload.
            if (request.HasFormContentType || request.ContentLength.GetValueOrDefault() > m_DeviceCommunicationAdapter.GetMaximumMessageSize())
            {
                var fileData = new BlobStreamDecorator(request.Body) { ContentType = request.ContentType };
                string blobSasUrl = await m_FileService.WriteFileAsync(deviceId, fileData);

                return new DeviceFileRequest(request)
                {
                    BlobUrl = blobSasUrl
                };
            }
            else
            {
                var inlineRequest = new DeviceInlineRequest(request);

                using (StreamReader reader = new StreamReader(request.Body, inlineRequest.Headers.GetEncoding()))
                {
                    inlineRequest.Body = await reader.ReadToEndAsync();
                }

                return inlineRequest;
            }
        }

        /// <summary>
        /// Invokes the specified device request.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DeviceResponse> InvokeRequestAsync(string deviceId, DeviceRequest request)
        {
            // no JSON formatting to save as much space as possible
            var payload = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            InvocationResult result = await m_DeviceCommunicationAdapter.InvokeCommandAsync(deviceId, payload);
            
            try
            {
                DeviceResponse response = JsonConvert.DeserializeObject<DeviceResponse>(result.Content, new DeviceResponseConverter());

                // verify whether a status code was received (throwing an exception here is likely to break previous integrations)
                // 200 OK will be returned to the client if none of the response statuses are set
                if (response.Status == null && response.Headers?.Status == null)
                    m_Logger.LogWarning($"Response status not set.");

                return response;
            }
            catch(JsonReaderException ex)
            {
                m_Logger.LogWarning($"A JsonReaderException occurred: {ex}");
                throw new InvalidResultException(ErrorCodes.InvalidResult, ErrorMessages.GetInvalidResultMessage(), ex); 
            }
            catch (JsonSerializationException ex)
            {
                m_Logger.LogWarning($"A JsonSerializationException occurred: {ex}");
                throw new InvalidResultException(ErrorCodes.InvalidResult, ErrorMessages.GetInvalidResultMessage(), ex);
            }
        }

        #endregion
    }
}
