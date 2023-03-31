// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.EditSelectAll
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;

namespace ActEditor.Core.Scripts
{
  public class EditSelectAll : IActScript
  {
    public ActEditorWindow ActEditor { get; set; }

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      this.ActEditor.SelectionEngine.SelectAll();
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && act[selectedActionIndex, selectedFrameIndex].NumberOfLayers > 0;
    }

    public object DisplayName => (object) "Select all";

    public string Group => "Edit";

    public string InputGesture => "Ctrl-A";

    public string Image => (string) null;
  }
}
