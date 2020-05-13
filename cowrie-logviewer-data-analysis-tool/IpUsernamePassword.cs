using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cowrie_logviewer_data_analysis_tool
{
    class IpUsernamePassword
    {
        public string ip { get; set; }
        public HashSet<UnamePass> Failed { get; set; }

        public HashSet<UnamePass> Success { get; set; }

    }

    class UnamePass
    {
        public string uname { get; set; }
        public string pass { get; set; }
        public int count { get; set; }
    }
}
