// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.EditPaletteAdvanced
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Tools.PaletteEditorTool;
using GRF;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using GRF.System;
using System.ComponentModel;
using TokeiLibrary;
using Utilities;

namespace ActEditor.Core.Scripts
{
  public class EditPaletteAdvanced : IActScript
  {
    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      EditPalette.CanOpen = false;
      PaletteEditorWindow dialog = new PaletteEditorWindow();
      string tempPath = TemporaryFilesManager.GetTemporaryFilePath("tmp_sprite_{0:0000}.spr");
      act.Sprite.Converter.Save(act.Sprite, tempPath);
      if (dialog.PaletteEditor.Open(tempPath))
      {
        dialog.Owner = WpfUtilities.TopWindow;
        dialog.Closing += (CancelEventHandler) delegate
        {
          EditPalette.CanOpen = true;
          tempPath = TemporaryFilesManager.GetTemporaryFilePath("tmp_sprite_{0:0000}.spr");
          if (act.Sprite.Palette != null && dialog.PaletteEditor.Sprite.Palette != null)
          {
            dialog.PaletteEditor.Sprite.Palette.EnableRaiseEvents = false;
            dialog.PaletteEditor.Sprite.Palette[0, 0] = act.Sprite.Palette[0, 0];
            dialog.PaletteEditor.Sprite.Palette.EnableRaiseEvents = true;
          }
          dialog.PaletteEditor.SaveAs(tempPath);
        };
        dialog.ShowDialog();
        Spr sprite = new Spr((MultiType) tempPath);
        bool flag = false;
        if (act.Sprite.Palette != null && sprite.Palette != null && act.Sprite.NumberOfImagesLoaded == sprite.NumberOfImagesLoaded && act.Sprite.NumberOfIndexed8Images == sprite.NumberOfIndexed8Images && Methods.ByteArrayCompare(sprite.Palette.BytePalette, 4, 1020, act.Sprite.Palette.BytePalette, 4))
        {
          flag = true;
          for (int index = 0; index < sprite.NumberOfIndexed8Images; ++index)
          {
            if (!Methods.ByteArrayCompare(sprite.Images[index].Pixels, act.Sprite.Images[index].Pixels))
              flag = false;
          }
        }
        if (flag)
          return;
        act.Commands.SetSprite(sprite);
        act.InvalidateVisual();
      }
      else
        EditPalette.CanOpen = true;
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && act.Sprite.NumberOfIndexed8Images > 0 && EditPalette.CanOpen;
    }

    public object DisplayName => (object) "Palette editor...";

    public string Group => "Edit";

    public string InputGesture => (string) null;

    public string Image => "pal.png";
  }
}
