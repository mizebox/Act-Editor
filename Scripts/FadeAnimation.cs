// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.FadeAnimation
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace ActEditor.Core.Scripts
{
  public class FadeAnimation : IActScript
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
        InputDialog inputDialog = new InputDialog("Nuber of frames to fade", "Fade animation", "7", false);
        inputDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = inputDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 1 : (!nullable.HasValue ? 1 : 0)) != 0)
          return;
        int ival;
        if (int.TryParse(inputDialog.Input, out ival))
        {
          if (ival <= 2 || ival > 30)
          {
            ErrorHandler.HandleException("The number of frames must be between 2 and 30.");
          }
          else
          {
            act.Commands.Begin();
            act.Commands.Backup((Action<Act>) (actInput =>
            {
              GRF.FileFormats.ActFormat.Action action = actInput[selectedActionIndex];
              while (action.NumberOfFrames - 1 < ival + selectedFrameIndex)
                action.Frames.Add(new GRF.FileFormats.ActFormat.Frame(action.Frames.Last<GRF.FileFormats.ActFormat.Frame>()));
              double[] numArray = new double[ival + 1];
              for (int index = 0; index <= ival; ++index)
                numArray[index] = 1.0 - (double) index / (double) ival;
              for (int index = 1; index <= ival; ++index)
              {
                GRF.FileFormats.ActFormat.Frame frame = action.Frames[selectedFrameIndex + index];
                if (index == ival)
                {
                  frame.Layers.Clear();
                }
                else
                {
                  foreach (Layer layer in frame.Layers)
                    layer.Color = new GrfColor((byte) (numArray[index] * (double) layer.Color.A), layer.Color.R, layer.Color.G, layer.Color.B);
                }
              }
            }), "Generate fade animation");
          }
        }
        else
          ErrorHandler.HandleException("Invalid integer format.");
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
      return act != null;
    }

    public object DisplayName
    {
      get
      {
        TextBlock displayName = new TextBlock();
        displayName.Inlines.Add("Generate ");
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("fade")));
        displayName.Inlines.Add(" animation");
        return (object) displayName;
      }
    }

    public string Group => "Animation";

    public string InputGesture => "Ctrl-Alt-T";

    public string Image => "fade.png";
  }
}
