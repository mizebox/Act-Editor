// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.SpriteManager
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.Dialogs;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using GRF.Image.Decoders;
using GrfToWpfBridge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TokeiLibrary;
using Utilities;
using Utilities.Services;

namespace ActEditor.Core
{
  public class SpriteManager
  {
    public static int SpriteConverterOption = -1;
    private ActEditorWindow _actEditor;
    private List<SpriteEditMode> _disabledModes = new List<SpriteEditMode>();

    private List<GrfImage> _images => this._actEditor.Act.Sprite.Images;

    private Spr _sprite => this._actEditor.Act.Sprite;

    public void Init(ActEditorWindow actEditor) => this._actEditor = actEditor;

    public void Execute(int absoluteIndex, GrfImage image, SpriteEditMode mode)
    {
      if (image == null)
        image = this._images[absoluteIndex];
      int relativeIndex;
      GrfImage imageCopy;
      switch (mode)
      {
        case SpriteEditMode.Replace:
          bool isDelayed1 = this._actEditor.Act.Commands.IsDelayed;
          try
          {
            if (!isDelayed1)
              this._actEditor.Act.Commands.Begin();
            if (absoluteIndex >= this._images.Count)
            {
              this._actEditor.Act.Commands.SpriteAdd(this._images.Last<GrfImage>().GrfImageType == GrfImageType.Indexed8 ? this._convertToAny(image) : this._getImage(image, GrfImageType.Bgr32));
              break;
            }
            GrfImage imageConverted = this._getImage(image, this._images[absoluteIndex].GrfImageType);
            Spr spr = this._sprite;
            this._actEditor.Act.Commands.Backup((Action<Act>) (act =>
            {
              foreach (Layer allLayer in act.GetAllLayers())
              {
                if (allLayer.GetAbsoluteSpriteId(spr) == absoluteIndex)
                {
                  allLayer.Width = imageConverted.Width;
                  allLayer.Height = imageConverted.Height;
                }
              }
            }), "Image adjustment");
            this._actEditor.Act.Commands.SpriteReplaceAt(absoluteIndex, imageConverted);
            break;
          }
          catch (OperationCanceledException ex)
          {
            break;
          }
          catch (Exception ex)
          {
            if (!isDelayed1)
            {
              this._actEditor.Act.Commands.CancelEdit();
              ErrorHandler.HandleException(ex);
              break;
            }
            throw;
          }
          finally
          {
            if (!isDelayed1)
              this._actEditor.Act.Commands.End();
          }
        case SpriteEditMode.Remove:
          try
          {
            relativeIndex = image.GrfImageType == GrfImageType.Indexed8 ? absoluteIndex : absoluteIndex - this._sprite.NumberOfIndexed8Images;
            this._actEditor.Act.Commands.Begin();
            this._actEditor.Act.Commands.Backup((Action<Act>) (act =>
            {
              foreach (Layer allLayer in act.GetAllLayers())
              {
                if (allLayer.IsImageTypeValid(image))
                {
                  if (allLayer.SpriteIndex > relativeIndex)
                    --allLayer.SpriteIndex;
                  else if (allLayer.SpriteIndex == relativeIndex)
                  {
                    allLayer.SpriteIndex = -1;
                    allLayer.Width = 0;
                    allLayer.Height = 0;
                  }
                }
              }
            }), "Index adjustment");
            this._actEditor.Act.Commands.SpriteRemove(absoluteIndex);
            break;
          }
          catch (OperationCanceledException ex)
          {
            break;
          }
          catch (Exception ex)
          {
            this._actEditor.Act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            this._actEditor.Act.Commands.End();
          }
        case SpriteEditMode.Before:
          relativeIndex = this._images[absoluteIndex].GrfImageType == GrfImageType.Indexed8 ? absoluteIndex : absoluteIndex - this._sprite.NumberOfIndexed8Images;
          imageCopy = image.Copy();
          bool isDelayed2 = this._actEditor.Act.Commands.IsDelayed;
          try
          {
            if (!isDelayed2)
              this._actEditor.Act.Commands.Begin();
            imageCopy = relativeIndex != 0 || this._images[absoluteIndex].GrfImageType != GrfImageType.Bgra32 ? this._getImage(imageCopy, this._images[absoluteIndex].GrfImageType) : this._convertToAny(imageCopy);
            Spr spr = this._sprite;
            this._actEditor.Act.Commands.Backup((Action<Act>) (act =>
            {
              foreach (Layer allLayer in act.GetAllLayers())
              {
                if (allLayer.IsImageTypeValid(imageCopy) && allLayer.GetAbsoluteSpriteId(spr) >= absoluteIndex)
                  ++allLayer.SpriteIndex;
              }
            }), "Index adjustment");
            this._actEditor.Act.Commands.SpriteInsert(absoluteIndex, imageCopy);
            break;
          }
          catch (OperationCanceledException ex)
          {
            break;
          }
          catch (Exception ex)
          {
            if (!isDelayed2)
            {
              this._actEditor.Act.Commands.CancelEdit();
              ErrorHandler.HandleException(ex);
              break;
            }
            throw;
          }
          finally
          {
            if (!isDelayed2)
              this._actEditor.Act.Commands.End();
          }
        case SpriteEditMode.After:
          imageCopy = image.Copy();
          bool isDelayed3 = this._actEditor.Act.Commands.IsDelayed;
          try
          {
            if (!isDelayed3)
              this._actEditor.Act.Commands.Begin();
            imageCopy = this._images[absoluteIndex].GrfImageType != GrfImageType.Indexed8 || absoluteIndex != this._sprite.NumberOfIndexed8Images - 1 ? this._getImage(imageCopy, this._images[absoluteIndex].GrfImageType) : this._convertToAny(imageCopy);
            Spr spr = this._sprite;
            this._actEditor.Act.Commands.Backup((Action<Act>) (act =>
            {
              foreach (Layer allLayer in act.GetAllLayers())
              {
                if (allLayer.IsImageTypeValid(imageCopy) && allLayer.GetAbsoluteSpriteId(spr) > absoluteIndex)
                  ++allLayer.SpriteIndex;
              }
            }), "Index adjustment");
            this._actEditor.Act.Commands.SpriteInsert(absoluteIndex + 1, imageCopy);
            break;
          }
          catch (OperationCanceledException ex)
          {
            break;
          }
          catch (Exception ex)
          {
            if (!isDelayed3)
            {
              this._actEditor.Act.Commands.CancelEdit();
              ErrorHandler.HandleException(ex);
              break;
            }
            throw;
          }
          finally
          {
            if (!isDelayed3)
              this._actEditor.Act.Commands.End();
          }
        case SpriteEditMode.ReplaceFlipHorizontal:
        case SpriteEditMode.ReplaceFlipVertical:
          this._actEditor.Act.Commands.SpriteFlip(absoluteIndex, mode == SpriteEditMode.ReplaceFlipHorizontal ? FlipDirection.Horizontal : FlipDirection.Vertical);
          break;
        case SpriteEditMode.Export:
          image.SaveTo(string.Format("image_{0:000}{1}", (object) absoluteIndex, image.GrfImageType == GrfImageType.Indexed8 ? (object) ".bmp" : (object) ".png"), PathRequest.ExtractSetting);
          try
          {
            if (PathRequest.ExtractSetting.Get() is string path)
            {
              if (File.Exists(path))
              {
                OpeningService.FileOrFolder(path);
                break;
              }
              break;
            }
            break;
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex);
            break;
          }
        case SpriteEditMode.Add:
          try
          {
            this._actEditor.Act.Commands.Begin();
            imageCopy = this._convertToAny(image);
            this._actEditor.Act.Commands.SpriteAdd(imageCopy);
            break;
          }
          catch (OperationCanceledException ex)
          {
            break;
          }
          catch (Exception ex)
          {
            this._actEditor.Act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            this._actEditor.Act.Commands.End();
          }
        case SpriteEditMode.Convert:
          try
          {
            imageCopy = image.Copy();
            GrfImage imageConverted = this._getImage(imageCopy, imageCopy.GrfImageType == GrfImageType.Indexed8 ? GrfImageType.Bgra32 : GrfImageType.Indexed8);
            int relativeNewIndex = imageConverted.GrfImageType == GrfImageType.Indexed8 ? this._sprite.NumberOfIndexed8Images : this._sprite.NumberOfBgra32Images;
            Spr spr = this._sprite;
            this._actEditor.Act.Commands.Begin();
            this._actEditor.Act.Commands.Backup((Action<Act>) (act =>
            {
              this._actEditor.Act.Sprite.Remove(absoluteIndex, act, EditOption.KeepCurrentIndexes);
              SpriteTypes spriteTypes = imageConverted.GrfImageType == GrfImageType.Indexed8 ? SpriteTypes.Indexed8 : SpriteTypes.Bgra32;
              foreach (Layer layer in act.GetAllLayers().Where<Layer>((Func<Layer, bool>) (layer => layer.GetAbsoluteSpriteId(spr) == absoluteIndex)))
              {
                layer.SpriteType = spriteTypes;
                layer.SpriteIndex = relativeNewIndex;
              }
              this._actEditor.Act.Sprite.InsertAny(imageConverted);
              this._actEditor.Act.Sprite.ShiftIndexesAbove(act, image.GrfImageType, -1, this._sprite.AbsoluteToRelative(absoluteIndex, image.GrfImageType == GrfImageType.Indexed8 ? 0 : 1));
            }), "Sprite convert", true);
            break;
          }
          catch (OperationCanceledException ex)
          {
            break;
          }
          catch (Exception ex)
          {
            this._actEditor.Act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            this._actEditor.Act.Commands.End();
          }
      }
      this._visualUpdate();
    }

