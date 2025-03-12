using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace I2MsgEncoder
{
    public class Util
    {
        public static string GetDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetTempDir(string name)
        {
            string baseTempDir = GetTempDir();
            string namedTempDir = Path.Combine(baseTempDir, name);

            if (!Directory.Exists(namedTempDir))
            {
                Directory.CreateDirectory(namedTempDir);
            }

            return namedTempDir;
        }

        public static string GetTempDir()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string baseTempDir = Path.Combine(baseDir, "temp");

            if (!Directory.Exists(baseTempDir))
            {
                Directory.CreateDirectory(baseTempDir);
            }

            return baseTempDir;
        }

        public static string GetTempFileName()
        {
            return Guid.NewGuid().ToString();
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static DateTime UnixTimeStampToDateTimeUTC(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static DateTime UnixToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /**
         * Convert location ID to I2 format
         * 1_US_XXXXXXXXX
         */
        public static string LocationToI2(string location)
        {
            Match i2Match = Regex.Match(location, @"(\d+)_(.+)_(.+)");
            if (i2Match.Success)
            {
                return location;
            }

            Match apiMatch = Regex.Match(location, @"(.+):(\d+):(.+)");
            if (apiMatch.Success)
            {
                return String.Format("{0}_{1}_{2}", apiMatch.Groups[2], apiMatch.Groups[3], apiMatch.Groups[1]);
            }
            return null;
        }

        /**
         * Convert location ID to API format
         * XXXXXXXXX:1:US
         */
        public static string LocationToAPI(string location)
        {
            Match apiMatch = Regex.Match(location, @"(.+):(\d+):(.+)");
            if (apiMatch.Success)
            {
                return location;
            }

            Match i2Match = Regex.Match(location, @"(\d+)_(.+)_(.+)");
            if (i2Match.Success)
            {
                return String.Format("{0}:{1}:{2}", i2Match.Groups[3], i2Match.Groups[1], i2Match.Groups[2]);
            }

            return null;
        }

    }
}
