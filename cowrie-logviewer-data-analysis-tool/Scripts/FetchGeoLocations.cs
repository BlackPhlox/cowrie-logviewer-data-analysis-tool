using cowrie_logviewer_data_analysis_tool.Runner;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static cowrie_logviewer_data_analysis_tool.Util.TextFileHandler;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class FetchGeoLocations : Script
    {
        string geoipAPI_URL = @"http://ip-api.com/batch?fields=17033426";

        private string[] ips;

        private List<string> lookUpips;

        private StreamWriter appendTo;

        private int requestsPerMinute = 10;
        private readonly int maxIpPerRequest = 100;

        private static HttpClient Client = new HttpClient();

        public override string ScriptName => "GeoLocGen";

        public override string Description => "Fetches batches of Geolocations using ip-api.com and generates a csv file";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public override void Run() {
            InternalSetup();
            lookUpips = new List<string>();

            for (int i = 0; i < ips.Length; i++)
            {
                if (Lookup(ips[i]) == null)
                {
                    Console.WriteLine("Look online : " + ips[i]);
                    lookUpips.Add(ips[i]);
                }
                else
                {
                    Console.WriteLine("Already found");
                }
            }

            Console.WriteLine("Do you want to form the request? for " + lookUpips.Count + " ips? (Y/N)" + (lookUpips.Count > maxIpPerRequest ? " (Split into " + (Math.Ceiling(lookUpips.Count * 1.0 / maxIpPerRequest)) + " requests of " + maxIpPerRequest + " at a time)" : ""));
            var k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Y)
            {
                Console.WriteLine("Add header? (Y/N)");
                var ch = Console.ReadKey(true);
                if (ch.Key == ConsoleKey.Y)
                    appendTo.WriteLine("query,countryCode,city,lat,lon,proxy,hosting,mobile,as");

                appendTo.AutoFlush = true;

                var collections = lookUpips.ToArray().Split(maxIpPerRequest);

                var limitCount = 0;

                foreach (var collection in collections)
                {
                    Console.WriteLine(limitCount % requestsPerMinute);
                    Console.WriteLine(requestsPerMinute);
                    if (limitCount != 0 && limitCount % requestsPerMinute == 0)
                    {
                        Console.WriteLine("Limit hit, waiting one minute");
                        Thread.Sleep(1000 * 60 + 1000*5);
                    }
                    string json = JsonConvert.SerializeObject(collection.ToArray(), Formatting.None);
                    Console.WriteLine(json);
                    var geoCollections = fetch(json).Result;
                    limitCount++;
                    Console.WriteLine(geoCollections);
                    foreach (var geoCollection in geoCollections)
                    {
                        appendTo.WriteLine($"{geoCollection.query},{geoCollection.countryCode},{geoCollection.city},{geoCollection.lat},{geoCollection.lon},{geoCollection.proxy},{geoCollection.hosting},{geoCollection.mobile},{geoCollection.@as.Replace(",","_")}");
                    }
                }
            }
            else
            {

            }

            appendTo.Close();
            Console.WriteLine("Complete");
        }

        public void InternalSetup()
        {
            //Path.Combine(InFolder.FullName, InFile);
            var InputFilePath = Path.Combine(InFolder.FullName, InFiles[1]);
            var lines2 = ReadLinesOfFile(InputFilePath);
            ips = new string[lines2.Length];
            for (int i = 0; i < lines2.Length; i++)
            {
                ips[i] = lines2[i].Trim();
            }

            appendTo = File.AppendText(OutFile);
        }

        public async Task<IPGeoDTO[]> fetch(string json)
        {
            var response = await Client.PostAsync(
                    /*@"http://www.mocky.io/v2/5e9ddcfc340000d5856eea30"*/ geoipAPI_URL,
                     new StringContent(json, Encoding.UTF8, "application/json"));
            var res_out = await response.Content.ReadAsStringAsync();
            IPGeoDTO[] ipgeo = JsonConvert.DeserializeObject<IPGeoDTO[]>(res_out);
            return ipgeo;
        }
    }
}
