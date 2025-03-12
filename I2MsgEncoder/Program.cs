using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Linq;
using I2MsgEncoder.Config;
using TWC.SE.StarBundle;
using I2MsgEncoder.Tasks;
using I2MsgEncoder.Records;

namespace I2MsgEncoder
{
    class Program
    {
        static UDPSender udpSender;

        static async Task Main(string[] args)
        {
            Console.Title = "Intellistar 2 Message Encoder";

            Log.Info("Intellistar 2 Message Encoder");
            Log.Info("Created by Jesse Cardone and the 4D Crew");

            var config = I2MsgEncoder.Config.Config.Load();

            Log.SetLogLevel(config.logLevel);

            if (config.stars.Count <= 0)
            {
                config.Save();
                Log.Warning("No stars have been specified in config.json. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            udpSender = new UDPSender(config.multicastAddress, 7787, config.ifAddress);

            /*
             * Send configs to each star in the config
             */
            foreach (Star star in config.stars)
            {
                Log.Info("Sending MachineProductCfg & StarFlags to headend {0}", star.headendId);

                if (!File.Exists(star.configFile))
                {
                    Log.Warning("File {0} not found, skipping headend {1}", star.configFile, star.headendId);
                    continue;
                }

                StarBundle bundle = new StarBundle("configs");

                /*
                 * Add MachineProductCfg
                 */

                // Load MachineProductCfg and change the headend id to the one specified
                MachineProductCfg prodCfg = MachineProductCfg.Load(star.configFile);
                prodCfg.SetHeadendId(star.headendId);

                // Save to the star bundle path
                string newFileName = "MachineProductCfg_" + star.headendId + ".xml";
                prodCfg.Save(Path.Combine(bundle.GetDirectory(), newFileName));

                // Add to our bundle
                bundle.AddAction(new AddAction()
                {
                    Src = Path.GetFileName(newFileName),
                    Dest = "Config\\MachineProductCfg.xml",
                    HeId = star.headendId
                });

                /*
                 * Add StarFlags
                 */
                StarFlags flags = new StarFlags();
                foreach (string flag in star.starFlags)
                {
                    flags.Add(flag);
                }

                string flagsFileName = "StarFlags_" + star.headendId + ".xml";
                flags.Save(Path.Combine(bundle.GetDirectory(), flagsFileName));
                bundle.AddAction(new AddAction()
                {
                    Src = Path.GetFileName(flagsFileName),
                    Dest = "Config\\StarFlags.xml",
                    HeId = star.headendId
                });

                /*
                 * Add AffiliateAds
                 */

                AffiliateAds ads = new AffiliateAds();
                foreach (string crawl in star.crawls)
                {
                    ads.AddCrawl(crawl);
                }
                string adsFile = "AffiliateAds_" + star.headendId + ".xml";
                ads.Save(Path.Combine(bundle.GetDirectory(), adsFile));
                bundle.AddAction(new AddAction()
                {
                    Src = Path.GetFileName(adsFile),
                    Dest = "Events\\AffiliateAds.xml",
                    HeId = star.headendId
                });

                // HD Holidays
                if (config.HasHD())
                {
                    XElement holidayMapping = HolidayMapping.Generate(false);
                    string mapFile = "HolidayMapping_HD.xml";
                    using (var writer = XmlWriter.Create(Path.Combine(bundle.GetDirectory(), mapFile), new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
                    {
                        holidayMapping.Save(writer);
                    }
                    bundle.AddAction(new AddAction()
                    {
                        Src = Path.GetFileName(mapFile),
                        Dest = "Mapping\\HolidayMapping.xml",
                        StarFlags = "Domestic_Universe"
                    });

                    XElement holidayBackgrounds = HolidayBackgrounds.Generate(false);
                    string bgFile = "LOT8BackgroundsHolidays_HD.xml";
                    using (var writer = XmlWriter.Create(Path.Combine(bundle.GetDirectory(), bgFile), new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
                    {
                        holidayBackgrounds.Save(writer);
                    }
                    bundle.AddAction(new AddAction()
                    {
                        Src = Path.GetFileName(bgFile),
                        Dest = "Events\\LOT8BackgroundsHolidays.xml",
                        StarFlags = "Domestic_Universe"
                    });
                }

                // SD Holidays
                if (config.HasSD())
                {
                    XElement holidayMapping = HolidayMapping.Generate(true);
                    string mapFile = "HolidayMapping_SD.xml";
                    using (var writer = XmlWriter.Create(Path.Combine(bundle.GetDirectory(), mapFile), new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
                    {
                        holidayMapping.Save(writer);
                    }
                    bundle.AddAction(new AddAction()
                    {
                        Src = Path.GetFileName(mapFile),
                        Dest = "Mapping\\HolidayMapping.xml",
                        StarFlags = "Domestic_SD_Universe"
                    });

                    XElement holidayBackgrounds = HolidayBackgrounds.Generate(true);
                    string bgFile = "LOT8BackgroundsHolidays_SD.xml";
                    using (var writer = XmlWriter.Create(Path.Combine(bundle.GetDirectory(), bgFile), new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
                    {
                        holidayBackgrounds.Save(writer);
                    }
                    bundle.AddAction(new AddAction()
                    {
                        Src = Path.GetFileName(bgFile),
                        Dest = "Events\\LOT8BackgroundsHolidays.xml",
                        StarFlags = "Domestic_SD_Universe"
                    });
                }

                // Send off bundle
                udpSender.Send(bundle.GetMessage());
            }

            /*
             * Connect to MQTT if specified
             */
            if (config.mqtt != null)
            {
                var handler = new MqttHandler(config, udpSender);
                handler.Start();
            }

            /*
             * Update radar in the background
             */
            var radarUpdateTask = new RadarUpdateTask(udpSender);
            //await radarUpdateTask.Update();
            radarUpdateTask.Start();

            var recordUpdateTask = new RecordUpdateTask(udpSender);
            recordUpdateTask.Start();


            //udpSender.sendCommand("loadRunPres(PresentationId=lot8s,Flavor=domesticSD/v,Duration=2700)");

            //udpSender.sendCommand("loadPres(PresentationId=lot8s,Flavor=domestic/azul,Duration=3600)");
            //Thread.Sleep(2000);
            //udpSender.sendCommand(String.Format("runPres(PresentationId=lot8s,StartTime=\"{0}\")", DateTime.Now.ToUniversalTime().AddSeconds(30).ToString("MM/dd/yyyy HH:mm:ss:ff")));

            //udpSender.sendCommand("loadPres(PresentationId=ldl0_0,Flavor=domesticSD/ldlC,Duration=72000,Iteration=1)");
            //Thread.Sleep(4000);
            //udpSender.sendCommand(String.Format("runPres(PresentationId=ldl0_0,StartTime=\"{0}\")", DateTime.Now.ToUniversalTime().AddSeconds(10).ToString("MM/dd/yyyy HH:mm:ss:ff")));

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        async void SendNational(UDPSender udpSender)
        {
            RecordProcessor processor = new RecordProcessor(udpSender);
            processor.AddLocation("USNM0004:1:US");
            processor.AddLocation("USGA0028:1:US");
            processor.AddLocation("USMD0018:1:US");
            processor.AddLocation("USME0017:1:US");
            processor.AddLocation("USMT0031:1:US");
            processor.AddLocation("USAL0054:1:US");
            processor.AddLocation("USND0037:1:US");
            processor.AddLocation("USID0025:1:US");
            processor.AddLocation("USMA0046:1:US");
            processor.AddLocation("USNY0081:1:US");
            processor.AddLocation("USVT0033:1:US");
            processor.AddLocation("USNC0121:1:US");
            processor.AddLocation("USIL0225:1:US");
            processor.AddLocation("USOH0188:1:US");
            processor.AddLocation("USOH0195:1:US");
            processor.AddLocation("USTX0327:1:US");
            processor.AddLocation("USCO0105:1:US");
            processor.AddLocation("USIA0231:1:US");
            processor.AddLocation("USMI0229:1:US");
            processor.AddLocation("USAZ0068:1:US");
            processor.AddLocation("USSC0140:1:US");
            processor.AddLocation("USCT0094:1:US");
            processor.AddLocation("USTX0617:1:US");
            processor.AddLocation("USIN0305:1:US");
            processor.AddLocation("USFL0228:1:US");
            processor.AddLocation("USMO0460:1:US");
            processor.AddLocation("USNV0049:1:US");
            processor.AddLocation("USAR0337:1:US");
            processor.AddLocation("USCA0638:1:US");
            processor.AddLocation("USKY1096:1:US");
            processor.AddLocation("USTN0325:1:US");
            processor.AddLocation("USFL0316:1:US");
            processor.AddLocation("USWI0455:1:US");
            processor.AddLocation("USMN0503:1:US");
            processor.AddLocation("USTN0357:1:US");
            processor.AddLocation("USLA0338:1:US");
            processor.AddLocation("USNY0996:1:US");
            processor.AddLocation("USNJ0355:1:US");
            processor.AddLocation("USVA0557:1:US");
            processor.AddLocation("USOK0400:1:US");
            processor.AddLocation("USNE0363:1:US");
            processor.AddLocation("USFL0372:1:US");
            processor.AddLocation("USPA1276:1:US");
            processor.AddLocation("USAZ0166:1:US");
            processor.AddLocation("USPA1290:1:US");
            processor.AddLocation("USME0328:1:US");
            processor.AddLocation("USOR0275:1:US");
            processor.AddLocation("USNC0558:1:US");
            processor.AddLocation("USSD0283:1:US");
            processor.AddLocation("USNV0076:1:US");
            processor.AddLocation("USCA0967:1:US");
            processor.AddLocation("USUT0225:1:US");
            processor.AddLocation("USTX1200:1:US");
            processor.AddLocation("USCA0982:1:US");
            processor.AddLocation("USCA0987:1:US");
            processor.AddLocation("USWA0395:1:US");
            processor.AddLocation("USWA0422:1:US");
            processor.AddLocation("USMO0787:1:US");
            processor.AddLocation("USFL0481:1:US");
            processor.AddLocation("USOK0537:1:US");
            processor.AddLocation("USDC0001:1:US");
            processor.AddRecordType("MORecord");
            processor.AddRecordType("DFRecord");
            await processor.Send();
        }
    }
}
