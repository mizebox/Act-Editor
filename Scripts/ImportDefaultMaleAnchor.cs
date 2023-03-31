// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ImportDefaultMaleAnchor
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using System;
using System.Collections.Generic;
using TokeiLibrary;

namespace ActEditor.Core.Scripts
{
  public class ImportDefaultMaleAnchor : IActScript
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
        Act loadedAct = new Act((MultiType) ApplicationManager.GetResource("ref_head_m.act"), new Spr());
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
                frame.Anchors.Clear();
                if (action2.NumberOfFrames != 0)
                {
                  int frameIndex2 = (int) ((double) frameIndex1 / (double) action1.NumberOfFrames * (double) action2.NumberOfFrames);
                  frame.Anchors.AddRange((IEnumerable<Anchor>) action2[frameIndex2].Anchors);
                }
              }
            }
            else
            {
              for (int frameIndex = 0; frameIndex < act[index].NumberOfFrames; ++frameIndex)
              {
                Frame frame1 = act[index, frameIndex];
                frame1.Anchors.Clear();
                Frame frame2 = loadedAct.TryGetFrame(index, frameIndex);
                if (frame2 == null)
                {
                  frame2 = loadedAct.TryGetFrame(index, loadedAct[index].NumberOfFrames - 1);
                  if (frame2 == null)
                    continue;
                }
                frame1.Anchors.AddRange((IEnumerable<Anchor>) frame2.Anchors);
              }
            }
          }
        }), "Import default anchors (male)", true);
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

    public object DisplayName => (object) "Set default (male)";

    public string Group => "Anchors/Set anchors";

    public string InputGesture => (string) null;

    public string Image => (string) null;
  }
}
