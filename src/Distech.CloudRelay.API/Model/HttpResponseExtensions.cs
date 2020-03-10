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
        public static void SetHeadersFromDeviceResponse(this HttpResponse response, DeviceHeaders headers)
        {
            // select all headers except Status and ContentType that are handled by the IActionResult implementation
            var headerMembers = headers.GetType().GetMembers().Where(m => m.MemberType == MemberTypes.Property
                                                                     && m.Name != nameof(DeviceHeaders.Status)
                                                                     && m.Name != nameof(DeviceHeaders.ContentType));

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
