// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.IActIndexSelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.EditorControls;

namespace ActEditor.Core
{
  public interface IActIndexSelector
  {
    void OnFrameChanged(int actionindex);

    bool IsPlaying { get; }

    event ActIndexSelector.FrameIndexChangedDelegate ActionChanged;

    event ActIndexSelector.FrameIndexChangedDelegate FrameChanged;

    event ActIndexSelector.FrameIndexChangedDelegate SpecialFrameChanged;

    void OnAnimationPlaying(int actionindex);

    void SetAction(int index);

    void SetFrame(int index);
  }
}
