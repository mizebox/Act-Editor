// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ScriptRunnerMenu
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.Dialogs;
using GRF.FileFormats.ActFormat;
using System;
using TokeiLibrary;

namespace ActEditor.Core.Scripts
{
  public class ScriptRunnerMenu : IActScript
  {
    public ScriptRunnerMenu() => this.IsEnabled = true;

    public bool IsEnabled { get; set; }

    public ActEditorWindow ActEditor { get; set; }

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      ScriptRunnerDialog scriptRunnerDialog = new ScriptRunnerDialog(this.ActEditor);
      scriptRunnerDialog.Owner = WpfUtilities.TopWindow;
      scriptRunnerDialog.Show();
      this.IsEnabled = false;
      scriptRunnerDialog.Closed += (EventHandler) delegate
      {
        this.IsEnabled = true;
      };
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return this.IsEnabled;
    }

    public object DisplayName => (object) "Script Runner...";

    public string Group => "Scripts";

    public string InputGesture => (string) null;

    public string Image => "dos.png";
  }
}
