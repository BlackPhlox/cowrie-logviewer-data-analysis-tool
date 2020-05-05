using cowrie_logviewer_data_analysis_tool.Runner;

namespace cowrie_logviewer_data_analysis_tool
{
    public class EventDTO
    {
        public string eventid { get; set; }
        public string src_ip { get; set; }
        public int src_port { get; set; }
        public string dst_ip { get; set; }
        public string session { get; set; }
        public string protocol { get; set; }
        public object message { get; set; }
        public string sensor { get; set; }
        public string timestamp { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string arch { get; set; }
        public float duration { get; set; }

        public Event parseAll(Script script) {
            return new Event(script.Lookup(src_ip),src_ip,timestamp);
        }
    }
}
