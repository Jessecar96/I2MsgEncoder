using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace I2MsgEncoder.Records
{
    class AirNowData
    {
        [JsonProperty("DateIssue")]
        public string DateIssue { get; set; }

        [JsonProperty("DateForecast")]
        public string DateForecast { get; set; }

        [JsonProperty("ReportingArea")]
        public string ReportingArea { get; set; }

        [JsonProperty("StateCode")]
        public string StateCode { get; set; }

        [JsonProperty("Latitude")]
        public long Latitude { get; set; }

        [JsonProperty("Longitude")]
        public long Longitude { get; set; }

        [JsonProperty("ParameterName")]
        public string ParameterName { get; set; }

        [JsonProperty("AQI")]
        public int AQI { get; set; }

        [JsonProperty("Category")]
        public AirNowCategory Category { get; set; }

        [JsonProperty("ActionDay")]
        public bool ActionDay { get; set; }

        [JsonProperty("Discussion")]
        public string Discussion { get; set; }

        /**
         * Get the TWC style pollutant name
         */
        public string GetPollutant()
        {
            switch (ParameterName)
            {
                case "O3":
                    return "OZONE";
                default:
                    return ParameterName;
            }
        }
    }

    class AirNowCategory
    {
        [JsonProperty("Number")]
        public int Number { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
