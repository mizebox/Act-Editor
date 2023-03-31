// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.InterpolationAnimationAdv
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
  public class InterpolationAnimationAdv : IActScript
  {
    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      try
      {
        InterpolateDialog interpolateDialog = new InterpolateDialog(act, selectedActionIndex);
        interpolateDialog.StartIndex = selectedFrameIndex;
        interpolateDialog.EndIndex = act[selectedActionIndex].NumberOfFrames - 1;
        interpolateDialog.Mode = InterpolateDialog.EditMode.Frame;
        interpolateDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = interpolateDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
          return;
        interpolateDialog.Execute(act);
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
      return act != null && act[selectedActionIndex].NumberOfFrames > 1;
    }

    public object DisplayName => (object) "Advanced interpolation";

    public string Group => "Animation";

    public string InputGesture => "Alt-I";

    public string Image => "advanced.png";
  }
}
