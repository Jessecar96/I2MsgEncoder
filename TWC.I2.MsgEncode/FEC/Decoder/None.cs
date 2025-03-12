// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.FEC.Decoder.None
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.IO;

namespace TWC.I2.MsgEncode.FEC.Decoder
{
  internal sealed class None : FecDecoder
  {
    public None(Stream inputStream, ushort packetSize, uint filesize)
      : base(inputStream, packetSize, filesize)
    {
      this.AllocateBuffers(1);
    }

    public override bool Decode(Stream outputStream)
    {
      outputStream.SetLength(0L);
      uint decodeSize = this.Filesize;
      uint num = (uint) ((int) decodeSize + (int) this.DataSize - 1) / (uint) this.DataSize;
      this.PacketsRead = 0U;
      while (num > 0U)
      {
        if ((int) num == 1)
        {
          this.ReadPackets(this.InputStream, 1U, decodeSize);
          decodeSize = 0U;
          num = 0U;
        }
        else
        {
          this.ReadPackets(this.InputStream, 1U, (uint) this.DataSize);
          decodeSize -= (uint) this.DataSize;
          --num;
        }
        if (!this.AttemptDecode(outputStream))
          return false;
      }
      return true;
    }

    private bool AttemptDecode(Stream outputStream)
    {
      if ((int) this.decodebuffercount != 1 || !this.validbuffers[0])
        return false;
      outputStream.Write(this.buffers[0], 0, (int) this.buffersize[0]);
      return true;
    }
  }
}
