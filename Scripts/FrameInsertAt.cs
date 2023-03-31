// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.FrameInsertAt
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.Dialogs;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using TokeiLibrary;

namespace ActEditor.Core.Scripts
{
  public class FrameInsertAt : IActScript
  {
    public object DisplayName => (object) "Add frame to...";

    public string Group => "Frame";

    public string InputGesture => "Ctrl-T";

    public string Image => "add.png";

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null)
        return;
      try
      {
        FrameInsertDialog frameInsertDialog = new FrameInsertDialog(act, selectedActionIndex);
        frameInsertDialog.StartIndex = selectedFrameIndex;
        frameInsertDialog.EndIndex = selectedFrameIndex + 1;
        frameInsertDialog.Mode = FrameInsertDialog.EditMode.Insert;
        frameInsertDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = frameInsertDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
          return;
        frameInsertDialog.Execute(act);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex, ErrorLevel.Warning);
      }
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null;
    }
  }
}
