using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace I2MsgEncoder
{
    class RecordCache
    {
        public static Dictionary<string, long> GetAll()
        {
            string file = Path.Combine(Util.GetTempDir(), "records.json");
            if (!File.Exists(file))
            {
                return new Dictionary<string, long>();
            }

            try
            {
                // Try to parse as JSON
                Dictionary<string, long> json = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(file));
                return json;
            }
            catch (Exception e)
            {
                return new Dictionary<string, long>();
            }
        }

        public static void SetLastSent(string record, long time)
        {
            if (record == "IDRecord")
            {
                record = "Pollen";
            }

            string file = Path.Combine(Util.GetTempDir(), "records.json");
            var data = GetAll();
            data[record] = time;
            File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public static long GetLastSent(string record)
        {
            var data = GetAll();
            return data[record];
        }

    }
}
