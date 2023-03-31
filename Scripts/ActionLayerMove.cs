// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ActionLayerMove
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Extension;

namespace ActEditor.Core.Scripts
{
  public class ActionLayerMove : IActScript
  {
    private readonly ActionLayerMove.MoveDirection _dir;
    private readonly IPreviewEditor _actEditor;
    private readonly string _displayName;

    public object DisplayName => this._dir != ActionLayerMove.MoveDirection.Up ? (object) "Move layers to front" : (object) "Move layers to back";

    public string InputGesture => this._dir != ActionLayerMove.MoveDirection.Up ? "Alt-F" : "Alt-B";

    public string Image => this._dir != ActionLayerMove.MoveDirection.Up ? "front.png" : "back.png";

    public string Group => "Action";

    public ActionLayerMove(ActionLayerMove.MoveDirection dir, IPreviewEditor actEditor)
    {
      this._dir = dir;
      this._actEditor = actEditor;
      this._displayName = this._dir == ActionLayerMove.MoveDirection.Up ? "Move to back" : "Bring to front";
    }

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      try
      {
        HashSet<int> newSelection = new HashSet<int>();
        selectedLayerIndexes = ((IEnumerable<int>) selectedLayerIndexes).OrderBy<int, int>((Func<int, int>) (p => p)).ToArray<int>();
        act.Commands.BeginNoDelay();
        for (int frameIndex = 0; frameIndex < act[selectedActionIndex].NumberOfFrames; ++frameIndex)
        {
          if (this._dir == ActionLayerMove.MoveDirection.Down)
          {
            int index = selectedLayerIndexes.Length - 1;
            int num = 0;
            while (index >= 0)
            {
              if (selectedLayerIndexes[index] < act[selectedActionIndex, frameIndex].NumberOfLayers)
              {
                if (selectedLayerIndexes[index] != act[selectedActionIndex, frameIndex].NumberOfLayers - num - 1)
                {
                  if (selectedLayerIndexes[index] + 2 <= act[selectedActionIndex, frameIndex].NumberOfLayers)
                    act.Commands.LayerSwitchRange(selectedActionIndex, frameIndex, selectedLayerIndexes[index], 1, selectedLayerIndexes[index] + 2);
                  if (frameIndex == selectedFrameIndex)
                    newSelection.Add(selectedLayerIndexes[index] + 1);
                }
                else if (frameIndex == selectedFrameIndex)
                  newSelection.Add(selectedLayerIndexes[index]);
              }
              --index;
              ++num;
            }
          }
          else
          {
            int index = 0;
            int num = 0;
            while (index < selectedLayerIndexes.Length)
            {
              if (selectedLayerIndexes[index] < act[selectedActionIndex, frameIndex].NumberOfLayers)
              {
                if (selectedLayerIndexes[index] != num)
                {
                  if (selectedLayerIndexes[index] - 1 >= 0)
                    act.Commands.LayerSwitchRange(selectedActionIndex, frameIndex, selectedLayerIndexes[index], 1, selectedLayerIndexes[index] - 1);
                  if (frameIndex == selectedFrameIndex)
                    newSelection.Add(selectedLayerIndexes[index] - 1);
                }
                else if (frameIndex == selectedFrameIndex)
                  newSelection.Add(selectedLayerIndexes[index]);
              }
              ++index;
              ++num;
            }
          }
        }
        ActionLayerMove.LayerGenericCommand command = new ActionLayerMove.LayerGenericCommand((Action<bool>) (redo =>
        {
          if (this._actEditor.SelectedAction != selectedActionIndex)
            return;
          this._actEditor.SelectionEngine.SetSelection(redo ? newSelection.ToHashSet<int>() : ((IEnumerable<int>) selectedLayerIndexes).ToHashSet<int>());
        }));
        act.Commands.StoreAndExecute((IActCommand) command);
      }
      catch (Exception ex)
      {
        act.Commands.CancelEdit();
        ErrorHandler.HandleException(ex, ErrorLevel.Warning);
      }
      finally
      {
        act.Commands.End();
        act.InvalidateVisual();
        act.InvalidateSpriteVisual();
      }
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null || selectedLayerIndexes.Length <= 0)
        return false;
      selectedLayerIndexes = ((IEnumerable<int>) selectedLayerIndexes).OrderBy<int, int>((Func<int, int>) (p => p)).ToArray<int>();
      if (this._dir == ActionLayerMove.MoveDirection.Down)
      {
        int index = selectedLayerIndexes.Length - 1;
        int num = 0;
        while (index >= 0)
        {
          if (selectedLayerIndexes[index] != act[selectedActionIndex, selectedFrameIndex].NumberOfLayers - num - 1)
            return true;
          --index;
          ++num;
        }
      }
      else
      {
        int index = 0;
        int num = 0;
        while (index < selectedLayerIndexes.Length)
        {
          if (selectedLayerIndexes[index] != num)
            return true;
          ++index;
          ++num;
        }
      }
      return false;
    }

    public class LayerGenericCommand : IActCommand
    {
      private readonly Action<bool> _callback;

      public LayerGenericCommand(Action<bool> callback) => this._callback = callback;

      public void Execute(Act act) => this._callback(true);

      public void Undo(Act act) => this._callback(false);

      public string CommandDescription => "Selection changed...";
    }

    public enum MoveDirection
    {
      Up,
      Down,
    }
  }
}
