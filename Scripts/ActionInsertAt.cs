// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ActionInsertAt
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
  public class ActionInsertAt : IActScript
  {
    public object DisplayName => (object) "Add action to...";

    public string Group => "Action";

    public string InputGesture => "Alt-T";

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
        ActionInsertDialog actionInsertDialog = new ActionInsertDialog(act);
        actionInsertDialog.Mode = ActionInsertDialog.EditMode.Insert;
        actionInsertDialog.StartIndex = selectedActionIndex;
        actionInsertDialog.EndIndex = selectedActionIndex + 1;
        actionInsertDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = actionInsertDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
          return;
        actionInsertDialog.Execute(act);
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
