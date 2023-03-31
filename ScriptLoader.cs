// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.ScriptLoader
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using GRF.IO;
using GRF.System;
using GRF.Threading;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using Utilities;
using Utilities.Extension;
using Utilities.Hash;

namespace ActEditor.Core
{
  public class ScriptLoader : IDisposable
  {
    public const string OutputPath = "Scripts";
    public const string OverrideIndex = "__IndexOverride";
    internal static string[] ScriptNames = new string[9]
    {
      "script_sample",
      "script0_magnify",
      "script0_magnifyAll",
      "script1_replace_color",
      "script1_replace_color_all",
      "script2_expand",
      "script3_mirror_frame",
      "script4_generate_single_sprite",
      "script5_remove_unused_sprites"
    };
    internal static string[] Libraries = new string[4]
    {
      "GRF.dll",
      "Utilities.dll",
      "TokeiLibrary.dll",
      "ErrorManager.dll"
    };
    private static ConfigAsker _librariesConfiguration;
    private readonly FileSystemWatcher _fsw;
    private readonly HashSet<MenuItem> _initialMenuItems = new HashSet<MenuItem>();
    private readonly object _lock = new object();
    private readonly int _procId;
    private ActEditorWindow _actEditor;
    private DockPanel _dockPanel;
    private Menu _menu;

    public ScriptLoader()
    {
      this._fsw = new FileSystemWatcher();
      this._procId = Process.GetCurrentProcess().Id;
      TemporaryFilesManager.UniquePattern(this._procId.ToString() + "_script_{0:0000}");
      string path = GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "Scripts");
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      this._fsw.Path = path;
      this._fsw.Filter = "*.cs";
      this._fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
      this._fsw.Changed += new FileSystemEventHandler(this._fileChanged);
      this._fsw.Created += new FileSystemEventHandler(this._fileChanged);
      this._fsw.Renamed += new RenamedEventHandler(this._fileChanged);
      this._fsw.Deleted += new FileSystemEventHandler(this._fileChanged);
      this._fsw.EnableRaisingEvents = true;
    }

    public static ConfigAsker LibrariesConfiguration
    {
      get
      {
        ConfigAsker librariesConfiguration = ScriptLoader._librariesConfiguration;
        if (librariesConfiguration != null)
          return librariesConfiguration;
        string[] strArray = new string[3]
        {
          GrfEditorConfiguration.ProgramDataPath,
          "Scripts",
          "scripts.dat"
        };
        return ScriptLoader._librariesConfiguration = new ConfigAsker(GrfPath.Combine(strArray));
      }
    }

    public void ReloadScripts()
    {
      if (this._actEditor == null || this._menu == null)
        return;
      if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        GrfThread.StartSTA((System.Action) (() => this.AddScriptsToMenu(this._actEditor, this._menu, this._dockPanel)));
      else
        this.AddScriptsToMenu(this._actEditor, this._menu, this._dockPanel);
    }

    public void RecompileScripts()
    {
      ScriptLoader.LibrariesConfiguration.DeleteKeys("");
      this.ReloadScripts();
    }

