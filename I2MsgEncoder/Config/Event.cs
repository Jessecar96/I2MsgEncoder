using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I2MsgEncoder.Config
{
    class Event
    {
        [JsonProperty("topic")]
        public string topic { get; set; }

        [JsonProperty("payload")]
        public string payload { get; set; } = null;

        [JsonProperty("offset")]
        public int offset { get; set; } = 0;

        [JsonProperty("cancel_offset")]
        public int cancelOffset { get; set; } = 0;

        [JsonProperty("cancel_presentation")]
        public List<string> cancelPresentations { get; set; } = new List<string>();

        [JsonProperty("run_presentation")]
        public List<Presentation> presentations { get; set; } = new List<Presentation>();

        [JsonProperty("run_presentation_post")]
        public List<Presentation> postPresentations { get; set; } = new List<Presentation>();
    }
}
