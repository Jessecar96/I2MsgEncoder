using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace I2MsgEncoder.Config
{
    class Presentation
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("flavor")]
        public string flavor { get; set; }

        [JsonProperty("duration")]
        public int duration { get; set; }

        [JsonProperty("logo")]
        public string logo { get; set; }
    }
}
