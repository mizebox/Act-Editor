// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ActionDelete
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;

namespace ActEditor.Core.Scripts
{
  public class ActionDelete : IActScript
  {
    public object DisplayName => (object) "Delete action";

    public string Group => "Action";

    public string InputGesture => "Alt-Delete";

    public string Image => "delete.png";

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null)
        return;
      if (act.NumberOfActions <= 1)
        return;
      try
      {
        act.Commands.ActionDelete(selectedActionIndex);
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
      return act != null && act.NumberOfActions > 1;
    }
  }
}
