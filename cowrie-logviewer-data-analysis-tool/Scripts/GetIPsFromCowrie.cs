using cowrie_logviewer_data_analysis_tool.Runner;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class GetIPsFromCowrie : BaseScript, IReadWriteFiles
    {
        public override string ScriptName => "GetIPsFromCowrie";

        public override string Description => "Generates TXT-file of ips that connected to cowrie";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public override void Run()
        {
            ReadFilesWriteToFile(InFolder.FullName, "cowrie.json.*", OutFile);
        }
        public override void Setup()
        {
            
        }

        public BlockingCollection<string> ReadFilesWriteToFile(string folder, string fileNameWC, string toFile)
        {
            var matchesCollection = new BlockingCollection<string>();

            var files = Directory.GetFiles(folder, fileNameWC,
                                         SearchOption.TopDirectoryOnly);

            Console.WriteLine("Reading Folder: " + folder);
            var ipset = new HashSet<string>();
            var readTask = Task.Run(() =>
            {
                using (var writer = new StreamWriter(toFile))
                {
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
                                        
                                        if(ipset.Add(_event.src_ip)) writer.WriteLine(_event.src_ip);
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
