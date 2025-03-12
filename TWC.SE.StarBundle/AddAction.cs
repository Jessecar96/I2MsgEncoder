// Decompiled with JetBrains decompiler
// Type: TWC.SE.StarBundle.AddAction
// Assembly: TWC.SE.StarBundle, Version=6.14.0.19271, Culture=neutral, PublicKeyToken=null
// MVID: 00D248FB-67F1-486F-BE9D-C21EA5CF9152
// Assembly location: Z:\I2 Jr Images\JR-225\TWC\i2\TWC.SE.StarBundle.dll

namespace TWC.SE.StarBundle
{
  public class AddAction : FileAction
  {
    public AddAction()
    {
      this.FileActionType = "Add";
    }

    protected AddAction(AddAction action)
      : base((FileAction) action)
    {
    }

    public override FileAction Clone()
    {
      return (FileAction) new AddAction(this);
    }
  }
}
