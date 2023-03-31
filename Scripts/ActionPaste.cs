// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ActionPaste
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using System.Windows;

namespace ActEditor.Core.Scripts
{
  public class ActionPaste : IActScript
  {
    public object DisplayName => (object) "Paste action";

    public string Group => "Action";

    public string InputGesture => "Alt-V";

    public string Image => "paste.png";

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null)
        return;
      IDataObject dataObject = Clipboard.GetDataObject();
      if (dataObject == null)
        return;
      GRF.FileFormats.ActFormat.Action action = dataObject.GetData("Action") as GRF.FileFormats.ActFormat.Action;
      if (action == null)
        return;
      try
      {
        act.Commands.BeginNoDelay();
        act.Commands.Backup((Action<Act>) (_ =>
        {
          act.DeleteActions(selectedActionIndex, 1);
          act.AddAction(action, selectedActionIndex);
        }), "Paste actions");
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        act.Commands.End();
        act.InvalidateVisual();
      }
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
