// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.PaletteEditorTool.SpriteEditorControl
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Tools.GrfShellExplorer;
using ErrorManager;
using GRF;
using GRF.Core;
using GRF.FileFormats;
using GRF.FileFormats.PalFormat;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using GRF.Threading;
using GrfToWpfBridge.Application;
using PaletteEditor;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.Paths;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WpfBugFix;
using Utilities;
using Utilities.Commands;
using Utilities.Controls;
using Utilities.Extension;

namespace ActEditor.Tools.PaletteEditorTool
{
  public partial class SpriteEditorControl : UserControl, IComponentConnector
  {
    private readonly object _lock = new object();
    private readonly object _lockPreview = new object();
    private readonly int[] _previewCount = new int[1024];
    private readonly WpfRecentFiles _recentFiles;
    private bool _addedToRightPanel;
    private int _numShowing;
    private byte[] _palette = new byte[1024];
    private Spr _spr;
    private GrfImage _imageEditing;
    private SpriteEditorControl.EditMode _editMode;
    private bool _gradientSelection;
    private Cursor CursorBucket;
    private Cursor CursorEraser;
    private Cursor CursorStamp;
    private Cursor CursorEyedrop;
    private Cursor CursorPen;
    private int[,] _brush;
    private int _maxBrushWidth;
    internal MenuItem _menuItemOpen;
    internal MenuItem _menuItemOpenRecent;
    internal MenuItem _menuItemSave;
    internal MenuItem _menuItemSaveAs;
    internal MenuItem _menuItemClose;
    internal MenuItem _miTools;
    internal MenuItem _menuItemSwitchGradient3;
    internal MenuItem _menuItemSwitchGradient2;
    internal MenuItem _menuItemSwitchGradient1;
    internal MenuItem _menuItemSwitchGradient4;
    internal DockPanel _dpUndoRedo;
    internal ToggleMemoryButton _tmbUndo;
    internal ToggleMemoryButton _tmbRedo;
    internal SpriteViewer _spriteViewer;
    internal FancyButton _buttonSelection;
    internal FancyButton _buttonPen;
    internal FancyButton _buttonBucket;
    internal FancyButton _buttonEraser;
    internal FancyButton _buttonStamp;
    internal ComboBox _cbSpriteId;
    internal Grid _mainGrid;
    internal SingleColorEditControl _sce;
    internal GradientColorEditControl _gceControl;
    internal TextBox _focusDummy;
    private bool _contentLoaded;

