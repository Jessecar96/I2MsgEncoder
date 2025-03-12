// Decompiled with JetBrains decompiler
// Type: TWC.I2.MsgEncode.ProcessingSteps.CheckStarFlagsMsgEncodeStep
// Assembly: TWC.I2.MsgEncode, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 4D486DFD-B57C-4280-97BE-EA4E5C8D8530
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.I2.MsgEncode.dll

using System.Collections.Generic;
using System.Xml;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
  public class CheckStarFlagsMsgEncodeStep : IMsgEncodeStep
  {
    private const string TAG = "CheckStarFlags";
    private IEnumerable<string> flags;

    public CheckStarFlagsMsgEncodeStep(IEnumerable<string> flags)
    {
      this.flags = flags;
    }

    public CheckStarFlagsMsgEncodeStep(string flag)
    {
      this.flags = (IEnumerable<string>) new string[1]
      {
        flag
      };
    }

    public string Tag
    {
      get
      {
        return "CheckStarFlags";
      }
    }

    public string Encode(string payloadFile, XmlElement descriptor)
    {
      foreach (string flag in this.flags)
      {
        XmlElement element = descriptor.OwnerDocument.CreateElement("Flag");
        element.InnerText = flag;
        descriptor.AppendChild((XmlNode) element);
      }
      return payloadFile;
    }
  }
}