    public void ReloadLibraries()
    {
      try
      {
        ((IEnumerable<string>) ScriptLoader.Libraries).ToList<string>().ForEach((Action<string>) (v => Utilities.Debug.Ignore((System.Action) (() => File.WriteAllBytes(GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "Scripts", v), ApplicationManager.GetResource(v, true))))));
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public void AddScriptsToMenu(
      IActScript actScript,
      ActEditorWindow actEditor,
      Menu menu,
      FrameworkElement dockPanel)
    {
      this._setupMenuItem(actScript, menu, actEditor, (UIElement) this._generateScriptMenu(actEditor, actScript), (List<MenuItem>) null);
      ScriptLoader._setupSize((ItemsControl) menu, dockPanel);
    }

    public void AddScriptsToMenu(ActEditorWindow actEditor, Menu menu, DockPanel dockPanel)
    {
      lock (this._lock)
      {
        this._refreshMenu(menu);
        this._actEditor = actEditor;
        this._dockPanel = dockPanel;
        List<MenuItem> toAdd = new List<MenuItem>();
        this.ReloadLibraries();
        this.DeleteDlls();
        foreach (string str1 in (IEnumerable<string>) ((IEnumerable<string>) Directory.GetFiles(GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "Scripts"), "*.cs")).OrderBy<string, string>((Func<string, string>) (p => p), (IComparer<string>) StringComparer.CurrentCultureIgnoreCase))
        {
          if (!(Path.GetFileNameWithoutExtension(str1) == "script_sample"))
          {
            string str2 = ScriptLoader.LibrariesConfiguration["[" + (object) Path.GetFileName(str1).GetHashCode() + "]", "NULL"];
            bool flag = false;
            string str3 = str1.ReplaceExtension(".dll");
            if (str2 != "NULL" && File.Exists(str3) && new Md5Hash().ComputeHash(File.ReadAllBytes(str1)) + "," + new Md5Hash().ComputeHash(File.ReadAllBytes(str3)) == str2)
              flag = true;
            if (flag)
            {
              try
              {
                string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath(this._procId.ToString() + "_script_{0:0000}");
                GrfPath.Delete(temporaryFilePath + ".dll");
                File.Copy(str3, temporaryFilePath + ".dll");
                File.Copy(str1, temporaryFilePath + ".cs");
                this._addScriptFromAssembly(temporaryFilePath + ".dll", toAdd);
                GrfPath.Delete(temporaryFilePath + ".cs");
              }
              catch
              {
                GrfPath.Delete(str3);
                string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath(this._procId.ToString() + "_script_{0:0000}");
                GrfPath.Delete(temporaryFilePath + ".dll");
                File.Copy(str1, temporaryFilePath + ".cs");
                this._addFromScript(str1, temporaryFilePath + ".cs", toAdd);
                GrfPath.Delete(temporaryFilePath + ".cs");
              }
            }
            else
            {
              string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath(this._procId.ToString() + "_script_{0:0000}");
              GrfPath.Delete(str1.ReplaceExtension(".dll"));
              File.Copy(str1, temporaryFilePath + ".cs");
              this._addFromScript(str1, temporaryFilePath + ".cs", toAdd);
              GrfPath.Delete(temporaryFilePath + ".cs");
            }
          }
        }
        ScriptLoader._setupSize((ItemsControl) this._menu, (FrameworkElement) dockPanel, (ICollection<MenuItem>) toAdd);
      }
    }

    private void _refreshMenu(Menu menu)
    {
      if (this._menu == null)
        menu.Dispatch<Menu>((Action<Menu>) delegate
        {
          foreach (ItemsControl itemsControl in (IEnumerable) menu.Items)
          {
            foreach (MenuItem menuItem in itemsControl.Items.OfType<MenuItem>())
              this._initialMenuItems.Add(menuItem);
          }
        });
      if (menu != null)
        menu.Dispatch<Menu>((Action<Menu>) delegate
        {
          for (int index1 = 0; index1 < menu.Items.Count; ++index1)
          {
            MenuItem removeItem2 = (MenuItem) menu.Items[index1];
            for (int index2 = 0; index2 < removeItem2.Items.Count; ++index2)
            {
              if (removeItem2.Items[index2] is MenuItem removeItem3 && !this._initialMenuItems.Contains(removeItem3))
              {
                removeItem2.Items.Remove((object) removeItem3);
                --index2;
              }
            }
            if (removeItem2.Items.Count == 0)
            {
              menu.Items.Remove((object) removeItem2);
              --index1;
            }
          }
        });
      this._menu = menu;
    }

    public void DeleteDlls()
    {
      string[] strArray = new string[2]
      {
        GrfEditorConfiguration.ProgramDataPath,
        "Scripts"
      };
      foreach (string file in Directory.GetFiles(GrfPath.Combine(strArray), "*.dll"))
      {
        if (!((IEnumerable<string>) ScriptLoader.Libraries).Contains<string>(Path.GetFileName(file)) && !File.Exists(file.ReplaceExtension(".cs")))
          GrfPath.Delete(file);
      }
    }

    public static IActScript GetScriptObjectFromAssembly(string assemblyPath)
    {
      object instance = Assembly.LoadFile(assemblyPath).CreateInstance("Scripts.Script");
      if (instance == null)
        throw new Exception("Couldn't instantiate the script object. Type not found?");
      return instance is IActScript actScript ? actScript : throw new Exception("Couldn't instantiate the script object. Type not found?");
    }

