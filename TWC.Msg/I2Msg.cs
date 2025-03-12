// Decompiled with JetBrains decompiler
// Type: TWC.Msg.I2Msg
// Assembly: TWC.Msg, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 1C82769B-8C1B-4E19-B8E1-A64EE0F638EC
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.Msg.dll

using System.IO;
using TWC.Util.DataProvider;

namespace TWC.Msg
{
    public sealed class I2Msg : DgMsg
    {
        public I2Msg(string fname) : this((Stream)File.OpenRead(fname), (byte)3)
        {
        }

        public I2Msg(string fname, byte numCompletionPkts) : this((Stream)File.OpenRead(fname), numCompletionPkts)
        {
        }

        public I2Msg(Stream stream) : this(stream, (byte)0)
        {
        }

        public I2Msg(Stream stream, byte numCompletionPkts) : base(DgPacket.MsgType.I2)
        {
            this.AddSegment(new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider)new StreamDataProvider(stream)));
            for (byte index = 0; (int)index < (int)numCompletionPkts; ++index)
                this.AddSegment(new DgMsg.Segment(DgMsg.Segment.Type.CmdSegment, (IDataProvider)new StringDataProvider("This is a test"), DgPacket.Flags.Completed));
        }
    }
}
