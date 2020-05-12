using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    internal class DownloadFile
    {
        public string sha2 { get; set; }
        public List<IpDate> ips { get; set; }
        public long size { get; set; }
        public MalwareDTO malwareDTO { get; set; }
        public long count { get; set; }
    }
    class IpDate {
        public string ip { get; set; }
        public DateTime date { get; set; }
        public string sha2 { get; set; }
    }
}