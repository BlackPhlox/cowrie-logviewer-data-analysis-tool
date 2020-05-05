using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static cowrie_logviewer_data_analysis_tool.Util.PrintUtil;
using static cowrie_logviewer_data_analysis_tool.Util.TextFileHandler;

namespace cowrie_logviewer_data_analysis_tool.Runner
{
    /// <summary>
    /// The abstract class that handles all logic behind each script so that they are made runnable
    /// </summary>
    public abstract class Script : BaseScript
    {
        public enum ScriptState { Loading, Running, Waiting, Stopped }

        private ScriptState state;
        public ScriptState State { get { return state; } set {
                state = value;
                HandleProgress();
                Trace.WriteLine("");
            }
        }

        public void SetState(string str, ScriptState s) {
            State = s;
            Trace.Write($"{str} ");
        }

        private Thread ProgressThread;

        public Dictionary<string, IPGeoDTO> ipLocations;

        public override void Setup()
        {
            if (InFiles != null) {
                var absFilePath = Path.Combine(InFolder.FullName, InFiles[0]);

                ipLocations = new Dictionary<string, IPGeoDTO>();

                var lines = ReadLinesOfFile(absFilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i == 0) continue; //Skip first csv type line
                    var ipData = IPGeoDTO.ParseFromCSV(lines[i]);
                    ipLocations.Add(ipData.query,ipData);
                }
            }
        }

        public IPGeoDTO Lookup(string ip)
        {
            if (ipLocations == null) return null;
            ipLocations.TryGetValue(ip, out IPGeoDTO info);
            return info;
        }

        /// <summary>
        /// Rendering loading animation
        /// </summary>
        private void HandleProgress() {
            ProgressThread = new Thread(delegate ()
            {
                var i = 0;
                WriteProgress(0);
                while (state == ScriptState.Loading || state == ScriptState.Running)
                {
                    WriteProgress(i, true);
                    Thread.Sleep(50);
                    ++i;
                }
            });
            ProgressThread.Start();
        }

    }
}
