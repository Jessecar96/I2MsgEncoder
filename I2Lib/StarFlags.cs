using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace I2MsgEncoder
{ 
    public class StarFlags
    {
        XDocument doc;

        public StarFlags()
        {
            doc = new XDocument();
            doc.Add(new XElement("StarFlags"));
        }

        public void Add(string flag)
        {
            doc.Root.Add(new XElement("Flag", flag));
        }

        public void Save(string file)
        {
            doc.Save(file);
        }

        public override string ToString()
        {
            return doc.ToString();
        }
    }
}
