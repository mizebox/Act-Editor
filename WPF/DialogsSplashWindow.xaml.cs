// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.SplashWindow
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using GRF.Threading;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using TokeiLibrary;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class SplashWindow : Window, IComponentConnector
  {
    internal Label _version;
    internal Label _updateMessage;
    private bool _contentLoaded;

    public SplashWindow()
    {
      this.InitializeComponent();
      this._version.Content = (object) GrfEditorConfiguration.PublicVersion;
      this.ShowInTaskbar = false;
      this.Topmost = true;
      this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public string Display
    {
      set => this._updateMessage.Dispatch<Label, object>((Func<Label, object>) (p => p.Content = (object) value));
    }

    public void Terminate(int time) => GrfThread.Start((Action) (() =>
    {
      Thread.Sleep(time);
      this.Dispatch<SplashWindow>((Action<SplashWindow>) (p => p.Close()));
    }));

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/splashwindow.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._version = (Label) target;
          break;
        case 2:
          this._updateMessage = (Label) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
