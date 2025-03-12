using I2MsgEncoder.Records;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace I2MsgEncoder.RecordProcessors
{
    /**
     * Generate ESRecord's for air quality
     */
    class ESRecordGenerator
    {
        public static async Task<XElement> GetRecord(string location)
        {
            var root = new XElement("ESRecord");

            var httpClient = HttpClientHolder.GetClient();
            var locData = LocationDB.FindLocation(Util.LocationToI2(location));
            var config = I2MsgEncoder.Config.Config.Load();
            List<AirNowData> data;

            try
            {
                String url = String.Format("http://www.airnowapi.org/aq/forecast/zipCode/?format=application/json&zipCode={0}&API_KEY={1}",
                    locData.LFData.Zip2locId, config.AirNowKey);
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var output = await response.Content.ReadAsStringAsync();
                data = JsonConvert.DeserializeObject<List<AirNowData>>(output);
            }
            catch (Exception e)
            {
                Log.Warning("Failed to get air quality data");
                return root;
            }

            var dtFormat = "MMddyyyyHHmmss";
            var pollutants = new List<XElement>();
            var primaryPollutant = data.OrderByDescending(item => item.AQI).First();
            string cityName = "";
            string stateCode = "";

            foreach (var pollutant in data)
            {
                DateTime forecastDate;
                DateTime expTime;

                try
                {
                    forecastDate = DateTime.ParseExact(pollutant.DateForecast.Trim(), "yyyy-MM-dd", new CultureInfo("en-US"));
                    expTime = new DateTime(forecastDate.Year, forecastDate.Month, forecastDate.Day, 23, 59, 59);
                }
                catch (Exception)
                {
                    Log.Warning("Failed to parse AirNow dates");
                    throw new Exception();
                }

                pollutants.Add(new XElement("ESData",

                    new XElement("ESdt", forecastDate.ToString(dtFormat)),

                    // Unix GMT
                    new XElement("ESvalTmUxGMT", ((DateTimeOffset)forecastDate).ToUnixTimeSeconds()),
                    new XElement("ESexpTmUxGMT", ((DateTimeOffset)expTime).ToUnixTimeSeconds()),

                    // GMT
                    new XElement("ESvalTmGMT", forecastDate.ToString(dtFormat)),
                    new XElement("ESexpTmGMT", expTime.ToString(dtFormat)),

                    // Local Time
                    new XElement("ESvalTmLAP", forecastDate.ToString(dtFormat)), // (Only one actually used)
                    new XElement("ESexpTmLAP", expTime.ToString(dtFormat)),

                    new XElement("ESprmry", pollutant == primaryPollutant ? "Y" : "N"),
                    new XElement("ESseq", 0), // Unknown
                    new XElement("ESplltnt", pollutant.GetPollutant()),
                    new XElement("ESaqi", pollutant.AQI),
                    new XElement("EScat", pollutant.Category.Name),
                    new XElement("EScatIdx", pollutant.Category.Number),
                    new XElement("ESactDy", pollutant.ActionDay ? "Yes" : "No"),
                    new XElement("ESdscsn"), // Unknown
                    new XElement("ESactDyTxt", pollutant.Discussion)
                ));

                cityName = pollutant.ReportingArea;
                stateCode = pollutant.StateCode;
            }

            // procTime must be EST
            var easternTime = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var convTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternTime);

            root.Add(new XElement("action", 1));
            root.Add(
                new XElement("ESHdr",
                    new XElement("EStyp", "F"), // This must be F for the I2 to use the data
                    new XElement("ESctyNm", cityName), // City name
                    new XElement("ESstCd", stateCode), // State
                    new XElement("ESctyCd", locData.LFData.EPAId),
                    new XElement("procTm", convTime.ToString("yyyyMMddHHmmss")) // Time the record was processed
                ),
                pollutants
            );

            return root;
        }
    }
}
