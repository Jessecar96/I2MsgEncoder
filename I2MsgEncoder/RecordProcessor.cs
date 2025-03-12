using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using I2MsgEncoder.RecordProcessors;
using I2MsgEncoder.Records;
using System.Net.Http;

namespace I2MsgEncoder
{
    class RecordProcessor
    {
        private Dictionary<string, XElement> records = new Dictionary<string, XElement>();
        private List<string> recordTypes = new List<string>();
        private List<string> locations = new List<string>();
        private string tempDir = Util.GetTempDir();
        private UDPSender udpSender;
        private XElement severeQualifiers;
        private HttpClient httpClient;

        public RecordProcessor(UDPSender udpSender)
        {
            this.udpSender = udpSender;
            this.httpClient = HttpClientHolder.GetClient();
        }

        public RecordProcessor(UDPSender udpSender, List<string> recordTypes, List<string> locations)
        {
            this.udpSender = udpSender;
            this.recordTypes = recordTypes;
            this.locations = locations;
            this.httpClient = HttpClientHolder.GetClient();
        }

        public void AddLocation(string location)
        {
            locations.Add(location);
        }

        public void AddLocations(List<string> locations)
        {
            this.locations.AddRange(locations);
        }

        public void AddRecordType(string recordType)
        {
            recordTypes.Add(recordType);
        }

        private XElement GetSevereQualifierMapping()
        {
            if (this.severeQualifiers == null)
            {
                var doc = XDocument.Load(Path.Combine(Util.GetDir(), "Data", "SevereQualifiers.xml"));
                this.severeQualifiers = doc.Root;
            }

            return this.severeQualifiers;
        }

        private async Task Download()
        {
            foreach(var record in recordTypes)
            {
                RecordCache.SetLastSent(record, Util.GetCurrentUnixTimestampMillis());
            }

            // Some record types require us to generate them since DSX does not
            if (recordTypes.Contains("ESRecord"))
            {
                foreach (string location in locations)
                {
                    var record = await ESRecordGenerator.GetRecord(location);
                    AddRecord("ESRecord", record);
                    recordTypes.Remove("ESRecord");
                }
            }

            // No record types? Don't continue
            if (recordTypes.Count == 0)
            {
                this.SendUDP();
                return;
            }

            // We need to split the locations
            int locationsPerRequest = 10;

            var list = new List<List<string>>();
            for (int i = 0; i < this.locations.Count; i += locationsPerRequest)
            {
                list.Add(this.locations.GetRange(i, Math.Min(locationsPerRequest, locations.Count - i)));
            }

            foreach (List<string> locations in list)
            {
                // Transform location IDs like 1_US_XXXXXXXX into XXXXXXXX:1:US
                var apiFormattedLocations = new List<string>();
                foreach (string location in locations)
                {
                    var newFormat = Util.LocationToAPI(location);
                    if (newFormat != null)
                    {
                        apiFormattedLocations.Add(newFormat);
                    }
                }

                string url = String.Format("http://dsx.weather.com/wxd/v2/({0})/en_US/({1})", String.Join(";", this.recordTypes.ToArray()), String.Join(";", apiFormattedLocations.ToArray()));
                var httpResponse = await this.httpClient.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode();
                string responseBody = await httpResponse.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(responseBody);

                foreach (dynamic record in json)
                {
                    ProcessRecord(record);
                }

                Thread.Sleep(500);
            }

            this.SendUDP();
        }

        private void ProcessRecord(dynamic record)
        {
            Log.Debug("Processing {0} {1}", record.status, record.id);
            if (record.status != 200)
            {
                Log.Warning("ERROR: Record {0} status was not 200", record.id);
                return;
            }

            // Convert doc to XML
            Type t = record.doc.GetType();

            if (t.Name == "JArray")
            {
                // If this record is an array (such as BERecord) we need to process each one
                foreach (dynamic document in record.doc)
                {
                    ProcessDocument((string)record.id, document);
                }
            }
            else
            {
                ProcessDocument((string)record.id, record.doc);
            }
        }

