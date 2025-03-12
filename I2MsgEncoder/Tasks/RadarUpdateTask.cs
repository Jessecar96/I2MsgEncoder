using I2MsgEncoder.Radar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace I2MsgEncoder.Tasks
{
    class RadarUpdateTask
    {
        private TWCRadarProcessor radarProcessor;
        private TWCSatRadProcessor satRadProcessor;
        private Timer timer;

        private bool currentlyUpdating = false;

        public RadarUpdateTask(UDPSender udpSender)
        {
            radarProcessor = new TWCRadarProcessor(udpSender, "Radar.US");
            satRadProcessor = new TWCSatRadProcessor(udpSender, "SatRad.US");
            timer = new Timer(1 * 60000); // 1 minute
            timer.Elapsed += UpdateRadar;
            timer.AutoReset = true;
        }

        private async void UpdateRadar(object sender, ElapsedEventArgs e)
        {
            // Make sure we don't update 
            if (currentlyUpdating) return;
            currentlyUpdating = true;

            try
            {
                await radarProcessor.SendAllFramesAsync();
            }
            catch (Exception err)
            {
                Log.Warning("Failed to update Radar: {0}", err.Message);
            }

            try
            {
                await satRadProcessor.SendAllFramesAsync();
            }
            catch (Exception err)
            {
                Log.Warning("Failed to update SatRad: {0}", err.Message);
            }

            currentlyUpdating = false;
        }

        public async Task Update()
        {
            await radarProcessor.SendAllFramesAsync();
            await satRadProcessor.SendAllFramesAsync();
        }

        public void Start()
        {
            timer.Start();
        }

    }
}
