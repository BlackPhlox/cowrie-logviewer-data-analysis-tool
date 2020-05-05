using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace cowrie_logviewer_data_analysis_tool.Runner
{
    abstract class BatchScript : BaseScript
    {
        public abstract Type[] ScriptTypes { get; }

        protected BaseScript[] Scripts { get; set; }

        public override void Setup()
        {
            SetupTrace();
            PromptInterfaces();

            var typeList = ScriptTypes;
            if (typeList.Length > 0 && typeList.All(t => t.IsSubclassOf(typeof(BaseScript)) && t != typeof(BatchScript) && !t.IsSubclassOf(typeof(BatchScript))))
            {
                var allClasses = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.IsClass && a.Namespace != null && a.Namespace.Contains(@"Scripts")).ToList();

                Scripts = new BaseScript[ScriptTypes.Length];
                foreach (var t in allClasses)
                {
                    if (ScriptTypes.Contains(t) && t.IsSubclassOf(typeof(BaseScript)) && !t.IsSubclassOf(typeof(BatchScript)))
                    {
                        Scripts[ScriptTypes.ToList().IndexOf(t)] = (BaseScript)Activator.CreateInstance(t);
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("No valid types specified");
            }
        }

        protected void SetScriptInFilename(Type type, string str_in)
        {
            Scripts.Where(s => s.GetType().Equals(type)).ToList().ForEach(s =>
            {
                if (s.InFiles == null)
                {
                    s.InFiles = new[] { str_in }.ToList();
                }
                else
                {
                    s.InFiles.Add(str_in);
                }
            });
        }

        protected void SetScriptInFilename(BaseScript bs, string str_in)
        {
            bs.InFiles.Add(str_in);
        }

        protected void SetScriptOutExtension(Type type, string str_ext_in)
        {
            Scripts.Where(s => s.GetType().Equals(type)).ToList().ForEach(s => s.OutExtension = str_ext_in);
        }

        protected void SetScriptInFolder(Type type, string str_folder_in)
        {
            Scripts.Where(s => s.GetType().Equals(type)).ToList().ForEach(s => s.InFolder = Directory.CreateDirectory(str_folder_in));
        }

        protected List<BaseScript> GetScripts(Type type, string v)
        {
            return Scripts.Where(s => s.GetType().Equals(type)).ToList();
        }
    }
}
