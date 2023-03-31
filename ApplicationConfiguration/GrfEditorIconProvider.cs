// Decompiled with JetBrains decompiler
// Type: ActEditor.ApplicationConfiguration.GrfEditorIconProvider
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.Image;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using Utilities.Extension;

namespace ActEditor.ApplicationConfiguration
{
  public class GrfEditorIconProvider
  {
    private static readonly Dictionary<string, BitmapSource> _associatedExtensions = new Dictionary<string, BitmapSource>();
    private static readonly Dictionary<string, BitmapSource> _associatedLargeExtensions = new Dictionary<string, BitmapSource>();

    public static BitmapSource GetLargeIcon(string file)
    {
      string extension = file.GetExtension();
      if (extension == null)
        return (BitmapSource) null;
      if (!GrfEditorIconProvider._associatedLargeExtensions.ContainsKey(extension))
        GrfEditorIconProvider._loadLargeExtension(extension);
      return GrfEditorIconProvider._associatedLargeExtensions[extension];
    }

    public static BitmapSource GetIcon(string file)
    {
      string extension = file.GetExtension();
      if (extension == null)
        return (BitmapSource) null;
      if (!GrfEditorIconProvider._associatedExtensions.ContainsKey(extension))
        GrfEditorIconProvider._loadExtension(extension);
      return GrfEditorIconProvider._associatedExtensions[extension];
    }

    private static void _loadExtension(string extension)
    {
      try
      {
        switch (extension)
        {
          case ".pal":
            byte[] resource1 = ApplicationManager.GetResource("pal.png");
            GrfImage grfImage1 = new GrfImage(ref resource1);
            GrfEditorIconProvider._associatedExtensions.Add(extension, grfImage1.Cast<BitmapSource>());
            break;
          case ".gnd":
          case ".rsw":
          case ".gat":
          case ".rsm":
          case ".lua":
          case ".lub":
          case ".imf":
          case ".xml":
          case ".str":
            byte[] resource2 = ApplicationManager.GetResource("file_" + extension.Substring(1) + ".png");
            if (resource2 == null)
            {
              GrfEditorIconProvider._loadAny(extension);
              break;
            }
            GrfImage grfImage2 = new GrfImage(ref resource2);
            GrfEditorIconProvider._associatedExtensions.Add(extension, grfImage2.Cast<BitmapSource>());
            break;
          default:
            GrfEditorIconProvider._loadAny(extension);
            break;
        }
      }
      catch
      {
        byte[] resource = ApplicationManager.GetResource("pal.png");
        GrfImage grfImage = new GrfImage(ref resource);
        GrfEditorIconProvider._associatedExtensions.Add(extension, grfImage.Cast<BitmapSource>());
      }
    }

    private static void _loadAny(string extension)
    {
      byte[] buffer;
      using (MemoryStream memoryStream = new MemoryStream())
      {
        IconReader.GetFileIcon(extension, true, false).ToBitmap().Save((Stream) memoryStream, ImageFormat.Png);
        buffer = memoryStream.GetBuffer();
      }
      GrfImage grfImage = new GrfImage(ref buffer);
      GrfEditorIconProvider._associatedExtensions.Add(extension, grfImage.Cast<BitmapSource>());
    }

    private static void _loadLargeExtension(string extension)
    {
      try
      {
        byte[] buffer;
        using (MemoryStream memoryStream = new MemoryStream())
        {
          IconReader.GetFileIcon(extension, false, false).ToBitmap().Save((Stream) memoryStream, ImageFormat.Png);
          buffer = memoryStream.GetBuffer();
        }
        GrfImage grfImage = new GrfImage(ref buffer);
        GrfEditorIconProvider._associatedLargeExtensions.Add(extension, grfImage.Cast<BitmapSource>());
      }
      catch
      {
        byte[] resource = ApplicationManager.GetResource("pal.png");
        GrfImage grfImage = new GrfImage(ref resource);
        GrfEditorIconProvider._associatedLargeExtensions.Add(extension, grfImage.Cast<BitmapSource>());
      }
    }
  }
}
