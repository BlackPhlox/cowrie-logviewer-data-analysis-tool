using System;
using System.Net;

namespace cowrie_logviewer_data_analysis_tool
{
    public class Event
    {
        public IPGeoDTO ipInfo;
        public IPAddress ipaddress;
        public DateTime date;

        public Event(IPGeoDTO ipInfo, string ip, string dt) {
            this.ipInfo = ipInfo;
            this.ipaddress = IPAddress.Parse(ip);
            this.date = DateTime.Parse(dt);
        }

        /*
        public enum EventId
        {
            [Description("cowrie.client.version")]
            VERSION,
            [Description("cowrie.session.closed")]
            CLOSED,
            [Description("cowrie.session.connect")]
            CONNECT,
            [Description("cowrie.client.kex")]
            KEX,
            [Description("cowrie.login.success")]
            SUCCESS,
            [Description("cowrie.direct-tcpip.request")]
            REQUEST,
            [Description("cowrie.direct-tcpip.data")]
            DATA,
            [Description("cowrie.session.params")]
            PARAMS,
            [Description("cowrie.command.input")]
            INPUT
        }
        */

        //EventId.GetDescription()
    }
}