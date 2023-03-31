// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.FrameViewer
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
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
using Utilities.Commands;
using Utilities.Tools;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class FrameViewer : UserControl, IPreview, IComponentConnector
  {
    protected readonly List<DrawingComponent> _components = new List<DrawingComponent>();
    protected ZoomEngine _zoomEngine = new ZoomEngine();
    protected bool _isAnyDown;
    protected Point _oldPosition;
    protected Point _relativeCenter = new Point(0.5, 0.8);
    protected FrameViewerSettings _settings = new FrameViewerSettings();
    protected bool _hasMoved;
    internal Grid _gridBackground;
    internal Canvas _primary;
    internal Grid _gridZoom;
    internal ComboBox _cbZoom;
    private bool _contentLoaded;

    public event MouseEventHandler FrameMouseMoved;

    public event MouseButtonEventHandler FrameMouseUp;

    public event MouseButtonEventHandler FrameMouseDown;

    public virtual void OnFrameMouseMoved(MouseEventArgs e)
    {
      MouseEventHandler frameMouseMoved = this.FrameMouseMoved;
      if (frameMouseMoved == null)
        return;
      frameMouseMoved((object) this, e);
    }

    public virtual void OnFrameMouseUp(MouseButtonEventArgs e)
    {
      MouseButtonEventHandler frameMouseUp = this.FrameMouseUp;
      if (frameMouseUp == null)
        return;
      frameMouseUp((object) this, e);
    }

    public virtual void OnFrameMouseDown(MouseButtonEventArgs e)
    {
      MouseButtonEventHandler frameMouseDown = this.FrameMouseDown;
      if (frameMouseDown == null)
        return;
      frameMouseDown((object) this, e);
    }

    public FrameViewer()
    {
      this.InitializeComponent();
      this.ReloadSettings();
      this._primary.Background = (Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorBackgroundColor);
      this._components.Add((DrawingComponent) new GridLine(Orientation.Horizontal));
      this._components.Add((DrawingComponent) new GridLine(Orientation.Vertical));
      this.SizeChanged += new SizeChangedEventHandler(this._framePreview_SizeChanged);
      this.MouseMove += new MouseEventHandler(this._framePreview_MouseMove);
      this.MouseDown += new MouseButtonEventHandler(this._framePreview_MouseDown);
      this.MouseUp += new MouseButtonEventHandler(this._framePreview_MouseUp);
      this.MouseWheel += new MouseWheelEventHandler(this._framePreview_MouseWheel);
    }

    public Point RelativeCenter
    {
      get => this._relativeCenter;
      set => this._relativeCenter = value;
    }

    public ActDraw MainDrawingComponent => this._components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary));

    public Grid GridBackground => this._gridBackground;

    public virtual void InitComponent(FrameViewerSettings settings)
    {
      this._settings = settings;
      if (this.Act == null)
        return;
      this.Act.Commands.CommandUndo += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) ((s, e) => this.Update());
      this.Act.Commands.CommandRedo += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) ((s, e) => this.Update());
    }

    internal void ReloadSettings() => this._zoomEngine = new ZoomEngine()
    {
      ZoomInMultiplier = this._settings.ZoomInMultipler
    };

    private void _framePreview_SizeChanged(object sender, SizeChangedEventArgs e) => this.SizeUpdate();

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
        if (this.ZoomEngine.Scale < 0.45)
          ((TileBrush) this._gridBackground.Background).Viewport = new Rect(this.RelativeCenter.X, this.RelativeCenter.Y, 16.0 / this._gridBackground.ActualWidth, 16.0 / this._gridBackground.ActualHeight);
        else
          ((TileBrush) this._gridBackground.Background).Viewport = new Rect(this.RelativeCenter.X, this.RelativeCenter.Y, 16.0 / (this._gridBackground.ActualWidth / this.ZoomEngine.Scale), 16.0 / (this._gridBackground.ActualHeight / this.ZoomEngine.Scale));
      }
      catch
      {
      }
    }

    public Canvas Canva => this._primary;

    public int CenterX => (int) (this._primary.ActualWidth * this._relativeCenter.X);

    public int CenterY => (int) (this._primary.ActualHeight * this._relativeCenter.Y);

    public ZoomEngine ZoomEngine => this._zoomEngine;

    public Act Act => this._settings.Act();

    public int SelectedAction => this._settings.SelectedAction();

    public int SelectedFrame => this._settings.SelectedFrame();

    public List<DrawingComponent> Components => this._components;

    public void Update()
    {
      this._updateBackground();
      while (this._components.Count > 2)
      {
        this._components[2].Remove((IPreview) this);
        this._components.RemoveAt(2);
      }
      foreach (ActReference actReference in this._settings.References.Where<ActReference>((Func<ActReference, bool>) (p => p.Show && p.Mode == ZMode.Back)))
        this._components.Add((DrawingComponent) new ActDraw(actReference.Act, (IPreview) this));
      if (this.Act != null)
      {
        ActDraw actDraw = new ActDraw(this.Act, (IPreview) this);
        actDraw.Selected += new DrawingComponent.DrawingComponentDelegate(this._primary_Selected);
        this._components.Add((DrawingComponent) actDraw);
      }
      foreach (ActReference actReference in this._settings.References.Where<ActReference>((Func<ActReference, bool>) (p => p.Show && p.Mode == ZMode.Front)))
        this._components.Add((DrawingComponent) new ActDraw(actReference.Act, (IPreview) this));
      Act act = this.Act;
      if (act != null)
      {
        int selectedAction = this.SelectedAction;
        int selectedFrame = this.SelectedFrame;
        GRF.FileFormats.ActFormat.Frame frame = act.TryGetFrame(selectedAction, selectedFrame);
        if (frame != null && this._settings.ShowAnchors() && frame.Anchors.Count > 0)
        {
          foreach (Anchor anchor in frame.Anchors)
            this._components.Add((DrawingComponent) new AnchorDraw(anchor)
            {
              Visible = true
            });
        }
      }
      foreach (DrawingComponent component in this._components)
        component.Render((IPreview) this);
    }

    protected virtual void _primary_Selected(object sender, int index, bool selected)
    {
    }

    protected virtual void _framePreview_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        this._isAnyDown = true;
        if (Keyboard.FocusedElement != this._cbZoom)
          Keyboard.Focus((IInputElement) this);
        this._oldPosition = e.GetPosition((IInputElement) this);
        if (e.RightButton != MouseButtonState.Pressed)
          return;
        this.CaptureMouse();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    protected void _framePreview_MouseUp(object sender, MouseButtonEventArgs e)
    {
      try
      {
        this._isAnyDown = false;
        if (e != null && this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) != this._cbZoom)
          e.Handled = true;
        this.OnFrameMouseUp(e);
        this.ReleaseMouseCapture();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

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

    private void _framePreview_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released || this.ContextMenu != null && this.ContextMenu.IsOpen)
          return;
        Point position = e.GetPosition((IInputElement) this);
        double num1 = position.X - this._oldPosition.X;
        double num2 = position.Y - this._oldPosition.Y;
        if (num1 == 0.0 && num2 == 0.0 || this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) == this._cbZoom && !this.IsMouseCaptured)
          return;
        if (e.RightButton == MouseButtonState.Pressed && this._isAnyDown)
        {
          this._relativeCenter.X += num1 / this.Canva.ActualWidth;
          this._relativeCenter.Y += num2 / this.Canva.ActualHeight;
          this._oldPosition = position;
          this.SizeUpdate();
          this._hasMoved = true;
        }
        this.OnFrameMouseMoved(e);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _cbZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._cbZoom.SelectedIndex < 0)
        return;
      this._zoomEngine.SetZoom(double.Parse(((string) ((ContentControl) this._cbZoom.SelectedItem).Content).Replace(" %", "")) / 100.0);
      this._cbZoom.Text = this._zoomEngine.ScaleText;
      this.SizeUpdate();
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

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/frameviewer.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
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
  }
}
