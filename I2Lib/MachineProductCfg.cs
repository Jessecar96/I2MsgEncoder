using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace I2MsgEncoder
{
    public class MachineProductCfg
    {
        XDocument config;

        public static MachineProductCfg Load(string file)
        {
            MachineProductCfg cfg = new MachineProductCfg();
            cfg.config = XDocument.Load(file);
            return cfg;
        }

        public MachineProductCfg()
        {
            config = new XDocument();

            config.Add(new XElement("Config",
                new XElement("ConfigDef",
                    new XElement("ConfigItems")
                )
            ));

            string[] items = new string[]
            {
                "AffiliateLogoId",
                "AffiliateText1",
                "AffiliateText2",
                "Airport1",
                "Airport1_1",
                "Airport1_2",
                "Airport2",
                "Airport2_1",
                "Airport2_2",
                "Airport3",
                "Airport3_1",
                "Airport3_2",
                "AreaServed",
                "AuthorizationCode",
                "AuthorizationDate",
                "BeachLocation",
                "BeachLocation_1",
                "BeachLocation_2",
                "ChannelId",
                "ChannelNumber",
                "ChannelText",
                "CityName",
                "ConnectMethod",
                "CountryCode",
                "DataFromHeadendNb",
                "disableLocalization",
                "disableLOCL",
                "DisplayLogoFlag",
                "DmaCode",
                "DmaName",
                "ExternalIpAddress",
                "FedFromHeadend",
                "FedFromHeadendName",
                "FrontChannelGateway",
                "FrontChannelIpAddress",
                "FrontChannelNetMask",
                "GatewayAddress",
                "HeadendAddress",
                "HeadendId",
                "HeadendName",
                "LocalDns1Address",
                "LocalDns2Address",
                "LocalIpAddress",
                "LocalOcmMkt",
                "LocalRadarCity1",
                "LocalRadarCity1_1",
                "LocalRadarCity1_2",
                "LocalRadarCity2",
                "LocalRadarCity2_1",
                "LocalRadarCity2_2",
                "LocalRadarCity3",
                "LocalRadarCity3_1",
                "LocalRadarCity3_2",
                "LocalRadarCity4",
                "LocalRadarCity4_1",
                "LocalRadarCity4_2",
                "LocalRadarCity5",
                "LocalRadarCity5_1",
                "LocalRadarCity5_2",
                "LocalRadarCity6",
                "LocalRadarCity6_1",
                "LocalRadarCity6_2",
                "LocalRadarCity7",
                "LocalRadarCity7_1",
                "LocalRadarCity7_2",
                "LocalRadarCity8",
                "LocalRadarCity8_1",
                "LocalRadarCity8_2",
                "LogoCd",
                "LogoDescription",
                "MapCity1",
                "MapCity1_1",
                "MapCity1_2",
                "MapCity2",
                "MapCity2_1",
                "MapCity2_2",
                "MapCity3",
                "MapCity3_1",
                "MapCity3_2",
                "MapCity4",
                "MapCity4_1",
                "MapCity4_2",
                "MapCity5",
                "MapCity5_1",
                "MapCity5_2",
                "MapCity6",
                "MapCity6_1",
                "MapCity6_2",
                "MapCity7",
                "MapCity7_1",
                "MapCity7_2",
                "MapCity8",
                "MapCity8_1",
                "MapCity8_2",
                "MasterUnitAddress",
                "MetroMapCity1",
                "MetroMapCity1_1",
                "MetroMapCity1_2",
                "MetroMapCity2",
                "MetroMapCity2_1",
                "MetroMapCity2_2",
                "MetroMapCity3",
                "MetroMapCity3_1",
                "MetroMapCity3_2",
                "MetroMapCity4",
                "MetroMapCity4_1",
                "MetroMapCity4_2",
                "MetroMapCity5",
                "MetroMapCity5_1",
                "MetroMapCity5_2",
                "MetroMapCity6",
                "MetroMapCity6_1",
                "MetroMapCity6_2",
                "MetroMapCity7",
                "MetroMapCity7_1",
                "MetroMapCity7_2",
                "MetroMapCity8",
                "MetroMapCity8_1",
                "MetroMapCity8_2",
                "MsoCode",
                "MsoName",
                "NearbyLocation1",
                "NearbyLocation1_1",
                "NearbyLocation1_2",
                "NearbyLocation2",
                "NearbyLocation2_1",
                "NearbyLocation2_2",
                "NearbyLocation3",
                "NearbyLocation3_1",
                "NearbyLocation3_2",
                "NearbyLocation4",
                "NearbyLocation4_1",
                "NearbyLocation4_2",
                "NearbyLocation5",
                "NearbyLocation5_1",
                "NearbyLocation5_2",
                "NearbyLocation6",
                "NearbyLocation6_1",
                "NearbyLocation6_2",
                "NearbyLocation7",
                "NearbyLocation7_1",
                "NearbyLocation7_2",
                "NearbyLocation8",
                "NearbyLocation8_1",
                "NearbyLocation8_2",
                "NetMask",
                "OcmOn8CityCode",
                "OcmOn8DataPid",
                "OcmOn8Enabled",
                "OcmOn8MsgIngesterMulticastIpAddress",
                "OcmOn8MsgIngesterMulticastPort",
                "OnAirName",
                "primaryCounty",
                "PrimaryLatitudeLongitude",
                "PrimaryLatitudeLongitude_1",
                "PrimaryLatitudeLongitude_2",
                "PrimaryLocation",
                "PrimaryLocation_1",
                "PrimaryLocation_2",
                "primaryMarineZone",
                "primaryZone",
                "PrimaryWMO",
                "RadioLogo",
                "RegionalMapCity1",
                "RegionalMapCity1_1",
                "RegionalMapCity1_2",
                "RegionalMapCity2",
                "RegionalMapCity2_1",
                "RegionalMapCity2_2",
                "RegionalMapCity3",
                "RegionalMapCity3_1",
                "RegionalMapCity3_2",
                "RegionalMapCity4",
                "RegionalMapCity4_1",
                "RegionalMapCity4_2",
                "RegionalMapCity5",
                "RegionalMapCity5_1",
                "RegionalMapCity5_2",
                "RegionalMapCity6",
                "RegionalMapCity6_1",
                "RegionalMapCity6_2",
                "RegionalMapCity7",
                "RegionalMapCity7_1",
                "RegionalMapCity7_2",
                "RegionalMapCity8",
                "RegionalMapCity8_1",
                "RegionalMapCity8_2",
                "secondaryCounties",
                "secondaryZones",
                "serialNumber",
                "Service",
                "StateCode",
                "StatesServed",
                "SummerGetawayLocation1",
                "SummerGetawayLocation1_1",
                "SummerGetawayLocation1_2",
                "SummerGetawayLocation2",
                "SummerGetawayLocation2_1",
                "SummerGetawayLocation2_2",
                "SummerGetawayLocation3",
                "SummerGetawayLocation3_1",
                "SummerGetawayLocation3_2",
                "SystemId",
                "SystemName",
                "TerminationDate",
                "TideStation1",
                "TideStation1_1",
                "TideStation1_2",
                "TideStation2",
                "TideStation2_1",
                "TideStation2_2",
                "TideStation3",
                "TideStation3_1",
                "TideStation3_2",
                "TideStation4",
                "TideStation4_1",
                "TideStation4_2",
                "TideStation5",
                "TideStation5_1",
                "TideStation5_2",
                "TideStation6",
                "TideStation6_1",
                "TideStation6_2",
                "TideStation7",
                "TideStation7_1",
                "TideStation7_2",
                "TideStation8",
                "TideStation8_1",
                "TideStation8_2",
                "timeZone",
                "TrafficIncidentPrimaryRadius",
                "TrafficIncidentSecondaryRadius",
                "TrafficKeyRoute1",
                "TrafficKeyRoute2",
                "TrafficKeyRoute3",
                "TrafficKeyRoute4",
                "TrafficKeyRoute5",
                "TrafficKeyRoute6",
                "TravelCity1",
                "TravelCity1_1",
                "TravelCity1_2",
                "TravelCity2",
                "TravelCity2_1",
                "TravelCity2_2",
                "TravelCity3",
                "TravelCity3_1",
                "TravelCity3_2",
                "TravelCity4",
                "TravelCity4_1",
                "TravelCity4_2",
                "TravelCity5",
                "TravelCity5_1",
                "TravelCity5_2",
                "WinterGetawayLocation1",
                "WinterGetawayLocation1_1",
                "WinterGetawayLocation1_2",
                "WinterGetawayLocation2",
                "WinterGetawayLocation2_1",
                "WinterGetawayLocation2_2",
                "WinterGetawayLocation3",
                "WinterGetawayLocation3_1",
                "WinterGetawayLocation3_2",
                "ZipCode"
            };

            foreach(string item in items)
            {
                config.Root.Element("ConfigDef").Element("ConfigItems").Add(
                    new XElement("ConfigItem",
                        new XAttribute("key", item),
                        new XAttribute("value", "")
                    )
                );
            }
        }

        public XAttribute ConfigItem(string name)
        {
            XElement result = config.Root.Element("ConfigDef").Element("ConfigItems").Descendants()
                .FirstOrDefault(el => el.Attribute("key")?.Value == name);
            return result.Attribute("value");
        }

        public MachineProductCfg SetPrimaryLocation(string value)
        {
            ConfigItem("PrimaryLocation").Value = value;
            return this;
        }

        public MachineProductCfg SetHeadendId(string value)
        {
            ConfigItem("HeadendId").Value = value;
            return this;
        }

        public MachineProductCfg SetHeadendName(string value)
        {
            ConfigItem("HeadendName").Value = value;
            return this;
        }

        public MachineProductCfg SetSerialNumber(string value)
        {
            ConfigItem("serialNumber").Value = value;
            return this;
        }

        public void Save(string file)
        {
            config.Save(file);
        }

        public UDPMessage GetMessage()
        {
            StarBundle bundle = new StarBundle("ProdConfigChange");
            string path = bundle.GetDirectory();

            // Save file to bundle dir
            string fileName = Path.Combine(path, "MachineProductCfg.xml");
            config.Save(fileName);

            // Add config to bundle
            bundle.AddAction(new TWC.SE.StarBundle.AddAction()
            {
                Src = "MachineProductCfg.xml",
                Dest = "Config\\MachineProductCfg.xml"
            });

            return bundle.GetMessage();
        }
    }
}
