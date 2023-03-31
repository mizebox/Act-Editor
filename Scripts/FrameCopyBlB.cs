// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.FrameCopyBlB
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ActEditor.Core.Scripts
{
  public class FrameCopyBlB : IActScript
  {
    public object DisplayName
    {
      get
      {
        TextBlock displayName = new TextBlock();
        displayName.Inlines.Add("Rotation copy from ");
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("bottom left")));
        displayName.Inlines.Add(" to ");
        displayName.Inlines.Add((Inline) new Bold((Inline) new Run("bottom")));
        return (object) displayName;
      }
    }

    public string Group => "Animation";

    public string InputGesture => (string) null;

    public string Image => "blb.png";

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
        int num = selectedActionIndex / 8 * 8;
        for (int index1 = num + 1; index1 < act.NumberOfActions && index1 < num + 8; index1 += 2)
        {
          act.Commands.FrameDeleteRange(index1 - 1, 0, act[index1 - 1].NumberOfFrames);
          for (int index2 = 0; index2 < act[index1].NumberOfFrames; ++index2)
            act.Commands.FrameCopyTo(index1, index2, index1 - 1, index2);
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex, ErrorLevel.Warning);
      }
      finally
      {
        act.Commands.End();
      }
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && selectedActionIndex / 8 * 8 + 7 < act.NumberOfActions;
    }
  }
}
