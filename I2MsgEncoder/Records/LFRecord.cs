using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace I2MsgEncoder.Records
{
    [XmlRoot("LFRecord")]
    public class LFRecord
    {
        [XmlElement("action")]
        public int Action;

        [XmlElement("LFHdr")]
        public LFHdr LFHdr;

        [XmlElement("LFData")]
        public LFData LFData;
    }

    public class LFHdr
    {
        [XmlElement("locType")]
        public int LocType;

        [XmlElement("locId")]
        public string LocId;

        [XmlElement("procTm")]
        public Int64 ProcTime;
    }

    /*
     * This class is not complete
     */
    public class LFData
    {
        [XmlElement("cityNm")]
        public string CityName;

        [XmlElement("stCd")]
        public string State;

        [XmlElement("prsntNm")]
        public string PresentatioName;

        [XmlElement("coopId")]
        public string CoopId;

        [XmlElement("lat")]
        public double Lat;

        [XmlElement("long")]
        public double Long;

        [XmlElement("obsStn")]
        public string ObsStn;

        [XmlElement("secObsStn")]
        public string SecObsStn;

        [XmlElement("tertObsStn")]
        public string TertObsStn;

        [XmlElement("gmtDiff")]
        public double GmtDiff;

        [XmlElement("regSat")]
        public string RegSat;

        [XmlElement("cntyId")]
        public string CntyId;

        [XmlElement("cntyNm")]
        public string CntyNm;

        [XmlElement("zoneId")]
        public string ZoneId;

        [XmlElement("zoneNm")]
        public string ZoneNm;

        [XmlElement("cntyFips")]
        public string CntyFips;

        [XmlElement("active")]
        public bool Active;

        [XmlElement("dySTInd")]
        public string DySTInd;

        [XmlElement("dmaCd")]
        public string DMACd;

        [XmlElement("zip2locId")]
        public string Zip2locId;

        [XmlElement("elev")]
        public string Elevation;

        [XmlElement("cliStn")]
        public string CliStn;

        [XmlElement("tmZnNm")]
        public string TmZnNm;

        [XmlElement("tmZnAbbr")]
        public string TmZnAbbr;

        [XmlElement("idxId")]
        public string IdxId;

        [XmlElement("primTecci")]
        public string PrimTecci;

        [XmlElement("arptId")]
        public string AirportId;

        [XmlElement("mrnZoneId")]
        public string MarineZone;

        [XmlElement("pllnId")]
        public string PollenId;

        [XmlElement("skiId")]
        public string SkiId;

        [XmlElement("tideId")]
        public string TideId;

        [XmlElement("epaId")]
        public string EPAId;

        [XmlElement("tPrsntNm")]
        public string PresentationName;

        [XmlElement("wmoId")]
        public string WMOId;
    }
}
