// Decompiled with JetBrains decompiler
// Type: TWC.Msg.DgCmdMsg
// Assembly: TWC.Msg, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 1C82769B-8C1B-4E19-B8E1-A64EE0F638EC
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.Msg.dll

using System.IO;
using TWC.Util.DataProvider;

namespace TWC.Msg
{
  public class DgCmdMsg : DgMsg
  {
    public DgCmdMsg()
      : base(DgPacket.MsgType.Command)
    {
    }

    public DgCmdMsg(string[] segs)
      : base(DgPacket.MsgType.Command)
    {
      foreach (string seg in segs)
        this.AddSegment(seg);
    }

    public DgCmdMsg(IDataProvider[] segs)
      : base(DgPacket.MsgType.Command)
    {
      foreach (IDataProvider seg in segs)
        this.AddSegment(seg);
    }

    public void AddSegment(IDataProvider dp)
    {
      this.AddSegment(new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, dp));
    }

    public void AddSegment(string str)
    {
      this.AddSegment((IDataProvider) new StringDataProvider(str));
    }

    public void AddSegment(Stream stream)
    {
      this.AddSegment((IDataProvider) new StreamDataProvider(stream));
    }

    public void AddSegmentFromUri(string uri)
    {
      this.AddSegment((IDataProvider) StreamDataProvider.MakeFromUri(uri));
    }
  }
}
