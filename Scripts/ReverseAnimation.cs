// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ReverseAnimation
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using System;

namespace ActEditor.Core.Scripts
{
  public class ReverseAnimation : IActScript
  {
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
        act.Commands.Begin();
        act.Commands.StoreAndExecute((IActCommand) new BackupCommand((Action<Act>) (_ => act[selectedActionIndex].Frames.Reverse()), "Reverse animation")
        {
          CopyMode = CopyStructureMode.Actions
        });
      }
      catch (Exception ex)
      {
        act.Commands.CancelEdit();
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
      return act != null && act[selectedActionIndex].NumberOfFrames > 1;
    }

    public object DisplayName => (object) "Reverse animation";

    public string Group => "Animation";

    public string InputGesture => (string) null;

    public string Image => "reverse.png";
  }
}
