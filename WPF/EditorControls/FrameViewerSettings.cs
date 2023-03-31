// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.FrameViewerSettings
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.Collections.Generic;

namespace ActEditor.Core.WPF.EditorControls
{
  public class FrameViewerSettings
  {
    public Func<double> ZoomInMultipler = (Func<double>) (() => 1.0);
    public Func<GRF.FileFormats.ActFormat.Act> Act = (Func<GRF.FileFormats.ActFormat.Act>) (() => (GRF.FileFormats.ActFormat.Act) null);
    public Func<int> SelectedAction = (Func<int>) (() => 0);
    public Func<int> SelectedFrame = (Func<int>) (() => 0);
    public Func<bool> ShowAnchors = (Func<bool>) (() => false);
    public Func<List<ActReference>> ReferencesGetter = (Func<List<ActReference>>) (() => new List<ActReference>());
    public Func<bool> IsPlaying = (Func<bool>) (() => false);

    public List<ActReference> References => this.ReferencesGetter();
  }
}
