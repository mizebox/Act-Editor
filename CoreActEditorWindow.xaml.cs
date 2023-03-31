// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.ActEditorWindow
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.Scripts;
using ActEditor.Core.WPF;
using ActEditor.Core.WPF.Dialogs;
using ActEditor.Core.WPF.EditorControls;
using ActEditor.Tools.GrfShellExplorer;
using ErrorManager;
using GRF;
using GRF.Core;
using GRF.Core.GroupedGrf;
using GRF.FileFormats;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using GRF.System;
using GRF.Threading;
using GrfToWpfBridge;
using GrfToWpfBridge.Application;
using GrfToWpfBridge.MultiGrf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.Paths;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WpfBugFix;
using Utilities;
using Utilities.CommandLine;
using Utilities.Commands;
using Utilities.Extension;
using Utilities.Services;

namespace ActEditor.Core
{
  public partial class ActEditorWindow : TkWindow, IPreviewEditor, IComponentConnector
  {
    private static Brush _uiGridBackground;
    private static Brush _uiGridBackgroundLight;
    private readonly MultiGrfReader _metaGrf = new MultiGrfReader();
    private readonly MetaGrfResourcesViewer _metaGrfViewer = new MetaGrfResourcesViewer();
    private readonly WpfRecentFiles _recentFiles;
    private readonly ScriptLoader _scriptLoader;
    private readonly SelectionEngine _selectionEngine = new SelectionEngine();
    private readonly SpriteManager _spriteManager = new SpriteManager();
    private bool _isNew;
    private List<ReferenceControl> _references = new List<ReferenceControl>();
    private Act _act;
    internal Grid _gridPrimary;
    internal System.Windows.Controls.Menu _mainMenu;
    internal TkMenuItem _miNew;
    internal TkMenuItem _miNewHeadgearMale;
    internal TkMenuItem _miNewHeadgear;
    internal TkMenuItem _miNewMonster;
    internal TkMenuItem _miNewHomunculus;
    internal TkMenuItem _miNewWeapon;
    internal TkMenuItem _miNewNpc;
    internal TkMenuItem _miOpen;
    internal TkMenuItem _miOpenFromGrf;
    internal System.Windows.Controls.MenuItem _miOpenRecent;
    internal TkMenuItem _miCloseCurrent;
    internal TkMenuItem _miSelectAct;
    internal TkMenuItem _miSave;
    internal TkMenuItem _miSaveAs;
    internal TkMenuItem _miSettings;
    internal TkMenuItem _miAbout;
    internal TkMenuItem _miClose;
    internal System.Windows.Controls.MenuItem _miEdit;
    internal TkMenuItem _miCopy;
    internal TkMenuItem _miPaste;
    internal TkMenuItem _miCut;
    internal TkMenuItem _miViewSameAction;
    internal TkMenuItem _miViewPrevAnim;
    internal TkMenuItem _miViewFrameMirror;
    internal System.Windows.Controls.MenuItem _miAnchors;
    internal System.Windows.Controls.MenuItem _miShowAnchors;
    internal System.Windows.Controls.MenuItem _miReverseAnchors;
    internal System.Windows.Controls.MenuItem _miAnchor;
    internal DockPanel _dpUndoRedo;
    internal ToggleMemoryButton _tmbUndo;
    internal ToggleMemoryButton _tmbRedo;
    internal Grid _preloader;
    internal Grid _framePreviewGrid;
    internal FramePreview _framePreview;
    internal ActIndexSelector _frameSelector;
    internal ScrollViewer _sv;
    internal StackPanel _stackPanelReferences;
    internal LayerEditor _layerEditor;
    internal Grid _gridSpriteSelected;
    internal SpriteSelector _spriteSelector;
    private bool _contentLoaded;

