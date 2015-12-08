using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace ActiveCollabSDK.SDK.Exceptions
{
    class CallFailed : Exception
    {
        public int httpCode { get; set; }
        public Dictionary<string, object> serverResponse { get; set; }
        public float? requestTime { get; set; }

        /// <summary>Construct the new exception instance</summary>
        /// <param name="httpResponseMessage">The HTTP response message.</param>
        /// <param name="requestTime">float?</param>
        /// <param name="message">string</param>
        public CallFailed(HttpResponseMessage httpResponseMessage, float? requestTime = null, string message = null)
                : base(GetMessage(httpResponseMessage, message))
        {
            httpCode = (int)httpResponseMessage.StatusCode;

            string content = httpResponseMessage.Content.ReadAsStringAsync().Result;

            if (!string.IsNullOrEmpty(content) && content.Substring(0, 1) == "{")
            {
                serverResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>
                        (httpResponseMessage.Content.ReadAsStringAsync().Result,
                        new JsonConverter[] { new NestedDictionaryConverter() });
            }
            else
            {
                serverResponse = null;
            }

            this.requestTime = requestTime;
        }

        /// <summary>Gets the message.</summary>
        /// <param name="httpResponseMessage">The HTTP response message.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string GetMessage(HttpResponseMessage httpResponseMessage, string message = null)
        {
            int statusCode = (int)httpResponseMessage.StatusCode;

            if (string.IsNullOrEmpty(message))
            {
                message = "HTTP error " + statusCode + ": " + Enum.GetName(typeof (HttpStatusCode), statusCode);
            }

            return message;
        }
    }
}
