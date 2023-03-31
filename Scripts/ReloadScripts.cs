// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.ReloadScripts
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;

namespace ActEditor.Core.Scripts
{
  public class ReloadScripts : IActScript
  {
    public ActEditorWindow ActEditor { get; set; }

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      try
      {
        this.ActEditor.ScriptLoader.RecompileScripts();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return true;
    }

    public object DisplayName => (object) "Reload scripts";

    public string Group => "Scripts";

    public string InputGesture => (string) null;

    public string Image => "refresh.png";
  }
}
