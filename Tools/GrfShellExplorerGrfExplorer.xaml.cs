// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.GrfShellExplorer.GrfExplorer
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Tools.GrfShellExplorer.PreviewTabs;
using ErrorManager;
using GRF.Core;
using GRF.IO;
using GRF.Threading;
using GrfToWpfBridge.TreeViewManager;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using TokeiLibrary;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles.ListView;
using Utilities;
using Utilities.Extension;

namespace ActEditor.Tools.GrfShellExplorer
{
  public partial class GrfExplorer : Window, IComponentConnector
  {
    public SelectMode SelectMode;
    private readonly string _filename;
    private readonly GrfHolder _grfHolder = new GrfHolder();
    private readonly object _listLoadLock = new object();
    private FileEntry _latestSelectedItem;
    private TreeViewPathManager _treeViewPathManager;
    private readonly object _filterLock = new object();
    private readonly GrfExplorer.FileEntryComparer<FileEntry> _grfEntrySorter = new GrfExplorer.FileEntryComparer<FileEntry>();
    private readonly GrfExplorer.FileEntryComparer<FileEntry> _grfSearchEntrySorter = new GrfExplorer.FileEntryComparer<FileEntry>();
    private readonly object _searchLock = new object();
    private ObservableCollection<FileEntry> _itemEntries = new ObservableCollection<FileEntry>();
    private ObservableCollection<FileEntry> _itemSearchEntries = new ObservableCollection<FileEntry>();
    private string _searchFilter = "";
    private string _searchSelectedPath = "";
    private string _searchString = "";
    internal TextBox _textBoxMainSearch;
    internal Grid _gridSearchResults;
    internal System.Windows.Controls.ListView _listBoxResults;
    internal Grid _gridBoxResultsHeight;
    internal TkView _treeView;
    internal TextBox _textBoxSearch;
    internal System.Windows.Controls.ListView _items;
    internal PreviewImage _previewImage;
    internal PreviewAct _previewAct;
    internal Grid _gridActionPresenter;
    internal Button _buttonOk;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public GrfExplorer() => this.InitializeComponent();

    private void _grfExplorer_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.Close();
    }

