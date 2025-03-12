using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I2MsgEncoder
{
    public class UDPMessage
    {
        public string fileName;
        public string command;
        public bool useGzip = true;

        public UDPMessage(string fileName, string command, bool useGzip)
        {
            this.fileName = fileName;
            this.command = command;
            this.useGzip = useGzip;
        }

        public UDPMessage(string fileName, string command)
        {
            this.fileName = fileName;
            this.command = command;
        }

        public UDPMessage(string command)
        {
            this.command = command;
        }

    }
}
