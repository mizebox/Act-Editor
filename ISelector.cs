// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.ISelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.EditorControls;
using GRF.FileFormats.ActFormat;

namespace ActEditor.Core
{
  public interface ISelector
  {
    event ActIndexSelector.FrameIndexChangedDelegate ActionChanged;

    event ActIndexSelector.FrameIndexChangedDelegate FrameChanged;

    event ActIndexSelector.FrameIndexChangedDelegate SpecialFrameChanged;

    int SelectedAction { get; }

    int SelectedFrame { get; }

    Act Act { get; }
  }
}