        private void ProcessDocument(string recordId, dynamic document)
        {
            Match m = Regex.Match(recordId, @"\/wxd\/v.\/(.+)\/en_US\/(.+)");
            string recordName = m.Groups[1].Value;
            string location = m.Groups[2].Value;

            // Get our location db record
            var dbFormattedLocation = Util.LocationToI2(location);
            var locRecord = LocationDB.FindLocation(dbFormattedLocation);
            if (locRecord == null)
            {
                Log.Warning("Location not found: {0}", dbFormattedLocation);
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(locRecord.LFData.TmZnNm);
            var easternTime = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            XElement recordDoc = JsonConvert.DeserializeXNode(document.ToString(), recordName).Root;

            // Pollen needs to be transformed into an IDRecord
            if (recordName == "Pollen")
            {
                Dictionary<string, XElement> pollenRecords = new Dictionary<string, XElement>();
                foreach (var pollenRecord in recordDoc.Elements())
                {
                    string pollenName = pollenRecord.Name.ToString();
                    int idxType;

                    switch (pollenName)
                    {
                        case "tree":
                            idxType = 11;
                            break;
                        case "grass":
                            idxType = 12;
                            break;
                        case "weed":
                            idxType = 13;
                            break;
                        default:
                            continue; // Any other types just ignore
                    }

                    if (!pollenRecords.ContainsKey(pollenName))
                    {
                        XElement IDRecord = new XElement("IDRecord");
                        IDRecord.Add(new XElement("action", 1));
                        IDRecord.Add(new XElement("IDHdr",
                            new XElement("idxTyp", idxType),
                            new XElement("idxId", "K" + locRecord.LFData.PollenId),
                            new XElement("coopId", locRecord.LFData.CoopId),
                            new XElement("procTm", TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternTime).ToString("yyyyMMddHHmmss"))
                        ));

                        pollenRecords.Add(pollenName, IDRecord);
                    }

                    pollenRecord.Name = "IDData";
                    pollenRecord.Element("idxTmISO").Remove();
                    pollenRecords[pollenName].Add(pollenRecord);
                }

                foreach (KeyValuePair<string, XElement> kvp in pollenRecords)
                {
                    AddRecord("IDRecord", kvp.Value);
                }
                return;
            }

            // Remove _ prefixed elements
            string recordL = recordName.Substring(0, 2);
            recordDoc.Element(recordL + "Hdr").Elements()
                .Where(r => r.Name.ToString().StartsWith("_")).Remove();
            recordDoc.Element(recordL + "Data").Elements()
                .Where(r => r.Name.ToString().StartsWith("_")).Remove();

            // Transform elements formatted like {0}_{1} into attributes
            IEnumerable<XElement> attrs = recordDoc.Descendants();
            List<XElement> toDelete = new List<XElement>();
            foreach (XElement el in attrs)
            {
                Match matches = Regex.Match(el.Name.ToString(), @"(.+)__(.+)");
                if (matches.Success)
                {
                    el.Parent.Element(matches.Groups[1].ToString())
                        .Add(new XAttribute(matches.Groups[2].ToString(), el.Value));
                    toDelete.Add(el);
                }
            }

            foreach (XElement el in toDelete)
            {
                el.Remove();
            }

            // Add obsStn and 
            XElement header = recordDoc.Element(recordL + "Hdr");
            if (header != null)
            {
                XElement stnNm = header.Element("stnNm");
                if (stnNm != null && locRecord.LFData.CityName != null)
                {
                    stnNm.Value = locRecord.LFData.CityName;
                }

                XElement obsStn = header.Element("obsStn");
                if (obsStn != null && locRecord.LFData.PrimTecci != null)
                {
                    obsStn.Value = locRecord.LFData.PrimTecci;
                }
                else if (obsStn != null && locRecord.LFData.ObsStn != null)
                {
                    obsStn.Value = locRecord.LFData.ObsStn;
                }

                XElement coopId = header.Element("coopId");
                if (coopId != null && locRecord.LFData.CoopId != null)
                {
                    coopId.Value = ((string)locRecord.LFData.CoopId);
                }

                // Update procTime timezones
                var easternRecords = new string[] { "MORecord", "DDRecord", "DFRecord", "DHRecord" };
                if (easternRecords.Contains(recordName) && header.Element("procTmISO") != null)
                {
                    // Replace procTime with EST
                    XElement procTime = header.Element("procTm");
                    XElement procTmISO = header.Element("procTmISO");

                    var convTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(procTmISO.Value, null, System.Globalization.DateTimeStyles.RoundtripKind), easternTime);
                    procTime.Value = convTime.ToString("yyyyMMddHHmmss");
                }
            }

