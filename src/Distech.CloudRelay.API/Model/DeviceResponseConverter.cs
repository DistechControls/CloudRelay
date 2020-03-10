using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Model
{
    public class DeviceResponseConverter : JsonConverter
    {
        /// <summary>
        /// Gets a value indicating whether this converter can read JSON.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value indicating whether this converter can write JSON.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Returns whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DevicePayload);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            DevicePayload response = null;

            // look for specific tokens throughout the root token collection in order to do a case-insensitive search
            // default to inline body when no blob URL is specified
            if (jObject.SelectTokens(string.Empty).Children().Any(token => token.Path.Equals(nameof(DeviceFileResponse.BlobUrl), StringComparison.OrdinalIgnoreCase)))
                response = new DeviceFileResponse();
            else
                response = new DeviceInlineResponse();

            serializer.Populate(jObject.CreateReader(), response);

            return response;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
