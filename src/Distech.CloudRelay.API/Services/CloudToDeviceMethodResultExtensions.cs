using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public static class CloudToDeviceMethodResultExtensions
    {
        public static InvocationResult ToInvocationResult(this CloudToDeviceMethodResult result)
        {
            string payload = result.GetPayloadAsJson();
            
            // if necessary, device result can be 'unstringified' to allow proper deserialization afterwards
            if (!string.IsNullOrEmpty(payload) && payload.StartsWith("\""))
                payload = JsonConvert.DeserializeObject<string>(payload);

            return new InvocationResult(result.Status, payload);
        }
    }
}
