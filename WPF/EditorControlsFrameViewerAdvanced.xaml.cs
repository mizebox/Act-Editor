// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.FrameViewerAdvanced
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using GRF.FileFormats.ActFormat;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Utilities.Tools;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class FrameViewerAdvanced : UserControl, IPreview, IComponentConnector
  {
    protected Point _relativeCenter = new Point(0.5, 0.8);
    protected readonly List<DrawingComponent> _components = new List<DrawingComponent>();
    internal Grid _gridBackground;
    internal Canvas _primary;
    private bool _contentLoaded;

    public Canvas Canva { get; private set; }

    public int CenterX { get; private set; }

    public int CenterY { get; private set; }

    public ZoomEngine ZoomEngine { get; private set; }

    public Act Act { get; private set; }

    public int SelectedAction { get; private set; }

    public int SelectedFrame { get; private set; }

    public List<DrawingComponent> Components { get; private set; }

    public Point RelativeCenter => this._relativeCenter;

    public event FrameViewerAdvanced.OnZoomEventDelegate ZoomChanged;

    public void OnZoomChanged()
    {
      FrameViewerAdvanced.OnZoomEventDelegate zoomChanged = this.ZoomChanged;
      if (zoomChanged == null)
        return;
      zoomChanged((object) this);
    }

    public FrameViewerAdvanced()
    {
      this.InitializeComponent();
      this.ZoomEngine = new ZoomEngine()
      {
        ZoomInMultiplier = (Func<double>) (() => 1.0)
      };
      this._primary.Background = (Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorBackgroundColor);
      this.MouseWheel += new MouseWheelEventHandler(this._framePreview_MouseWheel);
    }

    private void _framePreview_MouseWheel(object sender, MouseWheelEventArgs e)
    {
    }

    public void Update()
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/framevieweradvanced.xaml", UriKind.Relative));
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
        default:
          this._contentLoaded = true;
          break;
      }
    }

    Point IPreview.PointToScreen([In] Point obj0) => this.PointToScreen(obj0);

    public delegate void OnZoomEventDelegate(object sender);
  }
}
