// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.SoundEditDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.Avalon;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using ICSharpCode.AvalonEdit;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using TokeiLibrary.WPF.Styles;
using Utilities;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class SoundEditDialog : TkWindow, IComponentConnector
  {
    private readonly Act _act;
    internal TextEditor _textEditor;
    internal Grid _gridActionPresenter;
    internal Button _buttonOk;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public SoundEditDialog() => this.InitializeComponent();

    public SoundEditDialog(Act act)
      : base("Sound edit", "app.ico")
    {
      this.InitializeComponent();
      this._act = act;
      AvalonLoader.Load(this._textEditor);
      this._textEditor.Text = string.Join("\r\n", this._act.SoundFiles.ToArray());
    }

    protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      bool? dialogResult = this.DialogResult;
      if ((!dialogResult.GetValueOrDefault() ? 0 : (dialogResult.HasValue ? 1 : 0)) != 0)
      {
        List<string> soundFiles = ((IEnumerable<string>) this._textEditor.Text.Split(new string[1]
        {
          "\r\n"
        }, StringSplitOptions.None)).ToList<string>();
        while (soundFiles.Count > 0 && soundFiles.Last<string>() == "")
          soundFiles.RemoveAt(soundFiles.Count - 1);
        for (int index = 0; index < soundFiles.Count; ++index)
        {
          if (soundFiles[index].Length > 39)
          {
            ErrorHandler.HandleException("The sound file at " + (object) (index + 1) + " has a name too long. It must be below 40 characters.");
            this.DialogResult = new bool?(false);
            e.Cancel = true;
            return;
          }
        }
        if (soundFiles.Count == this._act.SoundFiles.Count)
        {
          if (Methods.ListToString(soundFiles) == Methods.ListToString(this._act.SoundFiles))
            return;
        }
        try
        {
          this._act.Commands.Begin();
          this._act.Commands.Backup((Action<Act>) (act =>
          {
            act.SoundFiles.Clear();
            act.SoundFiles.AddRange((IEnumerable<string>) soundFiles);
            foreach (GRF.FileFormats.ActFormat.Frame allFrame in act.GetAllFrames())
            {
              if (allFrame.SoundId >= soundFiles.Count)
                allFrame.SoundId = -1;
            }
          }), "Sound list modified");
        }
        catch (Exception ex)
        {
          this._act.Commands.CancelEdit();
          ErrorHandler.HandleException(ex);
        }
        finally
        {
          this._act.Commands.End();
          this._act.InvalidateVisual();
        }
      }
      base.OnClosing(e);
    }

    private void _buttonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(true);
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
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/soundeditdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._textEditor = (TextEditor) target;
          break;
        case 2:
          this._gridActionPresenter = (Grid) target;
          break;
        case 3:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        case 4:
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
