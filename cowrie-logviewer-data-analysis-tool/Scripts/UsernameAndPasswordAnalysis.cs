using cowrie_logviewer_data_analysis_tool.Runner;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class UsernameAndPasswordAnalysis : BaseScript, IReadWriteFiles
    {
        public override string ScriptName => "Username & Password Analysis";

        public override string Description => "Generates CSV-file of passwords and usernames used";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public override void Run()
        {
            ReadFilesWriteToFile(InFolder.FullName, "cowrie.json.*", OutFile);
        }

        public override void Setup() {
            OutExtension = "json";

            var cowriePath = @"C:\Users\thelu\OneDrive\Skrivebord\honeypotlogs 26_04\cowrie";
            InFolder = Directory.CreateDirectory(cowriePath); 

            var parent = Directory.GetParent(Directory.GetCurrentDirectory()).CreateSubdirectory("CowrieA");
            var f = parent.CreateSubdirectory($"{ReplaceInvalidChars(ScriptName)}.{DateTime.Now.ToString(ScriptDateTimeFormat)}");
            Trace.WriteLine("Working path: " + f.FullName);
            OutFolder = f;
        }

        public BlockingCollection<string> ReadFilesWriteToFile(string folder, string fileNameWC, string toFile)
        {
            var matchesCollection = new BlockingCollection<string>();

            var files = Directory.GetFiles(folder, fileNameWC,
                                         SearchOption.TopDirectoryOnly);

            Console.WriteLine("Reading Folder: " + folder);
            var exceptions = new ValueTuple<string, Regex>[]
                {
                    ("empty-string", new Regex(""))
                };
            var readTask = Task.Run(() =>
            {
                    //List<string> cmds = new List<string>();
                    Dictionary<string, EventDTO> events = new Dictionary<string, EventDTO>();
                    //Dictionary<string, IpUsernamePassword> ipup = new Dictionary<string, IpUsernamePassword>();
                    Dictionary<string, long> success = new Dictionary<string, long>();
                    Dictionary<string, long> failed = new Dictionary<string, long>();
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

                                        if (_event.eventid == "cowrie.login.success")
                                        {
                                            var key = _event.username.ToString() + "," + _event.password.ToString();
                                            if (success.ContainsKey(key))
                                            {
                                                success.TryGetValue(key, out long l);
                                                success.Remove(key);
                                                success.Add(key, l + 1);
                                            }
                                            else success.Add(key, 1);
                                        }

                                        if (_event.eventid == "cowrie.login.failed")
                                        {
                                            var key = _event.username.ToString() + "," + _event.password.ToString();
                                            if (failed.ContainsKey(key))
                                            {
                                                failed.TryGetValue(key, out long l);
                                                failed.Remove(key);
                                                failed.Add(key, l + 1);
                                            }
                                            else failed.Add(key, 1);
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
                            /*
                            foreach (var item in events)
                            {
                                if (ipup.ContainsKey(item.Value.src_ip)) {
                                    ipup.TryGetValue(item.Value.src_ip, out IpUsernamePassword value);
                                    if (item.Value.eventid.Equals("cowrie.login.failed")) {
                                        if (value.Failed == null)
                                        {
                                            value.Failed = new HashSet<UnamePass>() { new UnamePass() { uname = item.Value.username, pass = item.Value.password, count = 1 } };
                                        }
                                        else {
                                            value.Failed.
                                        }
                                    }
                                } else {
                                    AddNewToipup(ipup, item, item.Value.eventid.Equals("cowrie.login.failed"));
                                }
                            }
                            */
                        }
                    }

                    finally
                    {
                        matchesCollection.CompleteAdding();

                        using (var writerSuccess = new StreamWriter(OutFile.Substring(0, OutFile.Length - 5) + ".Success" + ".csv"))
                        {
                            writerSuccess.WriteLine("username,password,count");
                            foreach (var cmd in success)
                            {
                                writerSuccess.WriteLine(cmd.Key + "," + cmd.Value);
                            }
                        }

                        using (var writerFailed = new StreamWriter(OutFile.Substring(0, OutFile.Length - 5) + ".Failed" + ".csv"))
                        {
                            writerFailed.WriteLine("username,password,count");
                            foreach (var cmd in failed)
                            {
                                writerFailed.WriteLine(cmd.Key + "," + cmd.Value);
                            }
                        }
                    }
            });

            Task.WaitAll(readTask);

            return matchesCollection;
        }
        /*
        public void AddNewToipup(Dictionary<string, IpUsernamePassword> ipup, KeyValuePair<string, EventDTO> item, bool failed)
        {
            ipup.Add(item.Value.src_ip,
                new IpUsernamePassword()
                {
                    ip = item.Value.src_ip,
                    Failed = failed?null: new HashSet<UnamePass>()
                        { new UnamePass() { uname = item.Value.username, pass = item.Value.password,count = 1 } },
                    Success = failed? new HashSet<UnamePass>()
                        { new UnamePass() { uname = item.Value.username, pass = item.Value.password,count = 1 } }: null
                }
            );
        }
        */
    }
}
