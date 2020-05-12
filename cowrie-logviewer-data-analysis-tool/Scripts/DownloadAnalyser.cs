using cowrie_logviewer_data_analysis_tool.Runner;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cowrie_logviewer_data_analysis_tool.Util;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class DownloadAnalyser : Script
    {
        public override string ScriptName => "Download analyser";

        public override string Description => "Generates CSV-file of counties connected to the cowrie server over time";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        Dictionary<string, DownloadFile> dls = new Dictionary<string, DownloadFile>();

        public override void Run()
        {
            var cowriePath = @"C:\Users\thelu\OneDrive\Skrivebord\honeypotlogs 26_04\cowrie";
            InFolder = Directory.CreateDirectory(cowriePath);
            OutExtension = "json";

            var parent = Directory.GetParent(Directory.GetCurrentDirectory()).CreateSubdirectory("CowrieA");
            var f = parent.CreateSubdirectory($"{ReplaceInvalidChars(ScriptName)}.{DateTime.Now.ToString(ScriptDateTimeFormat)}");
            Trace.WriteLine("Working path: " + f.FullName);
            OutFolder = f;

            var filepath = @"C:\Users\thelu\OneDrive\Skrivebord\honeypotlogs 26_04\all\cowrie.tar";

            //https://stackoverflow.com/questions/42625845/asp-net-read-a-file-from-a-tar-gz-archive
            /*using (Stream source = new GZipInputStream(new FileStream(filepath, FileMode.Open)))  //wc.OpenRead() create one stream with archive tar.gz from our server
                {*/
            using (TarInputStream tarStr = new TarInputStream(new FileStream(filepath, FileMode.Open)))   //TarInputStream is a stream from ICSharpCode.SharpZipLib.Tar library(need install SharpZipLib in nutgets)
            {
                TarEntry te;
                try
                {
                    while ((te = tarStr.GetNextEntry()) != null)  // Go through all files from archive
                    {
                        if (te.Name.Contains("downloads"))
                        {
                            if (te.Size > 1000)
                            {
                                if (!te.Name.Contains("tmp"))
                                {
                                    var sha = te.Name.Replace("cowrie/downloads/", "");
                                    if (dls.Any(d => d.Value.size == te.Size))
                                    {
                                        var oldsha = dls.Where(d => d.Value.size == te.Size).FirstOrDefault();
                                        oldsha.Value.count += 1;
                                        oldsha.Value.ips.Add(new IpDate() { sha2 = sha });

                                        dls.Remove(oldsha.Key);
                                        dls.Add(oldsha.Key, oldsha.Value);
                                        continue;
                                    }
                                    dls.Add(sha, new DownloadFile() { sha2 = sha, size = te.Size, count = 1, ips = new List<IpDate>() { new IpDate() { sha2 = sha } } });
                                }
                            }
                        }
                    }
                }
                catch (TarException tare)
                {
                    Console.WriteLine(tare.StackTrace);
                    string text = File.ReadAllText(filepath);
                    Console.WriteLine(text);

                }
            }
            //}

            int limit = 1000;

            dls.Count();
            var s = dls.ToArray().Split(limit);

            int lc = 0;
            foreach (var item in s)
            {
                using (var writer = new StreamWriter(OutFile.Substring(0,OutFile.Length-5) + "." + (lc++) +".json"))
                {
                    foreach (var item1 in item)
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(item1.Value));
                    }
                }
            }
        }
    }
}
