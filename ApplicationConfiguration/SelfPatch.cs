// Decompiled with JetBrains decompiler
// Type: ActEditor.ApplicationConfiguration.SelfPatch
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;

namespace ActEditor.ApplicationConfiguration
{
  public abstract class SelfPatch
  {
    private readonly int _patchId;

    protected SelfPatch(int patchId)
    {
      this._patchId = patchId;
      SelfPatcher.Patches.Add(this);
    }

    public int PatchId => this._patchId;

    public abstract bool PatchAppliaction();

    public void Safe(Action action)
    {
      try
      {
        action();
      }
      catch
      {
      }
    }
  }
}
