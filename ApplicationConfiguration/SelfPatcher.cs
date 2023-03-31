// Decompiled with JetBrains decompiler
// Type: ActEditor.ApplicationConfiguration.SelfPatcher
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.Collections.Generic;
using System.Linq;

namespace ActEditor.ApplicationConfiguration
{
  public class SelfPatcher
  {
    public static List<ActEditor.ApplicationConfiguration.SelfPatch> Patches = new List<ActEditor.ApplicationConfiguration.SelfPatch>();
    public static readonly ActEditor.ApplicationConfiguration.SelfPatch Patch0013 = (ActEditor.ApplicationConfiguration.SelfPatch) new RefreshScripts(13);

    static SelfPatcher() => SelfPatcher.Patches = SelfPatcher.Patches.OrderBy<ActEditor.ApplicationConfiguration.SelfPatch, int>((Func<ActEditor.ApplicationConfiguration.SelfPatch, int>) (p => p.PatchId)).ToList<ActEditor.ApplicationConfiguration.SelfPatch>();

    public static void SelfPatch()
    {
      int num = GrfEditorConfiguration.PatchId;
      foreach (ActEditor.ApplicationConfiguration.SelfPatch patch in SelfPatcher.Patches)
      {
        if (patch.PatchId >= num)
        {
          patch.PatchAppliaction();
          num = patch.PatchId + 1;
        }
      }
      GrfEditorConfiguration.PatchId = num;
    }
  }
}
