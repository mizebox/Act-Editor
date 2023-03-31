// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ActionCopyMirror
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ActEditor.Core.Scripts
{
  public class ActionCopyMirror : IActScript
  {
    public object DisplayName
    {
      get
      {
        TextBlock displayName = new TextBlock();
        displayName.Inlines.Add("Mirror action from\r\n");
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("left/right")));
        displayName.Inlines.Add(" to ");
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("right/left")));
        return (object) displayName;
      }
    }

    public string InputGesture => "Alt-X";

    public string Image => "convert.png";

    public string Group => "Action";

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
        int num1 = selectedActionIndex % 8;
        int num2 = selectedActionIndex / 8 * 8;
        if (num1 == 0 || num1 == 4)
        {
          ErrorHandler.HandleException("Cannot mirror the frame. You must select an action which is neither the bottom or top one.");
        }
        else
        {
          int newActionIndex = 0;
          if (num1 == 1)
            newActionIndex = 7;
          if (num1 == 7)
            newActionIndex = 1;
          if (num1 == 2)
            newActionIndex = 6;
          if (num1 == 6)
            newActionIndex = 2;
          if (num1 == 3)
            newActionIndex = 4;
          if (num1 == 4)
            newActionIndex = 3;
          newActionIndex += num2;
          if (newActionIndex >= act.NumberOfActions)
            ErrorHandler.HandleException("Cannot mirror the frame because the action " + (object) newActionIndex + " doesn't exist.");
          else
            act.Commands.Backup((Action<Act>) (action => this._reverse(act, selectedActionIndex, newActionIndex)), "Frame copy");
        }
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
      return act != null && selectedActionIndex % 8 != 0 && selectedActionIndex % 8 != 4;
    }

    private void _reverse(Act act, int selectedActionIndex, int newSelectedActionIndex)
    {
      GRF.FileFormats.ActFormat.Action action = new GRF.FileFormats.ActFormat.Action(act[selectedActionIndex]);
      foreach (GRF.FileFormats.ActFormat.Frame frame in action.Frames)
      {
        foreach (Layer layer in frame.Layers)
        {
          layer.OffsetX *= -1;
          int num = 360 - layer.Rotation;
          layer.Rotation = num < 0 ? num + 360 : num;
          layer.Mirror = !layer.Mirror;
        }
        foreach (Anchor anchor in frame.Anchors)
          anchor.OffsetX *= -1;
      }
      act.SetAction(newSelectedActionIndex, action);
    }
  }
}
