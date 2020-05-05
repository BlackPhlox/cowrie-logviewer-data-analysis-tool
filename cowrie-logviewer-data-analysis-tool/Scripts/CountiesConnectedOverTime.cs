using cowrie_logviewer_data_analysis_tool.Runner;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class CountiesConnectedOverTime : Script, IReadWriteFiles
    {
        public override string ScriptName => "CountiesConnectedOverTime";

        public override string Description => "Generates CSV-file of counties connected to the cowrie server over time";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public new DirectoryInfo InFolder = Directory.CreateDirectory(@"C:\Users\thelu\OneDrive\Skrivebord\honeypotlogs 26_04\cowrie");

        public override void Run()
        {
            ReadFilesWriteToFile(InFolder.FullName, "cowrie.json.*", OutFile);
        }

        public BlockingCollection<string> ReadFilesWriteToFile(string folder, string fileNameWC, string toFile)
        {
            var matchesCollection = new BlockingCollection<string>();

            var files = Directory.GetFiles(folder, fileNameWC,
                                         SearchOption.TopDirectoryOnly);

            Console.WriteLine("Reading Folder: " + folder);

            var readTask = Task.Run(() =>
            {
                using (var writer = new StreamWriter(toFile))
                {
                    //CSV Header
                    writer.WriteLine("date,ip");
                    DateTime? currentHour = null;
                    Dictionary<DateTime, string[]> hours = new Dictionary<DateTime, string[]>();
                    try
                    {
                        foreach (var file in files)
                        {
                            string line2 = "";
                            try
                            {
                                using (var reader = new StreamReader(file))
                                {
                                    string line;

                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        line2 = line;
                                        EventDTO _event = null;

                                        try
                                        {
                                            _event = JsonConvert.DeserializeObject<EventDTO>(line);
                                        }
                                        catch (Exception)
                                        {
                                            continue;
                                        }

                                        Event e = new Event(Lookup(_event.src_ip), _event.src_ip, _event.timestamp);
                                        var wholeHour = e.date.Trim(TimeSpan.TicksPerMillisecond);
                                        wholeHour = wholeHour.Trim(TimeSpan.TicksPerSecond);
                                        wholeHour = wholeHour.Trim(TimeSpan.TicksPerMinute);
                                        wholeHour = wholeHour.Trim(TimeSpan.TicksPerHour);
                                        if (hours.ContainsKey(wholeHour))
                                        {
                                            hours.TryGetValue(wholeHour, out string[] ips);
                                            if (ips.ToList().Contains(e.ipaddress.ToString())) continue;
                                            else
                                            {
                                                var newips = ips.Append(e.ipaddress.ToString());
                                                hours.Remove(wholeHour);
                                                hours.Add(wholeHour, newips.ToArray());
                                            }
                                        }
                                        else
                                        {

                                            if (currentHour != null)
                                            {
                                                var c_hour = currentHour.Value;
                                                hours.TryGetValue(c_hour, out string[] ips);
                                                foreach (var ip in ips)
                                                {
                                                    var t = c_hour;
                                                    if (t.IsDaylightSavingTime())
                                                    {
                                                        if ((t - TimeSpan.FromHours(2)).Hour > t.Date.Hour)
                                                        {
                                                            writer.WriteLine(t.AddHours(2).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "," + ip);
                                                        }
                                                        else
                                                        {
                                                            writer.WriteLine(t.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "," + ip);
                                                        }
                                                    }
                                                    else writer.WriteLine(t.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "," + ip);
                                                }
                                            }

                                            currentHour = wholeHour;
                                            hours.Add(wholeHour, new string[] { e.ipaddress.ToString() });
                                        }
                                    }
                                }
                            }
                            catch (DirectoryNotFoundException e)
                            {
                                Console.WriteLine(e.StackTrace);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(line2);
                                Console.WriteLine(e.StackTrace);
                            }
                        }
                    }

                    finally
                    {
                        matchesCollection.CompleteAdding();
                    }
                }
            });

            Task.WaitAll(readTask);

            return matchesCollection;
        }
    }
}
