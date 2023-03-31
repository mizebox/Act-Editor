// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.SpriteExport
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using GRF.IO;
using GrfToWpfBridge;
using System;
using Utilities.Services;

namespace ActEditor.Core.Scripts
{
  public class SpriteExport : IActScript
  {
    public object DisplayName => (object) "__IndexOverride,12__%Export all sprites...";

    public string Group => "File";

    public string InputGesture => (string) null;

    public string Image => "export.png";

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act == null)
        return;
      try
      {
        string path = PathRequest.FolderExtract();
        if (path == null)
          return;
        string name = "image_{0:0000}";
        int i = 0;
        TaskManager.DisplayTaskC("Export", "Exporting sprites...", (Func<int>) (() => i), act.Sprite.NumberOfImagesLoaded, (Action<Func<bool>>) (isCancelling =>
        {
          for (int numberOfImagesLoaded = act.Sprite.NumberOfImagesLoaded; i < numberOfImagesLoaded; ++i)
          {
            GrfImage image = act.Sprite.Images[i];
            if (image.GrfImageType == GrfImageType.Indexed8)
              image.Save(GrfPath.Combine(path, string.Format(name, (object) i) + ".bmp"));
            else
              image.Save(GrfPath.Combine(path, string.Format(name, (object) i) + ".png"));
          }
        }));
        OpeningService.FileOrFolder(path);
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
      return act != null;
    }
  }
}
