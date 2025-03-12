using I2MsgEncoder.Config;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace I2MsgEncoder
{
    class MqttHandler
    {
        private I2MsgEncoder.Config.Config config;
        private UDPSender udpSender;

        public MqttHandler(I2MsgEncoder.Config.Config config, UDPSender udpSender)
        {
            this.config = config;
            this.udpSender = udpSender;
        }

        async public Task Start()
        {
            // Connect to server
            string clientId = String.Format("I2MsgEncoder-{0}", Guid.NewGuid().ToString());
            string ip;
            int port = 1883;

            string[] server = config.mqtt.server.Split(':');
            if (server[0] != null && server[1] != null)
            {
                ip = server[0];
                port = int.Parse(server[1]);
            }
            else
            {
                ip = config.mqtt.server;
            }

            var clientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(ip, port);

            // Add user/pass if we have it
            if (config.mqtt.username != null && config.mqtt.password != null)
            {
                clientOptionsBuilder.WithCredentials(config.mqtt.username, config.mqtt.password);
            }

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(clientOptionsBuilder.Build())
                .Build();

            var client = new MqttFactory().CreateManagedMqttClient();

            // Build list of topics to subscribe to
            List<string> topics = new List<string>();
            foreach (var item in config.mqtt.events)
            {
                if (topics.Contains(item.topic)) continue;
                topics.Add(item.topic);
            }

            topics.Add("i2/exec/normal");
            topics.Add("i2/exec/priority");

            // Subscribe to exec topic for each star id
            foreach (var star in config.stars)
            {
                topics.Add("i2/exec/" + star.headendId);
            }

            foreach (var topic in topics)
            {
                Log.Debug("Subscribing to topic {0}", topic);
                await client.SubscribeAsync(topic);
            }

            client.UseConnectedHandler(this.ConnectedHandler);
            client.UseDisconnectedHandler(this.DisconnectedHandler);
            client.UseApplicationMessageReceivedHandler(this.MessageReceivedHandler);

            await client.StartAsync(options);
        }

        private void ConnectedHandler(MqttClientConnectedEventArgs args)
        {
            Log.Info("Connected to MQTT broker {0}", args.AuthenticateResult.ReasonString);
        }

        private void DisconnectedHandler(MqttClientDisconnectedEventArgs args)
        {
            Log.Warning("Disconnected from MQTT broker");
        }

        private void MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                string topic = args.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

                Log.Debug("Received MQTT Message - topic: {0} payload: {1}", topic, payload);

                // Handle overall exec topics
                if (topic == "i2/exec/normal" || topic == "i2/exec/priority")
                {
                    HandleExec(payload);
                    return;
                }

                // Handle star id specific exec topics
                Match m = Regex.Match(topic, @"^i2\/exec\/(.+)$", RegexOptions.IgnoreCase);
                if (m.Success && m.Groups.Count > 1)
                {
                    HandleExec(payload, m.Groups[1].Value);
                    return;
                }

                // Loop over events to see if any match
                foreach (var item in config.mqtt.events)
                {
                    if (item.topic != topic || (item.payload != null && item.payload != payload))
                        continue;

                    // Run this in a new thread
                    var bw = new BackgroundWorker();
                    bw.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        TriggerEvent(item);
                    });

                    bw.RunWorkerAsync();

                    break; // Only match one task
                }

            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
            }
        }

        private void HandleExec(string command, string headendId = null)
        {
            // Replacement magic
            bool PresMatch = Regex.IsMatch(command, @"(?:loadPres|runPres|cancelPres)\(");
            if (PresMatch)
            {
                if (!this.config.mqtt.enablePresentations)
                    return;

                string TimeRegex = @"StartTime=(.*?)(\)|,)";
                Match m = Regex.Match(command, TimeRegex);
                if (m.Success && m.Groups.Count > 1)
                {
                    string ExtractedTime = m.Groups[1].Value;
                    DateTime dt = DateTime.ParseExact(ExtractedTime, @"MM\/dd\/yyyy HH:mm:ss:ff", new CultureInfo("en-US"));
                    dt = dt.AddMilliseconds(this.config.mqtt.presentationOffset);

                    string OutputTime = string.Format("StartTime={0}", dt.ToString(@"MM\/dd\/yyyy HH:mm:ss:ff"));
                    command = Regex.Replace(command, TimeRegex, OutputTime + "$2");
                }
            }

            bool HeartbeatMatch = Regex.IsMatch(command, @"heartbeat\(");
            if (HeartbeatMatch && !this.config.mqtt.enableHeartbeats)
                return;

            udpSender.SendCommand(command, headendId);
        }

        private void TriggerEvent(Event item)
        {
            List<string> presentationIds = new List<string>();
            List<int> presentationDurations = new List<int>();

            foreach (var pres in item.presentations)
            {
                // Load our presentations
                string id = pres.id ?? Guid.NewGuid().ToString();
                var args = new List<string>();
                args.Add("PresentationId=" + id);
                args.Add("Flavor=" + pres.flavor);
                args.Add("Duration=" + pres.duration);
                if (!string.IsNullOrEmpty(pres.logo))
                {
                    args.Add("Logo=" + pres.logo);
                }
                udpSender.SendCommand(String.Format("loadPres({0})", string.Join(",", args.ToArray())));

                presentationIds.Add(id);
                presentationDurations.Add(pres.duration);
            }

            // Find the max presentation duration for when to run the post presentations
            int presDuration = presentationDurations.Max();

            // Wait the specified amount of time
            DateTime runTime = DateTime.Now.ToUniversalTime().AddMilliseconds(item.offset);

            // Wait half the offset before sending the run command
            int waitTime = item.offset / 2;
            Thread.Sleep(waitTime);

            // Find our cancel time
            DateTime cancelTime = runTime.AddMilliseconds(-1 * item.cancelOffset);

            // Cancel any existing ones
            if (item.cancelPresentations != null)
            {
                foreach (var id in item.cancelPresentations)
                {
                    udpSender.SendCommand(String.Format("cancelPres(PresentationId={0},StartTime=\"{1}\")", id, cancelTime.ToString(@"MM\/dd\/yyyy HH:mm:ss:ff")));
                }
            }

            // Run our presentation(s)
            foreach (var id in presentationIds)
            {
                udpSender.SendCommand(String.Format("runPres(PresentationId={0},StartTime=\"{1}\")", id, runTime.ToString(@"MM\/dd\/yyyy HH:mm:ss:ff")));
            }

            // If there's no post presentations, that's all we need to do here
            if (item.postPresentations == null || item.postPresentations.Count == 0)
                return;

            // Reuse this array for post presentations
            presentationIds.Clear();

            // Calculate time to ending
            double presRunSeconds = presDuration / 30;
            DateTime presEndTime = runTime.AddSeconds(presRunSeconds);

            // Wait until current presentation is half over before loading the next ones
            Thread.Sleep((int)(presRunSeconds / 2) * 1000);

            // Load post presentations
            foreach (var pres in item.postPresentations)
            {
                string id = pres.id ?? Guid.NewGuid().ToString();
                udpSender.SendCommand(String.Format("loadPres(PresentationId={0},Flavor={1},Duration={2})", id, pres.flavor, pres.duration));
                presDuration = pres.duration;
                presentationIds.Add(id);
            }

            // Wait again for 1/4 of the presentation run time (puts us at 3/4 of the way through)
            Thread.Sleep((int)(presRunSeconds / 4) * 1000);

            // Send run commands for post presentations
            foreach (var id in presentationIds)
            {
                udpSender.SendCommand(String.Format("runPres(PresentationId={0},StartTime=\"{1}\")", id, presEndTime.ToString(@"MM\/dd\/yyyy HH:mm:ss:ff")));
            }
        }
    }
}