using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace I2MsgEncoder.Config
{
    class Star
    {
        [JsonProperty("headend_id")]
        public string headendId { get; set; }

        [JsonProperty("config")]
        public string configFile { get; set; }

        [JsonProperty("star_flags")]
        public List<string> starFlags { get; set; } = new List<string>();

        [JsonProperty("crawls")]
        public List<string> crawls { get; set; } = new List<string>();

        public bool IsSD()
        {
            return starFlags.Contains("Domestic_SD_Universe");
        }

        public bool IsHD()
        {
            return starFlags.Contains("Domestic_Universe");
        }
    }
}