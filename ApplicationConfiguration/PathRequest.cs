// Decompiled with JetBrains decompiler
// Type: ActEditor.ApplicationConfiguration.PathRequest
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using TokeiLibrary.Paths;
using Utilities;

namespace ActEditor.ApplicationConfiguration
{
  public static class PathRequest
  {
    public static Setting ExtractSetting => new Setting((object) null, typeof (GrfEditorConfiguration).GetProperty("ExtractingServiceLastPath"));

    public static Setting SaveAdvancedSetting => new Setting((object) null, typeof (GrfEditorConfiguration).GetProperty("SaveAdvancedLastPath"));

    public static string SaveFileEditor(params string[] extra) => TkPathRequest.SaveFile(new Setting((object) null, typeof (GrfEditorConfiguration).GetProperty("AppLastPath")), extra);

    public static string SaveFileExtract(params string[] extra) => TkPathRequest.SaveFile(PathRequest.ExtractSetting, extra);

    public static string OpenFileEditor(params string[] extra) => TkPathRequest.OpenFile(new Setting((object) null, typeof (GrfEditorConfiguration).GetProperty("AppLastPath")), extra);

    public static string OpenGrfFile(params string[] extra) => TkPathRequest.OpenFile(new Setting((object) null, typeof (GrfEditorConfiguration).GetProperty("AppLastGrfPath")), extra);

    public static string OpenFileExtract(params string[] extra) => TkPathRequest.OpenFile(PathRequest.ExtractSetting, extra);

    public static string[] OpenFilesExtract(params string[] extra) => TkPathRequest.OpenFiles(PathRequest.ExtractSetting, extra);

    public static string FolderEditor(params string[] extra) => TkPathRequest.Folder(new Setting((object) null, typeof (GrfEditorConfiguration).GetProperty("AppLastPath")), extra);

    public static string FolderExtract(params string[] extra) => TkPathRequest.Folder(PathRequest.ExtractSetting);

    public static string FolderSaveAdvanced(params string[] extra) => TkPathRequest.Folder(PathRequest.SaveAdvancedSetting);
  }
}
