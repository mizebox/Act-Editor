// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.SpriteSelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ErrorManager;
using GRF;
using GRF.FileFormats;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.FileFormats.SprFormat;
using GRF.FileFormats.SprFormat.Commands;
using GRF.Image;
using GRF.Threading;
using GrfToWpfBridge;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities.Commands;
using Utilities.Extension;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class SpriteSelector : UserControl, IComponentConnector
  {
    private readonly List<ImageDraw> _imageDraws = new List<ImageDraw>();
    private readonly Type[] _updateCommandTypes = new Type[5]
    {
      typeof (Flip),
      typeof (SpriteCommand),
      typeof (RemoveCommand),
      typeof (Insert),
      typeof (ChangePalette)
    };
    private IPreviewEditor _actEditor;
    private int _previousPosition;
    internal Grid _gridBackground;
    internal ScrollViewer _sv;
    internal TkMenuItem _miAdd;
    internal DockPanel _dp;
    internal Line _lineMoveLayer;
    private bool _contentLoaded;

    public SpriteSelector()
    {
      this.InitializeComponent();
      this.SizeChanged += (SizeChangedEventHandler) delegate
      {
        this._updateBackground();
      };
      this._sv.MouseRightButtonUp += new MouseButtonEventHandler(this._sv_MouseRightButtonUp);
      this.DragEnter += new DragEventHandler(this._spriteSelector_DragEnter);
      this.DragOver += new DragEventHandler(this._spriteSelector_DragOver);
      this.DragLeave += new DragEventHandler(this._spriteSelector_DragLeave);
      this.Drop += new DragEventHandler(this._spriteSelector_Drop);
      this._dp.Background = (Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorSpriteBackgroundColor);
      this._sv.ScrollChanged += (ScrollChangedEventHandler) delegate
      {
        this._sv.ScrollToHorizontalOffset((double) (int) this._sv.HorizontalOffset);
      };
    }

    private void _spriteSelector_Drop(object sender, DragEventArgs e)
    {
      try
      {
        if (!this._isImageDragged(e) || !(e.Data.GetData(DataFormats.FileDrop, true) is string[] data1))
          return;
        List<string> list = ((IEnumerable<string>) data1).ToList<string>();
        list.Reverse();
        try
        {
          SpriteManager.SpriteConverterOption = -1;
          try
          {
            foreach (string path in list.Where<string>((Func<string, bool>) (p => p.IsExtension(".bmp", ".tga", ".jpg", ".png"))))
            {
              byte[] data2 = File.ReadAllBytes(path);
              GrfImage image = new GrfImage(ref data2);
              if (this._previousPosition == this._childrenCount())
              {
                if (this._previousPosition == 0)
                  this._actEditor.SpriteManager.Execute(0, image, SpriteEditMode.Add);
                else
                  this._actEditor.SpriteManager.Execute(this._previousPosition - 1, image, SpriteEditMode.After);
              }
              else
                this._actEditor.SpriteManager.Execute(this._previousPosition, image, SpriteEditMode.Before);
            }
          }
          catch (OperationCanceledException ex)
          {
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex);
          }
          try
          {
            this._actEditor.Act.Commands.BeginNoDelay();
            foreach (string sprData in list.Where<string>((Func<string, bool>) (p => p.IsExtension(".spr"))))
            {
              Spr spr = new Spr((MultiType) sprData);
              this._actEditor.Act.Commands.Backup((Action<Act>) (act => { }), "Adding sprite from file");
              try
              {
                List<GrfImage> images = spr.Images;
                if (this._childrenCount() != 0)
                  images.Reverse();
                foreach (GrfImage image in images)
                {
                  if (this._previousPosition == this._childrenCount())
                  {
                    if (this._previousPosition == 0)
                      this._actEditor.SpriteManager.Execute(0, image, SpriteEditMode.Add);
                    else
                      this._actEditor.SpriteManager.Execute(this._previousPosition - 1, image, SpriteEditMode.After);
                  }
                  else
                    this._actEditor.SpriteManager.Execute(this._previousPosition, image, SpriteEditMode.Before);
                }
              }
              catch (OperationCanceledException ex)
              {
              }
              catch (Exception ex)
              {
                ErrorHandler.HandleException(ex);
              }
            }
          }
          catch (Exception ex)
          {
            this._actEditor.Act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
          }
          finally
          {
            this._actEditor.Act.Commands.End();
          }
        }
        catch (OperationCanceledException ex)
        {
        }
        finally
        {
          SpriteManager.SpriteConverterOption = -1;
          this._lineMoveLayer.Visibility = Visibility.Hidden;
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._lineMoveLayer.Visibility = Visibility.Hidden;
      }
    }

    private void _spriteSelector_DragLeave(object sender, DragEventArgs e)
    {
      this._lineMoveLayer.Visibility = Visibility.Hidden;
      this._previousPosition = -1;
    }

    private bool _isImageDragged(DragEventArgs e)
    {
      if (this._actEditor.Act != null && e.Data.GetDataPresent(DataFormats.FileDrop, true) && e.Data.GetData(DataFormats.FileDrop, true) is string[] data)
      {
        Func<string, bool> predicate = (Func<string, bool>) (p => p.IsExtension(".bmp", ".tga", ".jpg", ".png", ".spr"));
        if (((IEnumerable<string>) data).Any<string>(predicate) && !e.Data.GetDataPresent("ImageIndex"))
          return true;
      }
      return false;
    }

    private void _spriteSelector_DragOver(object sender, DragEventArgs e)
    {
      if (!this._isImageDragged(e) || !this._isImageDragged(e))
        return;
      double num1 = 0.0;
      IInputElement control = this._dp.InputHitTest(e.GetPosition((IInputElement) this._dp));
      int num2;
      if (!((control is Border ? control : (IInputElement) WpfUtilities.FindDirectParentControl<Border>(control as FrameworkElement)) is Border element))
      {
        if (this._previousPosition < 0)
          this._previousPosition = 0;
        num2 = this._previousPosition;
      }
      else
      {
        num2 = this._dp.Children.IndexOf((UIElement) element);
        this._previousPosition = num2;
      }
      if (num2 < 0)
      {
        this._lineMoveLayer.Visibility = Visibility.Hidden;
      }
      else
      {
        for (int index = 0; index < num2; ++index)
          num1 += ((FrameworkElement) this._dp.Children[index]).ActualWidth;
        double num3 = num1 - this._sv.HorizontalOffset;
        this._lineMoveLayer.Visibility = Visibility.Visible;
        this._lineMoveLayer.Stroke = (Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorSpriteSelectionBorder.ToColor());
        this._lineMoveLayer.Margin = new Thickness(num3 - 2.0, 0.0, 0.0, SystemParameters.HorizontalScrollBarHeight);
      }
    }

    private int _childrenCount() => this._dp.Children.Count - 1;

    private void _spriteSelector_DragEnter(object sender, DragEventArgs e)
    {
      try
      {
        if (this._isImageDragged(e))
        {
          this._previousPosition = this._childrenCount();
          e.Effects = DragDropEffects.All;
        }
        else
          e.Effects = DragDropEffects.None;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _sv_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (this._childrenCount() == 0 && this._actEditor.Act != null)
        return;
      e.Handled = true;
    }

    private void _updateBackground() => ((TileBrush) this._gridBackground.Background).Viewport = new Rect(0.0, 0.0, 16.0 / this._gridBackground.ActualWidth, 16.0 / this._gridBackground.ActualHeight);

    public void Init(IPreviewEditor actEditor)
    {
      this._actEditor = actEditor;
      this._actEditor.ActLoaded += new ActEditorWindow.ActEditorEventDelegate(this._actEditor_ActLoaded);
    }

    private void _actEditor_ActLoaded(object sender)
    {
      if (this._actEditor.Act == null)
        return;
      this._actEditor.Act.Commands.CommandExecuted += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._commandChanged);
      this._actEditor.Act.Commands.CommandRedo += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._commandChanged);
      this._actEditor.Act.Commands.CommandUndo += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._commandChanged);
      this._actEditor.Act.SpriteVisualInvalidated += (Act.InvalidateVisualDelegate) (e => this._commandChanged((object) null, (IActCommand) null));
      this._actEditor.Act.SpritePaletteInvalidated += (Act.InvalidateVisualDelegate) (e => this._commandChanged((object) null, (IActCommand) null));
      this.Update();
    }

    private void _commandChanged(object sender, IActCommand command)
    {
      ActGroupCommand cmd = command as ActGroupCommand;
      if (cmd != null)
      {
        if (!((IEnumerable<Type>) this._updateCommandTypes).Any<Type>((Func<Type, bool>) (type => cmd.Commands.Any<IActCommand>((Func<IActCommand, bool>) (c => type == c.GetType())))))
          return;
        this.InternalUpdate();
      }
      else if (command != null && ((IEnumerable<Type>) this._updateCommandTypes).Any<Type>((Func<Type, bool>) (type => command.GetType() == type)))
      {
        this.InternalUpdate();
      }
      else
      {
        if (command != null)
          return;
        this.InternalUpdate();
      }
    }

    public void PaletteUpdate()
    {
      int num = this._childrenCount();
      if (this._actEditor.Act == null)
      {
        this.Reset();
      }
      else
      {
        for (int index = 0; index < num; ++index)
          this._imageDraws[index].QuickRender((IPreview) this._actEditor.FramePreview);
      }
    }

    public void InternalUpdate()
    {
      int num = this._childrenCount();
      if (this._actEditor.Act == null)
      {
        this.Reset();
      }
      else
      {
        for (; num > this._actEditor.Act.Sprite.NumberOfImagesLoaded; --num)
        {
          int index = this._childrenCount() - 1;
          this._dp.Children.RemoveAt(index);
          this._imageDraws.RemoveAt(index);
        }
        for (int index = 0; index < num; ++index)
          this._imageDraws[index].Render((IPreview) this._actEditor.FramePreview);
        int imageIndex = num;
        for (int numberOfImagesLoaded = this._actEditor.Act.Sprite.NumberOfImagesLoaded; imageIndex < numberOfImagesLoaded; ++imageIndex)
        {
          ImageDraw imageDraw1 = new ImageDraw(imageIndex, this._actEditor);
          imageDraw1.IsSelectable = true;
          ImageDraw imageDraw2 = imageDraw1;
          this._imageDraws.Add(imageDraw2);
          imageDraw2.Render((IPreview) this._actEditor.FramePreview);
          this._dp.Children.Insert(this._childrenCount(), (UIElement) imageDraw2.Border);
        }
      }
    }

    public void Update() => this.InternalUpdate();

    public void DeselectAllExcept(ImageDraw imageDraw)
    {
      foreach (ImageDraw imageDraw1 in this._imageDraws)
      {
        if (!object.ReferenceEquals((object) imageDraw1, (object) imageDraw))
          imageDraw1.IsSelected = false;
      }
    }

    public void DeselectAll() => this.DeselectAllExcept((ImageDraw) null);

    public void Reset()
    {
      while (this._dp.Children.Count > 1)
      {
        this._dp.Children.RemoveAt(0);
        this._imageDraws.RemoveAt(0);
      }
    }

    private void _miAdd_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string[] source = PathRequest.OpenFilesExtract("filter", FileFormat.MergeFilters(Format.Image));
        if (source == null)
          return;
        if (source.Length <= 0)
          return;
        try
        {
          try
          {
            this._actEditor.Act.Commands.BeginNoDelay();
            SpriteManager.SpriteConverterOption = -1;
            try
            {
              List<GrfImage> list = ((IEnumerable<string>) source).Where<string>((Func<string, bool>) (p => p.IsExtension(".bmp", ".jpg", ".png", ".tga"))).Select<string, GrfImage>((Func<string, GrfImage>) (file1 => new GrfImage((MultiType) file1))).ToList<GrfImage>();
              int absoluteIndex = -1;
              foreach (GrfImage image in list)
              {
                if (absoluteIndex < 0)
                  this._actEditor.SpriteManager.Execute(0, image, SpriteEditMode.Add);
                else
                  this._actEditor.SpriteManager.Execute(absoluteIndex, image, SpriteEditMode.After);
                ++absoluteIndex;
              }
            }
            catch (OperationCanceledException ex)
            {
            }
          }
          catch (Exception ex)
          {
            this._actEditor.Act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
          }
          finally
          {
            this._actEditor.Act.Commands.End();
            SpriteManager.SpriteConverterOption = -1;
          }
        }
        catch (Exception ex)
        {
          ErrorHandler.HandleException(ex);
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public void Select(int absoluteIndex)
    {
      this.DeselectAll();
      if (absoluteIndex >= this._imageDraws.Count)
        return;
      GrfThread.Start((System.Action) (() => this._highlight(this._imageDraws[absoluteIndex])));
      double num = 0.0;
      for (int index = 0; index < absoluteIndex; ++index)
        num += ((FrameworkElement) this._dp.Children[index]).ActualWidth;
      this._sv.ScrollToHorizontalOffset(num + ((FrameworkElement) this._dp.Children[absoluteIndex]).ActualWidth / 2.0 - this._sv.ViewportWidth / 2.0);
    }

    private void _highlight(ImageDraw imageDraw)
    {
      if (imageDraw.IsPreviewing)
        return;
      try
      {
        imageDraw.IsPreviewing = true;
        for (int index = 0; index <= 20; ++index)
        {
          double colorFactor = (20.0 - (double) index) / 20.0;
          imageDraw.Dispatch<ImageDraw, Brush>((Func<ImageDraw, Brush>) (p => p.Overlay.Fill = (Brush) new SolidColorBrush(Color.FromArgb((byte) (colorFactor * (double) byte.MaxValue), byte.MaxValue, (byte) 0, (byte) 0))));
          Thread.Sleep(50);
        }
      }
      catch
      {
      }
      finally
      {
        imageDraw.Dispatch<ImageDraw, Brush>((Func<ImageDraw, Brush>) (p => p.Overlay.Fill = (Brush) Brushes.Transparent));
        imageDraw.IsPreviewing = false;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/spriteselector.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._gridBackground = (Grid) target;
          break;
        case 2:
          this._sv = (ScrollViewer) target;
          break;
        case 3:
          this._miAdd = (TkMenuItem) target;
          this._miAdd.Click += new RoutedEventHandler(this._miAdd_Click);
          break;
        case 4:
          this._dp = (DockPanel) target;
          break;
        case 5:
          this._lineMoveLayer = (Line) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
