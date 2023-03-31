// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ImportPaletteFrom
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using GRF;
using GRF.FileFormats;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.PalFormat;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using System;
using System.IO;
using Utilities.Extension;

namespace ActEditor.Core.Scripts
{
  public class ImportPaletteFrom : IActScript
  {
    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      try
      {
        EditPalette.CanOpen = false;
        string str = PathRequest.OpenFileExtract("filter", FileFormat.MergeFilters(Format.PaletteContainers, Format.Pal, Format.Spr));
        if (str == null)
          return;
        if (str.IsExtension(".pal"))
        {
          act.Commands.SpriteSetPalette(new Pal(File.ReadAllBytes(str)).BytePalette);
          act.InvalidateVisual();
        }
        else if (str.IsExtension(".spr"))
        {
          Spr spr = new Spr((MultiType) str);
          if (spr.Palette == null)
            throw new Exception("This sprite doesn't have a palette.");
          act.Commands.SpriteSetPalette(spr.Palette.BytePalette);
          act.InvalidateVisual();
        }
        else
        {
          if (!str.IsExtension(".bmp"))
            return;
          GrfImage grfImage = (GrfImage) str;
          if (grfImage.GrfImageType != GrfImageType.Indexed8)
            throw new Exception("Invalid image format. Only bitmap files with palettes are allowed.");
          act.Commands.SpriteSetPalette(grfImage.Palette);
          act.InvalidateVisual();
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        EditPalette.CanOpen = true;
      }
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && EditPalette.CanOpen && act.Sprite.NumberOfIndexed8Images > 0;
    }

    public object DisplayName => (object) "Import palette...";

    public string Group => "Edit";

    public string InputGesture => (string) null;

    public string Image => (string) null;
  }
}
