using System.Collections.Concurrent;

namespace cowrie_logviewer_data_analysis_tool.Runner
{
    interface IReadWriteFiles
    {
        BlockingCollection<string> ReadFilesWriteToFile(string folder, string fileNameWC, string toFile);
    }
}
