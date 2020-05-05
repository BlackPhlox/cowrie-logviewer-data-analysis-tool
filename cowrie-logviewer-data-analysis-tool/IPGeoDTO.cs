using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cowrie_logviewer_data_analysis_tool
{
    public class IPGeoDTO
    {
        //From https://ip-api.com/docs/api:batch#test using http://ip-api.com/batch?fields=17033426
        public string status { get; set; }
        public string countryCode { get; set; }
        public string city { get; set; }
        public float lat { get; set; }
        public float lon { get; set; }
        public bool proxy { get; set; }
        public bool hosting { get; set; }
        public bool mobile { get; set; }
        public string query { get; set; }
        public string @as { get; set; }

        public static IPGeoDTO ParseFromCSV(string line)
        {
            var lines = line.Split(',');
            var geo = new IPGeoDTO
            {
                //query,countryCode,city,lat,lon,proxy,hosting,mobile,as (status is ommited)
                query = lines[0],
                countryCode = lines[1],
                city = lines[2],
                lat = float.Parse(lines[3]),
                lon = float.Parse(lines[4]),
                proxy = bool.Parse(lines[5]),
                hosting = bool.Parse(lines[6]),
                mobile = bool.Parse(lines[7]),
                @as = lines[8]
            };
            return geo;
        }
    }
}
