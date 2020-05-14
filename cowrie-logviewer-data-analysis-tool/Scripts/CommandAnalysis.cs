using cowrie_logviewer_data_analysis_tool.Runner;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class CommandAnalysis : BaseScript, IReadWriteFiles
    {
        public override string ScriptName => "Commands";

        public override string Description => "Generates CSV-file of counties connected to the cowrie server over time";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public override void Run()
        {
            ReadFilesWriteToFile(InFolder.FullName, "cowrie.json.*", OutFile);
        }

        public override void Setup() { }

        public BlockingCollection<string> ReadFilesWriteToFile(string folder, string fileNameWC, string toFile)
        {
            var matchesCollection = new BlockingCollection<string>();

            var files = Directory.GetFiles(folder, fileNameWC,
                                         SearchOption.TopDirectoryOnly);

            Console.WriteLine("Reading Folder: " + folder);
            var exceptions = new ValueTuple<string, Regex>[]
                {
                    ("change password", new Regex("CMD: echo \"root")) ,
                    ("change password", new Regex("CMD: echo -e \"")) ,
                    ("change password", new Regex(@"\|passwd")),
                    ("change password", new Regex(" > /tmp/up.txt"))
                };
            var readTask = Task.Run(() =>
            {
                using (var writer = new StreamWriter(toFile))
                {
                    //CSV Header
                    writer.WriteLine("command,count");
                    //List<string> cmds = new List<string>();
                    Dictionary<string, long> cmds = new Dictionary<string, long>();
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
                                        if (_event.eventid != "cowrie.command.input") continue;

                                        if (exceptions.Any(e => e.Item2.IsMatch(_event.message.ToString())))
                                        {
                                            var str = exceptions.Where(e => e.Item2.IsMatch(_event.message.ToString())).FirstOrDefault().Item1;
                                            if (cmds.ContainsKey(str))
                                            {
                                                cmds.TryGetValue(str, out long l);
                                                cmds.Remove(str);
                                                cmds.Add(str, l + 1);
                                            }
                                            else cmds.Add(str, 1);
                                            continue;
                                        }

                                        if (cmds.ContainsKey(_event.message.ToString()))
                                        {
                                            var str = _event.message.ToString();
                                            cmds.TryGetValue(str, out long l);
                                            cmds.Remove(str);
                                            cmds.Add(str, l + 1);
                                        }
                                        else cmds.Add(_event.message.ToString(), 1);
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
                    foreach (var cmd in cmds)
                    {
                        writer.WriteLine(cmd.Key + "," + cmd.Value);
                    }

                }
            });

            Task.WaitAll(readTask);

            return matchesCollection;
        } 
    }
}
