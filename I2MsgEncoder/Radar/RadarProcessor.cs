using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImageMagick;
using Newtonsoft.Json;

namespace I2MsgEncoder.Radar
{
    abstract class RadarProcessor
    {
        protected int TILE_SIZE = 256;
        protected int ZOOM_LEVEL = 6;

        protected string sentFramesFile;
        protected UDPSender udpSender;
        protected HttpClient httpClient;
        protected MagickImage fullImg;
        protected string imageType;

        protected string type;
        protected string area;
        protected string mapProduct;

        public RadarProcessor(UDPSender udpSender, string imageType)
        {
            this.udpSender = udpSender;
            this.imageType = imageType;
            this.httpClient = HttpClientHolder.GetClient();
            sentFramesFile = Path.Combine(Util.GetTempDir(), imageType + "-frames.json");

            // Split imageType by .
            string[] parts = imageType.Split('.');
            this.type = parts[0];
            this.area = parts[1];

            // Bug fix
            OpenCL.IsEnabled = false;
        }

        protected void CleanupFiles()
        {
            string dir = Util.GetTempDir(this.imageType);
            string[] files = Directory.GetFiles(dir);
            int numFiles = 0;

            if (files.Length < 50) return;

            // Sort files so we only delete the oldest ones
            Array.Sort(files);

            for (int i = 0; i < files.Length; i++)
            {
                // Ignore non tiff files
                if (!files[i].EndsWith(".tiff")) continue;

                File.Delete(files[i]);
                numFiles++;

                // Once we get to totalFiles - 50 stop deleting
                if (numFiles >= files.Length - 50) break;
            }
        }

        protected ImageBoundries GetBoundries()
        {
            var doc = XDocument.Load(Path.Combine(Util.GetDir(), "Data", "ImageSequenceDefs.xml"));
            var el = doc.Root.Elements()
                .FirstOrDefault(el => el.Attribute("type").Value == this.type && el.Attribute("area").Value == this.area);

            if (el == null)
            {
                Log.Warning("ImageSequenceDef not found for {0}", this.imageType);
                return null;
            }

            return new ImageBoundries
            {
                LowerLeftLong = Convert.ToDouble(el.Element("LowerLeftLong").Value),
                LowerLeftLat = Convert.ToDouble(el.Element("LowerLeftLat").Value),
                UpperRightLon = Convert.ToDouble(el.Element("UpperRightLong").Value),
                UpperRightLat = Convert.ToDouble(el.Element("UpperRightLat").Value),
                VerticalAdjustment = Convert.ToDouble(el.Element("VerticalAdjustment").Value),
                OriginalImageWidth = Convert.ToInt32(el.Element("OriginalImageWidth").Value),
                OriginalImageHeight = Convert.ToInt32(el.Element("OriginalImageHeight").Value),
                ImagesInterval = Convert.ToInt32(el.Element("ImagesInterval").Value),
                Expiration = Convert.ToInt32(el.Element("Expiration").Value)
            };
        }

        public async Task SendLatestFrameAsync()
        {
            RadarFrame frame = await GetLatestFrameAsync();
            if (frame == null) return;

            udpSender.Send(frame.GetMessage());
            AddSentFrame(frame.timestamp);
        }

        public async Task SendAllFramesAsync()
        {
            List<int> times = await GetValidTimestampsAsync();
            foreach (int time in times)
            {
                if (IsFrameSent(time)) continue;

                var frame = await GetFrameAsync(time);
                if (frame == null) continue;

                Log.Info("Sending {0}.{1} frame {2}", frame.imageType, frame.location, frame.GetTimeStr());

                udpSender.Send(frame.GetMessage());
                AddSentFrame(frame.timestamp);
            }
        }

        public abstract Task<RadarFrame> GetFrameAsync(int timestamp);

        public abstract Task<RadarFrame> GetLatestFrameAsync();

        public abstract Task<List<RadarFrame>> GetAllFramesAsync();

