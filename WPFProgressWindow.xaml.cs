// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.ProgressWindow
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.Threading;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace ActEditor.Core.WPF
{
  public partial class ProgressWindow : TkWindow, IProgress, IComponentConnector
  {
    private readonly GrfToWpfBridge.Application.AsyncOperation _asyncOperation;
    private readonly TkProgressBar _bar = new TkProgressBar();
    private bool _enableClosing;
    internal ContentControl _content;
    internal Grid _gridActionPresenter;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public ProgressWindow(string title, string ico)
      : base(title, ico)
    {
      this.InitializeComponent();
      this.Progress = -1f;
      this.Owner = WpfUtilities.TopWindow;
      this._asyncOperation = new GrfToWpfBridge.Application.AsyncOperation(this._bar);
      Grid grid = new Grid();
      this._bar.Margin = new Thickness(6.0, 10.0, 6.0, 10.0);
      grid.Children.Add((UIElement) this._bar);
      this.MinWidth = 270.0;
      this.ContentControl.Content = (object) grid;
    }

    public ContentControl ContentControl => this._content;

    public Grid GridActionPresenter => this._gridActionPresenter;

    public bool EnableClosing
    {
      get => this._enableClosing;
      set => this._enableClosing = value;
    }

    public float Progress { get; set; }

    public bool IsCancelling { get; set; }

    public bool IsCancelled { get; set; }

    public void CancelOperation() => this.IsCancelling = true;

    public void Start(GrfThread operation) => this._asyncOperation.SetAndRunOperation(operation, new GrfThread.GrfThreadEventHandler(this._finished));

    private void _finished(object state)
    {
      this._enableClosing = true;
      this.Dispatch<ProgressWindow>((Action<ProgressWindow>) (p => p.Close()));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.CancelOperation();
      ((Panel) this.GridActionPresenter.Children[0]).Children[0].IsEnabled = false;
      if (!this._enableClosing)
        e.Cancel = true;
      else
        base.OnClosing(e);
    }

    public void Terminate()
    {
      this._enableClosing = true;
      this.Close();
    }

    private void _buttonCancel_Click(object sender, RoutedEventArgs e) => this.Close();

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/progresswindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._content = (ContentControl) target;
          break;
        case 2:
          this._gridActionPresenter = (Grid) target;
          break;
        case 3:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
