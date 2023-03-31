// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.AdjustAnchor
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using GRF;
using GRF.FileFormats;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using System;
using System.Collections.Generic;
using System.IO;
using Utilities.Extension;

namespace ActEditor.Core.Scripts
{
  public class AdjustAnchor : IActScript
  {
    public ActEditorWindow ActEditor { get; set; }

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
        string str = PathRequest.OpenFileEditor("filter", FileFormat.MergeFilters(Format.Act));
        if (str == null)
          return;
        if (!str.IsExtension(".act"))
          throw new Exception("Invalid file extension (act file supported only).");
        AdjustAnchor.AdjustAnchors(act, new Act((MultiType) File.ReadAllBytes(str), new Spr()));
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
      return act != null;
    }

    public object DisplayName => (object) "Adjust from file...";

    public string Group => "Anchors/Adjust anchors";

    public string InputGesture => (string) null;

    public string Image => (string) null;

    public static void AdjustAnchors(Act act, Act loadedAct)
    {
      act.Commands.Begin();
      act.Commands.Backup((Action<Act>) (_ =>
      {
        for (int index = 0; index < act.NumberOfActions && index < loadedAct.NumberOfActions; ++index)
        {
          if (0 <= index && index < 8 || 16 <= index && index < 24)
          {
            GRF.FileFormats.ActFormat.Action action1 = act[index];
            GRF.FileFormats.ActFormat.Action action2 = loadedAct[index];
            for (int frameIndex1 = 0; frameIndex1 < action1.NumberOfFrames; ++frameIndex1)
            {
              Frame frame = act[index, frameIndex1];
              if (action2.NumberOfFrames == 0)
              {
                frame.Anchors.Clear();
              }
              else
              {
                int frameIndex2 = (int) ((double) frameIndex1 / (double) action1.NumberOfFrames * (double) action2.NumberOfFrames);
                List<Anchor> anchors = action2[frameIndex2].Anchors;
                AdjustAnchor._adjust((IEnumerable<Layer>) frame, frame.Anchors, anchors);
              }
            }
          }
          else
          {
            for (int frameIndex = 0; frameIndex < act[index].NumberOfFrames; ++frameIndex)
            {
              Frame frame1 = act[index, frameIndex];
              Frame frame2 = loadedAct.TryGetFrame(index, frameIndex);
              if (frame2 == null)
              {
                frame2 = loadedAct.TryGetFrame(index, loadedAct[index].NumberOfFrames - 1);
                if (frame2 == null)
                {
                  frame1.Anchors.Clear();
                  continue;
                }
              }
              AdjustAnchor._adjust((IEnumerable<Layer>) frame1, frame1.Anchors, frame2.Anchors);
            }
          }
        }
      }), "Adjust anchors", true);
    }

    private static void _adjust(
      IEnumerable<Layer> frame,
      List<Anchor> anchors,
      List<Anchor> refAnchors)
    {
      if (anchors.Count == 0 && refAnchors.Count == 0)
        return;
      if (refAnchors.Count > 0 && anchors.Count == 0)
        anchors.Clear();
      else if (refAnchors.Count == 0 && anchors.Count > 0)
      {
        anchors.Clear();
      }
      else
      {
        if (anchors.Count <= 0 || refAnchors.Count <= 0)
          return;
        int num1 = refAnchors[0].OffsetX - anchors[0].OffsetX;
        int num2 = refAnchors[0].OffsetY - anchors[0].OffsetY;
        anchors[0] = refAnchors[0];
        foreach (Layer layer in frame)
        {
          layer.OffsetX += num1;
          layer.OffsetY += num2;
        }
      }
    }
  }
}
