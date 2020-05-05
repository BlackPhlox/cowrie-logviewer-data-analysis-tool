using System;

namespace cowrie_logviewer_data_analysis_tool.Util
{
    internal class CovidDTO
    {
        public string iso_code { get; set; }
        public DateTime date { get; set; }
        public long total_cases { get; set; }
    }
}