    internal static CompilerResults Compile(string script, out string dll)
    {
      if (File.Exists(script.ReplaceExtension(".dll")))
        GrfPath.Delete(script.ReplaceExtension(".dll"));
      CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider((IDictionary<string, string>) new Dictionary<string, string>()
      {
        {
          "CompilerVersion",
          "v3.5"
        }
      });
      string str = script.ReplaceExtension(".dll");
      CompilerParameters options = new CompilerParameters()
      {
        GenerateExecutable = false,
        OutputAssembly = str
      };
      foreach (AssemblyName referencedAssembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
        options.ReferencedAssemblies.Add(referencedAssembly.Name + ".dll");
      CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromFile(options, script);
      dll = str;
      return compilerResults;
    }

    public static void VerifyExampleScriptsInstalled()
    {
      string path1 = GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "Scripts");
      if (!Directory.Exists(path1))
        Directory.CreateDirectory(path1);
      try
      {
        ((IEnumerable<string>) ScriptLoader.Libraries).ToList<string>().ForEach((Action<string>) (v => Utilities.Debug.Ignore((System.Action) (() => File.WriteAllBytes(GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "Scripts", v), ApplicationManager.GetResource(v, true))))));
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      foreach (string scriptName in ScriptLoader.ScriptNames)
      {
        bool flag = false;
        string[] strArray = new string[2]
        {
          scriptName + ".cs",
          scriptName + ".dll"
        };
        foreach (string resource in strArray)
        {
          string path2 = GrfPath.Combine(path1, resource);
          if (!File.Exists(path2))
          {
            File.WriteAllBytes(path2, ApplicationManager.GetResource(resource));
            flag = true;
          }
        }
        if (flag)
          ScriptLoader.LibrariesConfiguration["[" + (object) Path.GetFileName(scriptName + ".cs").GetHashCode() + "]"] = new Md5Hash().ComputeHash(File.ReadAllBytes(GrfPath.Combine(path1, scriptName + ".cs"))) + "," + new Md5Hash().ComputeHash(File.ReadAllBytes(GrfPath.Combine(path1, scriptName + ".dll")));
      }
    }

    private void _fileChanged(object sender, FileSystemEventArgs e)
    {
      try
      {
        this._fsw.EnableRaisingEvents = false;
        this.ReloadScripts();
      }
      finally
      {
        this._fsw.EnableRaisingEvents = true;
      }
    }

    private static void _setupSize(ItemsControl menu, FrameworkElement dockPanel)
    {
      if (dockPanel == null)
        return;
      double length = 0.0;
      List<MenuItem> list = menu.Items.Cast<MenuItem>().ToList<MenuItem>();
      if (list.Count > 0 && !list.Last<MenuItem>().IsLoaded)
      {
        list.Last<MenuItem>().Loaded += (RoutedEventHandler) delegate
        {
          foreach (UIElement uiElement in (IEnumerable) menu.Items)
            length += uiElement.DesiredSize.Width;
          dockPanel.Margin = new Thickness(length, 0.0, 0.0, 0.0);
        };
      }
      else
      {
        foreach (MenuItem menuItem in (IEnumerable) menu.Items)
          length += menuItem.DesiredSize.Width;
        dockPanel.Margin = new Thickness(length, 0.0, 0.0, 0.0);
      }
    }

    private static void _setupSize(
      ItemsControl menu,
      FrameworkElement dockPanel,
      ICollection<MenuItem> toAdd)
    {
      double length = 0.0;
      menu.Dispatch<ItemsControl>((Action<ItemsControl>) delegate
      {
        foreach (object newItem in (IEnumerable<MenuItem>) toAdd)
          menu.Items.Add(newItem);
        if (toAdd.Count > 0 && !toAdd.Last<MenuItem>().IsLoaded)
        {
          toAdd.Last<MenuItem>().Loaded += (RoutedEventHandler) delegate
          {
            foreach (UIElement uiElement in (IEnumerable) menu.Items)
              length += uiElement.DesiredSize.Width;
            dockPanel.Margin = new Thickness(length, 0.0, 0.0, 0.0);
          };
        }
        else
        {
          foreach (UIElement uiElement in (IEnumerable) menu.Items)
            length += uiElement.DesiredSize.Width;
          dockPanel.Margin = new Thickness(length, 0.0, 0.0, 0.0);
        }
      });
    }

    private void _setupMenuItem(
      IActScript actScript,
      Menu menu,
      ActEditorWindow actEditor,
      UIElement scriptMenu,
      List<MenuItem> toAdd)
    {
      MenuItem menuItem = ScriptLoader._retrieveConcernedMenuItem(actScript, menu, toAdd);
      menuItem.SubmenuOpened += (RoutedEventHandler) delegate
      {
        int selectedActionIndex = -1;
        int selectedFrameIndex = -1;
        int[] selectedLayerIndexes = new int[0];
        if (actEditor.Act != null)
        {
          selectedActionIndex = this._actEditor._frameSelector.SelectedAction;
          selectedFrameIndex = this._actEditor._frameSelector.SelectedFrame;
          selectedLayerIndexes = this._actEditor.SelectionEngine.CurrentlySelected.OrderBy<int, int>((Func<int, int>) (p => p)).ToArray<int>();
        }
        scriptMenu.IsEnabled = actScript.CanExecute(actEditor.Act, selectedActionIndex, selectedFrameIndex, selectedLayerIndexes);
      };
      string[] parameter = ScriptLoader._getParameter(actScript, "__IndexOverride");
      if (parameter != null && parameter.Length > 0)
      {
        int result;
        if (!int.TryParse(parameter[0], out result))
          return;
        menuItem.Items.Insert(result, (object) scriptMenu);
      }
      else
        menuItem.Items.Add((object) scriptMenu);
    }

