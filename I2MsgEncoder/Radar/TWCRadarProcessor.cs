using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace I2MsgEncoder.Radar
{
    class TWCRadarProcessor : RadarProcessor
    {

        public TWCRadarProcessor(UDPSender udpSender, string imageType) : base(udpSender, imageType)
        {
            this.mapProduct = "twcRadarMosaic";
        }

        public override async Task<List<RadarFrame>> GetAllFramesAsync()
        {
            List<int> times = await GetValidTimestampsAsync();
            List<Task> tasks = new List<Task>();
            List<RadarFrame> frames = new List<RadarFrame>();

            foreach (int time in times)
            {
                var frame = await GetFrameAsync(time);
                frames.Add(frame);
            }

            return frames;
        }

        public override async Task<RadarFrame> GetLatestFrameAsync()
        {
            List<int> times = await GetValidTimestampsAsync();
            int latest = times.Max();
            return await GetFrameAsync(latest);
        }

        public override async Task<RadarFrame> GetFrameAsync(int timestamp)
        {
            string timeStr = Util.UnixToDateTime(timestamp).ToString(@"MM\/dd\/yyyy hh:mm tt");
            Log.Debug("Downloading {0} frame {1}", this.imageType, timeStr);
            
            string fileName = string.Format("{0}.tiff", timestamp);
            string filePath = Path.Combine(Util.GetTempDir(this.imageType), fileName);

            // If this frame already exists just use it as is
            if (File.Exists(filePath))
            {
                return new RadarFrame()
                {
                    timestamp = timestamp,
                    fileName = fileName,
                    filePath = filePath,
                    imageType = this.type,
                    location = this.area
                };
            }

            /*
             * Lookup boundries
             */
            var boundries = this.GetBoundries();
            LatLng upperRight = boundries.GetUpperRight();
            LatLng lowerLeft = boundries.GetLowerLeft();
            LatLng upperLeft = boundries.GetUpperLeft();
            LatLng lowerRight = boundries.GetLowerRight();

            // Calculate bounds for this frame
            CalculateBounds(upperRight, lowerLeft, upperLeft, lowerRight);

            // Initalize new image
            fullImg = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), imgW, imgH);

            var fileTasks = new List<Task>();
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int x = xStart; x <= xEnd; x++)
                {
                    fileTasks.Add(downloadTileAsync(timestamp, x, y));
                }
            }

            try
            {
                // Download all tiles
                await Task.WhenAll(fileTasks);
            }
            catch (Exception e)
            {
                // This will catch when any of the tiles fails to download
                Log.Warning("Frame download failed, Skipping Frame.");
                Log.Warning(e.Message);
                return null;
            }

            /*
             * Crop the full image of tiles to the boundries that the I2 expects.
             * Using the values that we calculated earlier.
             */
            fullImg.Crop(new MagickGeometry()
            {
                X = (int)upperLeftX,
                Y = (int)upperLeftY,
                Width = (int)(lowerRightX - upperLeftX),
                Height = (int)(lowerRightY - upperLeftY)
            });

            /*
             * WxPro color palette.
             * This is what the I2 expects its radar images to be composed of.
             * Source: i2\Managed\Config\PaletteConfig.xml
             */
            MagickColor[] rainColors = new MagickColor[]
            {
                MagickColor.FromRgb(64,204,85), // lightest
                MagickColor.FromRgb(0,153,0),
                MagickColor.FromRgb(0,102,0),
                MagickColor.FromRgb(191,204,85), // yellow
                MagickColor.FromRgb(191,153,0), // dark yellow
                MagickColor.FromRgb(255,51,0), // red
                MagickColor.FromRgb(191,51,0),
                MagickColor.FromRgb(128,0,0),
                MagickColor.FromRgb(64,0,0), // darkest
            };

            MagickColor[] snowColors = new MagickColor[]
            {
                MagickColor.FromRgb(150,150,150), // dark gray
                MagickColor.FromRgb(180,180,180), // light gray
                MagickColor.FromRgb(210,210,210), // gray
                MagickColor.FromRgb(230,230,230)  // white
            };

            MagickColor[] mixColors = new MagickColor[]
            {
                MagickColor.FromRgb(235,130,215), // light purple
                MagickColor.FromRgb(208,94,176),  // ..
                MagickColor.FromRgb(190,70,150),  // ..
                MagickColor.FromRgb(170,50,130)   // dark purple
            };

            MagickColor[] testColors = new MagickColor[]
            {
                MagickColor.FromRgb(50,50,50), // light purple
                MagickColor.FromRgb(100,100,100),  // ..
                MagickColor.FromRgb(150,150,150),  // ..
                MagickColor.FromRgb(200,200,200)   // dark purple
            };

            // Very dark green
            fullImg.ColorFuzz = new Percentage(4);
            fullImg.Opaque(MagickColor.FromRgb(0, 63, 0), rainColors[2]);

            // lighter greens
            fullImg.ColorFuzz = new Percentage(7);
            fullImg.Opaque(MagickColor.FromRgb(99, 239, 99), rainColors[0]);
            fullImg.Opaque(MagickColor.FromRgb(60, 199, 60), rainColors[1]);
            fullImg.Opaque(MagickColor.FromRgb(28, 158, 52), rainColors[1]);
            fullImg.Opaque(MagickColor.FromRgb(14, 104, 26), rainColors[2]);

            fullImg.ColorFuzz = new Percentage(8);

            // Yellow
            fullImg.Opaque(MagickColor.FromRgb(251, 235, 2), rainColors[3]);
            fullImg.Opaque(MagickColor.FromRgb(248, 210, 2), rainColors[3]);
            fullImg.Opaque(MagickColor.FromRgb(243, 159, 2), rainColors[3]);

            // Darker yellow
            fullImg.Opaque(MagickColor.FromRgb(238, 109, 2), rainColors[4]);
            fullImg.Opaque(MagickColor.FromRgb(232, 89, 3), rainColors[4]);
            fullImg.Opaque(MagickColor.FromRgb(227, 70, 40), rainColors[4]);

            // Reds
            fullImg.ColorFuzz = new Percentage(1);
            fullImg.Opaque(MagickColor.FromRgb(210, 11, 6), rainColors[5]); // REALLY GOOD

            fullImg.ColorFuzz = new Percentage(1);
            fullImg.Opaque(MagickColor.FromRgb(189, 8, 4), rainColors[5]); // almost none

            fullImg.ColorFuzz = new Percentage(1);
            fullImg.Opaque(MagickColor.FromRgb(189, 8, 4), rainColors[6]); // almost none

            // Really dark reds
            fullImg.ColorFuzz = new Percentage(4);
            fullImg.Opaque(MagickColor.FromRgb(200, 200, 200), rainColors[6]);
            fullImg.Opaque(MagickColor.FromRgb(216, 31, 5), rainColors[6]);
            fullImg.Opaque(MagickColor.FromRgb(221, 50, 4), rainColors[6]);
            fullImg.Opaque(MagickColor.FromRgb(202, 10, 5), rainColors[6]);
            fullImg.Opaque(MagickColor.FromRgb(181, 7, 4), rainColors[6]);

            // rainColors[7] and 8 aren't used, idk if they should be

            // ================
            // Snow - 4 levels
            // ================

            // Lightest
            fullImg.ColorFuzz = new Percentage(6);
            fullImg.Opaque(MagickColor.FromRgb(138, 245, 255), snowColors[0]);

            fullImg.ColorFuzz = new Percentage(7);
            fullImg.Opaque(MagickColor.FromRgb(124, 225, 233), snowColors[1]);

            fullImg.ColorFuzz = new Percentage(7);
            fullImg.Opaque(MagickColor.FromRgb(107, 199, 208), snowColors[2]);

            // Darkest
            fullImg.ColorFuzz = new Percentage(8);
            fullImg.Opaque(MagickColor.FromRgb(93, 177, 187), snowColors[3]);
            fullImg.Opaque(MagickColor.FromRgb(74, 146, 157), snowColors[3]);
            fullImg.Opaque(MagickColor.FromRgb(54, 115, 127), snowColors[3]);

            // ================
            // Ice - 4 levels
            // ================

            fullImg.ColorFuzz = new Percentage(3);
            fullImg.Opaque(MagickColor.FromRgb(247, 150, 198), mixColors[0]);
            fullImg.Opaque(MagickColor.FromRgb(255, 160, 207), mixColors[0]);

            fullImg.ColorFuzz = new Percentage(3);
            fullImg.Opaque(MagickColor.FromRgb(235, 134, 184), mixColors[1]);
            fullImg.Opaque(MagickColor.FromRgb(214, 106, 159), mixColors[1]);

            fullImg.ColorFuzz = new Percentage(3);
            fullImg.Opaque(MagickColor.FromRgb(223, 118, 170), mixColors[2]);

            // ================
            // Mix - 4 levels
            // ================

            fullImg.ColorFuzz = new Percentage(3);
            fullImg.Opaque(MagickColor.FromRgb(188, 165, 240), mixColors[0]);

            fullImg.ColorFuzz = new Percentage(3);
            fullImg.Opaque(MagickColor.FromRgb(177, 153, 229), mixColors[1]);

            fullImg.ColorFuzz = new Percentage(3);
            fullImg.Opaque(MagickColor.FromRgb(166, 142, 219), mixColors[2]);

            fullImg.ColorFuzz = new Percentage(5);
            fullImg.Opaque(MagickColor.FromRgb(150, 125, 204), mixColors[3]);

            // Resize image
            ResizeImage(boundries.OriginalImageWidth, boundries.OriginalImageHeight);

            fullImg.Write(filePath);
            fullImg.Dispose();

            return new RadarFrame()
            {
                timestamp = timestamp,
                fileName = fileName,
                filePath = filePath,
                imageType = this.type,
                location = this.area
            };
        }

        private async Task downloadTileAsync(int timestamp, int x, int y)
        {
            string url = String.Format("https://api.weather.com/v3/TileServer/tile?product=twcRadarMosaic&ts={0}&xyz={1}:{2}:{3}&apiKey=d522aa97197fd864d36b418f39ebb323", timestamp, x, y, ZOOM_LEVEL);

            // Find the position to place this tile
            int xPos = (x - xStart) * TILE_SIZE;
            int yPos = (y - yStart) * TILE_SIZE;

            HttpResponseMessage response = null;
            response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Tile was downloaded successfully, add it to our image
            var tileImg = new MagickImage(await response.Content.ReadAsStreamAsync());
            fullImg.Composite(tileImg, xPos, yPos, CompositeOperator.Over);
            tileImg.Dispose();
        }
    }
}
