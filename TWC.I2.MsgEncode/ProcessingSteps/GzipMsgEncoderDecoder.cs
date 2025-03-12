// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.ProcessingSteps.GzipMsgEncoderDecoder
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.IO;
using System.IO.Compression;
using System.Xml;
using TWC.Util;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
  public class GzipMsgEncoderDecoder : IMsgEncodeStep, IMsgDecodeStep
  {
    private string workDir;

    public GzipMsgEncoderDecoder()
      : this((string) null)
    {
    }

    public GzipMsgEncoderDecoder(string workDir)
    {
      this.workDir = workDir;
    }

    public string Tag
    {
      get
      {
        return "GzipCompressedMsg";
      }
    }

    public string Encode(string payloadFile, XmlElement descriptor)
    {
      descriptor.SetAttribute("fname", Path.GetFileName(payloadFile));
      string path = string.Format("{0}.{1}", (object) payloadFile, (object) "gz");
      using (Stream readStream = (Stream) File.OpenRead(payloadFile))
      {
        using (Stream writeStream = (Stream) new GZipStream((Stream) File.Open(path, FileMode.Create, FileAccess.Write), CompressionMode.Compress))
          Toolbox.CopyStream(readStream, writeStream);
      }
      return path;
    }

    public string Decode(string payloadFile, XmlElement descriptor)
    {
      string path = this.workDir == null ? Path.GetTempFileName() : Path.Combine(this.workDir, descriptor.GetAttribute("fname"));
      using (Stream readStream = (Stream) new GZipStream((Stream) File.OpenRead(payloadFile), CompressionMode.Decompress))
      {
        using (Stream writeStream = (Stream) File.Open(path, FileMode.Create, FileAccess.Write))
          Toolbox.CopyStream(readStream, writeStream);
      }
      return path;
    }
  }
}
