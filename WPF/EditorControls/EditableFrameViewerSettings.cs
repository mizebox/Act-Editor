// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.EditableFrameViewerSettings
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;

namespace ActEditor.Core.WPF.EditorControls
{
  public class EditableFrameViewerSettings : FrameViewerSettings
  {
    public Func<SelectionEngine> SelectionEngineMethod = (Func<SelectionEngine>) (() => (SelectionEngine) null);
    public Action<int> SetAnimation = (Action<int>) (v => { });

    public SelectionEngine SelectionEngine => this.SelectionEngineMethod();
  }
}
