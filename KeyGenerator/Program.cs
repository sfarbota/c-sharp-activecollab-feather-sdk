using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiveCollabSDK.SDK;

namespace ActiveCollabSDK.KeyGenerator
{
    class Program
    {
        /// <summary>Runs the program with the specified arguments</summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            var activeCollabSDKConnector = new Connector();
            var headers = new Dictionary<string, string>(); // Keep empty
            var userEmail = ConfigurationManager.AppSettings["UserEmail"];
            var userPassword = ConfigurationManager.AppSettings["UserPassword"];
            var cloudInstanceID = ConfigurationManager.AppSettings["CloudInstanceID"];
            var clientName = ConfigurationManager.AppSettings["ClientName"];
            var clientVendor = ConfigurationManager.AppSettings["ClientVendor"];

            var intentRequest = new Dictionary<string, object>()
                {
                    { "email", userEmail },
                    { "password", userPassword }
                };

            var intentResponse = Client.GetJson(activeCollabSDKConnector.Post(
                        "https://my.activecollab.com/api/v1/external/login",
                        headers, intentRequest
                ));

            var intent = ((Dictionary<string, Object>)intentResponse["user"])["intent"];

            var keyRequest = new Dictionary<string, object>()
                {
                    { "intent", intent },
                    { "client_name", clientName },
                    { "client_vendor", clientVendor }
                };

            var keyResponse = Client.GetJson(activeCollabSDKConnector.Post(
                    "https://app.activecollab.com/" + cloudInstanceID + "/api/v1/issue-token-intent",
                     headers, keyRequest
                ));

            var key = keyResponse["token"];

            Console.WriteLine("Your new Active Collab API key: " + key);
            Console.ReadLine();
        }
    }
}
