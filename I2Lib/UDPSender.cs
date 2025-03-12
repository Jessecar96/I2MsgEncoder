using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using TWC.I2.MsgEncode;
using TWC.I2.MsgEncode.FEC;
using TWC.I2.MsgEncode.ProcessingSteps;
using TWC.Msg;
using System.IO;
using System.Threading;

namespace I2MsgEncoder
{
    public class UDPSender
    {
        private string tempDir = Util.GetTempDir("messages");

        IPAddress ipaddr;
        IPEndPoint ipEndPoint;
        UdpClient udpClient;

        public UDPSender(string destIP, int destPort, string interfaceIP)
        {
            ipaddr = IPAddress.Parse(destIP);
            ipEndPoint = new IPEndPoint(ipaddr, destPort);

            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind((EndPoint)new IPEndPoint(IPAddress.Parse(interfaceIP), 7787));
            udpClient.JoinMulticastGroup(ipaddr, 64);
        }

        public UDPSender(string destIP, int destPort) : this(destIP, destPort, "10.100.102.10")
        {

        }

        public void Send(UDPMessage message)
        {
            if (message.fileName == null)
            {
                SendCommand(message.command);
            }
            else
            {
                SendFile(message.fileName, message.command);
            }
        }

        public void SendCommand(string command, string headendId = null)
        {
            Console.WriteLine("> " + command);
            string tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString());
            File.WriteAllText(tempFile, "");
            SendFile(tempFile, command, false, headendId);
            File.Delete(tempFile);
        }

        public void SendFile(string fileName, string command, bool gzipEncode = true, string headendId = null)
        {
            // tempFile is the file we need to send (data/image/starbundle/etc)
            string tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".i2m");

            // fecTempFile will be the result of the forward-error-correcting encoding process
            string fecTempFile = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".i2m");

            // Copy our input file to the temp location
            File.Copy(fileName, tempFile);

            // We can apply multiple steps to the i2m for things like compression, encryption, and checking headend ids/flags
            List<IMsgEncodeStep> steps = new List<IMsgEncodeStep>();

            // Add the exec command that we need to import this data
            steps.Add(new ExecMsgEncodeStep(command));

            // If needed, gzip encode
            if (gzipEncode)
            {
                steps.Add(new GzipMsgEncoderDecoder());
            }

            // Message to a specific headend id
            if (headendId != null)
            {
                steps.Add(new CheckHeadendIdMsgEncodeStep(headendId));
            }

            // Encoding this file into the i2m format
            MsgEncoder encoder = new MsgEncoder(steps);
            encoder.Encode(tempFile);

            // Add error correction data to send it over UDP, packet lenth is 1405 from DgPacket
            FecEncoder fecEncoder = FecEncoder.Create(FecEncoding.None, (ushort)DgPacket.MAX_PAYLOAD_SIZE, 1, 2);
            Stream inputStream = (Stream)File.OpenRead(tempFile);
            using (Stream outputStream = (Stream)File.OpenWrite(fecTempFile))
            {
                fecEncoder.Encode(inputStream, outputStream);
            }

            // Load our newly created i2m (which is a type of DgMsg from the IS1 days)
            I2Msg m = new I2Msg(fecTempFile);
            m.Id = (uint)Util.GetCurrentUnixTimestampMillis();
            m.Start();
            uint count = m.CalcMsgPacketCount();

            // Loop though the packets we need to send out over the UDP socket
            uint packets = 0;
            while (packets < count)
            {
                byte[] b = m.GetNextPacket();
                udpClient.Send(b, b.Length, ipEndPoint);
                packets++;
                Thread.Sleep(2);
            }

            // Dispose our message and delete any temp files
            m.Dispose();
            inputStream.Close();
            File.Delete(tempFile);
            File.Delete(fecTempFile);
        }
    }
}
