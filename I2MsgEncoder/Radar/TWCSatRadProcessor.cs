using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace I2MsgEncoder.Radar
{
    class TWCSatRadProcessor : RadarProcessor
    {

        public TWCSatRadProcessor(UDPSender udpSender, string imageType) : base(udpSender, imageType)
        {
            this.mapProduct = "satrad";
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
            string url = String.Format("https://api.weather.com/v3/TileServer/tile?product=satrad&ts={0}&xyz={1}:{2}:{3}&apiKey=d522aa97197fd864d36b418f39ebb323", timestamp, x, y, ZOOM_LEVEL);

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
