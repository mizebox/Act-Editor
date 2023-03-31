// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.LayerInterpolationAnimation
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace ActEditor.Core.Scripts
{
  public class LayerInterpolationAnimation : IActScript
  {
    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null)
        return;
      if (selectedLayerIndexes.Length == 0)
        return;
      try
      {
        InputDialog inputDialog = new InputDialog("Interpolating layer(s) (" + string.Join(",", ((IEnumerable<int>) selectedLayerIndexes).Select<int, string>((Func<int, string>) (p => p.ToString())).ToArray<string>()) + "). What is the target frame index?", "Interpolation animation", (act[selectedActionIndex].NumberOfFrames - 1).ToString((IFormatProvider) CultureInfo.InvariantCulture), false);
        inputDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = inputDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 1 : (!nullable.HasValue ? 1 : 0)) != 0)
          return;
        int ival;
        if (int.TryParse(inputDialog.Input, out ival))
        {
          if (ival < 0)
            ErrorHandler.HandleException("The target frame index must be above 0");
          else if (ival == selectedFrameIndex)
            ErrorHandler.HandleException("The target frame index must not be equal to " + (object) selectedFrameIndex);
          else if (ival >= act[selectedActionIndex].NumberOfFrames)
          {
            ErrorHandler.HandleException("The target frame index must be below " + (object) act[selectedActionIndex].NumberOfFrames);
          }
          else
          {
            act.Commands.Begin();
            act.Commands.Backup((Action<Act>) (actInput =>
            {
              GRF.FileFormats.ActFormat.Frame frame1 = act[selectedActionIndex, selectedFrameIndex];
              GRF.FileFormats.ActFormat.Frame frame2 = act[selectedActionIndex, ival];
              InterpolationAnimation.UseInterpolateSettings = false;
              foreach (int selectedLayerIndex in selectedLayerIndexes)
              {
                Layer layer = frame1.Layers[selectedLayerIndex];
                Layer subEnd = (Layer) null;
                if (selectedLayerIndex < frame2.NumberOfLayers && frame2.Layers[selectedLayerIndex].SpriteIndex == layer.SpriteIndex)
                {
                  subEnd = frame2.Layers[selectedLayerIndex];
                }
                else
                {
                  for (int index = 0; index < frame2.NumberOfLayers; ++index)
                  {
                    if (frame2.Layers[index].SpriteIndex == layer.SpriteIndex)
                      subEnd = frame2.Layers[index];
                  }
                }
                InterpolationAnimation.Interpolate(act, selectedActionIndex, selectedLayerIndex, layer, subEnd, selectedFrameIndex, ival, InterpolationAnimation.GetEaseMethod(0));
              }
            }), "Interpolate selected layers");
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
      return act != null && selectedLayerIndexes.Length > 0;
    }

    public object DisplayName
    {
      get
      {
        TextBlock displayName = new TextBlock();
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("Interpolate")));
        displayName.Inlines.Add(" selected layers");
        return (object) displayName;
      }
    }

    public string Group => "Animation";

    public string InputGesture => "Ctrl-Alt-I";

    public string Image => "interpolate.png";
  }
}
