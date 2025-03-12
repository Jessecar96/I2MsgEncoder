// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.ProcessingSteps.CheckHeadendIdMsgEncodeStep
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.Collections.Generic;
using System.Xml;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
  public class CheckHeadendIdMsgEncodeStep : IMsgEncodeStep
  {
    private IEnumerable<string> ids;

    public CheckHeadendIdMsgEncodeStep(IEnumerable<string> ids)
    {
      this.ids = ids;
    }

    public CheckHeadendIdMsgEncodeStep(string id)
    {
      this.ids = (IEnumerable<string>) new string[1]
      {
        id
      };
    }

    public string Tag
    {
      get
      {
        return "CheckHeadendId";
      }
    }

    public string Encode(string payloadFile, XmlElement descriptor)
    {
      foreach (string id in this.ids)
      {
        XmlElement element = descriptor.OwnerDocument.CreateElement("HeadendId");
        element.InnerText = id;
        descriptor.AppendChild((XmlNode) element);
      }
      return payloadFile;
    }
  }
}