    public ActEditorWindow()
      : base("Act Editor", "app.ico", resizeMode: ResizeMode.CanResize)
    {
      Spr.EnableImageSizeCheck = false;
      this._parseCommandLineArguments(false);
      this.DataContext = (object) this;
      SplashWindow diag = new SplashWindow();
      diag.Display = "Initializing components...";
      diag.Show();
      this.Loaded += (RoutedEventHandler) delegate
      {
        diag.Terminate(1000);
      };
      this.Title = "Act Editor";
      this.SizeToContent = SizeToContent.WidthAndHeight;
      this.InitializeComponent();
      diag.Display = "Loading scripting engine...";
      this._scriptLoader = new ScriptLoader();
      this.Loaded += (RoutedEventHandler) delegate
      {
        this.MinWidth = this.ActualWidth + SystemParameters.VerticalScrollBarWidth;
        this.MinHeight = this.ActualHeight;
        this.SizeToContent = SizeToContent.Manual;
        this.Top = (SystemParameters.FullPrimaryScreenHeight - this.ActualHeight) / 2.0;
        this.Left = (SystemParameters.FullPrimaryScreenWidth - this.ActualWidth) / 2.0;
        this.MinHeight += 50.0;
      };
      this.ShowInTaskbar = true;
      diag.Display = "Setting components...";
      this._frameSelector.Init(this);
      this._framePreview.Init((IPreviewEditor) this);
      this._layerEditor.Init(this);
      this._selectionEngine.Init((IPreviewEditor) this);
      this._spriteSelector.Init((IPreviewEditor) this);
      this._spriteManager.Init(this);
      this._initEvents();
      this._recentFiles = new WpfRecentFiles(GrfEditorConfiguration.ConfigAsker, 6, this._miOpenRecent, nameof (Act));
      this._recentFiles.FileClicked += new RecentFilesManager.RFMFileClickedEventHandler(this._recentFiles_FileClicked);
      diag.Display = "Loading Act Editor's scripts...";
      this._loadMenu();
      this.DragEnter += new System.Windows.DragEventHandler(this._actEditorWindow_DragEnter);
      this.Drop += new System.Windows.DragEventHandler(this._actEditorWindow_Drop);
      this.Loaded += (RoutedEventHandler) delegate
      {
        diag.Display = "Loading custom scripts...";
        GrfThread.Start((System.Action) (() =>
        {
          ScriptLoader.VerifyExampleScriptsInstalled();
          this._scriptLoader.AddScriptsToMenu(this, this._mainMenu, this._dpUndoRedo);
        }));
        try
        {
          if (this._parseCommandLineArguments() || this._recentFiles.Files.Count <= 0 || !GrfEditorConfiguration.ReopenLatestFile)
            return;
          if (!this._recentFiles.Files[0].IsExtension(".act") || !File.Exists(new TkPath(this._recentFiles.Files[0]).FilePath))
            return;
          this._open(this._recentFiles.Files[0]);
        }
        catch (Exception ex)
        {
          ErrorHandler.HandleException(ex);
        }
      };
      ApplicationShortcut.Link(ApplicationShortcut.Undo, (System.Action) (() =>
      {
        if (this.Act == null)
          return;
        this.Act.Commands.Undo();
      }), (FrameworkElement) this);
      ApplicationShortcut.Link(ApplicationShortcut.Redo, (System.Action) (() =>
      {
        if (this.Act == null)
          return;
        this.Act.Commands.Redo();
      }), (FrameworkElement) this);
      this._miReverseAnchors.Loaded += (RoutedEventHandler) ((e, s) => this._miReverseAnchors.IsChecked = GrfEditorConfiguration.ReverseAnchor);
      this._miReverseAnchors.Checked += (RoutedEventHandler) ((e, s) =>
      {
        GrfEditorConfiguration.ReverseAnchor = true;
        if (this.Act != null)
        {
          foreach (ReferenceControl reference in this._references)
          {
            if (reference.Act != null && reference.Act.Name == "Body")
            {
              this.Act.AnchoredTo = reference.Act;
              reference.Act.AnchoredTo = (Act) null;
              break;
            }
          }
        }
        this._framePreview.Update();
      });
      this._miReverseAnchors.Unchecked += (RoutedEventHandler) ((e, s) =>
      {
        GrfEditorConfiguration.ReverseAnchor = false;
        if (this.Act != null)
        {
          this.Act.AnchoredTo = (Act) null;
          foreach (ReferenceControl reference in this._references)
          {
            if (reference.Act != null && reference.Act.Name == "Body")
            {
              reference.RefreshSelection();
              break;
            }
          }
        }
        this._framePreview.Update();
      });
      this._metaGrfViewer.SaveResourceMethod = (Action<string>) (resources =>
      {
        GrfEditorConfiguration.Resources = Methods.StringToList(resources);
        this._metaGrfViewer.LoadResourcesInfo();
        this._metaGrf.Update(this._metaGrfViewer.Paths);
      });
      this._metaGrfViewer.LoadResourceMethod = (Func<List<string>>) (() => GrfEditorConfiguration.Resources);
      this._metaGrfViewer.LoadResourcesInfo();
      GrfThread.Start((System.Action) (() =>
      {
        try
        {
          this._metaGrf.Update(this._metaGrfViewer.Paths);
        }
        catch
        {
        }
      }), "ActEditor - MetaGrf loader");
      TemporaryFilesManager.UniquePattern("new_{0:0000}");
      EncodingService.SetDisplayEncoding(GrfEditorConfiguration.EncodingCodepage);
    }

    public SelectionEngine SelectionEngine => this._selectionEngine;

    public SpriteManager SpriteManager => this._spriteManager;

    public ScriptLoader ScriptLoader => this._scriptLoader;

    public MultiGrfReader MetaGrf => this._metaGrf;

    public int SelectedAction => this._frameSelector.SelectedAction;

    public int SelectedFrame
    {
      get => this._frameSelector.SelectedFrame;
      set
      {
        value = value < 0 ? 0 : value;
        this._frameSelector.SelectedFrame = value;
      }
    }

    public GRF.FileFormats.ActFormat.Frame Frame => this.Act[this._frameSelector.SelectedAction, this._frameSelector.SelectedFrame];

    public List<ReferenceControl> References
    {
      get => this._references;
      set => this._references = value;
    }

    public IActIndexSelector FrameSelector => (IActIndexSelector) this._frameSelector;

    public UIElement Element => (UIElement) this;

    public Act Act
    {
      get => this._act;
      set
      {
        this._act = value;
        if (this._act == null)
          return;
        this._act.AllActions((Action<GRF.FileFormats.ActFormat.Action>) (a =>
        {
          if (a.Frames.Count != 0)
            return;
          a.Frames.Add(new GRF.FileFormats.ActFormat.Frame());
        }));
      }
    }

    public Spr Sprite => this.Act != null ? this.Act.Sprite : (Spr) null;

