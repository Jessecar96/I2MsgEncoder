// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.ProcessingSteps.TransferIdMsgEncodeStep
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.Xml;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
  public class TransferIdMsgEncodeStep : IMsgEncodeStep
  {
    private string id;

    public TransferIdMsgEncodeStep(string id)
    {
      this.id = id;
    }

    public string Tag
    {
      get
      {
        return "TransferId";
      }
    }

    public string Encode(string payloadFile, XmlElement descriptor)
    {
      XmlElement element = descriptor.OwnerDocument.CreateElement("Id");
      element.InnerText = this.id;
      descriptor.AppendChild((XmlNode) element);
      return payloadFile;
    }
  }
}
