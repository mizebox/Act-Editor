// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.PaletteEditorTool.ImageViewer
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utilities.Tools;

namespace ActEditor.Tools.PaletteEditorTool
{
  public partial class ImageViewer : UserControl, IComponentConnector
  {
    private readonly ZoomEngine _zoomEngine = new ZoomEngine()
    {
      ZoomInMultiplier = (Func<double>) (() => (double) GrfEditorConfiguration.ActEditorZoomInMultiplier)
    };
    private BitmapSource _currentBitmap;
    private Point? _oldPosition;
    private Point _relativeCenter = new Point(0.5, 0.5);
    internal Grid _primary;
    internal Border _borderSpriteGlow;
    internal Border _borderSprite;
    internal Image _imageSprite;
    internal Grid _gridZoom;
    internal ComboBox _cbZoom;
    internal Border _spriteOverlay;
    private bool _contentLoaded;

    public BitmapSource Bitmap => this._currentBitmap;

    public ZoomEngine ZoomEngine => this._zoomEngine;

    public Point RelativeCenter
    {
      get => this._relativeCenter;
      set => this._relativeCenter = value;
    }

    public int CenterX => (int) (this._primary.ActualWidth * this._relativeCenter.X);

    public int CenterY => (int) (this._primary.ActualHeight * this._relativeCenter.Y);

    public Image PreviewImage => this._imageSprite;

    public ImageViewer()
    {
      this.InitializeComponent();
      this._cbZoom.SelectedIndex = 3;
      this.KeyDown += new KeyEventHandler(this._spriteViewer_KeyDown);
      this._primary.PreviewMouseWheel += new MouseWheelEventHandler(this._scrollViewer_MouseWheel);
      this._primary.PreviewMouseDown += new MouseButtonEventHandler(this._scrollViewer_PreviewMouseDown);
      this._primary.PreviewMouseMove += new MouseEventHandler(this._scrollViewer_PreviewMouseMove);
      this._primary.SizeChanged += (SizeChangedEventHandler) delegate
      {
        this._updatePreview();
      };
      this.PreviewMouseDown += new MouseButtonEventHandler(this._imageViewer_PreviewMouseDown);
      this.PreviewMouseMove += new MouseEventHandler(this._imageViewer_PreviewMouseMove);
      this.PreviewMouseUp += new MouseButtonEventHandler(this._imageViewer_PreviewMouseUp);
      this.MouseUp += (MouseButtonEventHandler) delegate
      {
        this.Cursor = Cursors.Arrow;
      };
    }

    private void _imageViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      this.ReleaseMouseCapture();
      this._oldPosition = new Point?();
    }

