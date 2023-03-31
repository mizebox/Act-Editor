// Decompiled with JetBrains decompiler
// Type: ActEditor.App
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using GRF.IO;
using GRF.System;
using GrfToWpfBridge.Application;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using TokeiLibrary;
using Utilities;

namespace ActEditor
{
  public partial class App : System.Windows.Application
  {
    private bool _contentLoaded;

    public App()
    {
      Configuration.ConfigAsker = GrfEditorConfiguration.ConfigAsker;
      ErrorHandler.SetErrorHandler((IErrorHandler) new DefaultErrorHandler());
      Settings.TempPath = GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "tmp");
      TemporaryFilesManager.ClearTemporaryFiles();
      SelfPatcher.SelfPatch();
      SprConverterV2M1.AutomaticDowngradeOnRleException = true;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      ApplicationManager.CrashReportEnabled = true;
      ImageConverterManager.AddConverter((AbstractImageConverter) new DefaultImageConverter());
      Configuration.SetImageRendering(this.Resources);
      this.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri("pack://application:,,,/" + Assembly.GetEntryAssembly().GetName().Name.Replace(" ", "%20") + ";component/WPF/Styles/GRFEditorStyles.xaml", UriKind.RelativeOrAbsolute)
      });
      if (!Methods.IsWinVistaOrHigher())
      {
        if (Methods.IsWinXPOrHigher())
        {
          try
          {
            this.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(new Uri("PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\\themes/aero.normalcolor.xaml", UriKind.Relative)) as ResourceDictionary);
          }
          catch
          {
            int num = (int) MessageBox.Show("Failed to apply a style override for Windows XP's theme.");
          }
        }
      }
      base.OnStartup(e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      this.StartupUri = new Uri("Core\\\\ActEditorWindow.xaml", UriKind.Relative);
      System.Windows.Application.LoadComponent((object) this, new Uri("/Act Editor;component/app.xaml", UriKind.Relative));
    }

    [STAThread]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public static void Main()
    {
      App app = new App();
      app.InitializeComponent();
      app.Run();
    }
  }
}
