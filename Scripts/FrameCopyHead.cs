// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.FrameCopyHead
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.Dialogs;
using GRF.FileFormats.ActFormat;
using System;
using System.Windows;

namespace ActEditor.Core.Scripts
{
  public class FrameCopyHead : IActScript
  {
    private readonly ActEditorWindow _editor;
    private bool _canUse = true;

    public object DisplayName => (object) "Setup Headgear...";

    public string InputGesture => (string) null;

    public string Image => "empty.png";

    public string Group => "Animation";

    public FrameCopyHead(ActEditorWindow editor)
    {
      this._editor = editor;
      this._canUse = true;
    }

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null)
        return;
      HeadEditorDialog headEditorDialog = new HeadEditorDialog();
      headEditorDialog.Init(this._editor, act);
      headEditorDialog.Owner = (Window) this._editor;
      this._canUse = false;
      headEditorDialog.Show();
      headEditorDialog.Closed += (EventHandler) delegate
      {
        this._canUse = true;
      };
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && this._canUse;
    }
  }
}
