using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ActiveCollabSDK.SDK.Exceptions;
using Newtonsoft.Json;

namespace ActiveCollabSDK.SDK
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Connector
    {
        /// <summary>GET data</summary>
        /// <param name="url">string</param>
        /// <param name="headers">array|null</param>
        /// <returns>HttpResponseMessage</returns>
        public HttpResponseMessage Get(string url, Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };

            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return Execute(request);
        }

        /// <summary>POST data</summary>
        /// <param name="url">string</param>
        /// <param name="headers">array|null</param>
        /// <param name="postData">The post data.</param>
        /// <param name="files">array</param>
        /// <returns>HttpResponseMessage</returns>
        public HttpResponseMessage Post(string url, Dictionary<string, string> headers, 
                Dictionary<string, object> postData = null, Dictionary<string, string> files = null)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post
            };

            if (files != null && files.Count > 0)
            {
                var counter = 1;
                var fileIDs = new List<string>();

                foreach (string file in files.Values)
                {
                    var fileRequest = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(Client.url + "/api/v" + Client.apiVersion + "/upload-files"),
                        Method = HttpMethod.Post
                    };

                    var fileContent = new MultipartFormDataContent();

                    fileRequest.Content = fileContent;

                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        fileRequest.Headers.Add(header.Key, header.Value);
                    }

                    var fileResponse = Execute(fileRequest, "attachment_" + counter++, file);
                    fileIDs.Add(
                            (
                                (Dictionary<string, object>)
                                (
                                    JsonConvert.DeserializeObject<IDictionary<string, object>>
                                    (
                                        fileResponse.Content.ReadAsStringAsync().Result, new JsonConverter[]
                                        {
                                            new NestedDictionaryConverter()
                                        }
                                    )
                                )["1"]
                            )["code"].ToString());
                }

                postData.Add("attach_uploaded_files", fileIDs.ToArray());
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(postData));

            if (request.Content.Headers.Contains("Content-type"))
            {
                request.Content.Headers.Remove("Content-type");
            }

            request.Content.Headers.Add("Content-type", "application/json");

            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return Execute(request);
        }

        /// <summary>Send a PUT request</summary>
        /// <param name="url">string</param>
        /// <param name="headers">array|null</param>
        /// <param name="putData">The put data.</param>
        /// <returns>HttpResponseMessage</returns>
        public HttpResponseMessage Put(string url, Dictionary<string, string> headers = null,
                Dictionary<string, object> putData = null)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Put
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(putData));

            if (request.Content.Headers.Contains("Content-type"))
            {
                request.Content.Headers.Remove("Content-type");
            }

            request.Content.Headers.Add("Content-type", "application/json");

            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return Execute(request);
        }

        /// <summary>Send a DELETE request</summary>
        /// <param name="url">string</param>
        /// <param name="headers">array|null</param>
        /// <param name="deleteData">array</param>
        /// <returns>HttpResponseMessage</returns>
        public HttpResponseMessage Delete(string url, Dictionary<string, string> headers = null,
                Dictionary<string, object> deleteData = null)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Delete
            };

            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(deleteData));

            if (!request.Content.Headers.Contains("Content-type"))
            {
                request.Content.Headers.Add("Content-type", "application/json");
            }

            return Execute(request);
        }

        /// <summary>Do the call</summary>
        /// <param name="http">resource</param>
        /// <param name="attachmentName">Name of the attachment.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>string</returns>
        /// <exception cref="CallFailed"></exception>
        /// <exception cref="AppException"></exception>
        private HttpResponseMessage Execute(HttpRequestMessage http, string attachmentName = null, string filePath = null)
        {
            FileStream fileStream = null;

            if (attachmentName != null && filePath != null)
            {
                var mimeType = "application/octet-stream";
                var boundary = "----ActiveCollabSDK" + GenerateAlphaNumericString(16);

                fileStream = new FileStream(filePath, FileMode.Open);
                var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    // Extra quotes are required.
                    Name = "\"" + attachmentName + "\"",
                    FileName = "\"" + filePath + "\""
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                http.Content = new MultipartFormDataContent(boundary);

                if (http.Content.Headers.Contains("Content-type"))
                {
                    http.Content.Headers.Remove("Content-type");
                }

                http.Content.Headers.Add("Content-type", "multipart/form-data; boundary=" + boundary);
                ((MultipartFormDataContent)http.Content).Add(fileContent);
            }

            using (var client = new HttpClient())
            {
                var stopwatch = Stopwatch.StartNew();

                var task = client.SendAsync(http)
                    .ContinueWith((taskWithMessage) =>
                    {
                        if (fileStream != null)
                        {
                            fileStream.Close();
                        }
                        return taskWithMessage.Result;
                    });
                task.Wait();

                stopwatch.Stop();

                var result = task.Result;

                if (result.IsSuccessStatusCode && IsJson(result))
                {
                    return result;
                }
                else
                {
                    throw new CallFailed(task.Result, stopwatch.ElapsedMilliseconds / 1000);
                }
            }
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

        /// <summary>Generates the alpha numeric string.</summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        private string GenerateAlphaNumericString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
        }
    }
}
