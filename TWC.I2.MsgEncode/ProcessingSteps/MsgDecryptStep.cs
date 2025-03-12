// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.ProcessingSteps.MsgDecryptStep
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.IO;
using System.Security.Cryptography;
using System.Xml;
using TWC.Util;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
  public class MsgDecryptStep : MsgDecryptEncryptBase, IMsgDecodeStep
  {
    public string Tag
    {
      get
      {
        return "EncryptedMsg";
      }
    }

    public string Decode(string payloadFile, XmlElement descriptor)
    {
      SymmetricAlgorithm algorithm = this.algorithm;
      algorithm.Key = this.password;
      algorithm.IV = this.iv;
      string tempFileName = Toolbox.GetTempFileName(MsgDecryptEncryptBase.TempFolder, "decrypt");
      CryptoStream cryptoStream = new CryptoStream((Stream) new FileStream(payloadFile, FileMode.Open), algorithm.CreateDecryptor(), CryptoStreamMode.Read);
      FileStream fileStream = new FileStream(tempFileName, FileMode.OpenOrCreate);
      try
      {
        Toolbox.CopyStream((Stream) cryptoStream, (Stream) fileStream);
      }
      finally
      {
        cryptoStream.Close();
        fileStream.Close();
      }
      return tempFileName;
    }
  }
}
