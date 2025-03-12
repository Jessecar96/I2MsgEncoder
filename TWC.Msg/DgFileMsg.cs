// Decompiled with JetBrains decompiler
// Type: TWC.Msg.DgFileMsg
// Assembly: TWC.Msg, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 1C82769B-8C1B-4E19-B8E1-A64EE0F638EC
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.Msg.dll

using System.IO;
using TWC.Util.DataProvider;

namespace TWC.Msg
{
  public class DgFileMsg : DgMsg
  {
    private DgMsg.Segment interestSeg = new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider) new NullDataProvider());
    private DgMsg.Segment preSeg = new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider) new NullDataProvider());
    private DgMsg.Segment fileSeg = new DgMsg.Segment(DgMsg.Segment.Type.FileSegment, (IDataProvider) new NullDataProvider());
    private DgMsg.Segment postSeg = new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider) new NullDataProvider());

    public DgFileMsg()
      : base(DgPacket.MsgType.File)
    {
      this.AddSegment(this.interestSeg);
      this.AddSegment(this.preSeg);
      this.AddSegment(this.fileSeg);
      this.AddSegment(this.postSeg);
    }

    public string DestinationFileName
    {
      get
      {
        return this.fileSeg.fileName;
      }
      set
      {
        this.fileSeg.fileName = value;
      }
    }

    public void AddSegment(DgFileMsg.SegmentId segId, IDataProvider provider, DgPacket.Compression compressionType)
    {
      switch (segId)
      {
        case DgFileMsg.SegmentId.Interest:
          this.interestSeg.provider = provider;
          this.interestSeg.compressionType = compressionType;
          break;
        case DgFileMsg.SegmentId.Pre:
          this.preSeg.provider = provider;
          this.preSeg.compressionType = compressionType;
          break;
        case DgFileMsg.SegmentId.File:
          this.fileSeg.provider = provider;
          this.fileSeg.compressionType = compressionType;
          break;
        case DgFileMsg.SegmentId.Post:
          this.postSeg.provider = provider;
          this.postSeg.compressionType = compressionType;
          break;
      }
    }

    public void AddSegment(DgFileMsg.SegmentId segId, string s)
    {
      this.AddSegment(segId, (IDataProvider) new StringDataProvider(s), DgPacket.Compression.None);
    }

    public void AddSegment(DgFileMsg.SegmentId segId, Stream stream)
    {
      this.AddSegment(segId, stream, DgPacket.Compression.None);
    }

    public void AddSegment(DgFileMsg.SegmentId segId, Stream stream, DgPacket.Compression compressionType)
    {
      switch (compressionType)
      {
        case DgPacket.Compression.None:
          this.AddSegment(segId, (IDataProvider) new StreamDataProvider(stream), compressionType);
          break;
        case DgPacket.Compression.GZip:
          this.AddSegment(segId, (IDataProvider) new GZipDataProvider(stream), compressionType);
          break;
      }
    }

    public void AddSegmentFromUri(DgFileMsg.SegmentId segId, string uri)
    {
      this.AddSegmentFromUri(segId, uri, DgPacket.Compression.None);
    }

    public void AddSegmentFromUri(DgFileMsg.SegmentId segId, string uri, DgPacket.Compression compressionType)
    {
      switch (compressionType)
      {
        case DgPacket.Compression.None:
          this.AddSegment(segId, (IDataProvider) StreamDataProvider.MakeFromUri(uri), compressionType);
          break;
        case DgPacket.Compression.GZip:
          this.AddSegment(segId, (IDataProvider) GZipDataProvider.MakeFromUri(uri), compressionType);
          break;
      }
    }

    public enum SegmentId
    {
      Interest,
      Pre,
      File,
      Post,
    }
  }
}
