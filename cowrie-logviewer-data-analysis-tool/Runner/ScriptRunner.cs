using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static cowrie_logviewer_data_analysis_tool.Util.PrintUtil;

namespace cowrie_logviewer_data_analysis_tool.Runner
{
    public class ScriptRunner
    {
        static List<BaseScript> Scripts;
        static int SelectorIndex = 0;
        static bool initial = true;
        public static void Main(string[] args)
        {
            //Get all classes in the Scripts namespace and filter only them that inherets Script.cs and initialize them
            var allClasses = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.IsClass && a.Namespace != null && a.Namespace.Contains(@"Scripts")).ToList();
            Scripts = allClasses.Where(c => c.IsSubclassOf(typeof(BaseScript))).Select(ts => (BaseScript)Activator.CreateInstance(ts)).ToList();
            Prompt();
        }

        /// <summary>
        /// Display interface for selecting avaliable scripts
        /// </summary>
        private static void Prompt() {
            int counter = 0;
            PrintFillMidLine("ScriptRunner", '-');
            Console.WriteLine("Scripts:");
            foreach (var s in Scripts)
            {
                Console.WriteLine((counter == SelectorIndex?">":"") + " " + s.ScriptName + (s.GetType().IsSubclassOf(typeof(BatchScript))?" (Batch)":""));
                counter++;
            }
            PrintFillLine('-');
            Console.Write("Description:\n");
            Console.WriteLine(Scripts[SelectorIndex].Description);

            Console.Write("Last modified: ");
            Console.WriteLine(Scripts[SelectorIndex].LastModified());

            Console.Write("By: ");
            Console.WriteLine(Scripts[SelectorIndex].By);

            PrintFillLine('-');

            if (initial)
            {
                Console.WriteLine("Select a script to start (Use Up/Down-arrows & press S or Enter to select)");
                initial = false;
            }

            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.UpArrow)
            {
                if (SelectorIndex - 1 < 0) SelectorIndex = Scripts.ToArray().Length - 1;
                else SelectorIndex--;
                Console.Clear();
                Prompt();
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (SelectorIndex + 1 > Scripts.ToArray().Length - 1) SelectorIndex = 0;
                else SelectorIndex++;
                Console.Clear();
                Prompt();
            }
            else if (key == ConsoleKey.S || key == ConsoleKey.Enter)
            {
                var script = Scripts[SelectorIndex];
                Console.Clear();
                script.Start();
            }
            else
            {
                Console.Clear();
                Prompt();
            }
        }
    }
}
