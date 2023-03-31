// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.SpriteConverterFormatDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using GRF;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using GRF.Image.Decoders;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles;
using Utilities.Extension;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class SpriteConverterFormatDialog : TkWindow, IComponentConnector
  {
    private readonly GrfImage _image;
    private readonly List<GrfImage> _images = new List<GrfImage>();
    private readonly List<RadioButton> _rbs = new List<RadioButton>();
    private readonly List<ScrollViewer> _svs = new List<ScrollViewer>();
    private readonly HashSet<byte> _unusedIndexes = new HashSet<byte>();
    private byte[] _originalPalette;
    private GrfImage _result;
    private bool _svEventsEnabled;
    internal TextBlock _description;
    internal ScrollViewer _sv0;
    internal System.Windows.Controls.Image _imageReal;
    internal TextBlock _tbTransparent;
    internal Rectangle _imageTransparent;
    internal ComboBox _cbTransparency;
    internal CheckBox _cbDithering;
    internal System.Windows.Controls.Image _imagePalette;
    internal RadioButton _rbOriginalPalette;
    internal ScrollViewer _sv1;
    internal System.Windows.Controls.Image _imageOriginal;
    internal RadioButton _rbMatch;
    internal ScrollViewer _sv3;
    internal System.Windows.Controls.Image _imageClosestMatch;
    internal RadioButton _rbMerge;
    internal ScrollViewer _sv4;
    internal System.Windows.Controls.Image _imageMergePalette;
    internal RadioButton _rbBgra32;
    internal ScrollViewer _sv2;
    internal System.Windows.Controls.Image _imageToBgra32;
    internal CheckBox _cbRepeat;
    internal Button _buttonOk;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public SpriteConverterFormatDialog(
      byte[] originalPalette,
      GrfImage image,
      Spr spr,
      int option = -1)
      : base("Format conflict", "app.ico", SizeToContent.Manual, ResizeMode.CanResize)
    {
      if (originalPalette == null)
        throw new ArgumentNullException(nameof (originalPalette));
      this.InitializeComponent();
      this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      this._images.Add((GrfImage) null);
      this._images.Add((GrfImage) null);
      this._images.Add((GrfImage) null);
      this._images.Add((GrfImage) null);
      this._originalPalette = originalPalette;
      this.RepeatOption = option;
      if (this.RepeatOption <= -1)
      {
        this.Visibility = Visibility.Visible;
      }
      else
      {
        this.Visibility = Visibility.Hidden;
        this.Width = 0.0;
        this.Height = 0.0;
        this.WindowStyle = WindowStyle.None;
        this.ShowInTaskbar = false;
        this.ShowActivated = false;
      }
      this._image = image;
      this._unusedIndexes = spr.GetUnusedPaletteIndexes();
      this._unusedIndexes.Remove((byte) 0);
      this._description.Text = "The image is invalid for this operation. Select one of options below.";
      this._load();
      this._cbTransparency.SelectionChanged += new SelectionChangedEventHandler(this._cbTransparency_SelectionChanged);
      this._cbDithering.Checked += new RoutedEventHandler(this._cbDithering_Checked);
      this._cbDithering.Unchecked += new RoutedEventHandler(this._cbDithering_Unchecked);
      this._setScrollViewers();
    }

    private bool _repeatBehavior { get; set; }

    public int RepeatOption { get; private set; }

    public GrfImage Result
    {
      get => this._result;
      set
      {
        if (value != null && value.GrfImageType == GrfImageType.Indexed8)
          this._imagePalette.Source = (ImageSource) ImageProvider.GetImage((MultiType) value.Palette, ".pal").Cast<BitmapSource>();
        this._result = value;
      }
    }

    private void _setScrollViewers()
    {
      this._svs.Add(this._sv0);
      this._svs.Add(this._sv1);
      this._svs.Add(this._sv2);
      this._svs.Add(this._sv3);
      this._svs.Add(this._sv4);
      this._svs.ForEach((Action<ScrollViewer>) (p => p.ScrollChanged += (ScrollChangedEventHandler) ((e, a) =>
      {
        if (!this._svEventsEnabled)
          return;
        this._setAllScrollViewers(p);
      })));
      this._svEventsEnabled = true;
    }

    private void _setAllScrollViewers(ScrollViewer sv)
    {
      this._svEventsEnabled = false;
      this._svs.ForEach((Action<ScrollViewer>) (p =>
      {
        if (sv.ScrollableWidth > 0.0)
          p.ScrollToHorizontalOffset(sv.HorizontalOffset / sv.ScrollableWidth * p.ScrollableWidth);
        if (sv.ScrollableHeight <= 0.0)
          return;
        p.ScrollToVerticalOffset(sv.VerticalOffset / sv.ScrollableHeight * p.ScrollableHeight);
      }));
      this._svEventsEnabled = true;
    }

    private void _load()
    {
      try
      {
        this._rbs.Add(this._rbOriginalPalette);
        this._rbs.Add(this._rbMatch);
        this._rbs.Add(this._rbMerge);
        this._rbs.Add(this._rbBgra32);
        this._imageReal.Source = (ImageSource) this._image.Cast<BitmapSource>();
        this._setImageDimensions((FrameworkElement) this._imageReal);
        if (this._image.GrfImageType == GrfImageType.Indexed8)
        {
          this._images[0] = this._loadFromOriginalPalette();
          this._images[0].MakeFirstPixelTransparent();
          this._imageOriginal.Source = (ImageSource) this._images[0].Cast<BitmapSource>();
        }
        else
          this._rbOriginalPalette.IsEnabled = false;
        this._setImageDimensions((FrameworkElement) this._imageOriginal);
        this._setImageDimensions((FrameworkElement) this._imageClosestMatch);
        this._setImageDimensions((FrameworkElement) this._imageMergePalette);
        this._setImageDimensions((FrameworkElement) this._imageToBgra32);
        this._tbTransparent.Text = "The transparent color is #AARRGGBB - #" + BitConverter.ToString(new byte[4]
        {
          this._originalPalette[3],
          this._originalPalette[2],
          this._originalPalette[1],
          this._originalPalette[0]
        }).Replace("-", string.Empty) + " ";
        this._imageTransparent.Fill = (Brush) new SolidColorBrush(Color.FromArgb(this._originalPalette[3], this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]));
        if (this.RepeatOption > -1)
          this._repeatBehavior = true;
        switch (GrfEditorConfiguration.FormatConflictOption)
        {
          case 0:
            this._rbOriginalPalette.IsChecked = new bool?(true);
            break;
          case 1:
            this._rbMatch.IsChecked = new bool?(true);
            break;
          case 2:
            this._rbMerge.IsChecked = new bool?(true);
            break;
          case 3:
            this._rbBgra32.IsChecked = new bool?(true);
            break;
        }
        this._cbTransparency.SelectedIndex = GrfEditorConfiguration.TransparencyMode;
        this._cbDithering.IsChecked = new bool?(GrfEditorConfiguration.UseDithering);
        this._imagePalette.Source = (ImageSource) ImageProvider.GetImage((MultiType) this._originalPalette, ".pal").Cast<BitmapSource>();
        this._update();
        this._updateSelection();
        this.Loaded += new RoutedEventHandler(this._spriteConverterFormatDialog_Loaded);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _spriteConverterFormatDialog_Loaded(object sender, RoutedEventArgs e)
    {
      if (this.RepeatOption > -1)
      {
        this._repeatBehavior = true;
        this._buttonOk_Click((object) null, (RoutedEventArgs) null);
      }
      else
        this.Visibility = Visibility.Visible;
    }

    private GrfImage _loadFromOriginalPalette()
    {
      GrfImage grfImage = this._image.Copy();
      grfImage.SetPalette(ref this._originalPalette);
      return grfImage;
    }

    private GrfImage _loadFromBestMatch(bool usePixelDithering)
    {
      GrfImage grfImage = this._image.Copy();
      Indexed8FormatConverter destinationFormat = new Indexed8FormatConverter();
      if (usePixelDithering)
        destinationFormat.Options |= Indexed8FormatConverter.PaletteOptions.UseDithering;
      destinationFormat.ExistingPalette = this._originalPalette;
      destinationFormat.BackgroundColor = GrfColor.White;
      grfImage.Convert((IImageFormatConverter) destinationFormat);
      return grfImage;
    }

    private GrfImage _showUsingBgra32Index0()
    {
      byte num1;
      byte num2;
      byte num3;
      byte num4;
      if (this._image.GrfImageType == GrfImageType.Indexed8)
      {
        num1 = this._image.Palette[0];
        num2 = this._image.Palette[1];
        num3 = this._image.Palette[2];
        num4 = this._image.Palette[3];
      }
      else
      {
        num1 = this._originalPalette[0];
        num2 = this._originalPalette[1];
        num3 = this._originalPalette[2];
        num4 = this._originalPalette[3];
      }
      GrfImage grfImage = this._image.Copy();
      grfImage.Convert((IImageFormatConverter) new Bgra32FormatConverter());
      int num5 = 0;
      for (int index = grfImage.Pixels.Length / 4; num5 < index; ++num5)
      {
        if ((int) num3 == (int) grfImage.Pixels[4 * num5] && (int) num2 == (int) grfImage.Pixels[4 * num5 + 1] && (int) num1 == (int) grfImage.Pixels[4 * num5 + 2] && (int) num4 == (int) grfImage.Pixels[4 * num5 + 3])
          grfImage.Pixels[4 * num5 + 3] = (byte) 0;
      }
      return grfImage;
    }

    protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e)
    {
    }

    private GrfImage _getImageUsingPixelZero(GrfImage image)
    {
      if (image == null || image.GrfImageType != GrfImageType.Indexed8)
        return (GrfImage) null;
      GrfImage imageUsingPixelZero = image.Copy();
      Buffer.BlockCopy((Array) this._originalPalette, 0, (Array) imageUsingPixelZero.Palette, 0, 4);
      if (this._image.GrfImageType == GrfImageType.Indexed8 && ((IEnumerable<byte>) this._image.Pixels).Any<byte>((Func<byte, bool>) (p => p == (byte) 0)))
      {
        for (int index = 0; index < imageUsingPixelZero.Pixels.Length; ++index)
        {
          if (this._image.Pixels[index] == (byte) 0)
            imageUsingPixelZero.Pixels[index] = (byte) 0;
        }
      }
      return imageUsingPixelZero;
    }

    private GrfImage _getImageUsingPixel(GrfImage image, Color color)
    {
      if (image == null || image.GrfImageType != GrfImageType.Indexed8)
        return (GrfImage) null;
      GrfImage imageUsingPixel = image.Copy();
      List<byte> byteList = new List<byte>();
      for (int index = 0; index < 256; ++index)
      {
        if ((int) image.Palette[4 * index] == (int) color.R && (int) image.Palette[4 * index + 1] == (int) color.G && (int) image.Palette[4 * index + 2] == (int) color.B)
          byteList.Add((byte) index);
      }
      Buffer.BlockCopy((Array) this._originalPalette, 0, (Array) imageUsingPixel.Palette, 0, 4);
      for (int index = 0; index < imageUsingPixel.Pixels.Length; ++index)
      {
        if (byteList.Contains(imageUsingPixel.Pixels[index]))
          imageUsingPixel.Pixels[index] = (byte) 0;
      }
      return imageUsingPixel;
    }

    private void _setImageDimensions(FrameworkElement image)
    {
      image.Width = (double) this._image.Width;
      image.Height = (double) this._image.Height;
    }

    private GrfImage _loadFromMerge(Color transparentColor, bool useDithering)
    {
      GrfImage im = this._image.Copy();
      List<byte> byteList = new List<byte>((IEnumerable<byte>) this._unusedIndexes);
      byte[] dst = new byte[1024];
      Buffer.BlockCopy((Array) this._originalPalette, 0, (Array) dst, 0, 1024);
      int count = byteList.Count;
      if (this._image.GrfImageType == GrfImageType.Indexed8)
      {
        List<byte> source = new List<byte>();
        for (int index = 0; index < 256; ++index)
        {
          if (Array.IndexOf<byte>(im.Pixels, (byte) index) > -1)
            source.Add((byte) index);
        }
        if (source.Count < count)
        {
          for (int index1 = 0; index1 < source.Count; ++index1)
          {
            byte num = source[index1];
            for (int index2 = 0; index2 < 256; ++index2)
            {
              if ((int) im.Palette[4 * (int) num] == (int) this._originalPalette[4 * index2] && (int) im.Palette[4 * (int) num + 1] == (int) this._originalPalette[4 * index2 + 1] && (int) im.Palette[4 * (int) num + 2] == (int) this._originalPalette[4 * index2 + 2])
              {
                source.Remove(num);
                --index1;
                if (byteList.Contains(num))
                {
                  byteList.Remove(num);
                  break;
                }
                break;
              }
            }
          }
        }
        else
        {
          List<Tuple<int, byte>> list = source.Select<byte, Tuple<int, byte>>((Func<byte, Tuple<int, byte>>) (t => new Tuple<int, byte>((int) im.Palette[4 * (int) t] << 16 | (int) im.Palette[4 * (int) t + 1] << 8 | (int) im.Palette[4 * (int) t + 2], t))).ToList<Tuple<int, byte>>().OrderBy<Tuple<int, byte>, int>((Func<Tuple<int, byte>, int>) (p => p.Item1)).ToList<Tuple<int, byte>>();
          List<byte> collection = new List<byte>();
          collection.Add(list[0].Item2);
          collection.Add(list[list.Count - 1].Item2);
          int num1 = byteList.Count - 2;
          int num2 = source.Count - 2;
          for (int index = 0; index < num1; ++index)
            collection.Add(list[(int) ((double) index / (double) num1 * (double) num2)].Item2);
          source = new List<byte>((IEnumerable<byte>) collection);
        }
        for (int index = 0; index < source.Count && byteList.Count > 0; ++index)
        {
          byte num = byteList[0];
          dst[4 * (int) num] = im.Palette[4 * (int) source[index]];
          dst[4 * (int) num + 1] = im.Palette[4 * (int) source[index] + 1];
          dst[4 * (int) num + 2] = im.Palette[4 * (int) source[index] + 2];
          dst[4 * (int) num + 3] = im.Palette[4 * (int) source[index] + 3];
          byteList.RemoveAt(0);
        }
      }
      else
      {
        GrfImage grfImage = im.Copy();
        Bgr32FormatConverter destinationFormat = new Bgr32FormatConverter();
        destinationFormat.BackgroundColor = GrfColor.White;
        grfImage.Convert((IImageFormatConverter) destinationFormat);
        List<int> source = new List<int>(grfImage.Pixels.Length / 4);
        for (int index3 = 0; index3 < grfImage.Pixels.Length / 4; ++index3)
        {
          int index4 = 4 * index3;
          if (grfImage.Pixels[index4 + 3] != (byte) 0)
            source.Add((int) grfImage.Pixels[index4 + 2] << 16 | (int) grfImage.Pixels[index4 + 1] << 8 | (int) grfImage.Pixels[index4]);
        }
        List<int> list = source.Distinct<int>().OrderBy<int, int>((Func<int, int>) (p => p)).ToList<int>();
        for (int index = 0; index < 256; ++index)
        {
          int num = (int) this._originalPalette[4 * index] << 16 | (int) this._originalPalette[4 * index + 1] << 8 | (int) this._originalPalette[4 * index + 2];
          if (list.Contains(num))
            list.Remove(num);
        }
        int num3 = count - 1;
        int num4 = list.Count < num3 ? list.Count : num3;
        for (int index = 0; index < num4 - 1; ++index)
        {
          byte num5 = byteList[0];
          dst[4 * (int) num5] = (byte) ((list[(int) ((double) index / (double) num4 * (double) list.Count)] & 16711680) >> 16);
          dst[4 * (int) num5 + 1] = (byte) ((list[(int) ((double) index / (double) num4 * (double) list.Count)] & 65280) >> 8);
          dst[4 * (int) num5 + 2] = (byte) (list[(int) ((double) index / (double) num4 * (double) list.Count)] & (int) byte.MaxValue);
          dst[4 * (int) num5 + 3] = byte.MaxValue;
          byteList.RemoveAt(0);
        }
        if (num4 > 0)
        {
          byte num6 = byteList[0];
          dst[4 * (int) num6] = (byte) ((list[list.Count - 1] & 16711680) >> 16);
          dst[4 * (int) num6 + 1] = (byte) ((list[list.Count - 1] & 65280) >> 8);
          dst[4 * (int) num6 + 2] = (byte) (list[list.Count - 1] & (int) byte.MaxValue);
          dst[4 * (int) num6 + 3] = byte.MaxValue;
          byteList.RemoveAt(0);
        }
      }
      Indexed8FormatConverter destinationFormat1 = new Indexed8FormatConverter();
      destinationFormat1.BackgroundColor = new GrfColor(transparentColor.A, transparentColor.R, transparentColor.G, transparentColor.B);
      if (useDithering)
        destinationFormat1.Options |= Indexed8FormatConverter.PaletteOptions.UseDithering;
      destinationFormat1.ExistingPalette = dst;
      im.Convert((IImageFormatConverter) destinationFormat1);
      return im;
    }

    private void _buttonOk_Click(object sender, RoutedEventArgs e)
    {
      if (GrfEditorConfiguration.FormatConflictOption == -1)
      {
        int num = (int) WindowProvider.ShowDialog("Please select an option or cancel.");
      }
      else
      {
        this.RepeatOption = this._repeatBehavior ? GrfEditorConfiguration.FormatConflictOption : -1;
        this.DialogResult = new bool?(true);
        this.Close();
      }
    }

    private void _buttonCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(false);
      bool? isChecked = this._cbRepeat.IsChecked;
      if ((!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0)
        this.RepeatOption = -2;
      this.Close();
    }

    private void _cbTransparency_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      GrfEditorConfiguration.TransparencyMode = this._cbTransparency.SelectedIndex;
      this._update();
      this._updateSelection();
    }

    private void _cbDithering_Checked(object sender, RoutedEventArgs e)
    {
      GrfEditorConfiguration.UseDithering = true;
      this._update();
      this._updateSelection();
    }

    private void _cbDithering_Unchecked(object sender, RoutedEventArgs e)
    {
      GrfEditorConfiguration.UseDithering = false;
      this._update();
      this._updateSelection();
    }

    private void _update()
    {
      try
      {
        GrfImage grfImage1 = (GrfImage) null;
        GrfImage grfImage2 = (GrfImage) null;
        GrfImage grfImage3 = (GrfImage) null;
        if (this._cbTransparency.SelectedIndex == 0)
        {
          grfImage3 = this._loadFromBgra32(SpriteConverterFormatDialog.Bgra32Mode.Normal);
          bool? isChecked = this._cbDithering.IsChecked;
          if ((!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0)
          {
            grfImage1 = this._loadFromBestMatch(true);
            grfImage2 = this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), true);
          }
          else
          {
            grfImage1 = this._loadFromBestMatch(false);
            grfImage2 = this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), false);
          }
        }
        else if (this._cbTransparency.SelectedIndex == 1)
        {
          grfImage3 = this._loadFromBgra32(SpriteConverterFormatDialog.Bgra32Mode.PixelIndexZero);
          bool? isChecked = this._cbDithering.IsChecked;
          if ((!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0)
          {
            grfImage1 = this._getImageUsingPixelZero(this._loadFromBestMatch(true));
            grfImage2 = this._getImageUsingPixelZero(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), true));
          }
          else
          {
            grfImage1 = this._getImageUsingPixelZero(this._loadFromBestMatch(false));
            grfImage2 = this._getImageUsingPixelZero(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), false));
          }
        }
        else if (this._cbTransparency.SelectedIndex == 2)
        {
          grfImage3 = this._loadFromBgra32(SpriteConverterFormatDialog.Bgra32Mode.PixelIndexPink);
          Color color = Color.FromArgb(byte.MaxValue, byte.MaxValue, (byte) 0, byte.MaxValue);
          bool? isChecked = this._cbDithering.IsChecked;
          if ((!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0)
          {
            grfImage1 = this._getImageUsingPixel(this._loadFromBestMatch(true), color);
            grfImage2 = this._getImageUsingPixel(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), true), color);
          }
          else
          {
            grfImage1 = this._getImageUsingPixel(this._loadFromBestMatch(false), color);
            grfImage2 = this._getImageUsingPixel(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), false), color);
          }
        }
        else if (this._cbTransparency.SelectedIndex == 3)
        {
          grfImage3 = this._loadFromBgra32(SpriteConverterFormatDialog.Bgra32Mode.FirstPixel);
          Color color = this._getColor(0);
          bool? isChecked = this._cbDithering.IsChecked;
          if ((!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0)
          {
            grfImage1 = this._getImageUsingPixel(this._loadFromBestMatch(true), color);
            grfImage2 = this._getImageUsingPixel(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), true), color);
          }
          else
          {
            grfImage1 = this._getImageUsingPixel(this._loadFromBestMatch(false), color);
            grfImage2 = this._getImageUsingPixel(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), false), color);
          }
        }
        else if (this._cbTransparency.SelectedIndex == 4)
        {
          grfImage3 = this._loadFromBgra32(SpriteConverterFormatDialog.Bgra32Mode.LastPixel);
          Color color = this._getColor(-1);
          bool? isChecked = this._cbDithering.IsChecked;
          if ((!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0)
          {
            grfImage1 = this._getImageUsingPixel(this._loadFromBestMatch(true), color);
            grfImage2 = this._getImageUsingPixel(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), true), color);
          }
          else
          {
            grfImage1 = this._getImageUsingPixel(this._loadFromBestMatch(false), color);
            grfImage2 = this._getImageUsingPixel(this._loadFromMerge(Color.FromArgb(byte.MaxValue, this._originalPalette[0], this._originalPalette[1], this._originalPalette[2]), false), color);
          }
        }
        grfImage1.MakeFirstPixelTransparent();
        grfImage2.MakeFirstPixelTransparent();
        this._imageClosestMatch.Source = (ImageSource) grfImage1.Cast<BitmapSource>();
        this._imageMergePalette.Source = (ImageSource) grfImage2.Cast<BitmapSource>();
        this._imageToBgra32.Source = (ImageSource) grfImage3.Cast<BitmapSource>();
        this._images[1] = grfImage1;
        this._images[2] = grfImage2;
        this._images[3] = grfImage3;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private Color _getColor(int pixel)
    {
      byte pixel1;
      byte pixel2;
      byte pixel3;
      byte pixel4;
      if (this._image.GrfImageType == GrfImageType.Indexed8)
      {
        pixel = pixel < 0 ? this._image.Pixels.Length - 1 : pixel;
        int pixel5 = (int) this._image.Pixels[pixel];
        pixel1 = this._image.Palette[4 * pixel5];
        pixel2 = this._image.Palette[4 * pixel5 + 1];
        pixel3 = this._image.Palette[4 * pixel5 + 2];
        pixel4 = this._image.Palette[4 * pixel5 + 3];
      }
      else
      {
        pixel = pixel < 0 ? this._image.Pixels.Length / 4 - 1 : pixel;
        pixel1 = this._image.Pixels[4 * pixel + 2];
        pixel2 = this._image.Pixels[4 * pixel + 1];
        pixel3 = this._image.Pixels[4 * pixel];
        pixel4 = this._image.Pixels[4 * pixel + 3];
      }
      return Color.FromArgb(pixel4, pixel1, pixel2, pixel3);
    }

    private GrfImage _loadFromBgra32(SpriteConverterFormatDialog.Bgra32Mode mode)
    {
      switch (mode)
      {
        case SpriteConverterFormatDialog.Bgra32Mode.Normal:
          GrfImage grfImage = this._image.Copy();
          grfImage.Convert((IImageFormatConverter) new Bgra32FormatConverter());
          return grfImage;
        case SpriteConverterFormatDialog.Bgra32Mode.PixelIndexZero:
          return this._showUsingBgra32Index0();
        case SpriteConverterFormatDialog.Bgra32Mode.PixelIndexPink:
          return this._showUsingBgra32TransparentColor(byte.MaxValue, byte.MaxValue, (byte) 0, byte.MaxValue);
        case SpriteConverterFormatDialog.Bgra32Mode.FirstPixel:
          return this._showUsingBgra32Pixel(0);
        case SpriteConverterFormatDialog.Bgra32Mode.LastPixel:
          return this._showUsingBgra32Pixel(-1);
        default:
          return (GrfImage) null;
      }
    }

    private GrfImage _showUsingBgra32Pixel(int pixel)
    {
      byte pixel1;
      byte pixel2;
      byte pixel3;
      byte pixel4;
      if (this._image.GrfImageType == GrfImageType.Indexed8)
      {
        pixel = pixel < 0 ? this._image.Pixels.Length - 1 : pixel;
        int pixel5 = (int) this._image.Pixels[pixel];
        pixel1 = this._image.Palette[4 * pixel5];
        pixel2 = this._image.Palette[4 * pixel5 + 1];
        pixel3 = this._image.Palette[4 * pixel5 + 2];
        pixel4 = this._image.Palette[4 * pixel5 + 3];
      }
      else
      {
        pixel = pixel < 0 ? this._image.Pixels.Length / 4 - 1 : pixel;
        pixel1 = this._image.Pixels[4 * pixel + 2];
        pixel2 = this._image.Pixels[4 * pixel + 1];
        pixel3 = this._image.Pixels[4 * pixel];
        pixel4 = this._image.Pixels[4 * pixel + 3];
      }
      return this._showUsingBgra32TransparentColor(pixel4, pixel1, pixel2, pixel3);
    }

    private GrfImage _showUsingBgra32TransparentColor(byte a, byte r, byte g, byte b)
    {
      GrfImage grfImage = this._image.Copy();
      grfImage.Convert((IImageFormatConverter) new Bgra32FormatConverter());
      int num = 0;
      for (int index = grfImage.Pixels.Length / 4; num < index; ++num)
      {
        if ((int) b == (int) grfImage.Pixels[4 * num] && (int) g == (int) grfImage.Pixels[4 * num + 1] && (int) r == (int) grfImage.Pixels[4 * num + 2] && (int) a == (int) grfImage.Pixels[4 * num + 3])
          grfImage.Pixels[4 * num + 3] = (byte) 0;
      }
      return grfImage;
    }

    private void _uncheckAll(object sender = null)
    {
      this._rbs.ForEach((Action<RadioButton>) (p => p.IsChecked = new bool?(false)));
      if (sender == null)
        return;
      this._rbs.ForEach((Action<RadioButton>) (p => p.Checked -= new RoutedEventHandler(this._rb_Checked)));
      ((ToggleButton) sender).IsChecked = new bool?(true);
      this._updateSelection();
      this._rbs.ForEach((Action<RadioButton>) (p => p.Checked += new RoutedEventHandler(this._rb_Checked)));
    }

    private void _updateSelection()
    {
      bool flag = true;
      bool? isChecked1 = this._rbOriginalPalette.IsChecked;
      if ((!isChecked1.GetValueOrDefault() ? 0 : (isChecked1.HasValue ? 1 : 0)) != 0)
      {
        GrfEditorConfiguration.FormatConflictOption = 0;
      }
      else
      {
        bool? isChecked2 = this._rbMatch.IsChecked;
        if ((!isChecked2.GetValueOrDefault() ? 0 : (isChecked2.HasValue ? 1 : 0)) != 0)
        {
          GrfEditorConfiguration.FormatConflictOption = 1;
        }
        else
        {
          bool? isChecked3 = this._rbMerge.IsChecked;
          if ((!isChecked3.GetValueOrDefault() ? 0 : (isChecked3.HasValue ? 1 : 0)) != 0)
          {
            GrfEditorConfiguration.FormatConflictOption = 2;
          }
          else
          {
            bool? isChecked4 = this._rbBgra32.IsChecked;
            if ((!isChecked4.GetValueOrDefault() ? 0 : (isChecked4.HasValue ? 1 : 0)) != 0)
            {
              GrfEditorConfiguration.FormatConflictOption = 3;
            }
            else
            {
              GrfEditorConfiguration.FormatConflictOption = -1;
              flag = false;
            }
          }
        }
      }
      if (flag)
        this.Result = this._images[GrfEditorConfiguration.FormatConflictOption];
      this.RepeatOption = this._repeatBehavior ? GrfEditorConfiguration.FormatConflictOption : -1;
    }

    private void _rb_Checked(object sender, RoutedEventArgs e) => this._uncheckAll(sender);

    private void _cbRepeat_Checked(object sender, RoutedEventArgs e) => this._repeatBehavior = true;

    private void _cbRepeat_Unchecked(object sender, RoutedEventArgs e) => this._repeatBehavior = false;

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/spriteconverterformatdialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._description = (TextBlock) target;
          break;
        case 2:
          this._sv0 = (ScrollViewer) target;
          break;
        case 3:
          this._imageReal = (System.Windows.Controls.Image) target;
          break;
        case 4:
          this._tbTransparent = (TextBlock) target;
          break;
        case 5:
          this._imageTransparent = (Rectangle) target;
          break;
        case 6:
          this._cbTransparency = (ComboBox) target;
          break;
        case 7:
          this._cbDithering = (CheckBox) target;
          break;
        case 8:
          this._imagePalette = (System.Windows.Controls.Image) target;
          break;
        case 9:
          this._rbOriginalPalette = (RadioButton) target;
          this._rbOriginalPalette.Checked += new RoutedEventHandler(this._rb_Checked);
          break;
        case 10:
          this._sv1 = (ScrollViewer) target;
          break;
        case 11:
          this._imageOriginal = (System.Windows.Controls.Image) target;
          break;
        case 12:
          this._rbMatch = (RadioButton) target;
          this._rbMatch.Checked += new RoutedEventHandler(this._rb_Checked);
          break;
        case 13:
          this._sv3 = (ScrollViewer) target;
          break;
        case 14:
          this._imageClosestMatch = (System.Windows.Controls.Image) target;
          break;
        case 15:
          this._rbMerge = (RadioButton) target;
          this._rbMerge.Checked += new RoutedEventHandler(this._rb_Checked);
          break;
        case 16:
          this._sv4 = (ScrollViewer) target;
          break;
        case 17:
          this._imageMergePalette = (System.Windows.Controls.Image) target;
          break;
        case 18:
          this._rbBgra32 = (RadioButton) target;
          this._rbBgra32.Checked += new RoutedEventHandler(this._rb_Checked);
          break;
        case 19:
          this._sv2 = (ScrollViewer) target;
          break;
        case 20:
          this._imageToBgra32 = (System.Windows.Controls.Image) target;
          break;
        case 21:
          this._cbRepeat = (CheckBox) target;
          this._cbRepeat.Checked += new RoutedEventHandler(this._cbRepeat_Checked);
          this._cbRepeat.Unchecked += new RoutedEventHandler(this._cbRepeat_Unchecked);
          break;
        case 22:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        case 23:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public enum Bgra32Mode
    {
      Normal,
      PixelIndexZero,
      PixelIndexPink,
      FirstPixel,
      LastPixel,
    }
  }
}
