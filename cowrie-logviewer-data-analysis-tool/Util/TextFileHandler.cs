using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace cowrie_logviewer_data_analysis_tool.Util
{
    public class TextFileHandler
    {
        /// <summary>
        /// Gets a specific number of lines, to get all lines use -1 as the lines parameter
        /// </summary>
        /// <param name="absolutePathToFile"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static string[] ReadLinesOfFile(string absolutePathToFile, int lines) {
            if (lines == -1) return ReadLinesOfFile(absolutePathToFile);
            IEnumerable<string> s = null;
            try
            {
                LoadingMsg($"the first {lines} lines of",absolutePathToFile);
                var file = new StreamReader(absolutePathToFile);
                s = ReadLines(file,lines);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            if (s == null) throw new FileNotFoundException(absolutePathToFile);
            return s.ToArray();
        }

        public static IEnumerable<string> ReadLines(StreamReader file, long lines) {
            string line;
            long count = 0;
            while ((line = file.ReadLine()) != null && lines > count)
            {
                yield return line;
                count++;
            }
            file.Close();
        }

        public static string[] ReadLinesOfFile(string absolutePathToFile)
        {
            try
            {
                LoadingMsg(absolutePathToFile);
                return File.ReadAllLines(absolutePathToFile);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            return null;
        }

        private static void LoadingMsg(string str) {
            Trace.WriteLine("Loading " + str);
        }

        private static void LoadingMsg(string disc, string str)
        {
            Trace.WriteLine($"Loading {disc} " + str);
        }

        public static void WaitForGeneralPrompt(string str)
        {
            Trace.WriteLine(str);
            Trace.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        public static object WaitForInput(string str) {
            Trace.WriteLine(str);
            var s = Console.ReadLine();
            return s;
        }

        public static object WaitForPrompt(string str, Func<object> f, bool overrideable) {
            Trace.WriteLine(str);
            Trace.WriteLine("Do you want to continue? (Y/N)");
            var k = Console.ReadKey(true).Key;
            if (k.Equals(ConsoleKey.Enter) || k.Equals(ConsoleKey.Y))
            {
                Trace.WriteLine("\nContinuing\n");
                return f.Invoke();
            }
            else if (k.Equals(ConsoleKey.N) || k.Equals(ConsoleKey.Escape))
            {
                WaitForGeneralPrompt("\nCancelled\n");
                return null;
            }
            else if (k.Equals(ConsoleKey.O) && overrideable)
            {
                Trace.WriteLine("");
                Trace.WriteLine("Starting override process, please finish previous prompt first (Y/N)");
                var guid = WaitForPrompt(str, f);
                var overridePrompt = WaitForOverridePrompt();
                return new[] { guid, overridePrompt };
            }
            else
            {
                WaitForPrompt(str, f);
                return null;
            }
        }

        public static string GetSrcFolder(string folder)
        {
            return Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName, folder);
        }

        public static string GetSrcPath(string folder, string fileName) {
            return Path.Combine(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName, folder), fileName);
        }

        public static object WaitForPrompt(string str, Func<object> f)
        {
            return WaitForPrompt(str, f, false);
        }

        public static object WaitForOverridePrompt()
        {
            Trace.WriteLine("");
            Trace.WriteLine("Warning! Override individual prompt");
            Trace.WriteLine("All prompts from now on will be accepted");
            Trace.WriteLine("THIS IS NOT RECOMMENDED");
            Trace.WriteLine("All tests and intergration tests should be in order beforehand.");
            Trace.WriteLine("To terminate during the batch process, press Ctrl+C");
            Trace.WriteLine("Are you sure you want to continue? (Y/N)");
            var k = Console.ReadKey(true).Key;
            if (k.Equals(ConsoleKey.Enter) || k.Equals(ConsoleKey.Y))
            {
                Trace.WriteLine("\nWrite \"ACCEPT\" to confirm and proceed\n");
                if (Console.ReadLine() != "ACCEPT")
                {
                    WaitForOverridePrompt();
                    return false;
                }
                else {
                    return true;
                }
            }
            else
            {
                WaitForGeneralPrompt("\nOverride cancelled\n");
                return false;
            }
        }

        public static void WriteToFile(string absOutFilePathWithOutExtensionName, OutFile ft)
        {
            using (StreamWriter file = new StreamWriter(absOutFilePathWithOutExtensionName+"."+ft.Extension()))
            {
                ft.Run(file);
            }
        }

        interface IOutFile
        {
            void WriteLine(StreamWriter sw);
        }

        public abstract class OutFile : IOutFile
        {
            public abstract string Extension();
            public abstract void WriteLine(StreamWriter sw);

            public virtual void Run(StreamWriter file) {
                WriteLine(file);
                Trace.WriteLine($"Written {this.Extension().ToUpper()}-file to {((FileStream)(file.BaseStream)).Name}");
            }
        }

        public class TXT : OutFile
        {
            readonly string[] lines;

            public TXT(string[] lines)
            {
                this.lines = lines;
            }

            public override string Extension()
            {
                return "txt";
            }

            public override void WriteLine(StreamWriter file)
            {
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
            }
        }

        public class CSV : OutFile
        {
            private readonly KeyValuePair<string, string[]>[] keyValues;
            private readonly string delimiter;
            private readonly string[] lines;
            private readonly string[] keys;

            public CSV(/*KeyValuePair<string, string[]>[] keyValues,*/string[] keys, string[] lines, string delimiter)
            {
                this.keys = keys;
                this.lines = lines;
                this.keyValues = keyValues;
                this.delimiter = delimiter;
            }

            public (string[], string[]) ToSeperateLines(IEnumerable<KeyValuePair<string, string[]>> keyValues)
            {
                return (keyValues.Select(kv => kv.Key).ToArray(), ToLines(keyValues.Select(kv => kv.Value).ToArray()));
            }

            public string[] ToLines(string[][] l)
            {
                string[] m = new string[l.Length];
                for (var i = 0; i < l.Length-1; i++)
                {
                    var str = new StringBuilder();
                    for (var j = 0; j < l[i].Length-1; j++)
                    {
                        str.Append(l[j][i]).Append(l[i].Length - 1 == j ? "" : delimiter);
                    }
                    m[i] = str.ToString();
                }
                return m;
            }

            public override void WriteLine(StreamWriter file)
            {

                /*var (keys, lines) = ToSeperateLines(keyValues);
                file.WriteLine(CsvHeader(keys, delimiter));
                */
                file.WriteLine(CsvHeader(keys, delimiter));
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
            }

            public string CsvHeader(string[] keys, string delimiter)
            {
                var csvHeaderString = new StringBuilder();
                var count = 0;
                foreach (var k in keys)
                {
                    csvHeaderString.Append($"{k}{(keys.Length - 1 != count ? delimiter : "")}");
                    count++;
                }
                return csvHeaderString.ToString();
            }

            public override string Extension()
            {
                return "csv";
            }
        }
    }
}

public static class FileHandlerLINQExtensionMethods
{

    public static void WriteLineAndSave(this StringBuilder sb, string line)
    {
        Debug.WriteLine(line);
        sb.AppendLine(line);
    }

    public static void WriteAndSave(this StringBuilder sb, string line)
    {
        Debug.Write(line);
        sb.Append(line);
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
        for (var i = 0; i < (float)array.Length / size; i++)
        {
            yield return array.Skip(i * size).Take(size);
        }
    }

    public static DateTime Trim(this DateTime date, long roundTicks)
    {
        return new DateTime(date.Ticks - date.Ticks % roundTicks, date.Kind);
    }
}