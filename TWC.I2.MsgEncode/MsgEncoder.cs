// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.MsgEncoder
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TWC.I2.MsgEncode
{
  public sealed class MsgEncoder
  {
    private readonly List<IMsgEncodeStep> encodeChain;

    public MsgEncoder(IMsgEncodeStep step)
    {
      this.encodeChain = new List<IMsgEncodeStep>() { step };
    }

    public MsgEncoder(IEnumerable<IMsgEncodeStep> encChain)
    {
      this.encodeChain = new List<IMsgEncodeStep>(encChain);
    }

    public void Encode(string payloadFile)
    {
      XmlDocument descriptorDoc = new XmlDocument();
      XmlElement element1 = descriptorDoc.CreateElement("Msg");
      descriptorDoc.AppendChild((XmlNode) element1);
      foreach (IMsgEncodeStep msgEncodeStep in this.encodeChain)
      {
        XmlElement element2 = descriptorDoc.CreateElement(msgEncodeStep.Tag);
        element1.AppendChild((XmlNode) element2);
        string sourceFileName = msgEncodeStep.Encode(payloadFile, element2);
        if (payloadFile != sourceFileName)
        {
          File.Delete(payloadFile);
          File.Move(sourceFileName, payloadFile);
        }
      }
      MsgEncoder.FormatMsgFile(payloadFile, descriptorDoc);
    }

    private static void FormatMsgFile(string fname, XmlDocument descriptorDoc)
    {
      string innerXml = descriptorDoc.InnerXml;
      using (BinaryWriter binaryWriter = new BinaryWriter((Stream) File.Open(fname, FileMode.Append, FileAccess.Write)))
      {
        binaryWriter.Write(innerXml.ToCharArray());
        binaryWriter.Write(Common.Magic.ToCharArray());
        binaryWriter.Write((uint) innerXml.Length);
      }
    }
  }
}
