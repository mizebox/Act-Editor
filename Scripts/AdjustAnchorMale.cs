// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.AdjustAnchorMale
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using System;
using TokeiLibrary;

namespace ActEditor.Core.Scripts
{
  public class AdjustAnchorMale : IActScript
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
        AdjustAnchor.AdjustAnchors(act, new Act((MultiType) ApplicationManager.GetResource("ref_head_m.act"), new Spr()));
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

    public object DisplayName => (object) "Adjust anchors (male)";

    public string Group => "Anchors/Adjust anchors";

    public string InputGesture => (string) null;

    public string Image => (string) null;
  }
}
