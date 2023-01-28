using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions;
using Couchbase.Management.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.Setup
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("This will create all the couchbase buckets and documents required to initially setup the project\n\n\n");
            try
            {
                await Create_LogIn_Config_BucketAsync();
                Console.WriteLine("IG_Config_LogIn Created");
                await LoadDatainBucketsAsync();
                Console.WriteLine("IG_Config_LogIn loaded with Currency data.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Some error occurred {ex.Message}");
            }

        }

        public static async Task<string> Create_LogIn_Config_BucketAsync()
        {
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{Constants.Couchbase.url}pools/default/buckets");

            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{Constants.Couchbase.username}:{Constants.Couchbase.password}")));

            request.Content = new StringContent("name=IG_Config_LogIn&bucketType=" + Constants.Couchbase.bucketType + "&ramQuota=" + Constants.Couchbase.ramQuota);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        public static async Task<bool> LoadDatainBucketsAsync()
        {
            await ImportJSON("currency.json");
            return true;
        }
        public static async Task ImportJSON(string filename)
        {
             //path D:\Documents\.NET Projects\Invoice.Generator\Couchbase.Setup\currency.json
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            path = path.Replace("bin",filename);

            var cluster = await Cluster.ConnectAsync($"couchbase://localhost", $"{Constants.Couchbase.username}", $"{Constants.Couchbase.password}");

            using (var reader = new StreamReader(path))
            {
                var jsonReader = new JsonTextReader(reader);
                JArray arr = (JArray)JToken.ReadFrom(jsonReader);

                
                foreach (JObject record in arr)
                {
                    await UpsertDocument(record,cluster);
                }

                Console.WriteLine();
            }


            //create primary index
            try
            {
                await cluster.QueryIndexes.CreatePrimaryIndexAsync(
                    "IG_Config_LogIn", options => options.IndexName("IG_Config_LogIn_primary_index").IgnoreIfExists(true)  //Deferred(bool deferred) in future
                );
            }
            catch (InternalServerFailureException)
            {
                Console.WriteLine("Index already exists");
            }
        }

        // Newtonsoft.Json.Linq emits `JObjects`
        public static async Task UpsertDocument(JObject record, ICluster cluster)
        {
            var bucket = await cluster.BucketAsync("IG_Config_LogIn");
            var scope = await bucket.ScopeAsync("_default");
            var _collection = await scope.CollectionAsync("_default");
            // define the key
            string key = ""+ Guid.NewGuid();

            // do any additional processing
            //record["importer"] = ".NET SDK";

            // upsert the document
            await _collection.UpsertAsync(key, record);

            // any required logging
            Console.WriteLine(record["currency"]["name"]+" with ID "+key+" added.");
        }
    }
}
