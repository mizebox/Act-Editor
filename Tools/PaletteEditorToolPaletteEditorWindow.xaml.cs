// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.PaletteEditorTool.PaletteEditorWindow
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace ActEditor.Tools.PaletteEditorTool
{
  public partial class PaletteEditorWindow : Window, IComponentConnector
  {
    internal SpriteEditorControl _palEditor;
    private bool _contentLoaded;

    public PaletteEditorWindow()
    {
      this.InitializeComponent();
      this.ShowInTaskbar = true;
      this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      this.SizeToContent = SizeToContent.WidthAndHeight;
      this.Loaded += (RoutedEventHandler) delegate
      {
        double num = this.Left;
        if (this.Left + 300.0 + 840.0 > SystemParameters.PrimaryScreenWidth)
          num = SystemParameters.PrimaryScreenWidth - 1140.0;
        if (num < 0.0)
          num = 0.0;
        this.Left = num;
        this.SizeToContent = SizeToContent.Manual;
        this.MinHeight = 440.0;
        this.MinWidth = (double) (300 + (GrfEditorConfiguration.PaletteEditorOpenWindowsEdits ? 0 : 840));
      };
    }

    public SpriteEditorControl PaletteEditor => this._palEditor;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/tools/paletteeditortool/paletteeditorwindow.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this._palEditor = (SpriteEditorControl) target;
      else
        this._contentLoaded = true;
    }
  }
}
