// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.ScriptRunnerDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.Avalon;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.LubFormat;
using GRF.IO;
using GRF.System;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class ScriptRunnerDialog : TkWindow, IComponentConnector
  {
    public const string ScriptTemplate = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Globalization;\r\nusing System.IO;\r\nusing System.Linq;\r\nusing System.Windows;\r\nusing System.Windows.Controls;\r\nusing System.Windows.Documents;\r\nusing System.Windows.Media;\r\nusing System.Windows.Media.Imaging;\r\nusing ErrorManager;\r\nusing GRF.FileFormats.ActFormat;\r\nusing GRF.FileFormats.SprFormat;\r\nusing GRF.FileFormats.PalFormat;\r\nusing GRF.Image;\r\nusing GRF.Image.Decoders;\r\nusing GRF.Graphics;\r\nusing GRF.Core;\r\nusing GRF.IO;\r\nusing GRF.System;\r\nusing GrfToWpfBridge;\r\nusing TokeiLibrary;\r\nusing TokeiLibrary.WPF;\r\nusing Utilities;\r\nusing Utilities.Extension;\r\nusing Action = GRF.FileFormats.ActFormat.Action;\r\nusing Frame = GRF.FileFormats.ActFormat.Frame;\r\nusing Point = System.Windows.Point;\r\n\r\nnamespace Scripts {\r\n    public class Script : IActScript {\r\n\t\tpublic object DisplayName {\r\n\t\t\tget { return {0}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string Group {\r\n\t\t\tget { return \"Scripts\"; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string InputGesture {\r\n\t\t\tget { return {1}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string Image {\r\n\t\t\tget { return {2}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic void Execute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {\r\n\t\t\tif (act == null) return;\r\n\t\t\t\r\n\t\t\ttry {\r\n\t\t\t\tact.Commands.Begin();\r\n\t\t\t\tact.Commands.Backup(_ => {\r\n{3}\r\n\t\t\t\t}, {0}, true);\r\n\t\t\t}\r\n\t\t\tcatch (Exception err) {\r\n\t\t\t\tact.Commands.CancelEdit();\r\n\t\t\t\tErrorHandler.HandleException(err, ErrorLevel.Warning);\r\n\t\t\t}\r\n\t\t\tfinally {\r\n\t\t\t\tact.Commands.End();\r\n\t\t\t\tact.InvalidateVisual();\r\n\t\t\t\tact.InvalidateSpriteVisual();\r\n\t\t\t}\r\n\t\t}\r\n\t\t\r\n\t\tpublic bool CanExecute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {\r\n\t\t\treturn true;\r\n\t\t\t//return act != null;\r\n\t\t}\r\n\t}\r\n}\r\n";
    public static string TmpFilePattern = "script_runner_{0:0000}.cs";
    private readonly ActEditorWindow _actEditor;
    internal Grid _mainGrid;
    internal TextEditor _textEditor;
    internal FancyButton _buttonRun;
    internal FancyButton _buttonClear;
    internal FancyButton _buttonSaveAs;
    internal FancyButton _buttonLoad;
    internal FancyButton _buttonHelp;
    internal StackPanel _sp;
    internal System.Windows.Controls.ListView _listView;
    internal Grid _gridActionPresenter;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    static ScriptRunnerDialog()
    {
      ScriptRunnerDialog.TmpFilePattern = Process.GetCurrentProcess().Id.ToString() + "_" + ScriptRunnerDialog.TmpFilePattern;
      TemporaryFilesManager.UniquePattern(ScriptRunnerDialog.TmpFilePattern);
    }

    public ScriptRunnerDialog()
    {
    }

    public ScriptRunnerDialog(ActEditorWindow actEditor)
      : base("Script Runner", "dos.png", resizeMode: ResizeMode.CanResize)
    {
      this._actEditor = actEditor;
      this.InitializeComponent();
      this.ShowInTaskbar = true;
      this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      AvalonLoader.Load(this._textEditor);
      this._textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
      this.SizeToContent = SizeToContent.WidthAndHeight;
      System.Windows.Controls.ListView listView = this._listView;
      ListViewDataTemplateHelper.GeneralColumnInfo[] generalColumnInfoArray1 = new ListViewDataTemplateHelper.GeneralColumnInfo[4];
      ListViewDataTemplateHelper.GeneralColumnInfo[] generalColumnInfoArray2 = generalColumnInfoArray1;
      ListViewDataTemplateHelper.ImageColumnInfo imageColumnInfo1 = new ListViewDataTemplateHelper.ImageColumnInfo();
      imageColumnInfo1.Header = "";
      imageColumnInfo1.DisplayExpression = "DataImage";
      imageColumnInfo1.SearchGetAccessor = "IsWarning";
      imageColumnInfo1.FixedWidth = 20.0;
      imageColumnInfo1.MaxHeight = 24.0;
      ListViewDataTemplateHelper.ImageColumnInfo imageColumnInfo2 = imageColumnInfo1;
      generalColumnInfoArray2[0] = (ListViewDataTemplateHelper.GeneralColumnInfo) imageColumnInfo2;
      generalColumnInfoArray1[1] = new ListViewDataTemplateHelper.GeneralColumnInfo()
      {
        Header = "Description",
        DisplayExpression = "Description",
        ToolTipBinding = "ToolTipDescription",
        TextAlignment = TextAlignment.Left,
        TextWrapping = TextWrapping.Wrap,
        IsFill = true
      };
      generalColumnInfoArray1[2] = new ListViewDataTemplateHelper.GeneralColumnInfo()
      {
        Header = "Line",
        DisplayExpression = "Line",
        FixedWidth = 50.0,
        ToolTipBinding = "Line",
        TextAlignment = TextAlignment.Right
      };
      generalColumnInfoArray1[3] = new ListViewDataTemplateHelper.GeneralColumnInfo()
      {
        Header = "Col",
        DisplayExpression = "Column",
        FixedWidth = 30.0,
        ToolTipBinding = "Column",
        TextAlignment = TextAlignment.Right
      };
      ListViewDataTemplateHelper.GeneralColumnInfo[] columnInfos = generalColumnInfoArray1;
      DefaultListViewComparer<ScriptRunnerDialog.CompilerErrorView> sorter = new DefaultListViewComparer<ScriptRunnerDialog.CompilerErrorView>();
      string[] triggers = new string[2]
      {
        "Default",
        "Black"
      };
      string[] strArray = new string[0];
      ListViewDataTemplateHelper.GenerateListViewTemplateNew(listView, columnInfos, (ListViewCustomComparer) sorter, (IList<string>) triggers, strArray);
      this.Loaded += (RoutedEventHandler) delegate
      {
        this.SizeToContent = SizeToContent.Manual;
        this.MinWidth = 600.0;
        this.Width = this.MinWidth;
        this.MinHeight = this._textEditor.MinHeight + this._sp.ActualHeight + this._gridActionPresenter.ActualHeight + 50.0;
        this.Height = this.MinHeight;
        this.Left = (SystemParameters.FullPrimaryScreenWidth - this.MinWidth) / 2.0;
        this.Top = (SystemParameters.FullPrimaryScreenHeight - this.MinHeight) / 2.0;
      };
      this._textEditor.Text = GrfEditorConfiguration.ActEditorScriptRunnerScript;
      this._textEditor.TextChanged += (EventHandler) delegate
      {
        GrfEditorConfiguration.ActEditorScriptRunnerScript = this._textEditor.Text;
      };
      ApplicationShortcut.Link(ApplicationShortcut.Open, (System.Action) (() => this._buttonLoad_Click((object) null, (RoutedEventArgs) null)), (FrameworkElement) this);
      ApplicationShortcut.Link(ApplicationShortcut.Save, (System.Action) (() => this._buttonSaveAs_Click((object) null, (RoutedEventArgs) null)), (FrameworkElement) this);
      ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.FromString("Ctrl-R", "RunScript"), (System.Action) (() => this._buttonRun_Click((object) null, (RoutedEventArgs) null)), (FrameworkElement) this);
    }

    public string ScriptName => "\"MyCustomScript\"";

    public string InputGesture => "null";

    public string Image => "null";

    private void _buttonCancel_Click(object sender, RoutedEventArgs e) => this.Close();

    private void _buttonRun_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath(ScriptRunnerDialog.TmpFilePattern);
        string newValue = this._fixIndent(this._textEditor.Text);
        string contents = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Globalization;\r\nusing System.IO;\r\nusing System.Linq;\r\nusing System.Windows;\r\nusing System.Windows.Controls;\r\nusing System.Windows.Documents;\r\nusing System.Windows.Media;\r\nusing System.Windows.Media.Imaging;\r\nusing ErrorManager;\r\nusing GRF.FileFormats.ActFormat;\r\nusing GRF.FileFormats.SprFormat;\r\nusing GRF.FileFormats.PalFormat;\r\nusing GRF.Image;\r\nusing GRF.Image.Decoders;\r\nusing GRF.Graphics;\r\nusing GRF.Core;\r\nusing GRF.IO;\r\nusing GRF.System;\r\nusing GrfToWpfBridge;\r\nusing TokeiLibrary;\r\nusing TokeiLibrary.WPF;\r\nusing Utilities;\r\nusing Utilities.Extension;\r\nusing Action = GRF.FileFormats.ActFormat.Action;\r\nusing Frame = GRF.FileFormats.ActFormat.Frame;\r\nusing Point = System.Windows.Point;\r\n\r\nnamespace Scripts {\r\n    public class Script : IActScript {\r\n\t\tpublic object DisplayName {\r\n\t\t\tget { return {0}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string Group {\r\n\t\t\tget { return \"Scripts\"; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string InputGesture {\r\n\t\t\tget { return {1}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string Image {\r\n\t\t\tget { return {2}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic void Execute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {\r\n\t\t\tif (act == null) return;\r\n\t\t\t\r\n\t\t\ttry {\r\n\t\t\t\tact.Commands.Begin();\r\n\t\t\t\tact.Commands.Backup(_ => {\r\n{3}\r\n\t\t\t\t}, {0}, true);\r\n\t\t\t}\r\n\t\t\tcatch (Exception err) {\r\n\t\t\t\tact.Commands.CancelEdit();\r\n\t\t\t\tErrorHandler.HandleException(err, ErrorLevel.Warning);\r\n\t\t\t}\r\n\t\t\tfinally {\r\n\t\t\t\tact.Commands.End();\r\n\t\t\t\tact.InvalidateVisual();\r\n\t\t\t\tact.InvalidateSpriteVisual();\r\n\t\t\t}\r\n\t\t}\r\n\t\t\r\n\t\tpublic bool CanExecute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {\r\n\t\t\treturn true;\r\n\t\t\t//return act != null;\r\n\t\t}\r\n\t}\r\n}\r\n".Replace("{0}", this.ScriptName).Replace("{1}", this.InputGesture).Replace("{2}", this.Image).Replace("{3}", newValue);
        File.WriteAllText(temporaryFilePath, contents);
        string dll;
        CompilerResults compilerResults = ScriptLoader.Compile(temporaryFilePath, out dll);
        GrfPath.Delete(temporaryFilePath);
        this._listView.ItemsSource = (IEnumerable) null;
        if (compilerResults.Errors.Count > 0)
        {
          this._listView.ItemsSource = (IEnumerable) compilerResults.Errors.Cast<CompilerError>().ToList<CompilerError>().Select<CompilerError, ScriptRunnerDialog.CompilerErrorView>((Func<CompilerError, ScriptRunnerDialog.CompilerErrorView>) (p => new ScriptRunnerDialog.CompilerErrorView(p))).ToList<ScriptRunnerDialog.CompilerErrorView>();
          this._sp.Visibility = Visibility.Visible;
        }
        else
        {
          IActScript objectFromAssembly = ScriptLoader.GetScriptObjectFromAssembly(dll);
          try
          {
            if (!objectFromAssembly.CanExecute(this._actEditor.Act, this._actEditor.SelectedAction, this._actEditor.SelectedFrame, this._actEditor.SelectionEngine.CurrentlySelected.ToArray<int>()))
              return;
            objectFromAssembly.Execute(this._actEditor.Act, this._actEditor.SelectedAction, this._actEditor.SelectedFrame, this._actEditor.SelectionEngine.CurrentlySelected.ToArray<int>());
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex);
          }
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private string _fixIndent(string text)
    {
      List<string> replacedLines = !text.Contains("act.Commands.") ? ((IEnumerable<string>) text.Split(new string[1]
      {
        "\r\n"
      }, StringSplitOptions.None)).ToList<string>() : throw new Exception("Command methods cannot be executed within another command (Backup).");
      LineHelper.FixIndent(replacedLines, 5);
      return string.Join("\r\n", replacedLines.ToArray());
    }

    private void _listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      ListViewItem objectAtPoint = this._listView.GetObjectAtPoint<ListViewItem>(e.GetPosition((IInputElement) this._listView));
      if (objectAtPoint == null)
        return;
      if (!(objectAtPoint.Content is ScriptRunnerDialog.CompilerErrorView content))
        return;
      try
      {
        List<string> list = ((IEnumerable<string>) this._textEditor.Text.Split(new string[1]
        {
          "\r\n"
        }, StringSplitOptions.None)).ToList<string>();
        int num = 0;
        for (int index = 0; index < content.Line - 1; ++index)
          num += list[index].Length + 2;
        this._textEditor.SelectionLength = 0;
        this._textEditor.TextArea.Caret.Offset = num + content.Column;
        this._textEditor.TextArea.Caret.BringCaretToView();
        this._textEditor.TextArea.Caret.Show();
        Keyboard.Focus((IInputElement) this._textEditor);
      }
      catch
      {
      }
    }

    public string TabsToSpace()
    {
      string text = this._textEditor.Text;
      StringBuilder stringBuilder = new StringBuilder();
      string str1 = text;
      char[] chArray = new char[1]{ '\t' };
      foreach (string str2 in str1.Split(chArray))
      {
        stringBuilder.Append(str2);
        stringBuilder.Append(new string(' ', 4 - str2.Length % 4));
      }
      return stringBuilder.ToString();
    }

    private void _buttonClear_Click(object sender, RoutedEventArgs e)
    {
      this._listView.ItemsSource = (IEnumerable) null;
      this._textEditor.Text = "";
    }

    private void _buttonSaveAs_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string path = PathRequest.SaveFileEditor("filter", "C# Files|*.cs");
        if (path == null)
          return;
        string newValue = this._fixIndent(this._textEditor.Text);
        string contents = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Globalization;\r\nusing System.IO;\r\nusing System.Linq;\r\nusing System.Windows;\r\nusing System.Windows.Controls;\r\nusing System.Windows.Documents;\r\nusing System.Windows.Media;\r\nusing System.Windows.Media.Imaging;\r\nusing ErrorManager;\r\nusing GRF.FileFormats.ActFormat;\r\nusing GRF.FileFormats.SprFormat;\r\nusing GRF.FileFormats.PalFormat;\r\nusing GRF.Image;\r\nusing GRF.Image.Decoders;\r\nusing GRF.Graphics;\r\nusing GRF.Core;\r\nusing GRF.IO;\r\nusing GRF.System;\r\nusing GrfToWpfBridge;\r\nusing TokeiLibrary;\r\nusing TokeiLibrary.WPF;\r\nusing Utilities;\r\nusing Utilities.Extension;\r\nusing Action = GRF.FileFormats.ActFormat.Action;\r\nusing Frame = GRF.FileFormats.ActFormat.Frame;\r\nusing Point = System.Windows.Point;\r\n\r\nnamespace Scripts {\r\n    public class Script : IActScript {\r\n\t\tpublic object DisplayName {\r\n\t\t\tget { return {0}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string Group {\r\n\t\t\tget { return \"Scripts\"; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string InputGesture {\r\n\t\t\tget { return {1}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic string Image {\r\n\t\t\tget { return {2}; }\r\n\t\t}\r\n\t\t\r\n\t\tpublic void Execute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {\r\n\t\t\tif (act == null) return;\r\n\t\t\t\r\n\t\t\ttry {\r\n\t\t\t\tact.Commands.Begin();\r\n\t\t\t\tact.Commands.Backup(_ => {\r\n{3}\r\n\t\t\t\t}, {0}, true);\r\n\t\t\t}\r\n\t\t\tcatch (Exception err) {\r\n\t\t\t\tact.Commands.CancelEdit();\r\n\t\t\t\tErrorHandler.HandleException(err, ErrorLevel.Warning);\r\n\t\t\t}\r\n\t\t\tfinally {\r\n\t\t\t\tact.Commands.End();\r\n\t\t\t\tact.InvalidateVisual();\r\n\t\t\t\tact.InvalidateSpriteVisual();\r\n\t\t\t}\r\n\t\t}\r\n\t\t\r\n\t\tpublic bool CanExecute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {\r\n\t\t\treturn true;\r\n\t\t\t//return act != null;\r\n\t\t}\r\n\t}\r\n}\r\n".Replace("{0}", this.ScriptName).Replace("{1}", this.InputGesture).Replace("{2}", this.Image).Replace("{3}", newValue);
        File.WriteAllText(path, contents);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _buttonLoad_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string path = PathRequest.OpenFileEditor("filter", "C# Files|*.cs");
        if (path == null)
          return;
        this._textEditor.Text = this._extractBackup(File.ReadAllText(path));
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private string _extractBackup(string input)
    {
      try
      {
        string str1 = input;
        int num1 = str1.IndexOf("act.Commands.Backup(_ => {");
        if (num1 < 0)
          return input;
        string str2 = str1.Substring(num1 + "act.Commands.Backup(_ => {".Length);
        int index = str2.IndexOf("act.Commands.CancelEdit();");
        if (index < 0)
          return input;
        int num2 = 0;
        while (index >= 0)
        {
          if (str2[index] == '}')
            ++num2;
          --index;
          if (num2 == 2)
            break;
        }
        if (index < 0)
          return input;
        List<string> list = ((IEnumerable<string>) str2.Substring(0, index - 1).Trim('\r', '\n').Split(new string[1]
        {
          "\r\n"
        }, StringSplitOptions.None)).ToList<string>();
        LineHelper.FixIndent(list, 0);
        return string.Join("\r\n", list.ToArray()).TrimEnd('\r', '\n', ' ', '\t');
      }
      catch
      {
        return input;
      }
    }

    private void _buttonHelp_Click(object sender, RoutedEventArgs e)
    {
      string str = GrfPath.Combine(Configuration.ApplicationPath, "doc", "index.html");
      if (File.Exists(str))
        Process.Start(str);
      else
        ErrorHandler.HandleException("Path not found : " + str);
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/scriptrunnerdialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._mainGrid = (Grid) target;
          break;
        case 2:
          this._textEditor = (TextEditor) target;
          break;
        case 3:
          this._buttonRun = (FancyButton) target;
          this._buttonRun.Click += new RoutedEventHandler(this._buttonRun_Click);
          break;
        case 4:
          this._buttonClear = (FancyButton) target;
          this._buttonClear.Click += new RoutedEventHandler(this._buttonClear_Click);
          break;
        case 5:
          this._buttonSaveAs = (FancyButton) target;
          this._buttonSaveAs.Click += new RoutedEventHandler(this._buttonSaveAs_Click);
          break;
        case 6:
          this._buttonLoad = (FancyButton) target;
          this._buttonLoad.Click += new RoutedEventHandler(this._buttonLoad_Click);
          break;
        case 7:
          this._buttonHelp = (FancyButton) target;
          this._buttonHelp.Click += new RoutedEventHandler(this._buttonHelp_Click);
          break;
        case 8:
          this._sp = (StackPanel) target;
          break;
        case 9:
          this._listView = (System.Windows.Controls.ListView) target;
          this._listView.MouseDoubleClick += new MouseButtonEventHandler(this._listView_MouseDoubleClick);
          break;
        case 10:
          this._gridActionPresenter = (Grid) target;
          break;
        case 11:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public class CompilerErrorView
    {
      public CompilerErrorView(CompilerError error)
      {
        this.Description = error.ErrorText;
        this.ToolTipDescription = error.ToString();
        this.Line = error.Line - 53;
        this.Column = error.Column - 6;
        this.IsWarning = error.IsWarning ? 0 : 1;
        this.DataImage = error.IsWarning ? (object) ApplicationManager.PreloadResourceImage("warning16.png") : (object) ApplicationManager.PreloadResourceImage("error16.png");
      }

      public string Description { get; set; }

      public string ToolTipDescription { get; set; }

      public int Line { get; set; }

      public int Column { get; set; }

      public object DataImage { get; set; }

      public int IsWarning { get; set; }

      public bool Default => true;
    }
  }
}
