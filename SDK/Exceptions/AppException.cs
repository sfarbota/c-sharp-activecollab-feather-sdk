using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace ActiveCollabSDK.SDK.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ActiveCollabSDK.SDK.Exception" />
    class AppException : Exception
    {
        private const int BadRequest = 400;
        private const int Unauthorized = 401;
        private const int Forbidden = 403;
        private const int NotFound = 404;
        private const int InvalidProperties = 400;
        private const int Conflict = 409;
        private const int OperationFailed = 500;
        private const int Unavailable = 503;
        public int httpCode { get; set; }
        public Dictionary<string, object> serverResponse { get; set; }
        public float? requestTime { get; set; }

        /// <summary>Construct the new exception instance</summary>
        /// <param name="httpResponseMessage">The HTTP response message.</param>
        /// <param name="requestTime">float?</param>
        /// <param name="message">string</param>
        public AppException(HttpResponseMessage httpResponseMessage, float? requestTime = null, string message = null)
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
            Dictionary<string, object> serverResponse = null;

            string content = httpResponseMessage.Content.ReadAsStringAsync().Result;

            if (!string.IsNullOrEmpty(content) && content.Substring(0, 1) == "{")
            {
                serverResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>
                        (httpResponseMessage.Content.ReadAsStringAsync().Result,
                        new JsonConverter[] { new NestedDictionaryConverter() });
            }
                
            if (string.IsNullOrEmpty(message))
            {
                switch ((int)httpResponseMessage.StatusCode)
                {
                    case BadRequest:
                        message = "Bad Request";
                        break;
                    case Unauthorized:
                        message = "Unauthorized";
                        break;
                    case Forbidden:
                        message = "Forbidden";
                        break;
                    case NotFound:
                        message = "Not Found";
                        break;
                    // NOTE: The "Invalid Properties" status is invalid here
                    // because code 400 is already used for "Bad Request".
                    //case InvalidProperties:
                    //    message = "Invalid Properties";
                    //    break;
                    case Conflict:
                        message = "Conflict";
                        break;
                    case OperationFailed:
                        message = "Operation failed";
                        break;
                    case Unavailable:
                        message = "Unavailable";
                        break;
                    default:
                        message = "Unknown HTTP error";
                        break;
                }

                if (serverResponse != null)
                {
                    message += "Error (" + serverResponse["type"] + "): " +
                            serverResponse["message"];
                }
                else
                {
                    message += "Error: " + httpResponseMessage.Content.ReadAsStringAsync().Result;
                }
            }
            
            return message;
        }
    }
}