    private void _onPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      this._buttonOk_Click((object) null, (RoutedEventArgs) null);
      e.Handled = true;
    }

    public GrfExplorer(string filename, SelectMode mode)
    {
      this.SelectMode = mode;
      this._filename = filename;
      this.InitializeComponent();
      this._loadEditorUI();
      this._load(this._filename);
      this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public string SelectedItem => this._latestSelectedItem != null ? this._latestSelectedItem.RelativePath : (string) null;

    private void _loadEditorUI()
    {
      this._treeViewPathManager = new TreeViewPathManager(this._treeView);
      this.ShowInTaskbar = false;
      this._items.ItemsSource = (IEnumerable) this._itemEntries;
      this._listBoxResults.ItemsSource = (IEnumerable) this._itemSearchEntries;
      this._treeView.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this._treeView_PreviewMouseLeftButtonDown);
      this._listBoxResults.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this._listBoxResults_PreviewMouseLeftButtonDown);
      ListViewDataTemplateHelper.GenerateListViewTemplate(this._listBoxResults, new ListViewDataTemplateHelper.ColumnInfo[4]
      {
        new ListViewDataTemplateHelper.ColumnInfo()
        {
          Header = "",
          DisplayExpression = "{Binding Path=DataImage}",
          SearchGetAccessor = "FileType",
          Alignment = TextAlignment.Center,
          Width = 20.0,
          MaxHeight = 24,
          IsImage = true,
          Margin = "-4"
        },
        new ListViewDataTemplateHelper.ColumnInfo()
        {
          Header = "File name",
          DisplayExpression = "{Binding Path=RelativePath}",
          SearchGetAccessor = "RelativePath",
          Alignment = TextAlignment.Left,
          Width = -1.0
        },
        new ListViewDataTemplateHelper.ColumnInfo()
        {
          Header = "Type",
          DisplayExpression = "{Binding Path=FileType}",
          SearchGetAccessor = "FileType",
          Alignment = TextAlignment.Right,
          Width = 40.0
        },
        new ListViewDataTemplateHelper.ColumnInfo()
        {
          Header = "Size",
          DisplayExpression = "{Binding Path=DisplaySize}",
          SearchGetAccessor = "NewSizeDecompressed",
          Alignment = TextAlignment.Right,
          Width = 60.0
        }
      }, (ListViewCustomComparer) new DefaultListViewComparer<FileEntry>(), (IList<string>) new string[6]
      {
        "Added",
        "Blue",
        "Encrypted",
        "#FFE08000",
        "Removed",
        "Red"
      });
      System.Windows.Controls.ListView items = this._items;
      ListViewDataTemplateHelper.GeneralColumnInfo[] generalColumnInfoArray1 = new ListViewDataTemplateHelper.GeneralColumnInfo[4];
      ListViewDataTemplateHelper.GeneralColumnInfo[] generalColumnInfoArray2 = generalColumnInfoArray1;
      ListViewDataTemplateHelper.ImageColumnInfo imageColumnInfo1 = new ListViewDataTemplateHelper.ImageColumnInfo();
      imageColumnInfo1.Header = "";
      imageColumnInfo1.DisplayExpression = "DataImage";
      imageColumnInfo1.SearchGetAccessor = "FileType";
      imageColumnInfo1.FixedWidth = 20.0;
      imageColumnInfo1.MaxHeight = 24.0;
      ListViewDataTemplateHelper.ImageColumnInfo imageColumnInfo2 = imageColumnInfo1;
      generalColumnInfoArray2[0] = (ListViewDataTemplateHelper.GeneralColumnInfo) imageColumnInfo2;
      ListViewDataTemplateHelper.GeneralColumnInfo[] generalColumnInfoArray3 = generalColumnInfoArray1;
      ListViewDataTemplateHelper.RangeColumnInfo rangeColumnInfo1 = new ListViewDataTemplateHelper.RangeColumnInfo();
      rangeColumnInfo1.Header = "File name";
      rangeColumnInfo1.DisplayExpression = "DisplayRelativePath";
      rangeColumnInfo1.SearchGetAccessor = "RelativePath";
      rangeColumnInfo1.IsFill = true;
      rangeColumnInfo1.TextAlignment = TextAlignment.Left;
      rangeColumnInfo1.ToolTipBinding = "RelativePath";
      rangeColumnInfo1.MinWidth = 100.0;
      ListViewDataTemplateHelper.RangeColumnInfo rangeColumnInfo2 = rangeColumnInfo1;
      generalColumnInfoArray3[1] = (ListViewDataTemplateHelper.GeneralColumnInfo) rangeColumnInfo2;
      generalColumnInfoArray1[2] = new ListViewDataTemplateHelper.GeneralColumnInfo()
      {
        Header = "Type",
        DisplayExpression = "FileType",
        FixedWidth = 40.0,
        ToolTipBinding = "FileType",
        TextAlignment = TextAlignment.Right
      };
      generalColumnInfoArray1[3] = new ListViewDataTemplateHelper.GeneralColumnInfo()
      {
        Header = "Size",
        DisplayExpression = "DisplaySize",
        SearchGetAccessor = "NewSizeDecompressed",
        FixedWidth = 60.0,
        TextAlignment = TextAlignment.Right,
        ToolTipBinding = "NewSizeDecompressed"
      };
      ListViewDataTemplateHelper.GeneralColumnInfo[] columnInfos = generalColumnInfoArray1;
      DefaultListViewComparer<FileEntry> sorter = new DefaultListViewComparer<FileEntry>();
      string[] triggers = new string[6]
      {
        "Added",
        "Blue",
        "Encrypted",
        "#FFE08000",
        "Removed",
        "Red"
      };
      string[] strArray = new string[0];
      ListViewDataTemplateHelper.GenerateListViewTemplateNew(items, columnInfos, (ListViewCustomComparer) sorter, (IList<string>) triggers, strArray);
      WpfUtils.AddDragDropEffects((Control) this._items);
      WpfUtils.AddDragDropEffects((Control) this._treeView, (Func<List<string>, bool>) (f => f.Select<string, string>((Func<string, string>) (p => p.GetExtension())).All<string>((Func<string, bool>) (p => p == ".grf" || p == ".rgz" || p == ".thor" || p == ".gpf"))));
      this._grfEntrySorter.SetOrder("DisplayRelativePath", ListSortDirection.Ascending);
      this._grfSearchEntrySorter.SetOrder("RelativePath", ListSortDirection.Ascending);
      this._items.PreviewKeyDown += new KeyEventHandler(this._onPreviewKeyDown);
      this._listBoxResults.PreviewKeyDown += new KeyEventHandler(this._onPreviewKeyDown);
      this.PreviewKeyDown += new KeyEventHandler(this._grfExplorer_KeyDown);
    }

    private void _listBoxResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        ListViewItem objectAtPoint = this._listBoxResults.GetObjectAtPoint<ListViewItem>(e.GetPosition((IInputElement) this._listBoxResults));
        if (objectAtPoint == null || !objectAtPoint.IsSelected)
          return;
        this._listBoxResults_SelectionChanged((object) objectAtPoint, (SelectionChangedEventArgs) null);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _treeView_SelectedItemChanged(
      object sender,
      RoutedPropertyChangedEventArgs<object> e)
    {
      if (!(this._treeView.SelectedItem is TkTreeViewItem))
        return;
      this._loadListItems();
      this._showPreview(this._grfHolder, this._treeViewPathManager.GetCurrentRelativePath(), (string) null);
      GrfEditorConfiguration.GrfShellLatest = this._treeViewPathManager.GetCurrentRelativePath();
    }

    private void _showPreview(GrfHolder grfData, string currentPath, string selectedItem)
    {
      if (selectedItem == null)
        return;
      if (selectedItem.IsExtension(".spr", ".jpg", ".png", ".tga", ".bmp", ".pal"))
      {
        this._previewImage.Load(grfData, grfData.FileTable[GrfPath.Combine(currentPath, selectedItem)]);
        this._previewImage.Visibility = Visibility.Visible;
        this._previewAct.Visibility = Visibility.Hidden;
      }
      else
      {
        if (!selectedItem.IsExtension(".act"))
          return;
        this._previewAct.Load(grfData, grfData.FileTable[GrfPath.Combine(currentPath, selectedItem)]);
        this._previewImage.Visibility = Visibility.Hidden;
        this._previewAct.Visibility = Visibility.Visible;
      }
    }

    private void _treeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      TreeViewItem treeViewItemClicked = WpfUtilities.GetTreeViewItemClicked((FrameworkElement) e.OriginalSource, (TreeView) this._treeView);
      if (treeViewItemClicked == null)
        return;
      treeViewItemClicked.IsSelected = true;
    }

    private void _treeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      TreeViewItem treeViewItemClicked = WpfUtilities.GetTreeViewItemClicked((FrameworkElement) e.OriginalSource, (TreeView) this._treeView);
      if (treeViewItemClicked == null || treeViewItemClicked != this._treeView.SelectedItem)
        return;
      this._treeView_SelectedItemChanged(sender, (RoutedPropertyChangedEventArgs<object>) null);
    }

    private void _gridBoxResultsHeight_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this._gridSearchResults.Visibility != Visibility.Visible)
        return;
      this._gridSearchResults.Height = this._gridBoxResultsHeight.ActualHeight;
    }

    private void _buttonOk_Click(object sender, RoutedEventArgs e)
    {
      if (this._latestSelectedItem != null)
      {
        this.DialogResult = new bool?(true);
        this.Close();
      }
      else
        ErrorHandler.HandleException("No item selected.");
    }

    private void _buttonCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(false);
      this.Close();
    }

    private void _listBoxResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left || this._listBoxResults.GetObjectAtPoint<ListViewItem>(e.GetPosition((IInputElement) this._listBoxResults)) == null || this._latestSelectedItem == null)
        return;
      if (this.SelectMode == SelectMode.Act)
      {
        if (this._latestSelectedItem.RelativePath.IsExtension(".act"))
        {
          e.Handled = true;
          this._buttonOk_Click((object) null, (RoutedEventArgs) null);
        }
        else
        {
          if (!this._latestSelectedItem.RelativePath.IsExtension(".spr"))
            return;
          FileEntry fileEntry = this._grfHolder.FileTable.TryGet(this._latestSelectedItem.RelativePath.ReplaceExtension(".act"));
          if (fileEntry == null)
            return;
          e.Handled = true;
          this._latestSelectedItem = fileEntry;
          this._buttonOk_Click((object) null, (RoutedEventArgs) null);
        }
      }
      else
      {
        if (this.SelectMode != SelectMode.Pal)
          return;
        if (!this._latestSelectedItem.RelativePath.IsExtension(".spr", ".pal"))
          return;
        e.Handled = true;
        this._buttonOk_Click((object) null, (RoutedEventArgs) null);
      }
    }

    private void _items_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left || this._items.GetObjectAtPoint<ListViewItem>(e.GetPosition((IInputElement) this._items)) == null || this._latestSelectedItem == null)
        return;
      if (this.SelectMode == SelectMode.Act)
      {
        if (this._latestSelectedItem.RelativePath.IsExtension(".act"))
        {
          e.Handled = true;
          this._buttonOk_Click((object) null, (RoutedEventArgs) null);
        }
        else
        {
          if (!this._latestSelectedItem.RelativePath.IsExtension(".spr"))
            return;
          FileEntry fileEntry = this._grfHolder.FileTable.TryGet(this._latestSelectedItem.RelativePath.ReplaceExtension(".act"));
          if (fileEntry == null)
            return;
          e.Handled = true;
          this._latestSelectedItem = fileEntry;
          this._buttonOk_Click((object) null, (RoutedEventArgs) null);
        }
      }
      else
      {
        if (this.SelectMode != SelectMode.Pal)
          return;
        if (!this._latestSelectedItem.RelativePath.IsExtension(".spr", ".pal"))
          return;
        e.Handled = true;
        this._buttonOk_Click((object) null, (RoutedEventArgs) null);
      }
    }

    private void _load(string filename) => new Thread((ThreadStart) (() =>
    {
      try
      {
        this._listBoxResults.Dispatch<System.Windows.Controls.ListView>((Action<System.Windows.Controls.ListView>) (p => this._itemSearchEntries.Clear()));
        this._items.Dispatch<System.Windows.Controls.ListView>((Action<System.Windows.Controls.ListView>) (p => this._itemEntries.Clear()));
        this._treeViewPathManager.ClearAll();
        this._treeViewPathManager.ClearCommands();
        this._grfHolder.Close();
        this._grfHolder.Open(filename);
        if (this._grfHolder.Header == null)
        {
          this.Dispatch<GrfExplorer, string>((Func<GrfExplorer, string>) (p => p.Title = "GrfShell Explorer"));
          this._treeViewPathManager.ClearAll();
          this._grfHolder.Close();
        }
        else
        {
          this.Dispatch<GrfExplorer, string>((Func<GrfExplorer, string>) (p => p.Title = "GrfShell Explorer - " + Methods.CutFileName(filename)));
          this._treeViewPathManager.AddPath(new TkPath()
          {
            FilePath = filename,
            RelativePath = ""
          });
          foreach (string directory in this._grfHolder.FileTable.Directories)
            this._treeViewPathManager.AddPath(new TkPath()
            {
              FilePath = filename,
              RelativePath = directory
            });
          string str1 = string.IsNullOrEmpty(GrfEditorConfiguration.GrfShellLatest) ? "data\\sprite" : GrfEditorConfiguration.GrfShellLatest;
          this._treeViewPathManager.ExpandFirstNode();
          this._treeViewPathManager.SelectFirstNode();
          foreach (string str2 in Methods.StringToList("data,data\\sprite"))
            this._treeViewPathManager.Expand(new TkPath()
            {
              FilePath = filename,
              RelativePath = str2
            });
          this._treeViewPathManager.Select(new TkPath()
          {
            FilePath = filename,
            RelativePath = str1
          });
          this._search();
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }))
    {
      Name = "GrfEditor - GRF loading thread"
    }.Start();

    private void _listBoxResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        if (this._listBoxResults.SelectedItem == null)
          return;
        this._latestSelectedItem = this._listBoxResults.SelectedItem as FileEntry;
        this._showPreview(this._grfHolder, Path.GetDirectoryName(this._listBoxResults.SelectedItem.ToString()), Path.GetFileName(this._listBoxResults.SelectedItem.ToString()));
        GrfEditorConfiguration.GrfShellLatest = this._latestSelectedItem == null ? "" : Path.GetDirectoryName(this._latestSelectedItem.RelativePath);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex.Message, ErrorLevel.Warning);
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this._grfHolder.Close();
      base.OnClosing(e);
    }

    private void _textBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
      this._searchFilter = this._textBoxSearch.Text;
      this._filter();
    }

    private void _textBox_TextChanged(object sender, EventArgs keyEventArgs)
    {
      this._searchString = this._textBoxMainSearch.Text;
      this._search();
    }

    private void _search()
    {
      string currentSearch = this._searchString;
      new Thread((ThreadStart) (() =>
      {
        lock (this._searchLock)
        {
          try
          {
            if (currentSearch != this._searchString)
              return;
            if (currentSearch == "")
            {
              int num1 = (int) this._gridSearchResults.Dispatch<Grid, Visibility>((Func<Grid, Visibility>) (p => p.Visibility = Visibility.Collapsed));
            }
            else
            {
              if (((IEnumerable<string>) currentSearch.Split(' ')).All<string>((Func<string, bool>) (p => p.Length == 0)))
                return;
              int num2 = (int) this._gridSearchResults.Dispatch<Grid, Visibility>((Func<Grid, Visibility>) (p => p.Visibility = Visibility.Visible));
              this.Dispatch<GrfExplorer>((Action<GrfExplorer>) (p => p._grfSearchEntrySorter.SetOrder(WpfUtils.GetLastGetSearchAccessor(this._listBoxResults), WpfUtils.GetLastSortDirection((DependencyObject) this._listBoxResults))));
              List<KeyValuePair<string, FileEntry>> fastAccessEntries = this._grfHolder.FileTable.FastAccessEntries;
              List<string> search = ((IEnumerable<string>) currentSearch.Split(' ')).ToList<string>();
              this._itemSearchEntries = new ObservableCollection<FileEntry>((IEnumerable<FileEntry>) fastAccessEntries.Where<KeyValuePair<string, FileEntry>>((Func<KeyValuePair<string, FileEntry>, bool>) (p => search.All<string>((Func<string, bool>) (q => p.Key.IndexOf(q, StringComparison.InvariantCultureIgnoreCase) != -1)))).Select<KeyValuePair<string, FileEntry>, FileEntry>((Func<KeyValuePair<string, FileEntry>, FileEntry>) (p => p.Value)).OrderBy<FileEntry, FileEntry>((Func<FileEntry, FileEntry>) (p => p), (IComparer<FileEntry>) this._grfSearchEntrySorter));
              this._itemSearchEntries.Where<FileEntry>((Func<FileEntry, bool>) (p => p.DataImage == null)).ToList<FileEntry>().ForEach((Action<FileEntry>) (p => p.DataImage = (object) GrfEditorIconProvider.GetIcon(p.RelativePath)));
              this._listBoxResults.Dispatch<System.Windows.Controls.ListView, IEnumerable>((Func<System.Windows.Controls.ListView, IEnumerable>) (p => p.ItemsSource = (IEnumerable) this._itemSearchEntries));
            }
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex.Message, ErrorLevel.Warning);
          }
        }
      }))
      {
        Name = "GrfEditor - Search filter for all items thread"
      }.Start();
    }

    private void _filter(string currentPath = null)
    {
      string currentSearch = this._searchFilter;
      new Thread((ThreadStart) (() =>
      {
        lock (this._filterLock)
        {
          try
          {
            if (currentSearch != this._searchFilter || (bool) this._treeView.Dispatcher.Invoke((Delegate) (() => this._treeView.SelectedItem == null)) || currentPath != null && (this._searchSelectedPath != currentPath || this._searchSelectedPath == null) || this._items == null)
              return;
            this.Dispatch<GrfExplorer>((Action<GrfExplorer>) (p => p._grfEntrySorter.SetOrder(WpfUtils.GetLastGetSearchAccessor(this._items), WpfUtils.GetLastSortDirection((DependencyObject) this._items))));
            List<Tuple<string, string, FileEntry>> tupleAccessEntries = this._grfHolder.FileTable.FastTupleAccessEntries;
            List<string> search = ((IEnumerable<string>) currentSearch.Split(' ')).ToList<string>();
            this._itemEntries = new ObservableCollection<FileEntry>((IEnumerable<FileEntry>) tupleAccessEntries.Where<Tuple<string, string, FileEntry>>((Func<Tuple<string, string, FileEntry>, bool>) (p => p.Item1 == this._searchSelectedPath && search.All<string>((Func<string, bool>) (q => p.Item2.IndexOf(q, StringComparison.InvariantCultureIgnoreCase) != -1)))).Select<Tuple<string, string, FileEntry>, FileEntry>((Func<Tuple<string, string, FileEntry>, FileEntry>) (p => p.Item3)).OrderBy<FileEntry, FileEntry>((Func<FileEntry, FileEntry>) (p => p), (IComparer<FileEntry>) this._grfEntrySorter));
            this._itemEntries.Where<FileEntry>((Func<FileEntry, bool>) (p => p.DataImage == null)).ToList<FileEntry>().ForEach((Action<FileEntry>) (p => p.DataImage = (object) GrfEditorIconProvider.GetIcon(p.RelativePath)));
            this._items.Dispatch<System.Windows.Controls.ListView, IEnumerable>((Func<System.Windows.Controls.ListView, IEnumerable>) (p => p.ItemsSource = (IEnumerable) this._itemEntries));
          }
          catch
          {
          }
        }
      }))
      {
        Name = "GrfEditor - Search filter for ListView items thread"
      }.Start();
    }

    private void _items_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._items.SelectedItem == null)
        return;
      this._latestSelectedItem = (FileEntry) this._items.SelectedItem;
      FileEntry selectedItem = (FileEntry) this._items.SelectedItem;
      this._showPreview(this._grfHolder, Path.GetDirectoryName(selectedItem.RelativePath), Path.GetFileName(selectedItem.RelativePath));
      GrfEditorConfiguration.GrfShellLatest = this._latestSelectedItem == null ? "" : Path.GetDirectoryName(this._latestSelectedItem.RelativePath);
    }

    private void _items_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        ListViewItem objectAtPoint = this._items.GetObjectAtPoint<ListViewItem>(e.GetPosition((IInputElement) this._items));
        if (objectAtPoint != null && objectAtPoint.IsSelected)
          this._items_SelectionChanged(sender, (SelectionChangedEventArgs) null);
        e.Handled = false;
      }
      catch
      {
      }
    }

    private void _loadListItems()
    {
      string currentPath = this._treeViewPathManager.GetCurrentRelativePath();
      this._treeView.Dispatcher.Invoke((Delegate) (() =>
      {
        if (this._treeView.SelectedItem == null)
          this._itemEntries.Clear();
        this._searchSelectedPath = this._treeViewPathManager.GetCurrentRelativePath();
      }));
      GrfThread.Start((Action) (() =>
      {
        lock (this._listLoadLock)
        {
          this._searchFilter = (string) this._textBoxSearch.Dispatcher.Invoke((Delegate) (() => this._textBoxSearch.Text));
          this._filter(currentPath);
        }
      }), "GrfEditor - Search filter for ListView thread");
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/tools/grfshellexplorer/grfexplorer.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._textBoxMainSearch = (TextBox) target;
          this._textBoxMainSearch.TextChanged += new TextChangedEventHandler(this._textBox_TextChanged);
          break;
        case 2:
          this._gridSearchResults = (Grid) target;
          break;
        case 3:
          this._listBoxResults = (System.Windows.Controls.ListView) target;
          this._listBoxResults.SelectionChanged += new SelectionChangedEventHandler(this._listBoxResults_SelectionChanged);
          this._listBoxResults.MouseDoubleClick += new MouseButtonEventHandler(this._listBoxResults_MouseDoubleClick);
          break;
        case 4:
          this._gridBoxResultsHeight = (Grid) target;
          this._gridBoxResultsHeight.SizeChanged += new SizeChangedEventHandler(this._gridBoxResultsHeight_SizeChanged);
          break;
        case 5:
          this._treeView = (TkView) target;
          this._treeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(this._treeView_SelectedItemChanged);
          this._treeView.PreviewMouseRightButtonDown += new MouseButtonEventHandler(this._treeView_PreviewMouseRightButtonDown);
          break;
        case 6:
          this._textBoxSearch = (TextBox) target;
          this._textBoxSearch.TextChanged += new TextChangedEventHandler(this._textBoxSearch_TextChanged);
          break;
        case 7:
          this._items = (System.Windows.Controls.ListView) target;
          this._items.SelectionChanged += new SelectionChangedEventHandler(this._items_SelectionChanged);
          this._items.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this._items_PreviewMouseLeftButtonDown);
          this._items.MouseDoubleClick += new MouseButtonEventHandler(this._items_MouseDoubleClick);
          break;
        case 8:
          this._previewImage = (PreviewImage) target;
          break;
        case 9:
          this._previewAct = (PreviewAct) target;
          break;
        case 10:
          this._gridActionPresenter = (Grid) target;
          break;
        case 11:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        case 12:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public class FileEntryComparer<T> : IComparer<T>
    {
      private readonly DefaultListViewComparer<T> _internalSearch = new DefaultListViewComparer<T>();
      private string _searchGetAccessor;

      public int Compare(T x, T y) => this._searchGetAccessor != null ? this._internalSearch.Compare((object) x, (object) y) : 0;

      public void SetOrder(string searchGetAccessor, ListSortDirection direction)
      {
        if (searchGetAccessor == null)
          return;
        this._searchGetAccessor = searchGetAccessor;
        this._internalSearch.SetSort(searchGetAccessor, direction);
      }
    }
  }
}
