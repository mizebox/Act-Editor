// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.BringToFront
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;

namespace ActEditor.Core.Scripts
{
  public class BringToFront : IActScript
  {
    public ActEditorWindow ActEditor { get; set; }

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      this.ActEditor._layerEditor.BringToFront();
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && selectedLayerIndexes.Length > 0;
    }

    public object DisplayName => (object) "Bring to front";

    public string Group => "Edit";

    public string InputGesture => "Ctrl-Shift-F";

    public string Image => "front.png";
  }
}
