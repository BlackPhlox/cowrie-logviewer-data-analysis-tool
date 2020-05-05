using cowrie_logviewer_data_analysis_tool.Runner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class Covid19CowrieAnalysis : BatchScript
    {
        public override string ScriptName => "Covid19Analysis";

        public override string Description => "Batch script that runs all analysis proccessies";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public override Type[] ScriptTypes => new[] { typeof(GetIPsFromCowrie), typeof(FetchGeoLocations), typeof(FormatCovid19Data), typeof(CommandAnalysis), typeof(CountiesConnectedOverTime) };

        public override void Run()
        {
            var parent = Directory.GetParent(Directory.GetCurrentDirectory()).CreateSubdirectory("CowrieA");
            var f = parent.CreateSubdirectory($"{ReplaceInvalidChars(ScriptName)}.{DateTime.Now.ToString(ScriptDateTimeFormat)}");
            Trace.WriteLine("Working path: " + f.FullName);

            var cowriePath = @"C:\Users\thelu\OneDrive\Skrivebord\honeypotlogs 26_04\cowrie";

            SetScriptOutExtension(typeof(GetIPsFromCowrie), "txt");
            SetScriptInFolder(typeof(GetIPsFromCowrie), cowriePath);

            SetScriptOutExtension(typeof(FormatCovid19Data), "csv");

            SetScriptOutExtension(typeof(CommandAnalysis), "csv");
            SetScriptInFolder(typeof(CommandAnalysis), cowriePath);

            SetScriptOutExtension(typeof(CountiesConnectedOverTime), "csv");
            SetScriptInFolder(typeof(CountiesConnectedOverTime), cowriePath);

            Dictionary<Type, string> io = new Dictionary<Type, string>();

            foreach (var script in Scripts)
            {
                script.OutFolder = f;
                if (script.InFolder == null) script.InFolder = f;
                Trace.WriteLine("Setting up script: " + script.ScriptName);
                script.Setup();
                io.Add(script.GetType(), script.OutFile);
            }

            //Existing ip map
            SetScriptInFilename(typeof(FetchGeoLocations), "");

            io.TryGetValue(typeof(GetIPsFromCowrie), out string rawIps);
            SetScriptInFilename(typeof(FetchGeoLocations), rawIps);

            foreach (var script in Scripts)
            {
                Trace.WriteLine("Running script: " + script.ScriptName);
                script.Run();
            }
            Trace.WriteLine("Batch script complete, press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
