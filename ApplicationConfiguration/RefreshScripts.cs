// Decompiled with JetBrains decompiler
// Type: ActEditor.ApplicationConfiguration.RefreshScripts
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core;
using GRF.IO;
using System.IO;

namespace ActEditor.ApplicationConfiguration
{
  public class RefreshScripts : SelfPatch
  {
    public RefreshScripts(int patchId)
      : base(patchId)
    {
    }

    public override bool PatchAppliaction()
    {
      try
      {
        string path1 = GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "Scripts");
        foreach (string scriptName in ScriptLoader.ScriptNames)
        {
          string[] strArray = new string[2]
          {
            scriptName + ".cs",
            scriptName + ".dll"
          };
          foreach (string path2 in strArray)
            GrfPath.Delete(Path.Combine(path1, path2));
        }
      }
      catch
      {
        return false;
      }
      return true;
    }
  }
}
