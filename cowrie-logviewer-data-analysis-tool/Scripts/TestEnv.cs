using cowrie_logviewer_data_analysis_tool.Runner;
using System;
using System.Net.Http;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class TestEnv : Script
    {

        private string[] ips;

        private static HttpClient Client = new HttpClient();

        public override string ScriptName => "TestEnv";

        public override string Description => "";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public override void Run() {
            var ds = new[]{ DateTime.Parse("2020-03-28T23:00:00"),
                            DateTime.Parse("2020-03-29T00:00:00"),
                            DateTime.Parse("2020-03-29T01:00:00"),
                            DateTime.Parse("2020-03-29T02:00:00"),
                            DateTime.Parse("2020-03-29T03:00:00")
            };
            /*
             Expected:
             2020-03-28T23:00:00
             2020-03-29T00:00:00
             2020-03-29T01:00:00
             2020-03-29T03:00:00 <- Local timezone summer time shift
             2020-03-29T04:00:00

             Then to UTC:
             2020-03-28T22:00:00
             2020-03-28T23:00:00
             2020-03-29T00:00:00
             2020-03-29T01:00:00
             2020-03-29T03:00:00
                
             */
            foreach (var d in ds) { Console.WriteLine(d.ToUniversalTime()); }

            foreach (var d in ds)
            {
                var t = d;
                if (t.IsDaylightSavingTime())
                {
                    if ((t - TimeSpan.FromHours(2)).Hour > t.Date.Hour)
                    {
                        Console.WriteLine(t.AddHours(2).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"));
                    }
                    else
                    {
                        Console.WriteLine(t.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"));
                    }
                }
                else Console.WriteLine(t.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"));
            }
        }
    }
}
