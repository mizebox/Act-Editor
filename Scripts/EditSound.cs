// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.EditSound
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.Dialogs;
using GRF.FileFormats.ActFormat;
using TokeiLibrary;

namespace ActEditor.Core.Scripts
{
  public class EditSound : IActScript
  {
    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      SoundEditDialog soundEditDialog = new SoundEditDialog(act);
      soundEditDialog.Owner = WpfUtilities.TopWindow;
      soundEditDialog.ShowDialog();
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null;
    }

    public object DisplayName => (object) "Edit sound list...";

    public string Group => "Edit";

    public string InputGesture => (string) null;

    public string Image => "soundOn.png";
  }
}
