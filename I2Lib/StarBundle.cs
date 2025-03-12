using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TWC.SE.StarBundle;
using System.IO.Compression;

namespace I2MsgEncoder
{
    public class StarBundle : IDisposable
    {
        string tempDir;
        string manifestDir;
        string fileName;
        StarBundleInfo manifest;

        public StarBundle(string bundleName)
        {
            this.fileName = Path.Combine(Util.GetTempDir(), bundleName + ".zip");
            tempDir = Util.GetTempDir("StarBundle-" + Util.GetTempFileName());
            manifestDir = Path.Combine(tempDir, "MetaData");
            if (!Directory.Exists(manifestDir))
            {
                Directory.CreateDirectory(manifestDir);
            }

            manifest = new StarBundleInfo();
            manifest.Version = 700000000000000000 + (ulong)Util.GetCurrentUnixTimestampMillis();
            manifest.Type = "Managed";
            manifest.FileActions = new List<FileAction>();
        }

        public string GetDirectory()
        {
            return tempDir;
        }

        public void AddAction(FileAction action)
        {
            manifest.FileActions.Add(action);
        }

        public string Save()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            manifest.GetManifestDoc().Save(Path.Combine(manifestDir, "manifest.xml"));

            ZipFile.CreateFromDirectory(tempDir, fileName);

            return fileName;
        }

        public void Dispose()
        {
            Directory.Delete(tempDir, true);
        }

        public UDPMessage GetMessage()
        {
            this.Save();
            this.Dispose();
            return new UDPMessage(fileName, "stageStarBundle()", false);
        }
    }
}
