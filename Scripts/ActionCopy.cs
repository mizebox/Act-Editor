// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ActionCopy
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;
using System.Windows;

namespace ActEditor.Core.Scripts
{
  public class ActionCopy : IActScript
  {
    public object DisplayName => (object) "Copy action";

    public string Group => "Action";

    public string InputGesture => "Alt-C";

    public string Image => "copy.png";

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null)
        return;
      Clipboard.SetDataObject((object) new DataObject("Action", (object) act[selectedActionIndex]));
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && selectedActionIndex > -1 && selectedActionIndex < act.NumberOfActions;
    }
  }
}
