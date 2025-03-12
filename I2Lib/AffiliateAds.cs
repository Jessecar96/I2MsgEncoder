using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TWC.SE.StarBundle;

namespace I2MsgEncoder
{
    public class AffiliateAds
    {
        XDocument doc;

        public AffiliateAds()
        {
            doc = new XDocument();
            doc.Add(new XElement("Events"));
        }

        public void AddCrawl(string message)
        {
            doc.Root.Add(new XElement("Event", new XElement("Text", message)));
        }

        public void Save(string file)
        {
            doc.Save(file);
        }
    }
}
