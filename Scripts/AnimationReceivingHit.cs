// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.AnimationReceivingHit
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Graphics;
using GRF.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ActEditor.Core.Scripts
{
  public class AnimationReceivingHit : IActScript
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
        act.Commands.Backup((Action<Act>) (actInput =>
        {
          GRF.FileFormats.ActFormat.Action action = actInput[selectedActionIndex];
          int num = selectedActionIndex % 8;
          if (action.NumberOfFrames == 1)
          {
            GRF.FileFormats.ActFormat.Frame frame = new GRF.FileFormats.ActFormat.Frame(action.Frames[0]);
            Point point = new Point(0.0f, 0.0f);
            if (num <= 1)
              point = new Point(4f, -9f);
            else if (num <= 3)
              point = new Point(4f, -4f);
            else if (num <= 5)
              point = new Point(-4f, -4f);
            else if (num <= 7)
              point = new Point(-4f, -9f);
            frame.Translate((int) point.X, (int) point.Y);
            action.Frames.Add(new GRF.FileFormats.ActFormat.Frame(frame));
            action.Frames.Add(new GRF.FileFormats.ActFormat.Frame(frame));
            action.Frames.Add(new GRF.FileFormats.ActFormat.Frame(frame));
            action.Frames.Add(new GRF.FileFormats.ActFormat.Frame(frame));
          }
          GRF.FileFormats.ActFormat.Frame frame1 = action.Frames.Last<GRF.FileFormats.ActFormat.Frame>();
          while (action.NumberOfFrames < 5)
            action.Frames.Add(new GRF.FileFormats.ActFormat.Frame(frame1));
          float[] scales = new float[5]
          {
            1f,
            1.15f,
            1.09f,
            1.03f,
            1f
          };
          for (int i = 1; i < 5; ++i)
          {
            List<Layer> list = action.Frames[i].Layers.Select<Layer, Layer>((Func<Layer, Layer>) (p => new Layer(p))).ToList<Layer>();
            list.ForEach((Action<Layer>) (p => p.Scale(scales[i], scales[i])));
            list.ForEach((Action<Layer>) (p => p.Color = new GrfColor((byte) (0.58 * (double) p.Color.A), p.Color.R, p.Color.G, p.Color.B)));
            action.Frames[i].Layers.AddRange((IEnumerable<Layer>) list);
          }
        }), "Generate receiving hit animation");
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
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("receiving damage")));
        displayName.Inlines.Add(" animation");
        return (object) displayName;
      }
    }

    public string Group => "Animation";

    public string InputGesture => (string) null;

    public string Image => (string) null;
  }
}
