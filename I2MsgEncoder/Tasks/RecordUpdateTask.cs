using I2MsgEncoder.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace I2MsgEncoder.Tasks
{
    class RecordUpdateTask
    {
        private Timer timer;
        private UDPSender udpSender;
        private bool isProcessing = false;

        private List<string> primaryLocations = new List<string>();
        private List<string> locations = new List<string>();
        private List<string> tideStations = new List<string>();
        private List<string> airports = new List<string>();

        public RecordUpdateTask(UDPSender udpSender)
        {
            this.udpSender = udpSender;

            // Setup timer
            timer = new Timer(5 * 1000); // 5 seconds
            timer.Elapsed += Update;
            timer.AutoReset = true;

            // Load stars
            var config = I2MsgEncoder.Config.Config.Load();

            // Loop through our stars and find all locations we need
            foreach (Star star in config.stars)
            {
                MachineProductCfg prodCfg = MachineProductCfg.Load(star.configFile);

                // Add locations that we need to download
                locations.Add(prodCfg.ConfigItem("PrimaryLocation").Value);
                primaryLocations.Add(prodCfg.ConfigItem("PrimaryLocation").Value);

                // LDL Locations
                for (int i = 1; i <= 8; i++)
                {
                    string val = prodCfg.ConfigItem("NearbyLocation" + i).Value;
                    if (String.IsNullOrEmpty(val)) continue;
                    locations.Add(val);
                }

                // Metro map locations
                for (int i = 1; i <= 8; i++)
                {
                    string val = prodCfg.ConfigItem("MetroMapCity" + i).Value;
                    if (String.IsNullOrEmpty(val)) continue;
                    locations.Add(val);
                }

                // Tide Stations
                for (int i = 1; i <= 8; i++)
                {
                    string val = prodCfg.ConfigItem("TideStation" + i).Value;
                    if (String.IsNullOrEmpty(val)) continue;
                    tideStations.Add(val);
                }

                // Airports
                for (int i = 1; i <= 3; i++)
                {
                    string val = prodCfg.ConfigItem("Airport" + i).Value;
                    if (String.IsNullOrEmpty(val)) continue;
                    airports.Add(val);
                }
            }

            // Remove duplicate locations
            primaryLocations = primaryLocations.Distinct().ToList();
            locations = locations.Distinct().ToList();
            tideStations = tideStations.Distinct().ToList();
            airports = airports.Distinct().ToList();
        }

        private void Update(object sender, ElapsedEventArgs e)
        {
            if (isProcessing)
                return;

            isProcessing = true;

            var intervals = GetRecordUpdateInvertvals();
            var latestRecords = RecordCache.GetAll();

            var recordsToUpdate = new List<string>();

            foreach (KeyValuePair<string, long> entry in intervals)
            {
                string record = entry.Key;
                long interval = entry.Value;

                if (!latestRecords.ContainsKey(record))
                {
                    Log.Debug("Cannot find {0} in cache, updating", record);
                    recordsToUpdate.Add(record);
                    continue;
                }

                long diff = Util.GetCurrentUnixTimestampMillis() - latestRecords[record];
                //Log.Debug("{0} last update {1} seconds ago. updating in {2} seconds", record, diff / 1000, (interval - diff) / 1000);
                if (diff >= interval)
                {
                    Log.Debug("Record {0} was last updated {1} seconds ago, updating", record, diff / 1000);
                    recordsToUpdate.Add(record);
                    continue;
                }
            }

            foreach (var record in recordsToUpdate)
            {
                var processor = new RecordProcessor(this.udpSender);
                processor.AddRecordType(record);

                switch (record)
                {
                    // Records that only need primary
                    case "ESRecord":
                    case "Pollen":
                        processor.AddLocations(primaryLocations);
                        break;

                    // Records for mostly all locations
                    case "MORecord":
                    case "BERecord":
                    case "DDRecord":
                    case "DHRecord":
                    case "DFRecord":
                        processor.AddLocations(locations);
                        break;

                    // ... Tides
                    case "TIRecord":
                        processor.AddLocations(tideStations);
                        break;
                }

                processor.Send();
            }

            isProcessing = false;
        }

        public void Start()
        {
            timer.Start();
        }

        private Dictionary<string, long> GetRecordUpdateInvertvals()
        {
            return new Dictionary<string, long>
            {
                { "MORecord", 5 * 60 * 1000 }, // Current condtitions, 5 minutes
                { "BERecord", 5 * 60 * 1000 }, // Warnings, 5 minutes
                
                { "DDRecord", 15 * 60 * 1000 }, // Detailed Daypart
                { "DHRecord", 15 * 60 * 1000 }, // Hourly
                { "DFRecord", 15 * 60 * 1000 }, // Days

                { "ESRecord", 3 * 60 * 60 * 1000 }, // Air Quality, 3 hours
                { "Pollen", 3 * 60 * 60 * 1000 }, // Pollen, 3 hours
                { "TIRecord", 3 * 60 * 60 * 1000 }, // Tides, 3 hours
            };
        }
    }
}