    public SpriteEditorControl()
    {
      this.InitializeComponent();
      this._cbSpriteId.Margin = new Thickness(100.0, 0.0, 0.0, 0.0);
      this._cbSpriteId.SelectionChanged += new SelectionChangedEventHandler(this._cbSpriteId_SelectionChanged);
      this._spriteViewer.PixelClicked += new ImageViewer.ImageViewerEventHandler(this._spriteViewer_PixelClicked);
      this._spriteViewer.PixelMoved += new ImageViewer.ImageViewerEventHandler(this._spriteViewer_PixelMoved);
      this._recentFiles = new WpfRecentFiles(Configuration.ConfigAsker, 6, this._menuItemOpenRecent, "Sprite editor");
      this._recentFiles.FileClicked += (RecentFilesManager.RFMFileClickedEventHandler) (f => this._openFile(new TkPath(f)));
      this._mainGrid.IsEnabled = false;
      this.AllowDrop = true;
      this._spriteViewer.DragEnter += new DragEventHandler(this._spriteEditorControl_DragEnter);
      this._spriteViewer.DragOver += new DragEventHandler(this._spriteEditorControl_DragEnter);
      this._spriteViewer.Drop += new DragEventHandler(this._spriteEditorControl_Drop);
      this._spriteViewer.MouseMove += new MouseEventHandler(this._spriteViewer_MouseMove);
      this._spriteViewer.MouseEnter += (MouseEventHandler) delegate
      {
        this._setCursor(this._editMode);
        this._paintBrush();
      };
      this._spriteViewer.MouseLeave += (MouseEventHandler) delegate
      {
        Mouse.OverrideCursor = (Cursor) null;
      };
      this._spriteViewer.LostMouseCapture += new MouseEventHandler(this._spriteViewer_LostMouseCapture);
      this._spriteViewer.MouseLeftButtonUp += new MouseButtonEventHandler(this._spriteViewer_MouseLeftButtonUp);
      this.PreviewKeyDown += new KeyEventHandler(this._spriteEditorControl_PreviewKeyDown);
      this.PreviewKeyUp += new KeyEventHandler(this._spriteEditorControl_PreviewKeyDown);
      this._spriteViewer.PreviewKeyDown += new KeyEventHandler(this._spriteEditorControl_PreviewKeyDown);
      this._spriteViewer.PreviewKeyUp += new KeyEventHandler(this._spriteEditorControl_PreviewKeyDown);
      this._mainGrid.SizeChanged += new SizeChangedEventHandler(this._mainTabControl_SizeChanged);
      this._sce.PaletteSelector.SelectionChanged += new ObservableList.ObservableListEventHandler(this._paletteSelector_SelectionChanged);
      this._gceControl.PaletteSelector.SelectionChanged += new ObservableList.ObservableListEventHandler(this._paletteSelector_SelectionChanged);
      ApplicationShortcut.Link(ApplicationShortcut.Save, (Action) (() => this._menuItemSave_Click((object) null, (RoutedEventArgs) null)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.Make("Decrease brush size", Key.Subtract, ModifierKeys.None), (Action) (() => this._brushIncrease(-1)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.Make("Increase brush size", Key.Add, ModifierKeys.None), (Action) (() => this._brushIncrease(1)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.FromString("Ctrl-Q", "Brush - Select"), (Action) (() => this._buttonSelection_Click((object) this._buttonSelection, (RoutedEventArgs) null)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.FromString("Ctrl-B", "Brush - Bucket"), (Action) (() => this._buttonBucket_Click((object) this._buttonBucket, (RoutedEventArgs) null)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.FromString("Ctrl-S", "Brush - Stamp"), (Action) (() => this._buttonStamp_Click((object) this._buttonStamp, (RoutedEventArgs) null)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.FromString("Ctrl-E", "Brush - Eraser"), (Action) (() => this._buttonEraser_Click((object) this._buttonEraser, (RoutedEventArgs) null)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.FromString("Ctrl-P", "Brush - Pen"), (Action) (() => this._buttonPen_Click((object) this._buttonPen, (RoutedEventArgs) null)), (FrameworkElement) this);
      this.Loaded += (RoutedEventHandler) delegate
      {
        Window parentControl = WpfUtilities.FindParentControl<Window>((DependencyObject) this);
        if (parentControl != null)
          parentControl.StateChanged += (EventHandler) ((e, a) => this._mainTabControl_SizeChanged((object) null, (SizeChangedEventArgs) null));
        Keyboard.Focus((IInputElement) this._focusDummy);
        this._focusDummy.Focus();
        this._buttonSelection.IsPressed = true;
        this._sce.PaletteSelector.Margin = new Thickness(270.0, 5.0, 2.0, 2.0);
        Grid parent = (Grid) this._sce.PaletteSelector.Parent;
        parent.Children.Remove((UIElement) this._sce.PaletteSelector);
        parent.Children.Remove((UIElement) this._sce.PickerControl);
        this._gceControl.PrimaryGrid.Children.Add((UIElement) this._sce.PaletteSelector);
        this._gceControl.PrimaryGrid.Children.Add((UIElement) this._sce.PickerControl);
        this._gceControl.PickerControl.Visibility = Visibility.Visible;
        this._gceControl.Panel.Visibility = Visibility.Visible;
        this._gceControl.GradientGrid.Visibility = Visibility.Visible;
        this._sce.PaletteSelector.GotFocus += (RoutedEventHandler) delegate
        {
          this._gradientSelection = false;
          this._sce.PickerControl.Visibility = Visibility.Visible;
          this._gceControl.PickerControl.Visibility = Visibility.Hidden;
          this._gceControl.Panel.Visibility = Visibility.Hidden;
          this._gceControl.GradientGrid.Visibility = Visibility.Hidden;
        };
        this._gceControl.PaletteSelector.GotFocus += (RoutedEventHandler) delegate
        {
          this._gradientSelection = true;
          this._sce.PickerControl.Visibility = Visibility.Hidden;
          this._gceControl.PickerControl.Visibility = Visibility.Visible;
          this._gceControl.Panel.Visibility = Visibility.Visible;
          this._gceControl.GradientGrid.Visibility = Visibility.Visible;
        };
        this._brushIncrease(0);
      };
    }

    private void _brushIncrease(int amount)
    {
      try
      {
        GrfEditorConfiguration.BrushSize += amount;
        if (GrfEditorConfiguration.BrushSize > 15)
          GrfEditorConfiguration.BrushSize = 15;
        if (GrfEditorConfiguration.BrushSize < 0)
          GrfEditorConfiguration.BrushSize = 0;
        this._generateBrush();
        Point position = Mouse.GetPosition((IInputElement) this._spriteViewer._imageSprite);
        position.X /= this._spriteViewer._imageSprite.Width;
        position.Y /= this._spriteViewer._imageSprite.Height;
        this._spriteViewer_PixelMoved((object) this, (int) ((double) this._spriteViewer.Bitmap.PixelWidth * position.X), (int) ((double) this._spriteViewer.Bitmap.PixelHeight * position.Y), true);
      }
      catch
      {
      }
    }

    private void _spriteViewer_PixelMoved(object sender, int x0, int y0, bool isWithin)
    {
      switch (this._editMode)
      {
        case SpriteEditorControl.EditMode.Stamp:
          if (!this._gradientSelection || !this._getGce().PaletteSelector.SelectedItem.HasValue)
            break;
          GrfImage image1 = this._spr.Images[this._cbSpriteId.Dispatch<ComboBox, int>((Func<ComboBox, int>) (p => p.SelectedIndex))];
          image1 = image1.Copy();
          for (int index1 = 0; index1 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index1)
          {
            for (int index2 = 0; index2 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index2)
            {
              if (this._brush[index1, index2] != 0)
              {
                int num1 = index1 - GrfEditorConfiguration.BrushSize + x0;
                int num2 = index2 - GrfEditorConfiguration.BrushSize + y0;
                if (num1 >= 0 && num1 < image1.Width && num2 >= 0 && num2 < image1.Height)
                {
                  int num3 = this._getGce().PaletteSelector.SelectedItem.Value / 8;
                  int index3 = num1 + image1.Width * num2;
                  int pixel = (int) image1.Pixels[index3];
                  image1.Pixels[index3] = (byte) (num3 * 8 + pixel % 8);
                }
              }
            }
          }
          this._spriteViewer.Dispatch<SpriteViewer>((Action<SpriteViewer>) (p => p.LoadImage(image1)));
          break;
        case SpriteEditorControl.EditMode.Eraser:
          GrfImage image2 = this._spr.Images[this._cbSpriteId.Dispatch<ComboBox, int>((Func<ComboBox, int>) (p => p.SelectedIndex))];
          image2 = image2.Copy();
          for (int index4 = 0; index4 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index4)
          {
            for (int index5 = 0; index5 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index5)
            {
              if (this._brush[index4, index5] != 0)
              {
                int num4 = index4 - GrfEditorConfiguration.BrushSize + x0;
                int num5 = index5 - GrfEditorConfiguration.BrushSize + y0;
                if (num4 >= 0 && num4 < image2.Width && num5 >= 0 && num5 < image2.Height)
                {
                  int index6 = num4 + image2.Width * num5;
                  image2.Pixels[index6] = (byte) 0;
                }
              }
            }
          }
          this._spriteViewer.Dispatch<SpriteViewer>((Action<SpriteViewer>) (p => p.LoadImage(image2)));
          break;
      }
    }

    private void _paintBrush()
    {
    }

    private void _spriteEditorControl_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.SystemKey == Key.LeftAlt)
      {
        e.Handled = true;
        if (!this._spriteViewer.IsMouseOver)
          return;
        if (e.IsDown)
          this._setCursor(SpriteEditorControl.EditMode.EyeDrop);
        else
          this._setCursor(this._editMode);
      }
      else
      {
        if (!this._spriteViewer.IsMouseOver)
          return;
        this._setCursor(this._editMode);
      }
    }

    private void _spriteViewer_LostMouseCapture(object sender, MouseEventArgs e)
    {
      if (this._spr == null)
        return;
      if (this._imageEditing != null)
      {
        this._spr.Palette.Commands.StoreAndExecute((IPaletteCommand) new ImageModifiedCommand(this._spr, this._cbSpriteId.SelectedIndex, this._imageEditing));
        this._imageEditing = (GrfImage) null;
      }
      this._spr.Palette.Commands.End();
    }

    public Spr Sprite => this._spr;

    private void _mainTabControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      int top = (int) ((this._mainGrid.ActualHeight - 550.0) / 2.0) - 0;
      this._sce.Margin = new Thickness(0.0, (double) top, 0.0, 0.0);
      this._gceControl.Margin = new Thickness(0.0, (double) top, 0.0, 0.0);
    }

    private void _spriteEditorControl_Drop(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop, true) || !(e.Data.GetData(DataFormats.FileDrop, true) is string[] data) || data.Length != 1)
        return;
      if (!data[0].IsExtension(".spr"))
        return;
      try
      {
        this._openFile(new TkPath(data[0]));
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _spriteEditorControl_DragEnter(object sender, DragEventArgs e)
    {
      e.Handled = true;
      if (e.Data.GetDataPresent(DataFormats.FileDrop, true) && e.Data.GetData(DataFormats.FileDrop, true) is string[] data && data.Length == 1)
      {
        if (data[0].IsExtension(".spr"))
        {
          e.Effects = DragDropEffects.Move;
          return;
        }
      }
      e.Effects = DragDropEffects.None;
    }

    private void _spriteViewer_PixelClicked(object sender, int x, int y, bool isWithin)
    {
      if (this._editMode != SpriteEditorControl.EditMode.Select && !Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
      {
        this._draw(x, y);
      }
      else
      {
        if (!isWithin)
          return;
        GrfImage image = this._spr.Images[this._cbSpriteId.SelectedIndex];
        this._sce.PaletteSelector.SelectedItem = new int?((int) image.Pixels[y * image.Width + x]);
        this._gceControl.PaletteSelector.SelectedItems.Clear();
        this._gceControl.PaletteSelector.AddSelection((int) image.Pixels[y * image.Width + x]);
      }
    }

    private void _draw(int x, int y)
    {
      try
      {
        if (this._editMode != SpriteEditorControl.EditMode.Eraser)
        {
          if (this._editMode == SpriteEditorControl.EditMode.Stamp)
          {
            if (!this._getGce().PaletteSelector.SelectedItem.HasValue)
              throw new Exception("You must select 1 gradient to use the Stamp tool.");
          }
          else if (this._getSce().PaletteSelector.SelectedItems.Count != 1)
            throw new Exception("You must select 1 color to use the pen.");
        }
        this._spriteViewer.CaptureMouse();
        this._spriteViewer_MouseMove((object) null, (MouseEventArgs) null);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _spriteViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (this._spriteViewer.IsMouseCaptured)
        this._spriteViewer.ReleaseMouseCapture();
      if (this._imageEditing == null)
        return;
      this._spr.Palette.Commands.StoreAndExecute((IPaletteCommand) new ImageModifiedCommand(this._spr, this._cbSpriteId.SelectedIndex, this._imageEditing));
      this._imageEditing = (GrfImage) null;
    }

    private void _spriteViewer_MouseMove(object sender, MouseEventArgs e)
    {
      if (Mouse.LeftButton == MouseButtonState.Released && Mouse.RightButton == MouseButtonState.Released && !this._spriteViewer.IsMouseCaptured && Keyboard.IsKeyDown(Key.LeftAlt))
        this._setCursor(SpriteEditorControl.EditMode.EyeDrop);
      else if (Mouse.LeftButton == MouseButtonState.Released && Mouse.RightButton == MouseButtonState.Released && !this._spriteViewer.IsMouseCaptured && !Keyboard.IsKeyDown(Key.LeftAlt))
        this._setCursor(this._editMode);
      if (!this._spriteViewer.IsMouseCaptured || this._editMode == SpriteEditorControl.EditMode.Select || Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      Point position = Mouse.GetPosition((IInputElement) this._spriteViewer._imageSprite);
      position.X /= this._spriteViewer._imageSprite.Width;
      position.Y /= this._spriteViewer._imageSprite.Height;
      int x = (int) ((double) this._spriteViewer.Bitmap.PixelWidth * position.X);
      int y = (int) ((double) this._spriteViewer.Bitmap.PixelHeight * position.Y);
      if (position.X >= 0.0 && position.X < 1.0 && position.Y >= 0.0 && position.Y < 1.0)
      {
        if (this._editMode == SpriteEditorControl.EditMode.Bucket)
        {
          if (this._imageEditing == null)
            this._imageEditing = this._spr.Images[this._cbSpriteId.SelectedIndex].Copy();
          if ((int) this._imageEditing.Pixels[y * this._imageEditing.Width + x] == (int) (byte) this._getSce().PaletteSelector.SelectedItems[0])
            return;
          this._colorConnected(this._imageEditing, x, y, this._imageEditing.Pixels[y * this._imageEditing.Width + x], (byte) this._getSce().PaletteSelector.SelectedItems[0]);
          this._spriteViewer.ForceUpdatePreview(this._imageEditing.Cast<BitmapSource>());
        }
        else if (this._editMode == SpriteEditorControl.EditMode.Pen)
        {
          if (this._imageEditing == null)
            this._imageEditing = this._spr.Images[this._cbSpriteId.SelectedIndex].Copy();
          if ((int) this._imageEditing.Pixels[y * this._imageEditing.Width + x] == (int) (byte) this._getSce().PaletteSelector.SelectedItems[0])
            return;
          this._imageEditing.Pixels[y * this._imageEditing.Width + x] = (byte) this._getSce().PaletteSelector.SelectedItems[0];
          this._spriteViewer.ForceUpdatePreview(this._imageEditing.Cast<BitmapSource>());
        }
      }
      if (this._editMode == SpriteEditorControl.EditMode.Stamp)
      {
        if (!this._gradientSelection || !this._getGce().PaletteSelector.SelectedItem.HasValue)
          throw new Exception("Please select a gradient for the Stamp tool.");
        if (this._imageEditing == null)
          this._imageEditing = this._spr.Images[this._cbSpriteId.SelectedIndex].Copy();
        bool flag = false;
        for (int index1 = 0; index1 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index1)
        {
          for (int index2 = 0; index2 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index2)
          {
            if (this._brush[index1, index2] != 0)
            {
              int num1 = index1 - GrfEditorConfiguration.BrushSize + x;
              int num2 = index2 - GrfEditorConfiguration.BrushSize + y;
              if (num1 >= 0 && num1 < this._imageEditing.Width && num2 >= 0 && num2 < this._imageEditing.Height)
              {
                int num3 = this._getGce().PaletteSelector.SelectedItem.Value / 8;
                int index3 = num1 + this._imageEditing.Width * num2;
                int pixel = (int) this._imageEditing.Pixels[index3];
                int num4 = (int) (byte) (num3 * 8 + pixel % 8);
                if (pixel != 0)
                {
                  if ((int) this._imageEditing.Pixels[index3] != num4)
                    flag = true;
                  this._imageEditing.Pixels[index3] = (byte) (num3 * 8 + pixel % 8);
                }
              }
            }
          }
        }
        if (!flag && sender != null)
          return;
        this._spriteViewer.ForceUpdatePreview(this._imageEditing.Cast<BitmapSource>());
      }
      if (this._editMode != SpriteEditorControl.EditMode.Eraser)
        return;
      if (this._imageEditing == null)
        this._imageEditing = this._spr.Images[this._cbSpriteId.SelectedIndex].Copy();
      bool flag1 = false;
      for (int index4 = 0; index4 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index4)
      {
        for (int index5 = 0; index5 < 2 * GrfEditorConfiguration.BrushSize + 1; ++index5)
        {
          if (this._brush[index4, index5] != 0)
          {
            int num5 = index4 - GrfEditorConfiguration.BrushSize + x;
            int num6 = index5 - GrfEditorConfiguration.BrushSize + y;
            if (num5 >= 0 && num5 < this._imageEditing.Width && num6 >= 0 && num6 < this._imageEditing.Height)
            {
              int index6 = num5 + this._imageEditing.Width * num6;
              if (this._imageEditing.Pixels[index6] != (byte) 0)
              {
                if (this._imageEditing.Pixels[index6] != (byte) 0)
                  flag1 = true;
                this._imageEditing.Pixels[index6] = (byte) 0;
              }
            }
          }
        }
      }
      if (!flag1 && sender != null)
        return;
      this._spriteViewer.ForceUpdatePreview(this._imageEditing.Cast<BitmapSource>());
    }

    private void _colorConnected(GrfImage image, int x, int y, byte target, byte newIndex)
    {
      if (x < 0 || x >= image.Width || y < 0 || y >= image.Height || (int) image.Pixels[y * image.Width + x] != (int) target)
        return;
      image.Pixels[y * image.Width + x] = newIndex;
      this._colorConnected(image, x - 1, y, target, newIndex);
      this._colorConnected(image, x + 1, y, target, newIndex);
      this._colorConnected(image, x, y - 1, target, newIndex);
      this._colorConnected(image, x, y + 1, target, newIndex);
    }

    private void _cbSpriteId_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._cbSpriteId.SelectedIndex < 0)
        this._spriteViewer.Clear();
      else
        this._spriteViewer.LoadIndexed8(this._cbSpriteId.SelectedIndex);
    }

    private bool _openFromFile(string file)
    {
      try
      {
        if (file.IsExtension(".spr"))
        {
          Spr spr = new Spr((MultiType) file);
          if (spr.NumberOfIndexed8Images <= 0)
            throw new Exception("The sprite file does not contain a palette (probably because it doesn't have any Indexed8 images). You must add one for a palette to be created.");
          this._recentFiles.AddRecentFile(file);
          this._set(spr);
        }
        else
        {
          if (!file.IsExtension(".pal"))
            return false;
          Pal pal = new Pal(File.ReadAllBytes(file));
          pal.BytePalette[3] = (byte) 0;
          if (this._spr == null)
            throw new Exception("No sprite has been loaded yet.");
          this._recentFiles.AddRecentFile(file);
          this._spr.Palette.Commands.SetPalette(pal.BytePalette);
        }
        this._mainGrid.IsEnabled = true;
        return true;
      }
      catch (Exception ex)
      {
        this._recentFiles.RemoveRecentFile(file);
        ErrorHandler.HandleException(ex);
      }
      this._mainGrid.IsEnabled = false;
      return false;
    }

    private void _set(Spr spr)
    {
      this._spr = spr;
      this._spriteViewer.SetSpr(spr);
      this._cbSpriteId.Items.Clear();
      for (int newItem = 0; newItem < this._spr.NumberOfIndexed8Images; ++newItem)
        this._cbSpriteId.Items.Add((object) newItem);
      this._cbSpriteId.SelectedIndex = 0;
      if (this._spr.Palette == null)
        this._spr.Palette = new Pal();
      this._tmbUndo.SetUndo<IPaletteCommand>((AbstractCommand<IPaletteCommand>) this._spr.Palette.Commands);
      this._tmbRedo.SetRedo<IPaletteCommand>((AbstractCommand<IPaletteCommand>) this._spr.Palette.Commands);
      this._spr.Palette.PaletteChanged += new Pal.PalEventHandler(this._pal_PaletteChanged);
      this._sce.SetPalette(this._spr.Palette);
      this._gceControl.SetPalette(this._spr.Palette);
    }

    private void _pal_PaletteChanged(object sender) => this._spriteViewer.LoadIndexed8(this._cbSpriteId.SelectedIndex);

    private void _paletteSelector_SelectionChanged(object sender, ObservabableListEventArgs args)
    {
      if (!this._gradientSelection && args.Items.Count == 1)
      {
        GrfThread.Start((Action) (() => this._showSelectedPixel(args)));
        this._sce.PaletteSelector.GridFocus.Focus();
        Keyboard.Focus((IInputElement) this._sce.PaletteSelector.GridFocus);
      }
      else
      {
        if (!this._gradientSelection || args.Items.Count <= 1)
          return;
        GrfThread.Start((Action) (() => this._showSelectedPixel(args)));
        this._gceControl.PaletteSelector.GridFocus.Focus();
        Keyboard.Focus((IInputElement) this._gceControl.PaletteSelector.GridFocus);
      }
    }

    private void _showSelectedPixel(ObservabableListEventArgs args)
    {
      List<int> intList = new List<int>();
      lock (this._lock)
      {
        using (List<int>.Enumerator enumerator = intList.GetEnumerator())
        {
label_9:
          while (enumerator.MoveNext())
          {
            int current = enumerator.Current;
            while (true)
            {
              int num;
              lock (this._lockPreview)
                num = this._previewCount[current];
              if (num > 0)
                Thread.Sleep(50);
              else
                goto label_9;
            }
          }
        }
        lock (this._lockPreview)
        {
          foreach (int index in (IEnumerable) args.Items)
          {
            ++this._previewCount[index];
            intList.Add(index);
          }
        }
        if (intList.Count == 0)
          return;
        if (this._numShowing == 0)
          Buffer.BlockCopy((Array) this._spr.Palette.BytePalette, 0, (Array) this._palette, 0, 1024);
        ++this._numShowing;
      }
      if (args.Action == ObservableListEventType.Added || args.Action == ObservableListEventType.Modified)
      {
        for (int index1 = 0; index1 <= 20; ++index1)
        {
          double num1 = (20.0 - (double) index1) / 20.0;
          GrfImage image = this._spr.Images[this._cbSpriteId.Dispatch<ComboBox, int>((Func<ComboBox, int>) (p => p.SelectedIndex))];
          image = image.Copy();
          foreach (int num2 in intList)
          {
            int index2 = 4 * num2;
            this._palette[index2] = (byte) (num1 * (double) ((int) byte.MaxValue - (int) image.Palette[index2]) + (double) image.Palette[index2]);
            this._palette[index2 + 1] = (byte) (num1 * (double) -image.Palette[index2 + 1] + (double) image.Palette[index2 + 1]);
            this._palette[index2 + 2] = (byte) (num1 * (double) -image.Palette[index2 + 2] + (double) image.Palette[index2 + 2]);
          }
          lock (this._lockPreview)
          {
            for (int index3 = 0; index3 < intList.Count; ++index3)
            {
              int index4 = intList[index3];
              if (this._previewCount[index4] > 1)
              {
                --this._previewCount[index4];
                intList.RemoveAt(index3);
                --index3;
              }
            }
          }
          if (intList.Count != 0)
          {
            image.SetPalette(ref this._palette);
            this._spriteViewer.Dispatch<SpriteViewer>((Action<SpriteViewer>) (p => p.LoadImage(image)));
            Thread.Sleep(50);
          }
          else
            break;
        }
      }
      lock (this._lock)
      {
        foreach (int index in intList)
        {
          lock (this._lockPreview)
            --this._previewCount[index];
        }
        --this._numShowing;
      }
    }

    private bool _openFile(TkPath file)
    {
      try
      {
        if (string.IsNullOrEmpty(file.RelativePath))
          return this._openFromFile(file.FilePath);
        if (!File.Exists(file.FilePath))
        {
          this._recentFiles.RemoveRecentFile(file.GetFullPath());
          return false;
        }
        this._recentFiles.AddRecentFile(file.GetFullPath());
        TkPath tkPath = new TkPath(file);
        byte[] numArray = (byte[]) null;
        using (GrfHolder grfHolder = new GrfHolder(file.FilePath))
        {
          if (grfHolder.FileTable.ContainsFile(file.RelativePath))
            numArray = grfHolder.FileTable[file.RelativePath].GetDecompressedData();
        }
        if (numArray == null)
        {
          ErrorHandler.HandleException("File not found : " + (string) file);
          return false;
        }
        if (tkPath.RelativePath.IsExtension(".spr"))
        {
          Spr spr = new Spr((MultiType) numArray);
          spr.LoadedPath = tkPath.ToString();
          if (spr.NumberOfIndexed8Images <= 0)
            throw new Exception("The sprite file does not contain a palette (probably because it doesn't have any Indexed8 images). You must add one for a palette to be created.");
          this._recentFiles.AddRecentFile(tkPath.ToString());
          this._set(spr);
          this._mainGrid.IsEnabled = true;
          return true;
        }
        if (tkPath.RelativePath.IsExtension(".pal"))
        {
          Pal pal = new Pal(numArray);
          pal.BytePalette[3] = (byte) 0;
          this._recentFiles.AddRecentFile(tkPath.ToString());
          this._spr.Palette.Commands.SetRawBytesInPalette(0, pal.BytePalette);
          return true;
        }
        ErrorHandler.HandleException("File format not supported : " + (string) file);
        return false;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      return false;
    }

    private void _miOpenFromGrf(string grfPath)
    {
      try
      {
        string str1 = grfPath;
        if (str1 == null)
          str1 = PathRequest.OpenGrfFile("filter", FileFormat.MergeFilters(Format.AllContainers, Format.Grf, Format.Gpf, Format.Thor));
        string str2 = str1;
        if (str2 == null)
          return;
        GrfExplorer grfExplorer = new GrfExplorer(str2, SelectMode.Pal);
        grfExplorer.Owner = WpfUtilities.TopWindow;
        this.IsEnabled = false;
        try
        {
          bool? nullable = grfExplorer.ShowDialog();
          if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
            return;
          string selectedItem = grfExplorer.SelectedItem;
          if (selectedItem == null)
            return;
          if (!selectedItem.IsExtension(".pal", ".spr"))
            throw new Exception("Only PAL or SPR files can be selected.");
          this._openFile(new TkPath(str2, selectedItem));
        }
        finally
        {
          this.IsEnabled = true;
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _menuItemOpen_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string str = TkPathRequest.OpenFile(new Setting((Action<object>) (v => Configuration.ConfigAsker["[ActEditor - App recent]"] = v.ToString()), (Func<object>) (() => (object) Configuration.ConfigAsker["[ActEditor - App recent]", "C:\\"])), "filter", "All files|*.pal;*.spr;*.grf;*.gpf;*.thor");
        if (str == null)
          return;
        if (str.IsExtension(".grf", ".thor", ".gpf"))
          this._miOpenFromGrf(str);
        else
          this._openFile(new TkPath(str));
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public void SaveAs(string file)
    {
      try
      {
        if (this._spr == null || file == null)
          return;
        if (file.Contains("?"))
          throw new Exception("The file couldn't be saved because of an invalid location (you cannot save inside a GRF).");
        if (file.IsExtension(".spr"))
        {
          try
          {
            this._spr.Palette.EnableRaiseEvents = false;
            this._spr.Palette.MakeFirstColorUnique();
            this._spr.Palette[3] = byte.MaxValue;
            this._spr.Converter.Save(this._spr, file.ReplaceExtension(".spr"));
            this._spr.Palette[3] = (byte) 0;
          }
          finally
          {
            this._spr.Palette.EnableRaiseEvents = true;
          }
        }
        else
        {
          try
          {
            this._spr.Palette.EnableRaiseEvents = false;
            this._spr.Palette.MakeFirstColorUnique();
            this._spr.Palette[3] = byte.MaxValue;
            this._spr.Palette.Save(file.ReplaceExtension(".pal"));
            this._spr.Palette[3] = (byte) 0;
          }
          finally
          {
            this._spr.Palette.EnableRaiseEvents = true;
          }
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _menuItemSave_Click(object sender, RoutedEventArgs e) => this.Save();

    private void _menuItemClose_Click(object sender, RoutedEventArgs e) => WpfUtilities.FindParentControl<Window>((DependencyObject) this).Close();

    private void _menuItemSaveAs_Click(object sender, RoutedEventArgs e) => this.SaveAs(TkPathRequest.SaveFile(new Setting((Action<object>) (v => Configuration.ConfigAsker["[ActEditor - App recent]"] = v.ToString()), (Func<object>) (() => (object) Configuration.ConfigAsker["[ActEditor - App recent]", "C:\\"])), "filter", FileFormat.MergeFilters(Format.PalAndSpr)));

    public void Save()
    {
      try
      {
        if (this._spr == null)
          return;
        this.SaveAs(this._spr.LoadedPath);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public bool Open(string file)
    {
      try
      {
        Spr spr = new Spr((MultiType) file);
        if (spr.NumberOfIndexed8Images <= 0)
          throw new Exception("The sprite file does not contain a palette (probably because it doesn't have any Indexed8 images). You must add one for a palette to be created.");
        this._set(spr);
        this._mainGrid.IsEnabled = true;
        return true;
      }
      catch (Exception ex)
      {
        this._recentFiles.RemoveRecentFile(file);
        ErrorHandler.HandleException(ex);
      }
      this._mainGrid.IsEnabled = false;
      return false;
    }

    private SingleColorEditControl _getSce() => this._sce;

    private GradientColorEditControl _getGce() => this._gceControl;

    private void _menuItemSwitchGradient_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        bool flag1 = sender == this._menuItemSwitchGradient1 || sender == this._menuItemSwitchGradient2;
        bool flag2 = sender == this._menuItemSwitchGradient1 || sender == this._menuItemSwitchGradient3;
        if (!this._gradientSelection)
        {
          if (this._getSce().PaletteSelector.SelectedItems.Count != 2)
            throw new Exception("You must select two colors to switch them.");
          try
          {
            this._spr.Palette.Commands.BeginNoDelay();
            Spr oldSprite = new Spr(this._spr);
            Spr newSprite = new Spr(this._spr);
            byte selectedItem1 = (byte) this._getSce().PaletteSelector.SelectedItems[0];
            byte selectedItem2 = (byte) this._getSce().PaletteSelector.SelectedItems[1];
            for (int index1 = 0; index1 < newSprite.NumberOfIndexed8Images; ++index1)
            {
              GrfImage image = newSprite.Images[index1];
              for (int index2 = 0; index2 < image.Pixels.Length; ++index2)
              {
                if ((int) image.Pixels[index2] == (int) selectedItem1)
                  image.Pixels[index2] = selectedItem2;
                else if ((int) image.Pixels[index2] == (int) selectedItem2)
                  image.Pixels[index2] = selectedItem1;
              }
            }
            byte[] numArray1 = new byte[4];
            byte[] numArray2 = new byte[4];
            Buffer.BlockCopy((Array) this._spr.Palette.BytePalette, (int) selectedItem1 * 4, (Array) numArray1, 0, 4);
            Buffer.BlockCopy((Array) this._spr.Palette.BytePalette, (int) selectedItem2 * 4, (Array) numArray2, 0, 4);
            if (flag2)
            {
              this._spr.Palette.Commands.SetRawBytesInPalette((int) selectedItem1 * 4, numArray2);
              this._spr.Palette.Commands.SetRawBytesInPalette((int) selectedItem2 * 4, numArray1);
            }
            if (!flag1)
              return;
            this._spr.Palette.Commands.StoreAndExecute((IPaletteCommand) new SpriteModifiedCommand(this._spr, oldSprite, newSprite));
          }
          finally
          {
            this._spr.Palette.Commands.End();
          }
        }
        else
        {
          if (!this._gradientSelection)
            return;
          if (this._getGce().PaletteSelector.SelectedItems.Count != 16)
            throw new Exception("You must select two gradients to switch them.");
          try
          {
            this._spr.Palette.Commands.BeginNoDelay();
            Spr oldSprite = new Spr(this._spr);
            Spr newSprite = new Spr(this._spr);
            byte selectedItem3 = (byte) this._getGce().PaletteSelector.SelectedItems[0];
            byte selectedItem4 = (byte) this._getGce().PaletteSelector.SelectedItems[8];
            for (int index3 = 0; index3 < newSprite.NumberOfIndexed8Images; ++index3)
            {
              GrfImage image = newSprite.Images[index3];
              for (int index4 = 0; index4 < image.Pixels.Length; ++index4)
              {
                if ((int) image.Pixels[index4] >= (int) selectedItem3 && (int) image.Pixels[index4] < (int) selectedItem3 + 8)
                  image.Pixels[index4] = (byte) ((uint) selectedItem4 + (uint) image.Pixels[index4] - (uint) selectedItem3);
                else if ((int) image.Pixels[index4] >= (int) selectedItem4 && (int) image.Pixels[index4] < (int) selectedItem4 + 8)
                  image.Pixels[index4] = (byte) ((uint) selectedItem3 + (uint) image.Pixels[index4] - (uint) selectedItem4);
              }
            }
            byte[] numArray3 = new byte[32];
            byte[] numArray4 = new byte[32];
            Buffer.BlockCopy((Array) this._spr.Palette.BytePalette, (int) selectedItem3 * 4, (Array) numArray3, 0, 32);
            Buffer.BlockCopy((Array) this._spr.Palette.BytePalette, (int) selectedItem4 * 4, (Array) numArray4, 0, 32);
            if (flag2)
            {
              this._spr.Palette.Commands.SetRawBytesInPalette((int) selectedItem3 * 4, numArray4);
              this._spr.Palette.Commands.SetRawBytesInPalette((int) selectedItem4 * 4, numArray3);
            }
            if (!flag1)
              return;
            this._spr.Palette.Commands.StoreAndExecute((IPaletteCommand) new SpriteModifiedCommand(this._spr, oldSprite, newSprite));
          }
          finally
          {
            this._spr.Palette.Commands.End();
          }
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _menuItemSwitchGradient4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (this._gradientSelection)
          return;
        if (this._getSce().PaletteSelector.SelectedItems.Count != 2)
          throw new Exception("You must select two colors to switch them.");
        try
        {
          this._spr.Palette.Commands.BeginNoDelay();
          Spr oldSprite = new Spr(this._spr);
          Spr newSprite = new Spr(this._spr);
          byte selectedItem1 = (byte) this._getSce().PaletteSelector.SelectedItems[0];
          byte selectedItem2 = (byte) this._getSce().PaletteSelector.SelectedItems[1];
          for (int index1 = 0; index1 < newSprite.NumberOfIndexed8Images; ++index1)
          {
            GrfImage image = newSprite.Images[index1];
            for (int index2 = 0; index2 < image.Pixels.Length; ++index2)
            {
              if ((int) image.Pixels[index2] == (int) selectedItem1)
                image.Pixels[index2] = selectedItem2;
            }
          }
          byte[] dst1 = new byte[4];
          byte[] dst2 = new byte[4];
          Buffer.BlockCopy((Array) this._spr.Palette.BytePalette, (int) selectedItem1 * 4, (Array) dst1, 0, 4);
          Buffer.BlockCopy((Array) this._spr.Palette.BytePalette, (int) selectedItem2 * 4, (Array) dst2, 0, 4);
          this._spr.Palette.Commands.StoreAndExecute((IPaletteCommand) new SpriteModifiedCommand(this._spr, oldSprite, newSprite));
        }
        finally
        {
          this._spr.Palette.Commands.End();
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _buttonBucket_Click(object sender, RoutedEventArgs e)
    {
      this._buttonSelect((FancyButton) sender);
      this._setEditMode(SpriteEditorControl.EditMode.Bucket);
    }

    private void _setEditMode(SpriteEditorControl.EditMode mode)
    {
      this._editMode = mode;
      this._generateBrush();
      if (this._editMode != SpriteEditorControl.EditMode.Select)
        return;
      GrfImage image = this._spr.Images[this._cbSpriteId.Dispatch<ComboBox, int>((Func<ComboBox, int>) (p => p.SelectedIndex))];
      image = image.Copy();
      this._spriteViewer.Dispatch<SpriteViewer>((Action<SpriteViewer>) (p => p.LoadImage(image)));
    }

    private void _setCursor(SpriteEditorControl.EditMode mode)
    {
      switch (mode)
      {
        case SpriteEditorControl.EditMode.Select:
          Mouse.OverrideCursor = (Cursor) null;
          break;
        case SpriteEditorControl.EditMode.Bucket:
          if (this.CursorBucket == null)
          {
            System.Windows.Controls.Image element = new System.Windows.Controls.Image();
            element.Source = (ImageSource) ApplicationManager.GetResourceImage("cs_bucket.png");
            element.Width = 16.0;
            element.Height = 16.0;
            this.CursorBucket = CursorHelper.CreateCursor((UIElement) element, new Point()
            {
              X = 14.0,
              Y = 15.0
            });
          }
          if (this.CursorBucket == null)
            break;
          Mouse.OverrideCursor = this.CursorBucket;
          break;
        case SpriteEditorControl.EditMode.EyeDrop:
          if (this.CursorEyedrop == null)
          {
            System.Windows.Controls.Image element = new System.Windows.Controls.Image();
            element.Source = (ImageSource) ApplicationManager.GetResourceImage("cs_eyedrop.png");
            element.Width = 16.0;
            element.Height = 16.0;
            this.CursorEyedrop = CursorHelper.CreateCursor((UIElement) element, new Point()
            {
              X = 2.0,
              Y = 14.0
            });
          }
          if (this.CursorEyedrop == null)
            break;
          Mouse.OverrideCursor = this.CursorEyedrop;
          break;
        case SpriteEditorControl.EditMode.Stamp:
          if (this.CursorStamp == null)
          {
            System.Windows.Controls.Image element = new System.Windows.Controls.Image();
            element.Source = (ImageSource) ApplicationManager.GetResourceImage("cs_brush.png");
            element.Width = 16.0;
            element.Height = 16.0;
            this.CursorStamp = CursorHelper.CreateCursor((UIElement) element, new Point()
            {
              X = 9.0,
              Y = 8.0
            });
          }
          if (this.CursorStamp == null)
            break;
          Mouse.OverrideCursor = this.CursorStamp;
          break;
        case SpriteEditorControl.EditMode.Eraser:
          if (this.CursorEraser == null)
          {
            System.Windows.Controls.Image element = new System.Windows.Controls.Image();
            element.Source = (ImageSource) ApplicationManager.GetResourceImage("cs_eraser.png");
            element.Width = 16.0;
            element.Height = 16.0;
            this.CursorEraser = CursorHelper.CreateCursor((UIElement) element, new Point()
            {
              X = 8.0,
              Y = 8.0
            });
          }
          if (this.CursorEraser == null)
            break;
          Mouse.OverrideCursor = this.CursorEraser;
          break;
        case SpriteEditorControl.EditMode.Pen:
          if (this.CursorPen == null)
          {
            System.Windows.Controls.Image element = new System.Windows.Controls.Image();
            element.Source = (ImageSource) ApplicationManager.GetResourceImage("cs_pen.png");
            element.Width = 16.0;
            element.Height = 16.0;
            this.CursorPen = CursorHelper.CreateCursor((UIElement) element, new Point()
            {
              X = 9.0,
              Y = 8.0
            });
          }
          if (this.CursorPen == null)
            break;
          Mouse.OverrideCursor = this.CursorPen;
          break;
      }
    }

    private void _setPixel(int x, int y)
    {
      if (y < 0 || y >= 2 * GrfEditorConfiguration.BrushSize + 1 || x < 0 || x >= 2 * GrfEditorConfiguration.BrushSize + 1)
        return;
      this._brush[y, x] = 1;
    }

    private void _setBrushPixel(int cx, int cy, int x, int y)
    {
      this._horizontalLine(cx - x, cy + y, cx + x);
      if (y == 0)
        return;
      this._horizontalLine(cx - x, cy - y, cx + x);
    }

    private void _horizontalLine(int x0, int y0, int x1)
    {
      for (int x = x0; x <= x1; ++x)
        this._setPixel(x, y0);
    }

    private void _generateBrush()
    {
      this._brush = new int[2 * GrfEditorConfiguration.BrushSize + 1, 2 * GrfEditorConfiguration.BrushSize + 1];
      int brushSize1 = GrfEditorConfiguration.BrushSize;
      int num1 = -brushSize1;
      int num2 = brushSize1;
      int num3 = 0;
      int brushSize2 = GrfEditorConfiguration.BrushSize;
      int brushSize3 = GrfEditorConfiguration.BrushSize;
      while (num2 >= num3)
      {
        int num4 = num3;
        int num5 = num1 + num3;
        ++num3;
        num1 = num5 + num3;
        this._setBrushPixel(brushSize2, brushSize3, num2, num4);
        if (num1 >= 0)
        {
          if (num2 != num4)
            this._setBrushPixel(brushSize2, brushSize3, num4, num2);
          int num6 = num1 - num2;
          --num2;
          num1 = num6 - num2;
        }
      }
    }

    private void _buttonSelect(FancyButton exceptionButton)
    {
      FancyButton[] fancyButtonArray = new FancyButton[5]
      {
        this._buttonPen,
        this._buttonSelection,
        this._buttonBucket,
        this._buttonStamp,
        this._buttonEraser
      };
      foreach (FancyButton fancyButton in fancyButtonArray)
      {
        if (fancyButton != exceptionButton)
          fancyButton.IsPressed = false;
      }
      exceptionButton.IsPressed = true;
    }

    private void _buttonPen_Click(object sender, RoutedEventArgs e)
    {
      this._buttonSelect((FancyButton) sender);
      this._setEditMode(SpriteEditorControl.EditMode.Pen);
    }

    private void _buttonSelection_Click(object sender, RoutedEventArgs e)
    {
      this._buttonSelect((FancyButton) sender);
      this._setEditMode(SpriteEditorControl.EditMode.Select);
    }

    private void _buttonBrush_Click(object sender, RoutedEventArgs e)
    {
      this._buttonSelect((FancyButton) sender);
      this._setEditMode(SpriteEditorControl.EditMode.Brush);
    }

    private void _buttonStamp_Click(object sender, RoutedEventArgs e)
    {
      this._buttonSelect((FancyButton) sender);
      this._setEditMode(SpriteEditorControl.EditMode.Stamp);
    }

    private void _buttonEraser_Click(object sender, RoutedEventArgs e)
    {
      this._buttonSelect((FancyButton) sender);
      this._setEditMode(SpriteEditorControl.EditMode.Eraser);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/Act Editor;component/tools/paletteeditortool/spriteeditorcontrol.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._menuItemOpen = (MenuItem) target;
          this._menuItemOpen.Click += new RoutedEventHandler(this._menuItemOpen_Click);
          break;
        case 2:
          this._menuItemOpenRecent = (MenuItem) target;
          break;
        case 3:
          this._menuItemSave = (MenuItem) target;
          this._menuItemSave.Click += new RoutedEventHandler(this._menuItemSave_Click);
          break;
        case 4:
          this._menuItemSaveAs = (MenuItem) target;
          this._menuItemSaveAs.Click += new RoutedEventHandler(this._menuItemSaveAs_Click);
          break;
        case 5:
          this._menuItemClose = (MenuItem) target;
          this._menuItemClose.Click += new RoutedEventHandler(this._menuItemClose_Click);
          break;
        case 6:
          this._miTools = (MenuItem) target;
          break;
        case 7:
          this._menuItemSwitchGradient3 = (MenuItem) target;
          this._menuItemSwitchGradient3.Click += new RoutedEventHandler(this._menuItemSwitchGradient_Click);
          break;
        case 8:
          this._menuItemSwitchGradient2 = (MenuItem) target;
          this._menuItemSwitchGradient2.Click += new RoutedEventHandler(this._menuItemSwitchGradient_Click);
          break;
        case 9:
          this._menuItemSwitchGradient1 = (MenuItem) target;
          this._menuItemSwitchGradient1.Click += new RoutedEventHandler(this._menuItemSwitchGradient_Click);
          break;
        case 10:
          this._menuItemSwitchGradient4 = (MenuItem) target;
          this._menuItemSwitchGradient4.Click += new RoutedEventHandler(this._menuItemSwitchGradient4_Click);
          break;
        case 11:
          this._dpUndoRedo = (DockPanel) target;
          break;
        case 12:
          this._tmbUndo = (ToggleMemoryButton) target;
          break;
        case 13:
          this._tmbRedo = (ToggleMemoryButton) target;
          break;
        case 14:
          this._spriteViewer = (SpriteViewer) target;
          break;
        case 15:
          this._buttonSelection = (FancyButton) target;
          this._buttonSelection.Click += new RoutedEventHandler(this._buttonSelection_Click);
          break;
        case 16:
          this._buttonPen = (FancyButton) target;
          this._buttonPen.Click += new RoutedEventHandler(this._buttonPen_Click);
          break;
        case 17:
          this._buttonBucket = (FancyButton) target;
          this._buttonBucket.Click += new RoutedEventHandler(this._buttonBucket_Click);
          break;
        case 18:
          this._buttonEraser = (FancyButton) target;
          this._buttonEraser.Click += new RoutedEventHandler(this._buttonEraser_Click);
          break;
        case 19:
          this._buttonStamp = (FancyButton) target;
          this._buttonStamp.Click += new RoutedEventHandler(this._buttonStamp_Click);
          break;
        case 20:
          this._cbSpriteId = (ComboBox) target;
          break;
        case 21:
          this._mainGrid = (Grid) target;
          break;
        case 22:
          this._sce = (SingleColorEditControl) target;
          break;
        case 23:
          this._gceControl = (GradientColorEditControl) target;
          break;
        case 24:
          this._focusDummy = (TextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public enum EditMode
    {
      Select,
      Bucket,
      EyeDrop,
      Brush,
      Stamp,
      Eraser,
      Pen,
    }
  }
}
