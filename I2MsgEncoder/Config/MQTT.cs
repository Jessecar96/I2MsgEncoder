using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I2MsgEncoder.Config
{
    class MQTT
    {
        [JsonProperty("server")]
        public string server { get; set; }

        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("presentation_offset")]
        public int presentationOffset { get; set; } = 0;

        [JsonProperty("enable_heartbeats")]
        public bool enableHeartbeats { get; set; } = true;

        [JsonProperty("enable_presentations")]
        public bool enablePresentations { get; set; } = true;

        [JsonProperty("events")]
        public List<Event> events { get; set; }
    }
}