    private static string[] _getParameter(IActScript actScript, string parameter)
    {
      if (!(actScript.DisplayName is string displayName))
        return (string[]) null;
      int num1 = displayName.IndexOf(parameter, 0, StringComparison.OrdinalIgnoreCase);
      if (num1 > -1)
      {
        int num2 = displayName.IndexOf("__", num1 + parameter.Length, StringComparison.Ordinal);
        if (num2 > -1)
          return displayName.Substring(num1 + parameter.Length, num2 - (num1 + parameter.Length)).Split(new string[1]
          {
            ","
          }, StringSplitOptions.RemoveEmptyEntries);
      }
      return (string[]) null;
    }

    private static string _getString(HeaderedItemsControl menuItem) => !(menuItem.Header is Label header) ? menuItem.Header.ToString() : header.Content.ToString();

    private static MenuItem _retrieveConcernedMenuItem(
      IActScript actScript,
      Menu menu,
      List<MenuItem> toAdd)
    {
      if (actScript.Group.Contains("/") && toAdd == null)
      {
        string[] strArray = actScript.Group.Split(new char[1]
        {
          '/'
        }, StringSplitOptions.RemoveEmptyEntries);
        List<MenuItem> list = menu.Items.Cast<MenuItem>().ToList<MenuItem>();
        MenuItem newItem = (MenuItem) null;
        ItemCollection items = menu.Items;
        foreach (string str in strArray)
        {
          string group = str;
          newItem = list.FirstOrDefault<MenuItem>((Func<MenuItem, bool>) (p => ScriptLoader._getString((HeaderedItemsControl) p) == group));
          if (newItem == null)
          {
            if (group == strArray[0])
            {
              MenuItem menuItem1 = new MenuItem();
              MenuItem menuItem2 = menuItem1;
              Label label1 = new Label();
              label1.Content = (object) group;
              label1.VerticalAlignment = VerticalAlignment.Center;
              label1.HorizontalAlignment = HorizontalAlignment.Center;
              label1.Margin = new Thickness(-5.0, 0.0, -5.0, 0.0);
              Label label2 = label1;
              menuItem2.Header = (object) label2;
              newItem = menuItem1;
            }
            else
            {
              MenuItem menuItem = new MenuItem();
              menuItem.Header = (object) group;
              newItem = menuItem;
            }
            items.Add((object) newItem);
          }
          list = newItem.Items.OfType<MenuItem>().ToList<MenuItem>();
          items = newItem.Items;
        }
        return newItem;
      }
      MenuItem newItem1 = menu.Items.Cast<MenuItem>().FirstOrDefault<MenuItem>((Func<MenuItem, bool>) (p => ScriptLoader._getString((HeaderedItemsControl) p) == actScript.Group));
      if (toAdd != null && newItem1 == null)
        newItem1 = toAdd.FirstOrDefault<MenuItem>((Func<MenuItem, bool>) (p => ScriptLoader._getString((HeaderedItemsControl) p) == actScript.Group));
      if (newItem1 == null)
      {
        MenuItem menuItem3 = new MenuItem();
        MenuItem menuItem4 = menuItem3;
        Label label3 = new Label();
        label3.Content = (object) actScript.Group;
        label3.VerticalAlignment = VerticalAlignment.Center;
        label3.HorizontalAlignment = HorizontalAlignment.Center;
        label3.Margin = new Thickness(-5.0, 0.0, -5.0, 0.0);
        Label label4 = label3;
        menuItem4.Header = (object) label4;
        newItem1 = menuItem3;
        if (toAdd != null)
          toAdd.Add(newItem1);
        else
          menu.Items.Add((object) newItem1);
      }
      return newItem1;
    }

