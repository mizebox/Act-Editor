// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.ReadonlyFramePreview
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ActEditor.Core.WPF.EditorControls;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using TokeiLibrary;
using Utilities.Tools;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class ReadonlyFramePreview : UserControl, IPreview, IComponentConnector
  {
    private readonly List<DrawingComponent> _components = new List<DrawingComponent>();
    private readonly ZoomEngine _zoomEngine = new ZoomEngine()
    {
      ZoomInMultiplier = (Func<double>) (() => (double) GrfEditorConfiguration.ActEditorZoomInMultiplier)
    };
    private bool _isAnyDown;
    private Point _oldPosition;
    private Point _relativeCenter = new Point(0.5, 0.8);
    private ISelector _preview;
    internal Grid _gridBackground;
    internal Canvas _primary;
    internal Grid _gridZoom;
    internal ComboBox _cbZoom;
    private bool _contentLoaded;

    public ReadonlyFramePreview()
    {
      this.InitializeComponent();
      this._primary.Background = (Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorBackgroundColor);
      this._components.Add((DrawingComponent) new GridLine(Orientation.Horizontal));
      this._components.Add((DrawingComponent) new GridLine(Orientation.Vertical));
      this.SizeChanged += new SizeChangedEventHandler(this._framePreview_SizeChanged);
      this.MouseMove += new MouseEventHandler(this._framePreview_MouseMove);
      this.MouseDown += new MouseButtonEventHandler(this._framePreview_MouseDown);
      this.MouseUp += new MouseButtonEventHandler(this._framePreview_MouseUp);
      this.MouseWheel += new MouseWheelEventHandler(this._framePreview_MouseWheel);
    }

    public bool IsEditing { get; set; }

    public Point RelativeCenter
    {
      get => this._relativeCenter;
      set => this._relativeCenter = value;
    }

    public ActDraw MainDrawingComponent => this._components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary));

    public Grid GridBackground => this._gridBackground;

    public Point EditPoint { get; set; }

    public Act Act => this._preview.Act;

    public int SelectedAction => this._preview.SelectedAction;

    public int SelectedFrame => this._preview.SelectedFrame;

    public List<DrawingComponent> Components => this._components;

    public ZoomEngine ZoomEngine => this._zoomEngine;

    public Canvas Canva => this._primary;

    public int CenterX => (int) (this._primary.ActualWidth * this._relativeCenter.X);

    public int CenterY => (int) (this._primary.ActualHeight * this._relativeCenter.Y);

    private void _framePreview_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
        return;
      this.ZoomEngine.Zoom((double) e.Delta);
      Point position = e.GetPosition((IInputElement) this._primary);
      double num1 = position.X / this._primary.ActualWidth - this._relativeCenter.X;
      double num2 = position.Y / this._primary.ActualHeight - this._relativeCenter.Y;
      this._relativeCenter.X = position.X / this._primary.ActualWidth - num1 / this.ZoomEngine.OldScale * this.ZoomEngine.Scale;
      this._relativeCenter.Y = position.Y / this._primary.ActualHeight - num2 / this.ZoomEngine.OldScale * this.ZoomEngine.Scale;
      this._cbZoom.SelectedIndex = -1;
      this._cbZoom.Text = this._zoomEngine.ScaleText;
      this.SizeUpdate();
    }

    private void _framePreview_SizeChanged(object sender, SizeChangedEventArgs e) => this.SizeUpdate();

    private void _framePreview_MouseUp(object sender, MouseButtonEventArgs e)
    {
      this._isAnyDown = false;
      this.ReleaseMouseCapture();
    }

    private void _framePreview_MouseDown(object sender, MouseButtonEventArgs e)
    {
      this._isAnyDown = true;
      if (Keyboard.FocusedElement != this._cbZoom)
        Keyboard.Focus((IInputElement) this.GridBackground);
      this._oldPosition = e.GetPosition((IInputElement) this);
      if (e.RightButton != MouseButtonState.Pressed)
        return;
      this.CaptureMouse();
    }

    private void _framePreview_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
        return;
      Point position = e.GetPosition((IInputElement) this);
      double num1 = position.X - this._oldPosition.X;
      double num2 = position.Y - this._oldPosition.Y;
      if (num1 == 0.0 && num2 == 0.0 || this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) == this._cbZoom && !this.IsMouseCaptured || e.RightButton != MouseButtonState.Pressed || !this._isAnyDown)
        return;
      this._relativeCenter.X += num1 / this.Canva.ActualWidth;
      this._relativeCenter.Y += num2 / this.Canva.ActualHeight;
      this._oldPosition = position;
      this.SizeUpdate();
    }

    public void Init(ISelector selector)
    {
      this._preview = selector;
      selector.FrameChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
      selector.SpecialFrameChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
      selector.ActionChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
    }

    private void _cbZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._cbZoom.SelectedIndex < 0)
        return;
      this._zoomEngine.SetZoom(double.Parse(((string) ((ContentControl) this._cbZoom.SelectedItem).Content).Replace(" %", "")) / 100.0);
      this._cbZoom.Text = this._zoomEngine.ScaleText;
      this.SizeUpdate();
    }

    private void _cbZoom_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      try
      {
        string s = this._cbZoom.Text.Replace(" ", "").Replace("%", "");
        this._cbZoom.SelectedIndex = -1;
        this._zoomEngine.SetZoom(double.Parse(s) / 100.0);
        this._cbZoom.Text = this._zoomEngine.ScaleText;
        this.SizeUpdate();
        e.Handled = true;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _cbZoom_MouseEnter(object sender, MouseEventArgs e)
    {
      this._cbZoom.Opacity = 1.0;
      this._cbZoom.StaysOpenOnEdit = true;
    }

    private void _cbZoom_MouseLeave(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Released)
        return;
      this._cbZoom.Opacity = 0.7;
    }

    public void Update()
    {
      this._updateBackground();
      while (this._components.Count > 2)
      {
        this._components[2].Remove((IPreview) this);
        this._components.RemoveAt(2);
      }
      if (this.Act != null)
        this._components.Add((DrawingComponent) new ActDraw(this.Act));
      foreach (DrawingComponent component in this._components)
        component.Render((IPreview) this);
    }

    public void SizeUpdate()
    {
      this._updateBackground();
      foreach (DrawingComponent component in this._components)
        component.QuickRender((IPreview) this);
    }

    private void _updateBackground()
    {
      try
      {
        if (this.ZoomEngine.Scale < 0.65)
          ((TileBrush) this._gridBackground.Background).Viewport = new Rect(this.RelativeCenter.X, this.RelativeCenter.Y, 16.0 / this._gridBackground.ActualWidth, 16.0 / this._gridBackground.ActualHeight);
        else
          ((TileBrush) this._gridBackground.Background).Viewport = new Rect(this.RelativeCenter.X, this.RelativeCenter.Y, 16.0 / (this._gridBackground.ActualWidth / this.ZoomEngine.Scale), 16.0 / (this._gridBackground.ActualHeight / this.ZoomEngine.Scale));
      }
      catch
      {
      }
    }

    public event ReadonlyFramePreview.EditChangedEventHandler EditChanged;

    public event ReadonlyFramePreview.EditChangedEventHandler EditFinished;

    public void OnEditFinished(Point newpoint)
    {
      ReadonlyFramePreview.EditChangedEventHandler editFinished = this.EditFinished;
      if (editFinished == null)
        return;
      editFinished((object) this, newpoint);
    }

    public void OnEditChanged(Point newpoint)
    {
      ReadonlyFramePreview.EditChangedEventHandler editChanged = this.EditChanged;
      if (editChanged == null)
        return;
      editChanged((object) this, newpoint);
    }

    public void RemoveEdit() => this._gridBackground.ReleaseMouseCapture();

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/readonlyframepreview.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._gridBackground = (Grid) target;
          break;
        case 2:
          this._primary = (Canvas) target;
          break;
        case 3:
          this._gridZoom = (Grid) target;
          break;
        case 4:
          this._cbZoom = (ComboBox) target;
          this._cbZoom.SelectionChanged += new SelectionChangedEventHandler(this._cbZoom_SelectionChanged);
          this._cbZoom.PreviewKeyDown += new KeyEventHandler(this._cbZoom_PreviewKeyDown);
          this._cbZoom.MouseLeave += new MouseEventHandler(this._cbZoom_MouseLeave);
          this._cbZoom.MouseEnter += new MouseEventHandler(this._cbZoom_MouseEnter);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    Point IPreview.PointToScreen([In] Point obj0) => this.PointToScreen(obj0);

    public delegate void EditChangedEventHandler(object sender, Point newPoint);
  }
}
