// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.InterpolationAnimation
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace ActEditor.Core.Scripts
{
  public class InterpolationAnimation : IActScript
  {
    public static bool UseInterpolateSettings;

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      InterpolationAnimation.\u003C\u003Ec__DisplayClass7 cDisplayClass7_1 = new InterpolationAnimation.\u003C\u003Ec__DisplayClass7();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass7_1.act = act;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass7_1.selectedActionIndex = selectedActionIndex;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass7_1.selectedFrameIndex = selectedFrameIndex;
      try
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        InputDialog inputDialog = new InputDialog("Interpolating frame " + (object) cDisplayClass7_1.selectedFrameIndex + " to frame " + (object) (cDisplayClass7_1.selectedFrameIndex + 1) + ". On how many frames should the animation be?", "Interpolation animation", Configuration.ConfigAsker["[ActEditor - Interpolation value]", "5"], false);
        inputDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = inputDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 1 : (!nullable.HasValue ? 1 : 0)) != 0)
          return;
        int ival;
        if (int.TryParse(inputDialog.Input, out ival))
        {
          if (ival <= 0 || ival > 50)
          {
            ErrorHandler.HandleException("The number of frames must be between 0 and 50.");
          }
          else
          {
            Configuration.ConfigAsker["[ActEditor - Interpolation value]"] = inputDialog.Input;
            // ISSUE: reference to a compiler-generated field
            cDisplayClass7_1.act.Commands.Begin();
            // ISSUE: reference to a compiler-generated field
            cDisplayClass7_1.act.Commands.Backup((Action<Act>) (actInput =>
            {
              int frameIndex = selectedFrameIndex + 1;
              if (frameIndex >= act[selectedActionIndex].Frames.Count)
                frameIndex = 0;
              GRF.FileFormats.ActFormat.Frame frame1 = act[selectedActionIndex, selectedFrameIndex];
              List<Layer> layers = act[selectedActionIndex, frameIndex].Layers;
              List<GRF.FileFormats.ActFormat.Frame> collection = new List<GRF.FileFormats.ActFormat.Frame>();
              HashSet<int> intSet = new HashSet<int>();
              Func<float, float> easeMethod = InterpolationAnimation.GetEaseMethod(0);
              InterpolationAnimation.UseInterpolateSettings = false;
              for (int index1 = 0; index1 < ival; ++index1)
              {
                GRF.FileFormats.ActFormat.Frame frame2 = new GRF.FileFormats.ActFormat.Frame(frame1);
                List<Layer> startLayers = frame2.Layers;
                float degree = (float) (((double) index1 + 1.0) / ((double) ival + 1.0));
                // ISSUE: variable of a compiler-generated type
                InterpolationAnimation.\u003C\u003Ec__DisplayClass7 cDisplayClass7 = cDisplayClass7_1;
                for (int layer = 0; layer < frame2.NumberOfLayers; ++layer)
                {
                  if (layer < layers.Count)
                  {
                    if (layers[layer].SpriteIndex == startLayers[layer].SpriteIndex)
                    {
                      startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], layers[layer], degree, easeMethod);
                      intSet.Add(layer);
                    }
                    else if (startLayers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1 && layers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1)
                    {
                      int index2 = layers.FindIndex((Predicate<Layer>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex));
                      Layer sub2 = layers[index2];
                      startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], sub2, degree, easeMethod);
                      intSet.Add(index2);
                    }
                    else
                      startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], (Layer) null, degree, easeMethod);
                  }
                  else if (startLayers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1 && layers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1)
                  {
                    int index3 = layers.FindIndex((Predicate<Layer>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex));
                    Layer sub2 = layers[index3];
                    startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], sub2, degree, easeMethod);
                    intSet.Add(index3);
                  }
                  else
                    startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], (Layer) null, degree, easeMethod);
                }
                for (int index4 = 0; index4 < layers.Count; ++index4)
                {
                  if (!intSet.Contains(index4))
                    startLayers.Add(InterpolationAnimation.Interpolate((Layer) null, layers[index4], degree, easeMethod));
                }
                collection.Add(frame2);
              }
              act[selectedActionIndex].Frames.InsertRange(selectedFrameIndex + 1, (IEnumerable<GRF.FileFormats.ActFormat.Frame>) collection);
            }), "Interpolate frames");
          }
        }
        else
          ErrorHandler.HandleException("Invalid integer format.");
      }
      catch (Exception ex)
      {
        // ISSUE: reference to a compiler-generated field
        cDisplayClass7_1.act.Commands.CancelEdit();
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        // ISSUE: reference to a compiler-generated field
        cDisplayClass7_1.act.Commands.End();
        // ISSUE: reference to a compiler-generated field
        cDisplayClass7_1.act.InvalidateVisual();
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

    public object DisplayName
    {
      get
      {
        TextBlock displayName = new TextBlock();
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("Interpolate")));
        displayName.Inlines.Add(" frames");
        return (object) displayName;
      }
    }

    public string Group => "Animation";

    public string InputGesture => "Ctrl-I";

    public string Image => "interpolate.png";

    public static Layer Interpolate(
      Layer sub1,
      Layer sub2,
      float degree,
      Func<float, float> easeFunc)
    {
      degree = easeFunc(degree);
      if (sub1 == null)
      {
        Layer layer = new Layer(sub2);
        layer.Color = new GrfColor((byte) ((double) layer.Color.A * (double) degree), layer.Color.R, layer.Color.G, layer.Color.B);
        return layer;
      }
      if (sub2 == null)
      {
        Layer layer = new Layer(sub1);
        layer.Color = new GrfColor((byte) ((double) layer.Color.A * (1.0 - (double) degree)), layer.Color.R, layer.Color.G, layer.Color.B);
        return layer;
      }
      Layer layer1 = new Layer(sub1);
      if (!InterpolationAnimation.UseInterpolateSettings || InterpolationAnimation.UseInterpolateSettings && GrfEditorConfiguration.InterpolateOffsets)
        layer1.Translate((int) ((double) (sub2.OffsetX - sub1.OffsetX) * (double) degree), (int) ((double) (sub2.OffsetY - sub1.OffsetY) * (double) degree));
      if (!InterpolationAnimation.UseInterpolateSettings || InterpolationAnimation.UseInterpolateSettings && GrfEditorConfiguration.InterpolateAngle)
      {
        int num1 = sub2.Rotation < sub1.Rotation ? sub2.Rotation + 360 - sub1.Rotation : sub2.Rotation - sub1.Rotation;
        int num2 = sub2.Rotation > sub1.Rotation ? sub1.Rotation + 360 - sub2.Rotation : sub1.Rotation - sub2.Rotation;
        if (num1 < num2)
          layer1.Rotate((int) ((double) num1 * (double) degree));
        else
          layer1.Rotate((int) ((double) -num2 * (double) degree));
      }
      if (!InterpolationAnimation.UseInterpolateSettings || InterpolationAnimation.UseInterpolateSettings && GrfEditorConfiguration.InterpolateScale)
      {
        layer1.ScaleX = (sub2.ScaleX - sub1.ScaleX) * degree + sub1.ScaleX;
        layer1.ScaleY = (sub2.ScaleY - sub1.ScaleY) * degree + sub1.ScaleY;
      }
      if (!InterpolationAnimation.UseInterpolateSettings || InterpolationAnimation.UseInterpolateSettings && GrfEditorConfiguration.InterpolateColor)
        layer1.Color = new GrfColor((byte) ((double) ((int) sub2.Color.A - (int) sub1.Color.A) * (double) degree + (double) sub1.Color.A), (byte) ((double) ((int) sub2.Color.R - (int) sub1.Color.R) * (double) degree + (double) sub1.Color.R), (byte) ((double) ((int) sub2.Color.G - (int) sub1.Color.G) * (double) degree + (double) sub1.Color.G), (byte) ((double) ((int) sub2.Color.B - (int) sub1.Color.B) * (double) degree + (double) sub1.Color.B));
      if ((!InterpolationAnimation.UseInterpolateSettings || InterpolationAnimation.UseInterpolateSettings && GrfEditorConfiguration.InterpolateMirror) && sub1.Mirror != sub2.Mirror)
        layer1.ScaleX *= (float) -((double) degree * (2.0 * (double) layer1.Width) - (double) layer1.Width) / (float) layer1.Width;
      return layer1;
    }

    public static Func<float, float> GetEaseMethod(int ease)
    {
      if (ease == 0)
        return (Func<float, float>) (v => v);
      float p = Math.Abs((float) ease / 25f);
      return ease < 0 ? (Func<float, float>) (v => (float) Math.Pow((double) v, 1.0 + (double) p)) : (Func<float, float>) (v => 1f - (float) Math.Pow(1.0 - (double) v, 1.0 + (double) p));
    }

    public static void Interpolate(
      Act act,
      int selectedActionIndex,
      int selected,
      Layer subStart,
      Layer subEnd,
      int from,
      int to,
      Func<float, float> easeFunc)
    {
      int num = to < from ? to + (act[selectedActionIndex].NumberOfFrames - from) - 1 : to - from - 1;
      for (int index1 = 0; index1 < num; ++index1)
      {
        GRF.FileFormats.ActFormat.Frame frame = act[selectedActionIndex, (from + index1 + 1) % act[selectedActionIndex].NumberOfFrames];
        float degree = (float) (((double) index1 + 1.0) / ((double) num + 1.0));
        int index2 = -1;
        if (selected < frame.NumberOfLayers && frame.Layers[selected].SpriteIndex == subStart.SpriteIndex)
        {
          index2 = selected;
        }
        else
        {
          for (int index3 = 0; index3 < frame.NumberOfLayers; ++index3)
          {
            if (frame.Layers[index3].SpriteIndex == subStart.SpriteIndex)
              index2 = index3;
          }
        }
        if (index2 >= 0)
        {
          frame.Layers[index2] = InterpolationAnimation.Interpolate(subStart, subEnd, degree, easeFunc);
        }
        else
        {
          Layer layer = InterpolationAnimation.Interpolate(subStart, subEnd, degree, easeFunc);
          frame.Layers.Add(layer);
        }
      }
    }

    public enum EaseMode
    {
      InOrOut,
      InAndOut,
    }
  }
}