    private void _imageViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e) => this._oldPosition = new Point?(e.GetPosition((IInputElement) this));

    public event ImageViewer.ImageViewerEventHandler PixelClicked;

    public event ImageViewer.ImageViewerEventHandler PixelMoved;

    protected virtual void OnPixelMoved(int x, int y, bool isWithin)
    {
      ImageViewer.ImageViewerEventHandler pixelMoved = this.PixelMoved;
      if (pixelMoved == null)
        return;
      pixelMoved((object) this, x, y, isWithin);
    }

    public void OnPixelClicked(int x, int y, bool isWithin)
    {
      ImageViewer.ImageViewerEventHandler pixelClicked = this.PixelClicked;
      if (pixelClicked == null)
        return;
      pixelClicked((object) this, x, y, isWithin);
    }

    private void _imageViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
        return;
      if (!this.IsMouseCaptured)
        this.CaptureMouse();
      if (!this._oldPosition.HasValue)
        return;
      Point point = this._oldPosition.Value;
      Point position = e.GetPosition((IInputElement) this);
      double num1 = position.X - point.X;
      double num2 = position.Y - point.Y;
      this._relativeCenter.X += num1 / this._primary.ActualWidth;
      this._relativeCenter.Y += num2 / this._primary.ActualHeight;
      this._oldPosition = new Point?(position);
      this._updatePreview();
    }

    private void _scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      Point position = Mouse.GetPosition((IInputElement) this._imageSprite);
      position.X /= this._imageSprite.Width;
      position.Y /= this._imageSprite.Height;
      bool isWithin = position.X >= 0.0 && position.X < 1.0 && position.Y >= 0.0 && position.Y < 1.0;
      if (e.LeftButton == MouseButtonState.Pressed && this._currentBitmap != null)
      {
        this.OnPixelClicked((int) ((double) this._currentBitmap.PixelWidth * position.X), (int) ((double) this._currentBitmap.PixelHeight * position.Y), isWithin);
      }
      else
      {
        if (this._currentBitmap == null)
          return;
        this.OnPixelMoved((int) ((double) this._currentBitmap.PixelWidth * position.X), (int) ((double) this._currentBitmap.PixelHeight * position.Y), isWithin);
      }
    }

    private void _scrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e) => this._scrollViewer_PreviewMouseMove(sender, (MouseEventArgs) e);

    private void _scrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed)
        return;
      this._zoomEngine.Zoom((double) e.Delta);
      Point position = e.GetPosition((IInputElement) this._primary);
      double num1 = position.X / this._primary.ActualWidth - this._relativeCenter.X;
      double num2 = position.Y / this._primary.ActualHeight - this._relativeCenter.Y;
      this._relativeCenter.X = position.X / this._primary.ActualWidth - num1 / this._zoomEngine.OldScale * this._zoomEngine.Scale;
      this._relativeCenter.Y = position.Y / this._primary.ActualHeight - num2 / this._zoomEngine.OldScale * this._zoomEngine.Scale;
      this._cbZoom.SelectedIndex = -1;
      this._cbZoom.Text = this._zoomEngine.ScaleText;
      this._updatePreview();
    }

    private void _spriteViewer_KeyDown(object sender, KeyEventArgs e)
    {
    }

    public void Load(BitmapSource image)
    {
      ((TileBrush) this._borderSprite.Background).Viewport = new Rect(0.0, 0.0, 16.0 / (double) image.PixelWidth, 16.0 / (double) image.PixelHeight);
      this._currentBitmap = image;
      this._updatePreview();
    }

    private void _updatePreview()
    {
      this._imageSprite.Source = (ImageSource) this._currentBitmap;
      this._imageSprite.Width = (this._currentBitmap == null ? 32.0 : (double) this._currentBitmap.PixelWidth) * this._zoomEngine.Scale;
      this._imageSprite.Height = (this._currentBitmap == null ? 32.0 : (double) this._currentBitmap.PixelHeight) * this._zoomEngine.Scale;
      double num1 = this._imageSprite.Width / 2.0;
      double num2 = this._imageSprite.Height / 2.0;
      double left = (double) this.CenterX - num1;
      double top = (double) this.CenterY - num2;
      this._borderSprite.Width = this._imageSprite.Width;
      this._borderSprite.Height = this._imageSprite.Height;
      this._borderSpriteGlow.Width = this._imageSprite.Width;
      this._borderSpriteGlow.Height = this._imageSprite.Height;
      this._imageSprite.Margin = new Thickness(left, top, 0.0, 0.0);
      this._borderSprite.Margin = new Thickness(left, top, 0.0, 0.0);
      this._borderSpriteGlow.Margin = new Thickness(left, top, 0.0, 0.0);
    }

    public void ForceUpdatePreview(BitmapSource bitmap)
    {
      this._currentBitmap = bitmap;
      this._updatePreview();
    }

    private void _cbZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._cbZoom.SelectedIndex < 0)
        return;
      this._zoomEngine.SetZoom(double.Parse(((string) ((ContentControl) this._cbZoom.SelectedItem).Content).Replace(" %", "")) / 100.0);
      this._cbZoom.Text = this._zoomEngine.ScaleText;
      this._updatePreview();
    }

    private void _cbZoom_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      try
      {
        this._zoomEngine.SetZoom(double.Parse(this._cbZoom.Text.Replace(" ", "").Replace("%", "")) / 100.0);
        this._cbZoom.Text = this._zoomEngine.ScaleText;
        this._updatePreview();
        e.Handled = true;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public void Clear()
    {
      this._currentBitmap = (BitmapSource) null;
      this._imageSprite.Source = (ImageSource) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/tools/paletteeditortool/imageviewer.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._primary = (Grid) target;
          break;
        case 2:
          this._borderSpriteGlow = (Border) target;
          break;
        case 3:
          this._borderSprite = (Border) target;
          break;
        case 4:
          this._imageSprite = (Image) target;
          break;
        case 5:
          this._gridZoom = (Grid) target;
          break;
        case 6:
          this._cbZoom = (ComboBox) target;
          this._cbZoom.SelectionChanged += new SelectionChangedEventHandler(this._cbZoom_SelectionChanged);
          this._cbZoom.PreviewKeyDown += new KeyEventHandler(this._cbZoom_PreviewKeyDown);
          break;
        case 7:
          this._spriteOverlay = (Border) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void ImageViewerEventHandler(object sender, int x, int y, bool isWithin);
  }
}
