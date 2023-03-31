// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.EditClearPalette
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;

namespace ActEditor.Core.Scripts
{
  public class EditClearPalette : IActScript
  {
    public object DisplayName => (object) "Clear palette";

    public string Group => "Edit";

    public string InputGesture => "Ctrl-Shift-L";

    public string Image => "delete.png";

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      try
      {
        act.Commands.Begin();
        act.Commands.Backup((Action<Act>) (_ =>
        {
          byte[] dataRgba1024 = new byte[1024];
          for (int index = 0; index < 1024; index += 4)
            dataRgba1024[index + 3] = byte.MaxValue;
          dataRgba1024[0] = byte.MaxValue;
          dataRgba1024[2] = byte.MaxValue;
          act.Sprite.Palette.SetPalette(dataRgba1024);
        }), (string) this.DisplayName, true);
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
      return act != null && act.Sprite.Palette != null;
    }
  }
}
