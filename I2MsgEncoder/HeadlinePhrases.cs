using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace I2MsgEncoder
{
    class HeadlinePhrases
    {
        private static XDocument document;

        private static void load()
        {
            document = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Headline_Event_Phrases.xml"));
        }

        public static string GetVocalCode(string ePhenom, string eSgnfcnc)
        {
            if (document == null)
            {
                load();
            }

            XElement result = document.Root.Descendants("Entry")
                .FirstOrDefault(el => (string)el.Attribute("value") == ePhenom + "_" + eSgnfcnc + ".wav");

            if (result != null)
            {
                return "HE" + result.Attribute("key").Value;
            }

            return null;
        }

    }
}
