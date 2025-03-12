// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.ProcessingSteps.MsgEncryptStep
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.IO;
using System.Security.Cryptography;
using System.Xml;
using TWC.Util;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
  public class MsgEncryptStep : MsgDecryptEncryptBase, IMsgEncodeStep
  {
    public string Tag
    {
      get
      {
        return "EncryptedMsg";
      }
    }

    public string Encode(string payloadFile, XmlElement descriptor)
    {
      SymmetricAlgorithm algorithm = this.algorithm;
      algorithm.Key = this.password;
      algorithm.IV = this.iv;
      string tempFileName = Toolbox.GetTempFileName("encrypt");
      FileStream fileStream = new FileStream(payloadFile, FileMode.Open);
      CryptoStream cryptoStream = new CryptoStream((Stream) new FileStream(tempFileName, FileMode.OpenOrCreate), algorithm.CreateEncryptor(), CryptoStreamMode.Write);
      try
      {
        Toolbox.CopyStream((Stream) fileStream, (Stream) cryptoStream);
      }
      finally
      {
        fileStream.Close();
        cryptoStream.Close();
      }
      return tempFileName;
    }
  }
}
