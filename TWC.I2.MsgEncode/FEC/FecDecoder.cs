// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.FEC.FecDecoder
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System;
using System.Collections.Generic;
using System.IO;
using TWC.I2.MsgEncode.FEC.Decoder;
using TWC.SE.Util.BackChannelEvents;
using TWC.Util;

namespace TWC.I2.MsgEncode.FEC
{
  public abstract class FecDecoder
  {
    public static string TempFolder;
    private Dictionary<uint, long> packetOffsets;
    protected uint decodebuffercount;
    protected bool[] validbuffers;
    protected int validbuffercount;
    protected ushort[] buffersize;
    protected byte[][] buffers;
    private Stream inputStream;
    private uint filesize;
    private uint packetsRead;
    private ushort packetSize;

    public static bool DecodeFile(ushort packetSize, string fname, string tempFolder)
    {
      FecDecoder.TempFolder = tempFolder;
      string tempFileName = Toolbox.GetTempFileName(FecEncoder.TempFolder, "fecdecoder");
      try
      {
        using (BufferedStream bufferedStream1 = new BufferedStream((Stream) new FileStream(fname, FileMode.Open, FileAccess.ReadWrite)))
        {
          using (BufferedStream bufferedStream2 = new BufferedStream((Stream) new FileStream(tempFileName, FileMode.OpenOrCreate)))
          {
            bool flag = FecDecoder.Decode(packetSize, (Stream) bufferedStream1, fname, (Stream) bufferedStream2);
            if (flag)
            {
              bufferedStream1.SetLength(0L);
              bufferedStream2.Seek(0L, SeekOrigin.Begin);
              Toolbox.CopyStream((Stream) bufferedStream2, (Stream) bufferedStream1);
            }
            return flag;
          }
        }
      }
      finally
      {
        File.Delete(tempFileName);
      }
    }

    public static bool Decode(ushort packetSize, Stream inputStream, string inputStreamName, Stream outputStream)
    {
      FecEncoding encoding;
      uint filesize;
      byte minGridWidth;
      byte maxGridWidth;
      if (!FecPacketHeader.Find(inputStream, packetSize, out encoding, out filesize, out minGridWidth, out maxGridWidth))
        return false;
      FecDecoder fecDecoder;
      switch (encoding)
      {
        case FecEncoding.None:
          fecDecoder = (FecDecoder) new None(inputStream, packetSize, filesize);
          break;
        case FecEncoding.Version1:
          fecDecoder = (FecDecoder) new Version1(inputStream, packetSize, filesize, minGridWidth, maxGridWidth);
          break;
        default:
          return false;
      }
      int recoveredUnits = 0;
      int missingUnits = 0;
      bool hadLoss = fecDecoder.ReadPacketOffsets(inputStream, out recoveredUnits, out missingUnits);
      bool success = fecDecoder.Decode(outputStream);
      EventLogger.LogEvent((IAppEvent) new FECEvent(inputStreamName, success, hadLoss, recoveredUnits, missingUnits));
      return success;
    }

    public ushort PacketSize
    {
      get
      {
        return this.packetSize;
      }
    }

    public ushort HeaderSize
    {
      get
      {
        return 9;
      }
    }

    public ushort DataSize
    {
      get
      {
        return (ushort) ((uint) this.PacketSize - (uint) this.HeaderSize);
      }
    }

    public abstract bool Decode(Stream outputStream);

    protected FecDecoder(Stream inputStream, ushort packetSize, uint filesize)
    {
      this.inputStream = inputStream;
      this.packetSize = packetSize;
      this.filesize = filesize;
      this.packetsRead = 0U;
      this.validbuffercount = 0;
      this.packetOffsets = new Dictionary<uint, long>();
    }

    protected uint PacketsRead
    {
      get
      {
        return this.packetsRead;
      }
      set
      {
        this.packetsRead = value;
      }
    }

    protected uint Filesize
    {
      get
      {
        return this.filesize;
      }
    }

    protected Stream InputStream
    {
      get
      {
        return this.inputStream;
      }
    }

    protected void ReadPackets(Stream inputStream, uint numBuffers, uint decodeSize)
    {
      this.InvalidateBuffers();
      this.decodebuffercount = numBuffers;
      for (uint dest = 0; dest < this.decodebuffercount; ++dest)
      {
        ++this.packetsRead;
        this.buffersize[(int) dest] = this.DataSize;
        if ((int) dest == (int) numBuffers - 1)
          this.buffersize[(int) dest] = (ushort) decodeSize;
        else
          decodeSize -= (uint) this.DataSize;
        long offset;
        if (this.packetOffsets.TryGetValue(this.packetsRead, out offset))
        {
          inputStream.Seek(offset, SeekOrigin.Begin);
          inputStream.Read(this.buffers[(int) dest], 0, (int) this.DataSize);
          this.validbuffers[(int) dest] = true;
          ++this.validbuffercount;
        }
        else
          this.ClearBuffer(dest);
      }
    }

    protected void AllocateBuffers(int count)
    {
      this.validbuffers = new bool[count];
      this.buffers = new byte[count][];
      this.buffersize = new ushort[count];
      for (int index = 0; index < this.buffers.Length; ++index)
        this.buffers[index] = new byte[(int) this.DataSize];
      this.InvalidateBuffers();
    }

    protected void ClearBuffer(uint dest)
    {
      Array.Clear((Array) this.buffers[(int) dest], 0, this.buffers[(int) dest].Length);
    }

    protected void XOrBuffer(int dest, int source)
    {
      for (int index = 0; index < (int) this.DataSize; ++index)
        this.buffers[dest][index] ^= this.buffers[source][index];
    }

    private bool ReadPacketOffsets(Stream inputStream, out int recoveredUnits, out int missingUnits)
    {
      bool flag = false;
      recoveredUnits = 0;
      missingUnits = 0;
      FecPacketHeader fecPacketHeader = new FecPacketHeader();
      this.packetOffsets.Clear();
      inputStream.Seek(0L, SeekOrigin.Begin);
      while (inputStream.Position <= inputStream.Length - (long) this.PacketSize)
      {
        fecPacketHeader.Read(inputStream);
        if (fecPacketHeader.IsValid())
        {
          uint key = fecPacketHeader.PacketId();
          if (!this.packetOffsets.ContainsKey(key))
            this.packetOffsets.Add(key, inputStream.Position);
          ++recoveredUnits;
        }
        else
        {
          flag = true;
          ++missingUnits;
        }
        inputStream.Seek((long) this.DataSize, SeekOrigin.Current);
      }
      return flag;
    }

    private void InvalidateBuffers()
    {
      for (int index = 0; index < this.validbuffers.Length; ++index)
        this.validbuffers[index] = false;
      this.validbuffercount = 0;
    }
  }
}