    private void _visualUpdate()
    {
      this._actEditor._framePreview.Update();
      this._actEditor._layerEditor.Update();
      this._actEditor.SelectionEngine.RefreshSelection();
      this._actEditor.Focus();
    }

    private IEnumerable<GrfColor> _getUsedColors(GrfImage image)
    {
      HashSet<byte> byteSet = new HashSet<byte>();
      for (int index = 0; index < image.Pixels.Length; ++index)
        byteSet.Add(image.Pixels[index]);
      byteSet.Remove((byte) 0);
      HashSet<GrfColor> usedColors = new HashSet<GrfColor>();
      foreach (byte num in byteSet)
        usedColors.Add(new GrfColor((IList<byte>) image.Palette, (int) num * 4));
      return (IEnumerable<GrfColor>) usedColors;
    }

    private GrfImage _getConvertedImage(GrfImage image)
    {
      byte[] newPalette = this._sprite.Palette == null ? (byte[]) null : this._sprite.Palette.BytePalette;
      if (this._paletteIsSet() && image.GrfImageType == GrfImageType.Indexed8 && Methods.ByteArrayCompare(newPalette, 4, 1020, image.Palette, 4))
      {
        image.SetPalette(ref newPalette);
      }
      else
      {
        if (SpriteManager.SpriteConverterOption != 0 && image.GrfImageType == GrfImageType.Indexed8)
        {
          IEnumerable<GrfColor> usedColors = this._getUsedColors(image);
          HashSet<GrfColor> paletteColors = this._getPaletteColors();
          bool flag = true;
          foreach (GrfColor grfColor in usedColors)
          {
            if (!paletteColors.Contains(grfColor))
            {
              flag = false;
              break;
            }
          }
          if (flag)
          {
            image.Palette[0] = newPalette[0];
            image.Palette[1] = newPalette[1];
            image.Palette[2] = newPalette[2];
            image.Palette[3] = newPalette[3];
            image.Convert((IImageFormatConverter) new Indexed8FormatConverter()
            {
              ExistingPalette = this._sprite.Palette.BytePalette,
              Options = Indexed8FormatConverter.PaletteOptions.UseExistingPalette
            });
            return image;
          }
        }
        if (SpriteManager.SpriteConverterOption == -2)
          throw new OperationCanceledException("Converter cancelled");
        if (image.GrfImageType != GrfImageType.Indexed8)
          image.Convert((IImageFormatConverter) new Bgra32FormatConverter());
        SpriteConverterFormatDialog converterFormatDialog = new SpriteConverterFormatDialog(this._sprite.Palette.BytePalette, image, this._sprite, SpriteManager.SpriteConverterOption);
        converterFormatDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = converterFormatDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
        {
          SpriteManager.SpriteConverterOption = converterFormatDialog.RepeatOption;
          image = converterFormatDialog.Result;
          if (image.GrfImageType == GrfImageType.Indexed8)
            this._actEditor.Act.Commands.SpriteSetPalette(image.Palette);
        }
        else
        {
          SpriteManager.SpriteConverterOption = converterFormatDialog.RepeatOption;
          throw new OperationCanceledException("Converter cancelled");
        }
      }
      return image;
    }