    private MenuItem _generateScriptMenu(ActEditorWindow actEditor, IActScript actScript)
    {
      MenuItem scriptMenu = new MenuItem();
      if (actScript.DisplayName is string)
      {
        string str = actScript.DisplayName.ToString();
        int num = str.IndexOf("__%", 0, StringComparison.Ordinal);
        if (num > -1)
          scriptMenu.Header = (object) str.Substring(num + 3);
        else
          scriptMenu.Header = (object) str;
      }
      else
        scriptMenu.Header = actScript.DisplayName;
      if (actScript.InputGesture != null)
        scriptMenu.InputGestureText = ((IEnumerable<string>) actScript.InputGesture.Split(':')).FirstOrDefault<string>();
      if (actScript.Image != null)
        scriptMenu.Icon = (object) new System.Windows.Controls.Image()
        {
          Source = ScriptLoader.GetImage(actScript.Image)
        };
      System.Action action = (System.Action) (() =>
      {
        int selectedActionIndex = -1;
        int selectedFrameIndex = -1;
        int[] selectedLayerIndexes = new int[0];
        if (this._actEditor.Act != null)
        {
          selectedActionIndex = actEditor._frameSelector.SelectedAction;
          selectedFrameIndex = actEditor._frameSelector.SelectedFrame;
          selectedLayerIndexes = actEditor.SelectionEngine.CurrentlySelected.OrderBy<int, int>((Func<int, int>) (p => p)).ToArray<int>();
        }
        if (!actScript.CanExecute(actEditor.Act, selectedActionIndex, selectedFrameIndex, selectedLayerIndexes))
          return;
        int num = -1;
        if (actEditor.Act != null)
          num = actEditor.Act.Commands.CommandIndex;
        actScript.Execute(actEditor.Act, selectedActionIndex, selectedFrameIndex, selectedLayerIndexes);
        if (actEditor.Act != null && actEditor.Act.Commands.CommandIndex == num)
          return;
        actEditor._frameSelector.Update();
      });
      if (actScript.InputGesture != null)
      {
        string inputGesture = actScript.InputGesture;
        char[] chArray = new char[1]{ ':' };
        foreach (string keyGesture in inputGesture.Split(chArray))
          ApplicationShortcut.Link((KeyGesture) ApplicationShortcut.FromString(keyGesture, keyGesture + "_cmd"), action, (FrameworkElement) actEditor);
      }
      scriptMenu.Click += (RoutedEventHandler) ((s, e) => action());
      return scriptMenu;
    }

    private void _addFromScript(string script, string localCopy, List<MenuItem> toAdd)
    {
      try
      {
        string dll;
        CompilerResults compilerResults = ScriptLoader.Compile(localCopy, out dll);
        if (compilerResults.Errors.Count != 0)
          throw new Exception(string.Join("\r\n", compilerResults.Errors.Cast<CompilerError>().ToList<CompilerError>().Select<CompilerError, string>((Func<CompilerError, string>) (p => p.ToString())).ToArray<string>()));
        ScriptLoader.LibrariesConfiguration["[" + (object) Path.GetFileName(script).GetHashCode() + "]"] = new Md5Hash().ComputeHash(File.ReadAllBytes(script)) + "," + new Md5Hash().ComputeHash(File.ReadAllBytes(dll));
        GrfPath.Delete(localCopy);
        GrfPath.Delete(script.ReplaceExtension(".dll"));
        Utilities.Debug.Ignore((System.Action) (() => File.Copy(dll, script.ReplaceExtension(".dll"))));
        this._addScriptFromAssembly(dll, toAdd);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _addScriptFromAssembly(string assemblyPath, List<MenuItem> toAdd)
    {
      object instance = Assembly.LoadFile(assemblyPath).CreateInstance("Scripts.Script");
      if (instance == null)
        throw new Exception("Couldn't instantiate the script object. Type not found?");
      if (!(instance is IActScript actScript))
        throw new Exception("Couldn't instantiate the script object. Type not found?");
      this._addActScript(actScript, toAdd);
    }

    private void _addActScript(IActScript actScript, List<MenuItem> toAdd) => this._menu.Dispatch((System.Action) (() => this._setupMenuItem(actScript, this._menu, this._actEditor, (UIElement) this._generateScriptMenu(this._actEditor, actScript), toAdd)));

    public static ImageSource GetImage(string image)
    {
      BitmapFrame resourceImage = ApplicationManager.GetResourceImage(image);
      if (resourceImage != null)
        return (ImageSource) WpfImaging.FixDPI((BitmapSource) resourceImage);
      string path = GrfPath.Combine(GrfEditorConfiguration.ProgramDataPath, "Scripts", image);
      if (!File.Exists(path))
        return (ImageSource) null;
      byte[] data = File.ReadAllBytes(path);
      return (ImageSource) new GrfImage(ref data).Cast<BitmapSource>();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || this._fsw == null)
        return;
      this._fsw.Dispose();
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }
  }
}
