using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ActiveCollabSDK.SDK;

namespace Example
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        /// <summary>Runs the program with the specified arguments</summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            Client.url = "https://app.activecollab.com/" + ConfigurationManager.AppSettings["CloudInstanceID"];
            Client.key = ConfigurationManager.AppSettings["ApiKey"];

            var users = Client.GetJson(Client.Get("users"));
            foreach (KeyValuePair<string, object> user in users)
            {
                var userProperties = (Dictionary<string, object>) user.Value;
                Console.WriteLine(userProperties["id"].ToString() + ":\t" + userProperties["email"]);
            }
            Console.ReadLine();



            Console.WriteLine("API info:\n");
            Console.WriteLine(DumpNestedDictionaries(Client.info()));
            Console.ReadLine();

            Console.WriteLine("Get project and save to variable:\n");
            var project = Client.GetJson(Client.Get("projects"))["1"]; // Get first project returned.
            //var project = Client.GetJson(Client.Get("projects/1"))["single"]; // Get project with ID 1.
            Console.WriteLine(DumpNestedDictionaries((Dictionary<string, object>)project));
            Console.ReadLine();

            Console.WriteLine("Create task with attachments and save to variable:\n");
            var projectId = ((Dictionary<string, object>)project)["id"];
            var newTask = Client.GetJson(Client.Post("projects/" + projectId + "/tasks", new Dictionary<string, object>()
                {
                    { "name", "Learn how to use ActiveCollab SDK" },
                    { "assignee_id", 1 }
                }, new string[]
                {
                    @"C:\Users\johndoe\Documents\example.txt",
                    @"C:\Users\johndoe\Documents\example.log"
                }));
            Console.WriteLine(DumpNestedDictionaries(newTask));
            Console.ReadLine();

            Console.WriteLine("Add labels to task:\n");
            var newTaskId = ((Dictionary<string, object>)newTask["single"])["id"];
            newTask = Client.GetJson(Client.Put("projects/" + projectId + "/tasks/" + newTaskId, new Dictionary<string, object>()
                {
                    { "labels", new string[] { "ENHANCEMENT", "TEST" } }
                }));
            Console.WriteLine(DumpNestedDictionaries(newTask));
            Console.ReadLine();

            Console.WriteLine("Complete task:\n");
            newTask = Client.GetJson(Client.Put("complete/task/" + newTaskId));
            Console.WriteLine(DumpNestedDictionaries(newTask));
            Console.ReadLine();

            Console.WriteLine("Delete task:\n");
            newTask = Client.GetJson(Client.Delete("projects/" + projectId + "/tasks/" + newTaskId));
            Console.WriteLine(DumpNestedDictionaries(newTask));
            Console.ReadLine();
        }

        /// <summary>Dumps the nested dictionaries.</summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="tabs">The extra tabs to add to the formatted output.</param>
        /// <returns></returns>
        private static string DumpNestedDictionaries(Dictionary<string, object> dictionary, string tabs = "")
        {
            return "{\n\t" + tabs + string.Join(
                    ",\n\t" + tabs,
                    dictionary.Select(
                        kv => kv.Key.ToString() + " = " + (
                            (kv.Value == null)
                                ? "null"
                                : (kv.Value.GetType() == typeof(Dictionary<string, object>))
                                    ? DumpNestedDictionaries((Dictionary<string, object>)kv.Value, tabs + "\t")
                                    : kv.Value
                        )
                    ).ToArray()
                ) + "\n" + tabs + "}";
        }
    }
}
