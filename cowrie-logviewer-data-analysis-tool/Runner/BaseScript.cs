using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using static cowrie_logviewer_data_analysis_tool.Util.PrintUtil;
using static cowrie_logviewer_data_analysis_tool.Util.TextFileHandler;

namespace cowrie_logviewer_data_analysis_tool.Runner
{
    public abstract class BaseScript : IScript
    {
        protected const string ScriptDateTimeFormat = "yyyy-MM-ddTHH.mm";

        public bool IsDebug { get; set; }

        public abstract string ScriptName { get; }

        public abstract string Description { get; }

        public abstract string By { get; }

        protected virtual string TracePath => Path.GetTempPath();

        public abstract void Run();

        protected virtual string FileString => $"ScriptLog-{ReplaceInvalidChars(ScriptName)} {DateTime.Now.ToString(ScriptDateTimeFormat)}.log";

        public virtual DirectoryInfo OutFolder { get; internal set; }

        public virtual DirectoryInfo InFolder { get; internal set; }

        public virtual List<string> InFiles { get; internal set; }

        public virtual string OutExtension { get; internal set; }

        public virtual string OutFile => Path.Combine(OutFolder.FullName, $"OUT.{ReplaceInvalidChars(ScriptName)}{DateTime.Now.ToString(ScriptDateTimeFormat)}.{OutExtension}");

        public DateTime LastModified()
        {
            var absFilePath = GetSrcPath("Scripts", $"{GetType().Name}.cs");
            return File.GetLastWriteTime(absFilePath);
        }

        public string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        protected void SetupTrace()
        {
            Trace.Listeners.Clear();

            TextWriterTraceListener twtl = new TextWriterTraceListener(Path.Combine(TracePath, FileString));
            twtl.Name = "TextLogger";
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }

        /// <summary>
        /// Scripts CLI header, not used in batch scripts due to redundant tracing
        /// </summary>
        public void Start()
        {
            IsDebug = false;
            PrintStart();
            Console.WriteLine($"Trace Enabled - Can be found at {TracePath}{FileString}");
            Console.WriteLine();
            PrintFillLine('-');
            SetupTrace();

            PrintInfoHeaderTrace();

            PromptInterfaces();

            Setup();

            Thread thread = new Thread(delegate ()
            {
                InFiles = new List<string>();
                Run();
                Console.WriteLine($"Trace-log saved at {TracePath}{FileString}");
                Console.WriteLine("Script complete, press any key to exit...");
                Console.ReadKey(true);
            });
            thread.Start();
        }

        public abstract void Setup();

        protected void PromptInterfaces()
        {
            foreach (var t in GetType().FindInterfaces(new TypeFilter(IsIPromptFilter), null))
            {
                var args = t.GenericTypeArguments;
                foreach (var arg in args)
                {
                    var varg = Activator.CreateInstance(arg);
                    var methods = t.GetMethods();
                    var msgMethod = methods.Where(m => m.Name == "PromptInputMessage").First();
                    var setValMethod = methods.Where(m => m.Name == "SetValue").First();
                    var msg = (string)msgMethod.Invoke(this, new[] { varg });
                    PromptInput(msg, varg, setValMethod);
                }
            }
        }

        private void PromptInput(string msg, object varg, MethodInfo method)
        {
            var str = (string)WaitForInput(msg);
            var success = (bool)method.Invoke(this, new[] { varg, str });
            if (!success)
            {
                Trace.WriteLine("Had an exception when running input, try again");
                PromptInput(msg, varg, method);
            }
        }

        public static bool IsIPromptFilter(Type typeObj, object criteriaObj)
        {
            if (typeObj.IsInterface && typeObj.Name.StartsWith("IPrompt") && typeObj.GetGenericArguments().Length == 1)
                return true;
            else
                return false;
        }

        private void PromptEnv()
        {
            Console.WriteLine();
            PrintFillMidLine("Press S or Enter key to start the script or ", ' ');
            PrintFillMidLine("Press B to toggle debug mode", ' ');
            PrintFillLine('-');

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.B)
            {
                IsDebug = !IsDebug;
                Console.Clear();
                PrintStart();
            }
            else if (key == ConsoleKey.Enter || key == ConsoleKey.S)
            {
                Console.WriteLine("\nStarting script\n");
            }
            else
            {
                Console.Clear();
                PrintStart();
            }
        }

        /// <summary>
        /// Display information about the script without being traced
        /// </summary>
        private void PrintStart()
        {
            PrintFillMidLine("ScriptRunner", '-');
            Console.WriteLine("Script name: " + ScriptName);
            Console.WriteLine("Script-filename: " + GetType().Name);
            Console.WriteLine("Last modified: " + LastModified().ToString(ScriptDateTimeFormat));
            Console.WriteLine("Discription: \n" + Description);
            Console.WriteLine("Made by: " + By);
            Console.WriteLine("Debug mode: " + (IsDebug ? "Yes" : "No"));
            PrintFillLine('-');
            PromptEnv();
        }

        /// <summary>
        /// Display information that will be in the header of the trace
        /// </summary>
        private void PrintInfoHeaderTrace()
        {
            Trace.WriteLine("ScriptRunner Trace");
            Trace.WriteLine("Windows ID: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            Trace.WriteLine("Machine: " + System.Environment.MachineName);
            Trace.WriteLine("User: " + System.Environment.UserName);
            Trace.WriteLine("Script name: " + ScriptName);
            Trace.WriteLine("Script-filename: " + GetType().Name);
            Trace.WriteLine("Last modified: " + LastModified().ToString(ScriptDateTimeFormat));
            Trace.WriteLine("Current date: " + DateTime.Now.ToString(ScriptDateTimeFormat));
            Trace.WriteLine("Discription: \n" + Description);
            Trace.WriteLine("Made by: " + By);
            Trace.WriteLine("Debug mode: " + (IsDebug ? "Yes" : "No"));
            Trace.WriteLine(new string('-', Console.WindowWidth));
        }
    }
}