    public static Brush UIGridBackground
    {
      get
      {
        if (ActEditorWindow._uiGridBackground == null)
        {
          SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 189, (byte) 189, (byte) 189));
          solidColorBrush.Freeze();
          ActEditorWindow._uiGridBackground = (Brush) solidColorBrush;
        }
        return ActEditorWindow._uiGridBackground;
      }
    }

    public static Brush UIGridBackgroundLight
    {
      get
      {
        if (ActEditorWindow._uiGridBackgroundLight == null)
        {
          SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 235, (byte) 235, (byte) 235));
          solidColorBrush.Freeze();
          ActEditorWindow._uiGridBackgroundLight = (Brush) solidColorBrush;
        }
        return ActEditorWindow._uiGridBackgroundLight;
      }
    }

    public Func<bool> IsActOpened => new Func<bool>(this._isActOpened);

    private void _loadMenu()
    {
      this._scriptLoader.AddScriptsToMenu((IActScript) new SpriteExport(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditSelectAll()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditDeselectAll()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new InvertSelection()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[1]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new BringToFront()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new BringToBack()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[1]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditSound(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[1]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditBackground(this), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[1]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditClearPalette(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditPalette(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditPaletteAdvanced(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ImportPaletteFrom(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new EditAnchor()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[2]).Items.Add((object) new Separator());
      ItemCollection items = ((ItemsControl) this._mainMenu.Items[2]).Items;
      TkMenuItem tkMenuItem = new TkMenuItem();
      tkMenuItem.Header = (object) "Set anchors";
      tkMenuItem.IconPath = "forward.png";
      TkMenuItem newItem = tkMenuItem;
      items.Add((object) newItem);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ImportAnchor(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ImportDefaultMaleAnchor(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ImportDefaultFemaleAnchor(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionCopy(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionPaste(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[3]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionDelete(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionInsertAt(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionSwitchSelected(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionCopyAt(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[3]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionAdvanced(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[3]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionCopyMirror(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[3]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionLayerMove(ActionLayerMove.MoveDirection.Down, (IPreviewEditor) this), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ActionLayerMove(ActionLayerMove.MoveDirection.Up, (IPreviewEditor) this), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameDelete(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameInsertAt(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameSwitchSelected(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameCopyAt(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[4]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameAdvanced(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[4]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameDuplicate(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameCopyBrB(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameCopyBBl(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameCopyBBr(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameCopyBlB(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ReverseAnimation(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new FrameCopyHead(this), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[5]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new FadeAnimation(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new AnimationReceivingHit(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[5]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new InterpolationAnimation(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new LayerInterpolationAnimation(), this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[5]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new InterpolationAnimationAdv(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ScriptRunnerMenu()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[6]).Items.Add((object) new Separator());
      this._scriptLoader.AddScriptsToMenu((IActScript) new OpenScriptsFolder(), this, this._mainMenu, (FrameworkElement) null);
      this._scriptLoader.AddScriptsToMenu((IActScript) new ReloadScripts()
      {
        ActEditor = this
      }, this, this._mainMenu, (FrameworkElement) null);
      ((ItemsControl) this._mainMenu.Items[6]).Items.Add((object) new Separator());
      Binder.Bind<TkMenuItem, bool>(this._miViewSameAction, (Expression<Func<bool>>) (() => GrfEditorConfiguration.KeepPreviewSelectionFromActionChange));
    }

    private bool _parseCommandLineArguments(bool init = true)
    {
      try
      {
        foreach (GenericCLOption option in CommandLineParser.GetOptions(Environment.CommandLine, false))
        {
          if (init)
          {
            if (option.CommandName == "-REM" || option.CommandName == "REM")
              return true;
            if (option.Args.Count > 0 && option.Args.All<string>((Func<string, bool>) (p => p.GetExtension() == ".act")))
            {
              this._open(option.Args[0]);
              return true;
            }
          }
          else if (option.Args.Count > 0 && option.Args.All<string>((Func<string, bool>) (p => p.GetExtension() == ".spr")))
            return true;
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      return false;
    }

    public event ActEditorWindow.ActEditorEventDelegate ReferencesChanged;

    public event ActEditorWindow.ActEditorEventDelegate ActLoaded;

    public Grid GridPrimary => this._gridPrimary;

    public LayerEditor LayerEditor => this._layerEditor;

    public SpriteSelector SpriteSelector => this._spriteSelector;

    public FramePreview FramePreview => this._framePreview;

    public void OnActLoaded()
    {
      ActEditorWindow.ActEditorEventDelegate actLoaded = this.ActLoaded;
      if (actLoaded == null)
        return;
      actLoaded((object) this);
    }

    public void OnReferencesChanged()
    {
      ActEditorWindow.ActEditorEventDelegate referencesChanged = this.ReferencesChanged;
      if (referencesChanged == null)
        return;
      referencesChanged((object) this);
    }

    private void _actEditorWindow_Drop(object sender, System.Windows.DragEventArgs e)
    {
      try
      {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true) || !(e.Data.GetData(System.Windows.DataFormats.FileDrop, true) is string[] data) || data.Length <= 0 || !((IEnumerable<string>) data).Any<string>((Func<string, bool>) (p => p.IsExtension(".act"))))
          return;
        this._isNew = false;
        this._open(((IEnumerable<string>) data).First<string>((Func<string, bool>) (p => p.IsExtension(".act"))));
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _actEditorWindow_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true) || !(e.Data.GetData(System.Windows.DataFormats.FileDrop, true) is string[] data) || data.Length <= 0 || !((IEnumerable<string>) data).Any<string>((Func<string, bool>) (p => p.IsExtension("*.act"))))
        return;
      e.Effects = System.Windows.DragDropEffects.All;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      try
      {
        if (!this._closeAct())
        {
          e.Cancel = true;
          return;
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      base.OnClosing(e);
      ApplicationManager.Shutdown();
    }

    private void _recentFiles_FileClicked(string file)
    {
      this._isNew = false;
      this._open(file);
    }

    protected override void GRFEditorWindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
    }

    private void _open(string file) => this._open(new TkPath(file));

    private void _openFromFile(string file)
    {
      try
      {
        if (!file.IsExtension(".act"))
        {
          this._recentFiles.RemoveRecentFile(file);
          ErrorHandler.HandleException("Invalid file extension; only .act files are allowed.");
        }
        else if (!File.Exists(file))
        {
          this._recentFiles.RemoveRecentFile(file);
          ErrorHandler.HandleException("File not found while trying to open the Act.\r\n\r\n" + file);
        }
        else
        {
          this._recentFiles.AddRecentFile(file);
          if (!File.Exists(file.ReplaceExtension(".spr")))
          {
            this._recentFiles.RemoveRecentFile(file);
            ErrorHandler.HandleException("File not found : " + file.ReplaceExtension(".spr"));
          }
          else
          {
            this.Act = new Act((MultiType) file, (MultiType) file.ReplaceExtension(".spr"));
            this.Act.LoadedPath = file;
            this._addEvents();
            this.OnActLoaded();
          }
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _open(TkPath file)
    {
      try
      {
        if (!this._closeAct())
          return;
        if (file.FilePath.IsExtension(".act") || string.IsNullOrEmpty(file.RelativePath))
          this._openFromFile(file.FilePath);
        else if (!File.Exists(file.FilePath))
        {
          this._recentFiles.RemoveRecentFile(file.GetFullPath());
        }
        else
        {
          this._recentFiles.AddRecentFile(file.GetFullPath());
          TkPath tkPath = new TkPath(file);
          tkPath.RelativePath = tkPath.RelativePath.ReplaceExtension(".spr");
          byte[] actData = (byte[]) null;
          byte[] sprData = (byte[]) null;
          using (GrfHolder grfHolder = new GrfHolder(file.FilePath))
          {
            if (grfHolder.FileTable.ContainsFile(file.RelativePath))
              actData = grfHolder.FileTable[file.RelativePath].GetDecompressedData();
            if (grfHolder.FileTable.ContainsFile(file.RelativePath.ReplaceExtension(".spr")))
              sprData = grfHolder.FileTable[file.RelativePath.ReplaceExtension(".spr")].GetDecompressedData();
          }
          if (actData == null)
            ErrorHandler.HandleException("File not found : " + (string) file);
          else if (sprData == null)
          {
            ErrorHandler.HandleException("File not found : " + (string) tkPath);
          }
          else
          {
            this.Act = new Act((MultiType) actData, (MultiType) sprData);
            this.Act.LoadedPath = file.GetFullPath();
            this._addEvents();
            this.OnActLoaded();
          }
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _addEvents()
    {
      this.Dispatch<ActEditorWindow, string>((Func<ActEditorWindow, string>) (p =>
      {
        ActEditorWindow actEditorWindow = p;
        string str1 = Methods.CutFileName(this.Act.LoadedPath);
        string str2 = this._isNew ? " *" : "";
        string str3;
        string str4 = str3 = "Act Editor - " + str1 + str2;
        actEditorWindow.Title = str3;
        return str4;
      }));
      this.Act.Commands.CommandIndexChanged += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) delegate
      {
        if (!this.Act.Commands.IsModified && !this._isNew)
        {
          this.Dispatch<ActEditorWindow, string>((Func<ActEditorWindow, string>) (p => p.Title = "Act Editor - " + Methods.CutFileName(this.Act.LoadedPath)));
        }
        else
        {
          if (!this.Act.Commands.IsModified && !this._isNew)
            return;
          this.Dispatch<ActEditorWindow, string>((Func<ActEditorWindow, string>) (p => p.Title = "Act Editor - " + Methods.CutFileName(this.Act.LoadedPath) + " *"));
        }
      };
      this._tmbUndo.SetUndo<IActCommand>((AbstractCommand<IActCommand>) this.Act.Commands);
      this._tmbRedo.SetRedo<IActCommand>((AbstractCommand<IActCommand>) this.Act.Commands);
    }

    private void _initEvents()
    {
      this._references.Add(new ReferenceControl(this, "ref_body_m", "ref_body_f", "Body", false));
      this._references.Add(new ReferenceControl(this, "ref_head_m", "ref_head_f", "Head", false));
      this._references.Add(new ReferenceControl(this, "ref_body_m", "ref_body_f", "Other", false));
      this._references.Add(new ReferenceControl(this, "ref_body_f", "ref_body_f", "Neighbor", true));
      this._stackPanelReferences.Children.Add((UIElement) this._references[0]);
      this._stackPanelReferences.Children.Add((UIElement) this._references[1]);
      this._stackPanelReferences.Children.Add((UIElement) this._references[2]);
      this._stackPanelReferences.Children.Add((UIElement) this._references[3]);
      this._references.ForEach((Action<ReferenceControl>) (p => p.Init()));
    }

    private void _miOpen_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string file = PathRequest.OpenFileExtract("filter", FileFormat.MergeFilters(GRF.FileFormats.Format.Act));
        if (file == null)
          return;
        this._isNew = false;
        this._open(file);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
    }

    private void _restoreFocus() => this.Focus();

    private void _miSettings_Click(object sender, RoutedEventArgs e) => WindowProvider.Show((TkWindow) new ActEditorSettings(this, this._metaGrfViewer), (System.Windows.Controls.Control) this._miSettings, (Window) this);

    private void _miClose_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!this._closeAct())
          return;
        this.Close();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private bool _closeAct()
    {
      if (this.Act != null && this.Act.Commands.IsModified)
      {
        MessageBoxResult messageBoxResult = WindowProvider.ShowDialog("The project has been modified, would you like to save it first?", "Modified Act", MessageBoxButton.YesNoCancel);
        if (messageBoxResult == MessageBoxResult.Yes && !this.SaveAs() || messageBoxResult == MessageBoxResult.Cancel)
          return false;
      }
      if (this.Act != null)
      {
        this.Act.Commands.ClearCommands();
        this.Act = (Act) null;
        this.Dispatch<ActEditorWindow, string>((Func<ActEditorWindow, string>) (p => p.Title = "Act Editor"));
        this._references.ForEach((Action<ReferenceControl>) (p => p.Reset()));
        this._frameSelector.Reset();
        this._spriteSelector.Reset();
        this._layerEditor.Reset();
        this._framePreview.Reset();
      }
      return true;
    }

    private void _miNew_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!this._closeAct())
          return;
        this.Act = new Act(new Spr());
        this.Act.AddAction();
        string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath("new_{0:0000}");
        this.Act.LoadedPath = temporaryFilePath + ".act";
        this.Act.Sprite.Converter.Save(this.Act.Sprite, temporaryFilePath + ".spr");
        this.Act.Save(temporaryFilePath + ".act");
        this._isNew = true;
        this._open(temporaryFilePath + ".act");
        this._recentFiles.RemoveRecentFile(temporaryFilePath + ".act");
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
    }

    public bool Save()
    {
      try
      {
        if (this.Act != null)
        {
          if (this._isNew)
            return this.SaveAs();
          TkPath tkPath = new TkPath(this.Act.LoadedPath);
          if (!string.IsNullOrEmpty(tkPath.RelativePath))
          {
            if (Methods.IsFileLocked(tkPath.FilePath))
            {
              ErrorHandler.HandleException("The file " + tkPath.FilePath + " is locked by another process. Try closing other GRF applicactions or use the 'Save as...' option.");
              return false;
            }
            using (GrfHolder grfHolder = new GrfHolder(tkPath.FilePath))
            {
              string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath("to_grf_{0:0000}");
              this.Act.Sprite.Save(temporaryFilePath + ".spr");
              this.Act.Save(temporaryFilePath + ".act");
              grfHolder.Commands.AddFileAbsolute(tkPath.RelativePath.ReplaceExtension(".act"), File.ReadAllBytes(temporaryFilePath + ".act"));
              grfHolder.Commands.AddFileAbsolute(tkPath.RelativePath.ReplaceExtension(".spr"), File.ReadAllBytes(temporaryFilePath + ".spr"));
              grfHolder.QuickSave();
              if (!grfHolder.CancelReload)
                this.Act.Commands.SaveCommandIndex();
            }
          }
          else
          {
            this.Act.Sprite.Save(this.Act.LoadedPath.ReplaceExtension(".spr"));
            this.Act.Save();
            this.Act.Commands.SaveCommandIndex();
          }
          this.Dispatch<ActEditorWindow, string>((Func<ActEditorWindow, string>) (p => p.Title = "Act Editor - " + Methods.CutFileName(this.Act.LoadedPath)));
          this._isNew = false;
          return true;
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
      return false;
    }

    private void _miSave_Click(object sender, RoutedEventArgs e) => this.Save();

    public bool SaveAs()
    {
      try
      {
        if (this.Act != null)
        {
          string path = GrfEditorConfiguration.AppLastPath;
          if (Path.GetFileNameWithoutExtension(path) != Path.GetFileNameWithoutExtension(this.Act.LoadedPath))
            path = this.Act.LoadedPath;
          string file = PathRequest.SaveFileEditor("fileName", path, "filter", "Act and Spr files|*.act;*spr|" + FileFormat.MergeFilters(GRF.FileFormats.Format.Pal | GRF.FileFormats.Format.Spr | GRF.FileFormats.Format.Image | GRF.FileFormats.Format.Gif | GRF.FileFormats.Format.Act));
          if (file != null)
          {
            SaveFileDialog latestSaveFileDialog = TkPathRequest.LatestSaveFileDialog;
            if (latestSaveFileDialog.FilterIndex == 1)
            {
              if (file.IsExtension(".act", ".spr"))
              {
                this.Act.SaveWithSprite(file.ReplaceExtension(".act"));
                this._recentFiles.AddRecentFile(file);
                this.Act.LoadedPath = file.ReplaceExtension(".act");
                this.Dispatch<ActEditorWindow, string>((Func<ActEditorWindow, string>) (p => p.Title = "Act Editor - " + Methods.CutFileName(this.Act.LoadedPath)));
                this.Act.Commands.SaveCommandIndex();
                this._isNew = false;
                return true;
              }
            }
            if (latestSaveFileDialog.FilterIndex == 2)
            {
              if (file.IsExtension(".act"))
                goto label_11;
            }
            if (!file.IsExtension(".act"))
            {
              if (latestSaveFileDialog.FilterIndex == 3)
              {
                if (file.IsExtension(".pal"))
                  goto label_15;
              }
              if (!file.IsExtension(".pal"))
              {
                if (latestSaveFileDialog.FilterIndex == 4)
                {
                  if (file.IsExtension(".spr"))
                    goto label_19;
                }
                if (!file.IsExtension(".spr"))
                {
                  if (latestSaveFileDialog.FilterIndex == 5)
                  {
                    if (file.IsExtension(".gif"))
                      goto label_23;
                  }
                  if (!file.IsExtension(".gif"))
                  {
                    if (!file.IsExtension(".bmp", ".png", ".jpg", ".tga"))
                    {
                      ErrorHandler.HandleException("Invalid file extension.");
                      return false;
                    }
                    ImageSource image = ActImaging.Imaging.GenerateImage(this.Act, this.SelectedAction, this.SelectedFrame);
                    PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                    pngBitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource) ActImaging.Imaging.ForceRender(image, BitmapScalingMode.NearestNeighbor)));
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                      pngBitmapEncoder.Save((Stream) memoryStream);
                      byte[] data = new byte[memoryStream.Length];
                      memoryStream.Seek(0L, SeekOrigin.Begin);
                      memoryStream.Read(data, 0, data.Length);
                      new GrfImage(ref data).Save(file);
                      goto label_42;
                    }
                  }
label_23:
                  try
                  {
                    for (int index = 0; index < this.Act.Sprite.NumberOfIndexed8Images; ++index)
                      this.Act.Sprite.Images[index].Palette[3] = (byte) 0;
                    GifSavingDialog dialog = new GifSavingDialog(this.Act, this.SelectedAction);
                    dialog.Owner = WpfUtilities.TopWindow;
                    if (!GrfEditorConfiguration.ActEditorGifHideDialog)
                    {
                      bool? nullable = dialog.ShowDialog();
                      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                        goto label_42;
                    }
                    ProgressWindow prog = new ProgressWindow("Saving as gif...", "app.ico");
                    prog.EnableClosing = true;
                    prog.Loaded += (RoutedEventHandler) delegate
                    {
                      prog.Start(new GrfThread((System.Action) (() =>
                      {
                        try
                        {
                          List<Act> actList3 = new List<Act>();
                          List<Act> actList4 = new List<Act>();
                          foreach (ReferenceControl referenceControl in this._references.Where<ReferenceControl>((Func<ReferenceControl, bool>) (reference => reference.ShowReference)))
                          {
                            if (referenceControl.Mode == ZMode.Back)
                              actList3.Insert(0, referenceControl.Act);
                            else
                              actList4.Add(referenceControl.Act);
                          }
                          ActImaging.Imaging.SaveAsGif(file, Act.MergeAct(actList3.ToArray(), this.Act, actList4.ToArray()), this.SelectedAction, (IProgress) prog, dialog.Dispatch<string[]>((Func<string[]>) (() => dialog.Extra)));
                        }
                        catch (Exception ex)
                        {
                          ErrorHandler.HandleException(ex);
                        }
                      }), (IProgress) prog, 200, exitWhenFunctionIsOver: true, isSTA: true));
                    };
                    prog.ShowDialog();
                    goto label_42;
                  }
                  finally
                  {
                    for (int index = 0; index < this.Act.Sprite.NumberOfIndexed8Images; ++index)
                      this.Act.Sprite.Images[index].Palette[3] = byte.MaxValue;
                  }
                }
label_19:
                this.Act.Sprite.Converter.Save(this.Act.Sprite, file.ReplaceExtension(".spr"));
                goto label_42;
              }
label_15:
              File.WriteAllBytes(file, this.Act.Sprite.Palette.BytePalette);
              goto label_42;
            }
label_11:
            this.Act.Save(file.ReplaceExtension(".act"));
            this._recentFiles.AddRecentFile(file);
          }
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
label_42:
      return false;
    }

    private void _miSaveAs_Click(object sender, RoutedEventArgs e) => this.SaveAs();

    private void _miCloseCurrent_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        this._closeAct();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
    }

    private void _miAbout_Click(object sender, RoutedEventArgs e)
    {
      AboutDialog aboutDialog = new AboutDialog(GrfEditorConfiguration.PublicVersion, GrfEditorConfiguration.RealVersion, GrfEditorConfiguration.Author, GrfEditorConfiguration.ProgramName);
      aboutDialog.Owner = WpfUtilities.TopWindow;
      ((System.Windows.Controls.TextBox) aboutDialog.FindName("_textBlock")).Text += "\r\n\r\nCredits : Nebraskka (suggestions and feedback)";
      aboutDialog.ShowDialog();
      this._restoreFocus();
    }

    private void _gridSpriteSelected_SizeChanged(object sender, SizeChangedEventArgs e) => this._spriteSelector.Height = this._gridSpriteSelected.ActualHeight;

    private void _miOpenFromGrf_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string str = PathRequest.OpenGrfFile("filter", FileFormat.MergeFilters(GRF.FileFormats.Format.AllContainers, GRF.FileFormats.Format.Grf, GRF.FileFormats.Format.Gpf, GRF.FileFormats.Format.Thor));
        if (str == null)
          return;
        GrfExplorer grfExplorer = new GrfExplorer(str, SelectMode.Act);
        grfExplorer.Owner = WpfUtilities.TopWindow;
        bool? nullable = grfExplorer.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
          return;
        string selectedItem = grfExplorer.SelectedItem;
        if (selectedItem == null)
          return;
        if (!selectedItem.IsExtension(".act"))
          throw new Exception("Only ACT files can be selected.");
        this._isNew = false;
        this._open(new TkPath(str, selectedItem));
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
    }

    private void _miCopy_Click(object sender, RoutedEventArgs e) => this._framePreview.Copy();

    private void _miPaste_Click(object sender, RoutedEventArgs e) => this._framePreview.Paste();

    private void _miCut_Click(object sender, RoutedEventArgs e) => this._framePreview.Cut();

    private void _miAnchor_Click(object sender, RoutedEventArgs e)
    {
      foreach (System.Windows.Controls.MenuItem menuItem in (IEnumerable) this._miAnchor.Items)
      {
        if (menuItem != sender)
        {
          menuItem.IsChecked = false;
        }
        else
        {
          menuItem.Checked -= new RoutedEventHandler(this._miAnchor_Click);
          menuItem.IsChecked = true;
          menuItem.Checked += new RoutedEventHandler(this._miAnchor_Click);
        }
      }
      if (this._framePreview == null)
        return;
      this._framePreview.AnchorIndex = int.Parse(((FrameworkElement) sender).Tag.ToString());
      this._framePreview.Update();
    }

    private void _miSelectAct_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (this.Act != null)
          OpeningService.FilesOrFolders(this.Act.LoadedPath);
        else
          ErrorHandler.HandleException("No act loaded.");
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private bool _isActOpened() => this.Act != null;

    private void _miShowAnchors_Loaded(object sender, RoutedEventArgs e) => this._miShowAnchors.IsChecked = GrfEditorConfiguration.ShowAnchors;

    private void _miShowAnchors_Checked(object sender, RoutedEventArgs e)
    {
      GrfEditorConfiguration.ShowAnchors = true;
      this._framePreview.Update();
    }

    private void _miShowAnchors_Unchecked(object sender, RoutedEventArgs e)
    {
      GrfEditorConfiguration.ShowAnchors = false;
      this._framePreview.Update();
    }

    private void _miNewHeadgear_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!this._closeAct())
          return;
        this.Act = new Act((MultiType) ApplicationManager.GetResource("ref_head_f.act"), new Spr());
        this.Act.AllFrames((Action<GRF.FileFormats.ActFormat.Frame>) (p => p.Layers.Clear()));
        string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath("new_{0:0000}");
        this.Act.LoadedPath = temporaryFilePath + ".act";
        this.Act.Sprite.Save(temporaryFilePath + ".spr");
        this.Act.Save(temporaryFilePath + ".act");
        this._isNew = true;
        this._open(temporaryFilePath + ".act");
        this._recentFiles.RemoveRecentFile(temporaryFilePath + ".act");
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
    }

    private void _new(string name)
    {
      try
      {
        if (!this._closeAct())
          return;
        this.Act = new Act((MultiType) ApplicationManager.GetResource(name), new Spr());
        string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath("new_{0:0000}");
        this.Act.LoadedPath = temporaryFilePath + ".act";
        this.Act.Sprite.Save(temporaryFilePath + ".spr");
        this.Act.Save(temporaryFilePath + ".act");
        this._isNew = true;
        this._open(temporaryFilePath + ".act");
        this._recentFiles.RemoveRecentFile(temporaryFilePath + ".act");
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
    }

    private void _miNewNpc_Click(object sender, RoutedEventArgs e) => this._new("NPC.act");

    private void _miNewWeapon_Click(object sender, RoutedEventArgs e) => this._new("weapon.act");

    private void _miNewMonster_Click(object sender, RoutedEventArgs e) => this._new("monster.act");

    private void _miNewHomunculus_Click(object sender, RoutedEventArgs e) => this._new("homunculus.act");

    private void _miNewHeadgearMale_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!this._closeAct())
          return;
        this.Act = new Act((MultiType) ApplicationManager.GetResource("ref_head_m.act"), new Spr());
        this.Act.AllFrames((Action<GRF.FileFormats.ActFormat.Frame>) (p => p.Layers.Clear()));
        string temporaryFilePath = TemporaryFilesManager.GetTemporaryFilePath("new_{0:0000}");
        this.Act.LoadedPath = temporaryFilePath + ".act";
        this.Act.Sprite.Save(temporaryFilePath + ".spr");
        this.Act.Save(temporaryFilePath + ".act");
        this._isNew = true;
        this._open(temporaryFilePath + ".act");
        this._recentFiles.RemoveRecentFile(temporaryFilePath + ".act");
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._restoreFocus();
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/acteditorwindow.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(System.Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._gridPrimary = (Grid) target;
          break;
        case 2:
          this._mainMenu = (System.Windows.Controls.Menu) target;
          break;
        case 3:
          this._miNew = (TkMenuItem) target;
          this._miNew.Click += new RoutedEventHandler(this._miNew_Click);
          break;
        case 4:
          this._miNewHeadgearMale = (TkMenuItem) target;
          this._miNewHeadgearMale.Click += new RoutedEventHandler(this._miNewHeadgearMale_Click);
          break;
        case 5:
          this._miNewHeadgear = (TkMenuItem) target;
          this._miNewHeadgear.Click += new RoutedEventHandler(this._miNewHeadgear_Click);
          break;
        case 6:
          this._miNewMonster = (TkMenuItem) target;
          this._miNewMonster.Click += new RoutedEventHandler(this._miNewMonster_Click);
          break;
        case 7:
          this._miNewHomunculus = (TkMenuItem) target;
          this._miNewHomunculus.Click += new RoutedEventHandler(this._miNewHomunculus_Click);
          break;
        case 8:
          this._miNewWeapon = (TkMenuItem) target;
          this._miNewWeapon.Click += new RoutedEventHandler(this._miNewWeapon_Click);
          break;
        case 9:
          this._miNewNpc = (TkMenuItem) target;
          this._miNewNpc.Click += new RoutedEventHandler(this._miNewNpc_Click);
          break;
        case 10:
          this._miOpen = (TkMenuItem) target;
          this._miOpen.Click += new RoutedEventHandler(this._miOpen_Click);
          break;
        case 11:
          this._miOpenFromGrf = (TkMenuItem) target;
          this._miOpenFromGrf.Click += new RoutedEventHandler(this._miOpenFromGrf_Click);
          break;
        case 12:
          this._miOpenRecent = (System.Windows.Controls.MenuItem) target;
          break;
        case 13:
          this._miCloseCurrent = (TkMenuItem) target;
          this._miCloseCurrent.Click += new RoutedEventHandler(this._miCloseCurrent_Click);
          break;
        case 14:
          this._miSelectAct = (TkMenuItem) target;
          this._miSelectAct.Click += new RoutedEventHandler(this._miSelectAct_Click);
          break;
        case 15:
          this._miSave = (TkMenuItem) target;
          this._miSave.Click += new RoutedEventHandler(this._miSave_Click);
          break;
        case 16:
          this._miSaveAs = (TkMenuItem) target;
          this._miSaveAs.Click += new RoutedEventHandler(this._miSaveAs_Click);
          break;
        case 17:
          this._miSettings = (TkMenuItem) target;
          this._miSettings.Click += new RoutedEventHandler(this._miSettings_Click);
          break;
        case 18:
          this._miAbout = (TkMenuItem) target;
          this._miAbout.Click += new RoutedEventHandler(this._miAbout_Click);
          break;
        case 19:
          this._miClose = (TkMenuItem) target;
          this._miClose.Click += new RoutedEventHandler(this._miClose_Click);
          break;
        case 20:
          this._miEdit = (System.Windows.Controls.MenuItem) target;
          break;
        case 21:
          this._miCopy = (TkMenuItem) target;
          this._miCopy.Click += new RoutedEventHandler(this._miCopy_Click);
          break;
        case 22:
          this._miPaste = (TkMenuItem) target;
          this._miPaste.Click += new RoutedEventHandler(this._miPaste_Click);
          break;
        case 23:
          this._miCut = (TkMenuItem) target;
          this._miCut.Click += new RoutedEventHandler(this._miCut_Click);
          break;
        case 24:
          this._miViewSameAction = (TkMenuItem) target;
          break;
        case 25:
          this._miViewPrevAnim = (TkMenuItem) target;
          break;
        case 26:
          this._miViewFrameMirror = (TkMenuItem) target;
          break;
        case 27:
          this._miAnchors = (System.Windows.Controls.MenuItem) target;
          break;
        case 28:
          this._miShowAnchors = (System.Windows.Controls.MenuItem) target;
          this._miShowAnchors.Checked += new RoutedEventHandler(this._miShowAnchors_Checked);
          this._miShowAnchors.Unchecked += new RoutedEventHandler(this._miShowAnchors_Unchecked);
          this._miShowAnchors.Loaded += new RoutedEventHandler(this._miShowAnchors_Loaded);
          break;
        case 29:
          this._miReverseAnchors = (System.Windows.Controls.MenuItem) target;
          break;
        case 30:
          this._miAnchor = (System.Windows.Controls.MenuItem) target;
          break;
        case 31:
          ((System.Windows.Controls.MenuItem) target).Checked += new RoutedEventHandler(this._miAnchor_Click);
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this._miAnchor_Click);
          break;
        case 32:
          ((System.Windows.Controls.MenuItem) target).Checked += new RoutedEventHandler(this._miAnchor_Click);
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this._miAnchor_Click);
          break;
        case 33:
          ((System.Windows.Controls.MenuItem) target).Checked += new RoutedEventHandler(this._miAnchor_Click);
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this._miAnchor_Click);
          break;
        case 34:
          ((System.Windows.Controls.MenuItem) target).Checked += new RoutedEventHandler(this._miAnchor_Click);
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this._miAnchor_Click);
          break;
        case 35:
          ((System.Windows.Controls.MenuItem) target).Checked += new RoutedEventHandler(this._miAnchor_Click);
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this._miAnchor_Click);
          break;
        case 36:
          this._dpUndoRedo = (DockPanel) target;
          break;
        case 37:
          this._tmbUndo = (ToggleMemoryButton) target;
          break;
        case 38:
          this._tmbRedo = (ToggleMemoryButton) target;
          break;
        case 39:
          this._preloader = (Grid) target;
          break;
        case 40:
          this._framePreviewGrid = (Grid) target;
          break;
        case 41:
          this._framePreview = (FramePreview) target;
          break;
        case 42:
          this._frameSelector = (ActIndexSelector) target;
          break;
        case 43:
          this._sv = (ScrollViewer) target;
          break;
        case 44:
          this._stackPanelReferences = (StackPanel) target;
          break;
        case 45:
          this._layerEditor = (LayerEditor) target;
          break;
        case 46:
          this._gridSpriteSelected = (Grid) target;
          this._gridSpriteSelected.SizeChanged += new SizeChangedEventHandler(this._gridSpriteSelected_SizeChanged);
          break;
        case 47:
          this._spriteSelector = (SpriteSelector) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void ActEditorEventDelegate(object sender);
  }
}
