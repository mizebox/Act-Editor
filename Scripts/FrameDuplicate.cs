// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.FrameDuplicate
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace ActEditor.Core.Scripts
{
  public class FrameDuplicate : IActScript
  {
    public object DisplayName => (object) "Duplicate frames...";

    public string Group => "Frame";

    public string InputGesture => "Ctrl-W";

    public string Image => "copy.png";

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
        InputDialog inputDialog = new InputDialog("Number of times to copy the frames.", "Frame duplication", "2");
        inputDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = inputDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 1 : (!nullable.HasValue ? 1 : 0)) != 0)
          return;
        int result;
        if (int.TryParse(inputDialog.Input, out result))
        {
          if (result > 0)
          {
            try
            {
              act.Commands.BeginNoDelay();
              int numberOfFrames = act[selectedActionIndex].NumberOfFrames;
              for (int index = 0; index < result; ++index)
              {
                for (int frameIndexFrom = 0; frameIndexFrom < numberOfFrames; ++frameIndexFrom)
                  act.Commands.FrameCopy(selectedActionIndex, frameIndexFrom);
              }
              return;
            }
            catch (Exception ex)
            {
              act.Commands.CancelEdit();
              ErrorHandler.HandleException(ex);
              return;
            }
            finally
            {
              act.Commands.End();
              act.InvalidateVisual();
            }
          }
        }
        ErrorHandler.HandleException("The input value was not in the correct format. Integer value must be greater than 0.");
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
      return act != null && act[selectedActionIndex].NumberOfFrames > 0;
    }
  }
}