    private HashSet<GrfColor> _getPaletteColors()
    {
      byte[] bytePalette = this._sprite.Palette.BytePalette;
      HashSet<GrfColor> paletteColors = new HashSet<GrfColor>();
      for (int offset = 0; offset < 1024; offset += 4)
        paletteColors.Add(new GrfColor((IList<byte>) bytePalette, offset));
      return paletteColors;
    }

    private GrfImage _getImage(GrfImage image, GrfImageType desiredType)
    {
      if (image.GrfImageType == GrfImageType.Indexed8 && desiredType == GrfImageType.Indexed8)
      {
        if (this._paletteIsSet())
          image = this._getConvertedImage(image);
        return image;
      }
      if (desiredType == GrfImageType.Indexed8)
      {
        if (!this._paletteIsSet())
        {
          image.Convert((IImageFormatConverter) new Indexed8FormatConverter()
          {
            Options = Indexed8FormatConverter.PaletteOptions.AutomaticallyGeneratePalette
          });
          this._actEditor.Act.Commands.SpriteSetPalette(image.Palette);
        }
        else
        {
          image = this._getConvertedImage(image);
          if (image.GrfImageType != GrfImageType.Indexed8)
            image.Convert((IImageFormatConverter) new Indexed8FormatConverter()
            {
              ExistingPalette = this._sprite.Palette.BytePalette,
              Options = Indexed8FormatConverter.PaletteOptions.UseExistingPalette
            });
        }
        return image;
      }
      if (desiredType == GrfImageType.Bgra32 && image.GrfImageType == GrfImageType.Indexed8)
      {
        image.Palette[3] = (byte) 0;
        image.Convert((IImageFormatConverter) new Bgra32FormatConverter());
      }
      image.Convert((IImageFormatConverter) new Bgra32FormatConverter());
      return image;
    }

    private bool _paletteIsSet() => this._sprite.Palette != null && this._sprite.Palette.BytePalette != null;

    private GrfImage _convertToAny(GrfImage image)
    {
      if (image.GrfImageType == GrfImageType.Indexed8)
      {
        if (!this._paletteIsSet())
          this._actEditor.Act.Commands.SpriteSetPalette(image.Palette);
        else
          image = this._getConvertedImage(image);
        return image;
      }
      image.Convert((IImageFormatConverter) new Bgra32FormatConverter());
      return image;
    }

    public void AddDisabledMode(SpriteEditMode mode) => this._disabledModes.Add(mode);

    public bool IsModeDisabled(SpriteEditMode mode) => this._disabledModes.Any<SpriteEditMode>((Func<SpriteEditMode, bool>) (p => p == mode));
  }
}
