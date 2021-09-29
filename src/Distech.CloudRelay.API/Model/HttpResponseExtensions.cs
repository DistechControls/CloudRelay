using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// Sets the client response headers based on the headers received from the device.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="headers"></param>
        public static void SetHeadersFromDeviceResponse(this HttpResponse response, DeviceResponseHeaders headers)
        {
            // select all headers except Status and ContentType that are handled by the IActionResult implementation
            var headerMembers = headers.GetType().GetMembers().Where(m => m.MemberType == MemberTypes.Property
                                                                     && m.Name != nameof(DeviceResponseHeaders.Status)
                                                                     && m.Name != nameof(DeviceResponseHeaders.ContentType));

            foreach (var headerMember in headerMembers)
            {
                var value = (headerMember as PropertyInfo)?.GetValue(headers)?.ToString();

                // discard the member property if its value is null (no data was received from the device)
                if (string.IsNullOrEmpty(value))
                    continue;

                var name = headerMember.Name;
                var attr = headerMember.GetCustomAttribute<JsonPropertyAttribute>();

                if (!string.IsNullOrEmpty(attr?.PropertyName))
                    name = attr.PropertyName;

                response.Headers.Add(name, value); 
            }
        }
    }
}