            if (recordName == "MORecord")
            {
                XElement moData = recordDoc.Element(recordL + "Data");
                if (moData != null && locRecord.LFData.PrimTecci != null)
                {
                    // Add station name (not required but whatever)
                    moData.Element("stnNm").Value = locRecord.LFData.ObsStn;

                    string locObsTm = moData.Element("locObsTm").Value;
                    string locObsDay = moData.Element("locObsDay").Value;
                    string tmpF = moData.Element("tmpF").Value.Replace("-", "M");
                    string iconExt = moData.Element("iconExt").Value;

                    // Build vocal code for current conditions
                    string vocalCd = String.Format("OI{0}:OZ{1}{2}:OT{3}:OTF{3}:OX{4}",
                        locRecord.LFData.PrimTecci, locObsDay,
                        locObsTm.Substring(locObsTm.Length - 2), tmpF, iconExt);

                    // Add vocal code
                    moData.Add(new XElement("vocalCd", vocalCd));
                }
            }
            else if (recordName == "DFRecord")
            {
                var dfData = recordDoc.Elements(recordL + "Data");

                // DFRecord day of week needs to be uppercase for the 7 day forecast weekend colors to work correctly
                foreach (var data in dfData)
                {
                    var el = data.Element("dow");
                    el.Value = el.Value.ToUpper();
                }

                // We're going to try and steal some qualifiers from DDRecord and turn them into extQulfr
                if (records.ContainsKey("DDRecord"))
                {
                    // Find the DDRecord that matches this location
                    var ddRecord = records["DDRecord"].Elements("DDRecord")
                        .FirstOrDefault(el => el.Element("DDHdr").Element("coopId").Value == locRecord.LFData.CoopId);

                    if (ddRecord != null)
                    {
                        var qualMapping = GetSevereQualifierMapping();

                        foreach (var ddData in ddRecord.Elements("DDData"))
                        {
                            var audioCd = ddData.Element("audioCd").Value;
                            var locValDay = ddData.Element("locValDay").Value;
                            var locValTm = ddData.Element("locValTm").Value;

                            foreach(var map in qualMapping.Elements())
                            {
                                if (audioCd.Contains(map.Attribute("Src").Value))
                                {
                                    Log.Debug("Found severe qualifier {0} => {1}", map.Attribute("Src").Value, map.Attribute("Dest").Value);
                                    var elDay = dfData.FirstOrDefault(el => el.Element("locValDay").Value == locValDay);
                                    elDay.Add(new XElement("extQulfr", map.Attribute("Dest").Value));
                                    elDay.Add(new XElement("extQulfr12", map.Attribute("Dest").Value));
                                    elDay.Add(new XElement("extQulfr12_24", map.Attribute("Dest").Value));
                                    break; // Only use the first match
                                }
                            }

                        }
                    }
                }
            }
            else if (recordName == "BERecord")
            {
                XElement beData = recordDoc.Element(recordL + "Data");
                XElement beHdr = recordDoc.Element(recordL + "Hdr");
                if (beHdr != null)
                {
                    beHdr.Element("bEvent").Elements()
                        .Where(r => r.Name.ToString().StartsWith("_")).Remove();
                }
                if (beData != null)
                {
                    // Add checksum (unknown if this does anything)
                    string chksum = SHA1(beHdr.Element("bPIL").Value + beHdr.Element("bEvent").Element("eOfficeId").Value + beHdr.Element("bEvent").Element("eEndTmUTC").Value);
                    beHdr.Add(new XElement("bSgmntChksum", chksum));

                    // Add bVocHdlnCd for the narration
                    string ePhenom = beHdr.Element("bEvent").Element("ePhenom").Value;
                    string eSgnfcnc = beHdr.Element("bEvent").Element("eSgnfcnc").Value;
                    beData.Element("bHdln").Add(new XElement("bVocHdlnCd", HeadlinePhrases.GetVocalCode(ePhenom, eSgnfcnc)));

                    var narrTxt = beData.Element("bNarrTxt");
                    if (narrTxt != null)
                    {
                        narrTxt.Element("bNarrTxtLang").Remove();
                    }
                }
            }
            else if (recordName == "DDRecord")
            {
                foreach (var data in recordDoc.Elements(recordL + "Data"))
                {
                    var locValDay = data.Element("locValDay").Value;
                    var locValTm = data.Element("locValTm").Value;

                    // Only apply these to the day, not night (190000 is night)
                    if (locValTm != "070000")
                        continue;

                    // Holidays
                    var year = DateTime.Now.Year;
                    var newYearsDay = new DateTime(year, 1, 1);
                    var MLKDay = HolidayMapping.FindDay(year, 1, DayOfWeek.Monday, 3);
                    var easter = HolidayMapping.EasterSunday(year);
                    var memorialDay = HolidayMapping.FindDay(year, 5, DayOfWeek.Monday, 4);
                    var jul4 = new DateTime(year, 7, 4);
                    var laborDay = HolidayMapping.FindDay(year, 9, DayOfWeek.Monday, 1);
                    var thanksgiving = HolidayMapping.FindDay(year, 11, DayOfWeek.Thursday, 4);
                    var dec24 = new DateTime(year, 12, 24);
                    var dec25 = new DateTime(year, 12, 25);
                    var newYearsEve = new DateTime(year, 12, 31);

                    int code = 0;
                    string day = "";

                    if (locValDay == newYearsDay.ToString("yyyyMMdd"))
                    {
                        code = 19;
                        day = "New Year's Day";
                    }
                    else if (locValDay == MLKDay.ToString("yyyyMMdd"))
                    {
                        code = 20;
                        day = "MLK Day";
                    }
                    else if (locValDay == easter.ToString("yyyyMMdd"))
                    {
                        code = 21;
                        day = "Easter";
                    }
                    else if (locValDay == memorialDay.ToString("yyyyMMdd"))
                    {
                        code = 22;
                        day = "Memorial Day";
                    }
                    else if (locValDay == jul4.ToString("yyyyMMdd"))
                    {
                        code = 23;
                        day = "Independence Day";
                    }
                    else if (locValDay == laborDay.ToString("yyyyMMdd"))
                    {
                        code = 24;
                        day = "Labor Day";
                    }
                    else if (locValDay == thanksgiving.ToString("yyyyMMdd"))
                    {
                        code = 25;
                        day = "Thanksgiving Day";
                    }
                    else if (locValDay == dec24.ToString("yyyyMMdd"))
                    {
                        code = 26;
                        day = "Christmas Eve";
                    }
                    else if (locValDay == dec25.ToString("yyyyMMdd"))
                    {
                        code = 27;
                        day = "Christmas Day";
                    }
                    else if (locValDay == newYearsEve.ToString("yyyyMMdd"))
                    {
                        code = 28;
                        day = "New Year's Eve";
                    }

                    if (code != 0)
                    {
                        var audioCd = data.Element("audioCd");
                        audioCd.Value = Regex.Replace(audioCd.Value, @"DA(\d{2})", "DA" + code);
                    }

                    if (day != "")
                    {
                        data.Element("altDyPrtNm").Value = day;
                    }
                }
            }
            else if (recordName == "TIRecord")
            {
                // Use correct tide station ID
                XElement TIHeader = recordDoc.Element(recordL + "Hdr");
                TIHeader.Element("TIstnId").Value = locRecord.LFData.TideId;
            }

            AddRecord(recordName, recordDoc);
        }

        private void AddRecord(string recordName, XElement recordDoc)
        {
            // Check if we have a thing for this record type already
            if (!this.records.ContainsKey(recordName))
            {
                XElement doc = new XElement("Data");
                doc.Add(new XAttribute("type", recordName));
                this.records.Add(recordName, doc);
            }

            // Add this location to our records
            this.records[recordName].Add(recordDoc);
        }

        private void SendUDP()
        {
            foreach (KeyValuePair<string, XElement> entry in this.records)
            {
                Log.Info("Sending {0}", entry.Key);

                string file = Path.Combine(tempDir, entry.Key + ".xml");
                File.WriteAllText(file, entry.Value.ToString());

                udpSender.SendFile(file, String.Format("storeData(__QGROUP__={0},Feed={0})", entry.Key));
                Thread.Sleep(100);
            }
        }

        public async Task Send()
        {
            await this.Download();
        }

        static string SHA1(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

    }
}
