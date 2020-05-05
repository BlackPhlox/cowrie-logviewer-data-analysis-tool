using CsvHelper.Configuration.Attributes;
using System.Globalization;
using System.Linq;

namespace cowrie_logviewer_data_analysis_tool
{
    class CountyCodeDTO
    {
        [Index(1)]
        public string alpha_2 { get; set; }
        [Index(2)]
        public string alpha_3 { get; set; }

        [Index(3)]
        public string country_code { get; set; }
    }

    class CountyCode
    {
        public string alpha_2 { get; set; }
        public string alpha_3 { get; set; }

        public string country_code { get; set; }
        public RegionInfo regionInfo { get; set; }
        public static CountyCode Parse(CountyCodeDTO dto) {
            var cc = new CountyCode() { alpha_2 = dto.alpha_2, alpha_3 = dto.alpha_3, country_code = dto.country_code};
            var exceptions = new[] { "AQ", "BV", "TF" , "HM", "GS","EH" };
            if (exceptions.Contains(dto.alpha_2)) return cc;
            cc.regionInfo = new RegionInfo(dto.alpha_2);
            return cc;
        } 
    }
}
