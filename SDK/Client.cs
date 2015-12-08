using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ActiveCollabSDK.SDK.Exceptions;
using Newtonsoft.Json;

/// <summary>activeCollab API client</summary>
namespace ActiveCollabSDK.SDK
{
    /// <summary>
    /// 
    /// </summary>
    public static class Client
    {
        /// <summary>The version</summary>
        private const string Version = "5.0.0";

        /// <summary>Return user agent string</summary>
        /// <returns>string</returns>
        public static string GetUserAgent()
        {
            return "activeCollab API Wrapper; v" + Version;
        }

        #region Info

        /// <summary>Cached info response</summary>
        private static Dictionary<string, object> _infoResponse = null;

        /// <summary>Return info</summary>
        /// <param name="property">string</param>
        /// <returns>string</returns>
        public static Dictionary<string, object> info(string property = null)
        {
            if (_infoResponse == null)
            {
                _infoResponse = GetJson(Get("info"));
            }

            if (property != null)
            {
                if (_infoResponse.ContainsKey(property) && _infoResponse[property].GetType().IsGenericType)
                {
                    switch (_infoResponse[property].GetType().GetGenericTypeDefinition().ToString().ToLower())
                    {
                        case "string":
                            if (_infoResponse[property] == null)
                            {
                                return null;
                            }
                            else
                            {
                                return new Dictionary<string, object>()
                            {
                                {property, (string) _infoResponse[property]}
                            };
                            }
                        case "dictionary<,>":
                            return (Dictionary<string, object>)_infoResponse[property];
                        default:
                            return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return _infoResponse;
            }
        }

        #endregion


        #region Make and process requests

        /// <summary>API URL</summary>
        /// <value>The URL.</value>
        public static string url { get; set; }

        /// <summary>API version</summary>
        private static string myApiVersion = "1";

        /// <summary>Gets or sets the API version.</summary>
        /// <value>The API version.</value>
        public static string apiVersion
        {
            get
            {
                return myApiVersion;
            }
            set
            {
                myApiVersion = value;
            }
        }

        /// <summary>API key</summary>
        /// <value>The key.</value>
        public static string key { get; set; }

        /// <summary>Connector instance</summary>
        private static Connector myConnector = new Connector();

        /// <summary>Gets or sets the connector.</summary>
        /// <value>The connector.</value>
        public static Connector connector
        {
            get
            {
                return myConnector;
            }
            set
            {
                myConnector = value;
            }
        }

        /// <summary>Issues the token.</summary>
        /// <param name="emailOrUsername">string</param>
        /// <param name="password">string</param>
        /// <param name="clientName">string</param>
        /// <param name="clientVendor">string</param>
        /// <param name="readOnly">bool</param>
        /// <returns>string</returns>
        /// <exception cref="ActiveCollabSDK.SDK.Exceptions.IssueTokenException"></exception>
        /// <exception cref="Exceptions\IssueTokenException"></exception>
        public static string IssueToken(string emailOrUsername, string password, string clientName,
                string clientVendor, bool readOnly = false)
        {
            HttpResponseMessage response = connector.Post(PrepareUrl("issue-token"), null,
                    PrepareParams(new Dictionary<string, object>() {
                        { "password", password },
                        { "clientName", clientName },
                        { "clientVendor", clientVendor },
                        { "readOnly", readOnly.ToString() }
                    }));

            string error = "0";

            if (response != null && GetJson(response) != null && IsJson(response))
            {
                Dictionary<string, object> json = GetJson(response);

                if (json.ContainsKey("is_error"))
                {
                    error = (string)json["error"];
                }
                else
                {
                    return (string)json["token"];
                }
            }

            throw new IssueTokenException(error);
        }

        /// <summary>Send a GET request</summary>
        /// <param name="path">string</param>
        /// <returns>HttpResponseMessage</returns>
        public static HttpResponseMessage Get(string path)
        {
            return connector.Get(PrepareUrl(path), PrepareHeaders());
        }

        /// <summary>Send a POST request</summary>
        /// <param name="path">string</param>
        /// <param name="parameters">array|null</param>
        /// <param name="attachments">array|null</param>
        /// <returns>HttpResponseMessage</returns>
        public static HttpResponseMessage Post(string path, Dictionary<string, object> parameters = null, string[] attachments = null)
        {
            return connector.Post(PrepareUrl(path), PrepareHeaders(), PrepareParams(parameters), PrepareAttachments(attachments));
        }

        /// <summary>Send a PUT request</summary>
        /// <param name="path">string</param>
        /// <param name="parameters">array|null</param>
        /// <param name="attachments">array|null</param>
        /// <returns>HttpResponseMessage</returns>
        public static HttpResponseMessage Put(string path, Dictionary<string, object> parameters = null, string[] attachments = null)
        {
            return connector.Put(PrepareUrl(path), PrepareHeaders(), PrepareParams(parameters));
        }

        /// <summary>Send a DELETE command</summary>
        /// <param name="path">string</param>
        /// <param name="parameters">array|null</param>
        /// <returns>HttpResponseMessage</returns>
        public static HttpResponseMessage Delete(string path, Dictionary<string, object> parameters = null)
        {
            return connector.Delete(PrepareUrl(path), PrepareHeaders(), PrepareParams(parameters));
        }

        /// <summary>Prepare headers</summary>
        /// <returns>array</returns>
        private static Dictionary<string, string> PrepareHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "X-Angie-AuthApiToken", key } };
            return headers;
        }

