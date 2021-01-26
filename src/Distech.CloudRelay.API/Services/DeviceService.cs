using Distech.CloudRelay.API.Model;
using Distech.CloudRelay.Common.DAL;
using Distech.CloudRelay.Common.Exceptions;
using Distech.CloudRelay.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
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
        private readonly ITelemetryService m_TelemetryService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="deviceCommunicationAdapter"></param>
        /// <param name="fileService"></param>
        /// <param name="logger"></param>
        /// <param name="telemetryService"></param>
        public DeviceService(IDeviceCommunicationAdapter deviceCommunicationAdapter, IFileService fileService, ILogger<DeviceService> logger, ITelemetryService telemetryService)
        {
            m_DeviceCommunicationAdapter = deviceCommunicationAdapter;
            m_FileService = fileService;
            m_Logger = logger;
            m_TelemetryService = telemetryService;
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
                var blobSasUrl = string.Empty;

                await m_TelemetryService.IncrementCounterAsync("blob_upload", new string[] { });
                blobSasUrl = await m_TelemetryService.StartTimerAsync(() => m_FileService.WriteFileAsync(deviceId, fileData), "blob_upload", new string[] { });
                m_TelemetryService.Dispose();
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
        public async Task<DevicePayload> InvokeRequestAsync(string deviceId, DeviceRequest request)
        {
            // no JSON formatting to save as much space as possible
            var payload = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var result = default(InvocationResult);
            
            await m_TelemetryService.IncrementCounterAsync("direct_method", new string[] { });
            result = await m_TelemetryService.StartTimerAsync(() => m_DeviceCommunicationAdapter.InvokeCommandAsync(deviceId, payload), "direct_method", new string[] { });
            m_TelemetryService.Dispose();

            try
            {
                return JsonConvert.DeserializeObject<DevicePayload>(result.Content, new DeviceResponseConverter());
            }
            catch(JsonReaderException ex)
            {
                throw new InvalidResultException(ErrorCodes.InvalidResult, ErrorMessages.GetInvalidResultMessage(), ex); 
            }
            catch (JsonSerializationException ex)
            {
                throw new InvalidResultException(ErrorCodes.InvalidResult, ErrorMessages.GetInvalidResultMessage(), ex);
            }
        }

        #endregion
    }
}
