// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.GenericControls.DummyStringView
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

namespace ActEditor.Core.WPF.GenericControls
{
  public class DummyStringView
  {
    private readonly string _item;

    public DummyStringView(string item) => this._item = item;

    public override string ToString() => this._item;
  }
}