        protected async Task<List<int>> GetValidTimestampsAsync()
        {
            try
            {
                var httpResponse = await httpClient.GetAsync("https://api.weather.com/v3/TileServer/series/productSet?apiKey=d522aa97197fd864d36b418f39ebb323&filter=" + this.mapProduct);
                httpResponse.EnsureSuccessStatusCode();
                string responseBody = await httpResponse.Content.ReadAsStringAsync();

                var boundries = this.GetBoundries();
                if (boundries == null)
                {
                    return new List<int>();
                }

                dynamic json = JsonConvert.DeserializeObject(responseBody);
                List<int> times = new List<int>();
                foreach (dynamic series in json.seriesInfo[this.mapProduct].series)
                {
                    int time = Int32.Parse(series.ts.ToString());

                    // Ignore frames older than the expiration
                    if (time < Util.GetCurrentUnixTimestampMillis() / 1000 - boundries.Expiration)
                        continue;

                    // Ignore frames not at the correct interval
                    if (time % boundries.ImagesInterval != 0)
                        continue;

                    times.Add(time);
                }

                return times;
            }
            catch (HttpRequestException e)
            {
                Log.Warning("Failed downloading valid radar timestamps.");
                return new List<int>();
            }
        }

        protected bool IsFrameSent(int timestamp)
        {
            List<int> frames = GetSentFrames();
            return frames.Contains(timestamp);
        }

        private List<int> GetSentFrames()
        {
            // If the file doesn't exist then we've never sent any frames
            if (!File.Exists(sentFramesFile))
            {
                return new List<int>();
            }

            try
            {
                // Try to parse as JSON
                dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(sentFramesFile));
                List<int> times = new List<int>();

                foreach (int time in json)
                {
                    times.Add(time);
                }

                return times;
            }
            catch (Exception e)
            {
                return new List<int>();
            }
        }

        private void AddSentFrame(int timestamp)
        {
            List<int> current = GetSentFrames();
            current.Sort();
            if (current.Count() - 50 > 0)
            {
                current.RemoveRange(0, current.Count() - 50);
            }
            current.Add(timestamp);
            File.WriteAllText(sentFramesFile, JsonConvert.SerializeObject(current));

            CleanupFiles();
        }

        protected double upperLeftX, upperLeftY, lowerRightX, lowerRightY = 0;
        protected int xStart, xEnd, yStart, yEnd = 0;
        protected int imgW, imgH = 0;

        protected void CalculateBounds(LatLng upperRight, LatLng lowerLeft, LatLng upperLeft, LatLng lowerRight)
        {
            /*
             * Now we need to find the tile coordinates that the 4 corners of the map lie in.
             * First the Lat/Lon need to be converted to "World Coordinates" used by the Mercator projection.
             */
            Point upperRightTile = WorldCoordinateToTile(LatLonProject(upperRight));
            Point lowerLeftTile = WorldCoordinateToTile(LatLonProject(lowerLeft));
            Point upperLeftTile = WorldCoordinateToTile(LatLonProject(upperLeft));
            Point lowerRightTile = WorldCoordinateToTile(LatLonProject(lowerRight));

            /*
             * Now find the pixel values of the radar boundries that the I2 expects.
             */
            Point upperLeftPx = WorldCoordinateToPixels(LatLonProject(upperLeft));
            Point lowerRightPx = WorldCoordinateToPixels(LatLonProject(lowerRight));

            /*
             * Now find the position of the 4 corners within the tiles that we're downloading.
             */
            upperLeftX = upperLeftPx.x - upperLeftTile.x * TILE_SIZE;
            upperLeftY = upperLeftPx.y - upperLeftTile.y * TILE_SIZE;
            lowerRightX = lowerRightPx.x - upperLeftTile.x * TILE_SIZE;
            lowerRightY = lowerRightPx.y - upperLeftTile.y * TILE_SIZE;

            /*
             * These are the boundires of the tile coordinates that we'll be downloading.
             */
            xStart = (int)upperLeftTile.x;
            xEnd = (int)upperRightTile.x;
            yStart = (int)upperLeftTile.y;
            yEnd = (int)lowerLeftTile.y;

            /*
             * Number of tiles to download in each axis.
             */
            int xtiles = xEnd - xStart;
            int ytiles = yEnd - yStart;

            /*
             * Size of the image of all the tiles we're downloading.
             */
            imgW = TILE_SIZE * (xtiles + 1);
            imgH = TILE_SIZE * (ytiles + 1);
        }

