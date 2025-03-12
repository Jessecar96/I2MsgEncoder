using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace I2MsgEncoder.Config
{
    class Config
    {
        [JsonProperty("address")]
        public string multicastAddress { get; set; } = "224.1.1.101";

        [JsonProperty("interface")]
        public string ifAddress { get; set; } = "10.100.102.10";

        [JsonProperty("log_level")]
        public string logLevel { get; set; } = "Info";

        [JsonProperty("airnow_key")]
        public string AirNowKey { get; set; } = "B7DE8841-84E3-47D2-925B-BB7A07B46AB1";

        [JsonProperty("stars")]
        public List<Star> stars { get; set; } = new List<Star>();

        [JsonProperty("mqtt")]
        public MQTT mqtt { get; set; } = null;

        public static Config Load()
        {
            string file = Path.Combine(Util.GetDir(), "config.json");
            if (!File.Exists(file))
            {
                return new Config();
            }

            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
        }

        public void Save()
        {
            using (StreamWriter file = File.CreateText(Path.Combine(Util.GetDir(), "config.json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, this);
            }
        }

        public bool HasSD()
        {
            foreach(Star star in stars)
            {
                if (star.IsSD())
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasHD()
        {
            foreach (Star star in stars)
            {
                if (star.IsHD())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
