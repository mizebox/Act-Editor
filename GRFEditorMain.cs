// Decompiled with JetBrains decompiler
// Type: ActEditor.GRFEditorMain
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ActEditor
{
  public class GRFEditorMain
  {
    private static readonly string[] _registeredAssemblies = new string[24]
    {
      "ErrorManager",
      "ICSharpCode.AvalonEdit",
      "GRF",
      "TokeiLibrary",
      "PaletteRecolorer",
      "Be.Windows.Forms.HexBox",
      "zlib.net",
      "Utilities",
      "cps",
      "Encryption",
      "Gif.Components",
      "ColorPicker",
      "GrfMenuHandler64",
      "GrfMenuHandler32",
      "msvcp100",
      "msvcr100",
      "ActImaging",
      "Database",
      "Lua",
      "XDMessaging",
      "ErrorManager",
      "GrfToWpfBridge",
      "System.Threading",
      "PaletteEditor"
    };

    [STAThread]
    public static void Main(string[] args)
    {
      AppDomain.CurrentDomain.AssemblyResolve += (ResolveEventHandler) ((sender, arguments) =>
      {
        AssemblyName assemblyName = new AssemblyName(arguments.Name);
        if (assemblyName.Name.EndsWith(".resources"))
          return (Assembly) null;
        string name1 = "ActEditor.Files." + assemblyName.Name + ".dll";
        using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name1))
        {
          if (manifestResourceStream != null)
          {
            byte[] numArray = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(numArray, 0, numArray.Length);
            return Assembly.Load(numArray);
          }
        }
        string name2 = "ActEditor.Files.Compressed." + new AssemblyName(arguments.Name).Name + ".dll";
        using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name2))
        {
          if (manifestResourceStream != null)
          {
            byte[] numArray = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(numArray, 0, numArray.Length);
            return Assembly.Load(GRFEditorMain.Decompress(numArray));
          }
        }
        if (((IEnumerable<string>) GRFEditorMain._registeredAssemblies).ToList<string>().Contains(assemblyName.Name))
        {
          int num = (int) MessageBox.Show("Failed to load assembly : " + name1 + "\r\n\r\nThe application will now shutdown.", "Assembly loader");
          Process.GetCurrentProcess().Kill();
        }
        return (Assembly) null;
      });
      Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
      App app = new App();
      app.StartupUri = new Uri("Core\\ActEditorWindow.xaml", UriKind.Relative);
      app.Run();
    }

    public static byte[] Decompress(byte[] data)
    {
      using (MemoryStream memoryStream1 = new MemoryStream(data))
      {
        using (GZipStream gzipStream = new GZipStream((Stream) memoryStream1, CompressionMode.Decompress))
        {
          byte[] buffer = new byte[4096];
          using (MemoryStream memoryStream2 = new MemoryStream())
          {
            int count;
            do
            {
              count = gzipStream.Read(buffer, 0, 4096);
              if (count > 0)
                memoryStream2.Write(buffer, 0, count);
            }
            while (count > 0);
            return memoryStream2.ToArray();
          }
        }
      }
    }

    public static byte[] Compress(byte[] data)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (GZipStream gzipStream = new GZipStream((Stream) memoryStream, CompressionMode.Compress, true))
          gzipStream.Write(data, 0, data.Length);
        return memoryStream.ToArray();
      }
    }
  }
}