        /// <summary>Prepare URL from the given path</summary>
        /// <param name="path">string</param>
        /// <returns>string</returns>
        private static string PrepareUrl(string path)
        {
            if (path.Substring(0, 1) != "/")
            {
                path = "/" + path;
            }

            return url + "/api/v" + apiVersion + path;
        }

        /// <summary>Prepare params</summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>array</returns>
        private static Dictionary<string, object> PrepareParams(Dictionary<string, object> parameters)
        {
            return parameters == null ? new Dictionary<string, object>() : parameters;
        }

        /// <summary>Prepare attachments for request</summary>
        /// <param name="attachments">array|null</param>
        /// <returns>array|null</returns>
        /// <exception cref="FileNotReadable"></exception>
        /// <exception cref="Exceptions\FileNotReadable"></exception>
        private static Dictionary<string, string> PrepareAttachments(string[] attachments = null)
        {
            Dictionary<string, string> fileParams = new Dictionary<string, string>();

            if (attachments != null)
            {
                int counter = 1;

                foreach (string attachment in attachments)
                {
                    string path = attachment;

                    if (IsReadable(path))
                    {
                        fileParams["attachment_" + counter++] = attachment;
                    }
                    else
                    {
                        throw new FileNotReadable(attachment);
                    }
                }
            }

            return fileParams;
        }

        /// <summary>Gets the json.</summary>
        /// <param name="httpResponseMessage">The HTTP response message.</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetJson(HttpResponseMessage httpResponseMessage)
        {
            Dictionary<string, object> json = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(GetBody(httpResponseMessage)) && IsJson(httpResponseMessage))
            {
                json = (Dictionary<string, object>)JsonConvert.DeserializeObject<IDictionary<string, object>>(httpResponseMessage.Content.ReadAsStringAsync().Result, new JsonConverter[] { new NestedDictionaryConverter() });
            }

            return json;
        }

        /// <summary>Determines whether the specified path is readable.</summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        static private bool IsReadable(string path)
        {
            bool result = false;

            WebRequest webRequest = WebRequest.Create(path);

            // Timeout is in milliseconds.
            webRequest.Timeout = 1200;
            webRequest.Method = "HEAD";

            FileWebResponse response = null;

            try
            {
                response = (FileWebResponse)webRequest.GetResponse();
                result = true;
            }
            catch (WebException)
            {
                // Leave result as false.
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified HTTP response message is json.
        /// </summary>
        /// <param name="httpResponseMessage">The HTTP response message.</param>
        /// <returns></returns>
        private static bool IsJson(HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.Content.Headers.ContentType.ToString().Contains("application/json");
        }

        /// <summary>Gets the body.</summary>
        /// <param name="httpResponseMessage">The HTTP response message.</param>
        /// <returns></returns>
        private static string GetBody(HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        #endregion
    }
}
