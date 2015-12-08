using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ActiveCollabSDK.SDK
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Converters.CustomCreationConverter{System.Collections.Generic.IDictionary{System.String,System.Object}}" />
    class NestedDictionaryConverter : CustomCreationConverter<IDictionary<string, object>>
    {
        /// <summary>Creates the specified object type.</summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns></returns>
        public override IDictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            // In addition to handling IDictionary<string, object>,
            // we want to handle the deserialization of dict value, which is of type object.
            return objectType == typeof(object) || base.CanConvert(objectType);
        }

        /// <summary>Reads the JSON.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        /// <exception cref="JsonSerializationException">No object created.</exception>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject
                || reader.TokenType == JsonToken.Null)
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }

            // If the next token is an array, convert it to a Dictionary<string, object>.
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var value = new List<object>();
                if (value == null)
                    throw new JsonSerializationException("No object created.");

                var arraySerializer = new JsonSerializer();
                arraySerializer.Populate(reader, value);

                var jo = JArray.Parse(JsonConvert.SerializeObject(value));
                var rootItems = ((JArray)jo).ToArray();
                var resultItems = new Dictionary<string, object>();
                var counter = 1;

                foreach (var childItem in rootItems)
                {
                    resultItems.Add(counter++.ToString(), childItem);
                }
                
                var serializedResultItems = JsonConvert.SerializeObject(resultItems);
                return base.ReadJson(new JsonTextReader(new System.IO.StringReader(serializedResultItems)), objectType, existingValue, serializer);
            }

            // If the next token is not an object or an array,
            // then fall back on standard deserializer (strings, numbers etc.).
            return serializer.Deserialize(reader);
        }
    }
}