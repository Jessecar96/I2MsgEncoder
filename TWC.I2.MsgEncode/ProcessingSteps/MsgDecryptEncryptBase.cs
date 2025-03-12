// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.ProcessingSteps.MsgDecryptEncryptBase
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.Security.Cryptography;
using System.Text;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
  public abstract class MsgDecryptEncryptBase
  {
    protected byte[] password = Encoding.UTF8.GetBytes("sixteencharactersixteencharacter");
    protected byte[] iv = Encoding.UTF8.GetBytes("sixteencharacter");
    protected string keyfile = "KeyFile.tdes";
    protected SymmetricAlgorithm algorithm = SymmetricAlgorithm.Create();
    public static string TempFolder;
  }
}