        /**
        * Resize the output to what the I2 expects.
        * This produces some strecting issues since it's not a perfect integer scale, but it's decent enough.
        */
        protected void ResizeImage(int width, int height)
        {
            /*
             * Use nearest neighbor for scaling.
             * The I2 expects sharp images with exact color values. Smoothing is bad!
             */
            fullImg.Interpolate = PixelInterpolateMethod.Nearest;
            fullImg.FilterType = FilterType.Point;

            /*
             * The I2 needs TIFF files, and we use LZW compression so the files aren't 30MB.
             */
            fullImg.Format = MagickFormat.Tiff;
            fullImg.Settings.Compression = CompressionMethod.LZW;

            MagickGeometry size = new MagickGeometry();
            size.IgnoreAspectRatio = true;
            size.Width = width;
            size.Height = height;
            fullImg.Resize(size);
        }

        /*
         * Gets "World Coordinate" from Lat/Long
         * Source: https://developers.google.com/maps/documentation/javascript/examples/map-coordinates
         */
        protected Point LatLonProject(double lat, double lng)
        {
            double siny = Math.Sin(lat * Math.PI / 180);

            // Truncating to 0.9999 effectively limits latitude to 89.189. This is
            // about a third of a tile past the edge of the world tile.
            siny = Math.Min(Math.Max(siny, -0.9999), 0.9999);

            return new Point
            {
                x = TILE_SIZE * (0.5 + lng / 360),
                y = TILE_SIZE * (0.5 - Math.Log((1 + siny) / (1 - siny)) / (4 * Math.PI))
            };
        }

        protected Point LatLonProject(LatLng point)
        {
            return LatLonProject(point.lat, point.lng);
        }

        /*
         * Gets the tile coordinates from projected coordinates.
         * Source: https://developers.google.com/maps/documentation/javascript/examples/map-coordinates
         */
        protected Point WorldCoordinateToTile(Point coord)
        {
            var scale = 1 << ZOOM_LEVEL;
            return new Point
            {
                x = Math.Floor(coord.x * scale / TILE_SIZE),
                y = Math.Floor(coord.y * scale / TILE_SIZE)
            };
        }

        /*
         * Gets pixel coordinates from projected coordinates.
         * Source: https://developers.google.com/maps/documentation/javascript/examples/map-coordinates
         */
        protected Point WorldCoordinateToPixels(Point coord)
        {
            var scale = 1 << ZOOM_LEVEL;
            return new Point
            {
                x = Math.Floor(coord.x * scale),
                y = Math.Floor(coord.y * scale)
            };
        }

        protected class Point
        {
            public double x;
            public double y;
        }

        protected class LatLng
        {
            public double lat;
            public double lng;
        }

        protected class ImageBoundries
        {
            public double LowerLeftLong { get; set; }
            public double LowerLeftLat { get; set; }
            public double UpperRightLon { get; set; }
            public double UpperRightLat { get; set; }
            public double VerticalAdjustment { get; set; }
            public int OriginalImageWidth { get; set; }
            public int OriginalImageHeight { get; set; }
            public int ImagesInterval { get; set; }
            public int Expiration { get; set; }

            public LatLng GetUpperRight()
            {
                return new LatLng { lat = this.UpperRightLat, lng = this.UpperRightLon };
            }

            public LatLng GetLowerLeft()
            {
                return new LatLng { lat = this.LowerLeftLat, lng = this.LowerLeftLong };
            }

            public LatLng GetUpperLeft()
            {
                return new LatLng { lat = this.UpperRightLat, lng = this.LowerLeftLong };
            }

            public LatLng GetLowerRight()
            {
                return new LatLng { lat = this.LowerLeftLat, lng = this.UpperRightLon };
            }

        }
    }

    public class RadarFrame
    {
        public string imageType;
        public string location;
        public string fileName;
        public string filePath;
        public int timestamp;

        private string GetTime()
        {
            DateTime time = Util.UnixTimeStampToDateTimeUTC(timestamp);
            return time.ToString(@"MM\/dd\/yyyy HH:mm:ss");
        }

        public string GetTimeStr()
        {
            return Util.UnixToDateTime(timestamp).ToString(@"MM\/dd\/yyyy hh:mm tt"); ;
        }

        public UDPMessage GetMessage()
        {
            return new UDPMessage(filePath, "storeImage(ImageType=" + imageType + ",Location=" + location + ",FileExtension=.tiff,IssueTime=\"" + GetTime() + "\")", false);
        }
    }
}
