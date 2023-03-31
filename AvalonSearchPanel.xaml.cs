// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Avalon.SearchPanel
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.GenericControls;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace ActEditor.Core.Avalon
{
  public partial class SearchPanel : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty MatchCaseProperty = DependencyProperty.Register(nameof (MatchCase), typeof (bool), typeof (SearchPanel), (PropertyMetadata) new FrameworkPropertyMetadata((object) false, new PropertyChangedCallback(SearchPanel._searchPatternChangedCallback)));
    public static readonly DependencyProperty SearchPatternProperty = DependencyProperty.Register(nameof (SearchPattern), typeof (string), typeof (SearchPanel), (PropertyMetadata) new FrameworkPropertyMetadata((object) "", new PropertyChangedCallback(SearchPanel._searchPatternChangedCallback)));
    public static readonly DependencyProperty WholeWordsProperty = DependencyProperty.Register(nameof (WholeWords), typeof (bool), typeof (SearchPanel), (PropertyMetadata) new FrameworkPropertyMetadata((object) false, new PropertyChangedCallback(SearchPanel._searchPatternChangedCallback)));
    public static readonly DependencyProperty UseRegexProperty = DependencyProperty.Register(nameof (UseRegex), typeof (bool), typeof (SearchPanel), (PropertyMetadata) new FrameworkPropertyMetadata((object) false, new PropertyChangedCallback(SearchPanel._searchPatternChangedCallback)));
    public static readonly RoutedCommand Find = new RoutedCommand(nameof (Find), typeof (SearchPanel), new InputGestureCollection()
    {
      (InputGesture) new KeyGesture(Key.F, ModifierKeys.Control)
    });
    private readonly System.Windows.Controls.ToolTip _messageView = new System.Windows.Controls.ToolTip()
    {
      Placement = PlacementMode.Bottom,
      StaysOpen = false
    };
    private SearchPanel.SearchPanelAdorner _adorner;
    private TextDocument _currentDocument;
    private ICSharpCode.AvalonEdit.TextEditor _editor;
    private SearchPanel.SearchResultBackgroundRenderer _renderer;
    private ISearchStrategy _strategy;
    private TextArea _textArea;
    internal FancyButton _buttonFancyMode;
    internal Border _border1;
    internal LeftComboBox _cbSubMenu;
    internal TextBox _searchTextBox;
    internal TextBlock _labelFind;
    internal FancyButton _buttonOpenSubMenu;
    internal FancyButton _buttonPrev;
    internal FancyButton _buttonNext;
    internal Border _border2;
    internal TextBox _replaceTextBox;
    internal TextBlock _labelReplace;
    internal FancyButton _buttonReplaceSingle;
    internal FancyButton _buttonReplaceAll;
    internal FancyButton _buttonClose;
    private bool _contentLoaded;

    public SearchPanel()
    {
      this.InitializeComponent();
      this._isSearch = true;
      CommandManager.AddPreviewCanExecuteHandler((UIElement) this, new CanExecuteRoutedEventHandler(this.OnPreviewCanExecuteHandler));
      CommandManager.AddPreviewExecutedHandler((UIElement) this, new ExecutedRoutedEventHandler(this.OnPreviewExecutedEvent));
    }

    public bool IsClosed { get; set; }

    public string SearchPattern
    {
      get => (string) this.GetValue(SearchPanel.SearchPatternProperty);
      set => this.SetValue(SearchPanel.SearchPatternProperty, (object) value);
    }

    public bool MatchCase
    {
      get => (bool) this.GetValue(SearchPanel.MatchCaseProperty);
      set => this.SetValue(SearchPanel.MatchCaseProperty, (object) value);
    }

    public bool WholeWords
    {
      get => (bool) this.GetValue(SearchPanel.WholeWordsProperty);
      set => this.SetValue(SearchPanel.WholeWordsProperty, (object) value);
    }

    public bool UseRegex
    {
      get => (bool) this.GetValue(SearchPanel.UseRegexProperty);
      set => this.SetValue(SearchPanel.UseRegexProperty, (object) value);
    }

    private bool _isSearch { get; set; }

    private static void _searchPatternChangedCallback(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is SearchPanel searchPanel))
        return;
      searchPanel.ValidateSearchText();
      searchPanel.UpdateSearch();
    }

    public void OnPreviewCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
    {
      if (e.Command == ApplicationCommands.Undo)
      {
        e.CanExecute = true;
        e.Handled = true;
      }
      else
      {
        if (e.Command != ApplicationCommands.Redo)
          return;
        e.CanExecute = true;
        e.Handled = true;
      }
    }

    public void OnPreviewExecutedEvent(object sender, ExecutedRoutedEventArgs e)
    {
      if (e.Command == ApplicationCommands.Undo)
      {
        this._editor.Undo();
        e.Handled = true;
      }
      else
      {
        if (e.Command != ApplicationCommands.Redo)
          return;
        this._editor.Redo();
        e.Handled = true;
      }
    }

    public void ValidateSearchText()
    {
      if (this._searchTextBox == null)
        return;
      BindingExpression bindingExpression = this._searchTextBox.GetBindingExpression(TextBox.TextProperty);
      try
      {
        Validation.ClearInvalid((BindingExpressionBase) bindingExpression);
        this.UpdateSearch();
      }
      catch (SearchPatternException ex)
      {
        ValidationError validationError = new ValidationError(bindingExpression.ParentBinding.ValidationRules[0], (object) bindingExpression, (object) ex.Message, (Exception) ex);
        Validation.MarkInvalid((BindingExpressionBase) bindingExpression, validationError);
      }
    }

    public void Attach(TextArea textArea, ICSharpCode.AvalonEdit.TextEditor editor)
    {
      if (textArea == null)
        throw new ArgumentNullException(nameof (textArea));
      this._editor = editor;
      this._textArea = textArea;
      this._currentDocument = textArea.Document;
      this._renderer = new SearchPanel.SearchResultBackgroundRenderer()
      {
        MarkerBrush = (Brush) new SolidColorBrush(Colors.Yellow)
      };
      this._searchTextBox.TextChanged += new TextChangedEventHandler(this._searchTextBox_TextChanged);
      this._adorner = new SearchPanel.SearchPanelAdorner(textArea, this);
      if (this._currentDocument != null)
        this._currentDocument.TextChanged += new EventHandler(this._currentDocument_TextChanged);
      this._textArea.DocumentChanged += new EventHandler(this._textArea_DocumentChanged);
      this._textArea.PreviewKeyDown += new KeyEventHandler(this._textArea_PreviewKeyDown);
      this._searchTextBox.LostFocus += new RoutedEventHandler(this._searchTextBox_LostFocus);
      this._replaceTextBox.LostFocus += new RoutedEventHandler(this._replaceTextBox_LostFocus);
      this._searchTextBox.GotFocus += new RoutedEventHandler(this._searchTextBox_GotFocus);
      this._replaceTextBox.GotFocus += new RoutedEventHandler(this._replaceTextBox_GotFocus);
      this.KeyDown += new KeyEventHandler(this._searchPanel_KeyDown);
      this.CommandBindings.Add(new CommandBinding((ICommand) SearchPanel.Find, (ExecutedRoutedEventHandler) ((sender, e) => this.Open())));
      this.CommandBindings.Add(new CommandBinding((ICommand) SearchCommands.FindNext, (ExecutedRoutedEventHandler) ((sender, e) => this.FindNext())));
      this.CommandBindings.Add(new CommandBinding((ICommand) SearchCommands.FindPrevious, (ExecutedRoutedEventHandler) ((sender, e) => this.FindPrevious())));
      this.CommandBindings.Add(new CommandBinding((ICommand) SearchCommands.CloseSearchPanel, (ExecutedRoutedEventHandler) ((sender, e) => this.Close())));
      textArea.CommandBindings.Add(new CommandBinding((ICommand) SearchCommands.FindNext, (ExecutedRoutedEventHandler) ((sender, e) => this.FindNext())));
      textArea.CommandBindings.Add(new CommandBinding((ICommand) SearchCommands.FindPrevious, (ExecutedRoutedEventHandler) ((sender, e) => this.FindPrevious())));
      textArea.CommandBindings.Add(new CommandBinding((ICommand) SearchCommands.CloseSearchPanel, (ExecutedRoutedEventHandler) ((sender, e) => this.Close())));
      this.IsClosed = true;
    }

    private void _replaceTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
      this._labelReplace.Visibility = Visibility.Hidden;
      this._border2.BorderBrush = (Brush) new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 186, (byte) 86, (byte) 0));
      this._replaceTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Black);
      this._searchTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Black);
    }

    private void _searchTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
      this._labelFind.Visibility = Visibility.Hidden;
      this._border1.BorderBrush = (Brush) new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 186, (byte) 86, (byte) 0));
      this._replaceTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Black);
      this._searchTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Black);
    }

    private void _replaceTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      this._border2.BorderBrush = (Brush) new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 132, (byte) 144, (byte) 161));
      if (this._replaceTextBox.Text == "")
      {
        this._labelReplace.Visibility = Visibility.Visible;
      }
      else
      {
        this._labelReplace.Visibility = Visibility.Hidden;
        this._searchTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Gray);
        this._replaceTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Gray);
      }
    }

    private void _searchTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      this._border1.BorderBrush = (Brush) new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 132, (byte) 144, (byte) 161));
      if (this._searchTextBox.Text == "")
      {
        this._labelFind.Visibility = Visibility.Visible;
      }
      else
      {
        this._labelFind.Visibility = Visibility.Hidden;
        this._searchTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Gray);
        this._replaceTextBox.Foreground = (Brush) new SolidColorBrush(Colors.Gray);
      }
    }

    private void _searchTextBox_TextChanged(object sender, TextChangedEventArgs e) => this.SearchPattern = this._searchTextBox.Text;

    private void _textArea_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
      {
        this._isSearch = false;
        this._buttonFancyMode_Click(sender, (RoutedEventArgs) null);
        this.Open();
        this._searchTextBox.SelectAll();
        this._searchTextBox.Focus();
        e.Handled = true;
      }
      if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control || e.Key != Key.H)
        return;
      this._isSearch = true;
      this._buttonFancyMode_Click(sender, (RoutedEventArgs) null);
      this.Open();
      this._searchTextBox.SelectAll();
      this._searchTextBox.Focus();
      e.Handled = true;
    }

    public void Open()
    {
      string text = this._textArea.Selection.GetText();
      if (!string.IsNullOrEmpty(text))
      {
        this._searchTextBox.Text = this._cut(text);
        this._searchTextBox.SelectAll();
        this._searchTextBox.Focus();
      }
      if (!this.IsClosed)
        return;
      AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer((Visual) this._textArea);
      if (adornerLayer != null)
      {
        adornerLayer.Add((Adorner) this._adorner);
        this._searchTextBox.SelectAll();
        this._searchTextBox.Focus();
        new Thread((ThreadStart) (() =>
        {
          Thread.Sleep(100);
          this._searchTextBox.Dispatch<TextBox>((Action<TextBox>) (p => p.SelectAll()));
          this._searchTextBox.Dispatch<TextBox, bool>((Func<TextBox, bool>) (p => p.Focus()));
        })).Start();
      }
      this._textArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._renderer);
      this.IsClosed = false;
      this.DoSearch(false);
    }

    private string _cut(string selectedText)
    {
      if (selectedText.Contains(Environment.NewLine))
        selectedText = selectedText.Substring(0, selectedText.IndexOf(Environment.NewLine, StringComparison.Ordinal));
      if (selectedText.Contains<char>('\r'))
        selectedText = selectedText.Substring(0, selectedText.IndexOf('\r'));
      if (selectedText.Contains<char>('\n'))
        selectedText = selectedText.Substring(0, selectedText.IndexOf('\n'));
      return selectedText;
    }

    private void _currentDocument_TextChanged(object sender, EventArgs e) => this.DoSearch(false);

    public void UpdateSearch()
    {
      if (this._renderer.CurrentResults.Any<SearchResult>())
        this._messageView.IsOpen = false;
      try
      {
        this._strategy = SearchStrategyFactory.Create(this.SearchPattern ?? "", !this.MatchCase, this.WholeWords, this.UseRegex ? SearchMode.RegEx : SearchMode.Normal);
        this.OnSearchOptionsChanged(new SearchOptionsChangedEventArgs(this.SearchPattern, this.MatchCase, this.UseRegex, this.WholeWords));
        this.DoSearch(true);
      }
      catch
      {
      }
    }

    public event EventHandler<SearchOptionsChangedEventArgs> SearchOptionsChanged;

    protected virtual void OnSearchOptionsChanged(SearchOptionsChangedEventArgs e)
    {
      if (this.SearchOptionsChanged == null)
        return;
      this.SearchOptionsChanged((object) this, e);
    }

    public void FindNext()
    {
      SearchResult result = this._renderer.CurrentResults.FindFirstSegmentWithStartAfter(this._textArea.Caret.Offset + 1) ?? this._renderer.CurrentResults.FirstSegment;
      if (result == null)
        return;
      this.SelectResult(result);
    }

    public void FindPrevious()
    {
      SearchResult searchResult = this._renderer.CurrentResults.FindFirstSegmentWithStartAfter(this._textArea.Caret.Offset);
      if (searchResult != null)
        searchResult = this._renderer.CurrentResults.GetPreviousSegment(searchResult);
      if (searchResult == null)
        searchResult = this._renderer.CurrentResults.LastSegment;
      if (searchResult != null && searchResult.StartOffset <= this._textArea.Caret.Offset && this._textArea.Caret.Offset <= searchResult.EndOffset)
        searchResult = this._renderer.CurrentResults.GetPreviousSegment(searchResult) ?? this._renderer.CurrentResults.LastSegment;
      if (searchResult == null)
        return;
      this.SelectResult(searchResult);
    }

    public void SelectResult(SearchResult result)
    {
      this._textArea.Caret.Offset = result.StartOffset;
      this._textArea.Selection = Selection.Create(this._textArea, result.StartOffset, result.EndOffset);
      this._textArea.Caret.BringCaretToView();
    }

    private void _searchPanel_KeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          e.Handled = true;
          this._messageView.IsOpen = false;
          if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            this.FindPrevious();
          else
            this.FindNext();
          if (this._searchTextBox == null)
            break;
          ValidationError validationError = Validation.GetErrors((DependencyObject) this._searchTextBox).FirstOrDefault<ValidationError>();
          if (validationError == null)
            break;
          this._messageView.Content = (object) ("Found errors :  " + validationError.ErrorContent);
          this._messageView.PlacementTarget = (UIElement) this._searchTextBox;
          this._messageView.IsOpen = true;
          break;
        case Key.Escape:
          e.Handled = true;
          this.Close();
          break;
      }
    }

    public void CloseAndRemove()
    {
      this.Close();
      this._textArea.DocumentChanged -= new EventHandler(this._textArea_DocumentChanged);
      if (this._currentDocument == null)
        return;
      this._currentDocument.TextChanged -= new EventHandler(this._currentDocument_TextChanged);
    }

    public void Close()
    {
      bool keyboardFocusWithin = this.IsKeyboardFocusWithin;
      AdornerLayer.GetAdornerLayer((Visual) this._textArea)?.Remove((Adorner) this._adorner);
      this._messageView.IsOpen = false;
      this._textArea.TextView.BackgroundRenderers.Remove((IBackgroundRenderer) this._renderer);
      if (keyboardFocusWithin)
        this._textArea.Focus();
      this.IsClosed = true;
      this._renderer.CurrentResults.Clear();
    }

    private void _textArea_DocumentChanged(object sender, EventArgs e)
    {
      if (this._currentDocument != null)
        this._currentDocument.TextChanged -= new EventHandler(this._currentDocument_TextChanged);
      this._currentDocument = this._textArea.Document;
      if (this._currentDocument == null)
        return;
      this._currentDocument.TextChanged += new EventHandler(this._currentDocument_TextChanged);
      this.DoSearch(false);
    }

    public void DoSearch(bool changeSelection)
    {
      if (this.IsClosed)
        return;
      this._renderer.CurrentResults.Clear();
      if (!string.IsNullOrEmpty(this.SearchPattern))
      {
        int offset = this._textArea.Caret.Offset;
        if (changeSelection)
          this._textArea.ClearSelection();
        foreach (SearchResult result in this._strategy.FindAll((ITextSource) this._textArea.Document, 0, this._textArea.Document.TextLength))
        {
          if (changeSelection && result.StartOffset >= offset)
          {
            this.SelectResult(result);
            changeSelection = false;
          }
          this._renderer.CurrentResults.Add(result);
        }
        if (!this._renderer.CurrentResults.Any<SearchResult>())
        {
          this._messageView.IsOpen = true;
          this._messageView.Content = (object) "No results found";
          this._messageView.PlacementTarget = (UIElement) this._searchTextBox;
        }
        else
          this._messageView.IsOpen = false;
      }
      this._textArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    private void _buttonPrev_Click(object sender, RoutedEventArgs e) => this.FindPrevious();

    private void _buttonNext_Click(object sender, RoutedEventArgs e) => this.FindNext();

    private void _buttonClose_Click(object sender, RoutedEventArgs e) => this.Close();

    private void _buttonFancyMode_Click(object sender, RoutedEventArgs e)
    {
      this._isSearch = !this._isSearch;
      if (this._isSearch)
      {
        this._buttonFancyMode.ImagePath = "replace.png";
        this._replaceTextBox.Visibility = Visibility.Collapsed;
      }
      else
      {
        this._buttonFancyMode.ImagePath = "search.png";
        this._replaceTextBox.Visibility = Visibility.Visible;
      }
    }

    private void _buttonOpenSubMenu_Click(object sender, RoutedEventArgs e) => this._cbSubMenu.IsDropDownOpen = true;

    private void _buttonReplaceSingle_Click(object sender, RoutedEventArgs e)
    {
      SearchResult result = this._renderer.CurrentResults.FindFirstSegmentWithStartAfter(this._textArea.Caret.Offset) ?? this._renderer.CurrentResults.FirstSegment;
      if (result == null)
        return;
      this._replace((ICSharpCode.AvalonEdit.Document.TextSegment) result);
      this.FindNext();
    }

    private void _replace(ICSharpCode.AvalonEdit.Document.TextSegment result)
    {
      if (!this._editor.IsReadOnly)
      {
        this._textArea.Document.Replace(result.StartOffset, result.EndOffset - result.StartOffset, this._replaceTextBox.Text);
        this._textArea.Caret.Offset = result.EndOffset - (result.EndOffset - result.StartOffset - this._replaceTextBox.Text.Length);
      }
      else
      {
        this._messageView.Content = (object) "The document is readonly.";
        this._messageView.PlacementTarget = (UIElement) this._searchTextBox;
        this._messageView.IsOpen = true;
      }
    }

    private void _buttonReplaceAll_Click(object sender, RoutedEventArgs e)
    {
      if (this._editor.IsReadOnly)
      {
        this._messageView.Content = (object) "The document is readonly.";
        this._messageView.PlacementTarget = (UIElement) this._searchTextBox;
        this._messageView.IsOpen = true;
      }
      else
      {
        List<SearchResult> list = this._renderer.CurrentResults.OrderBy<SearchResult, int>((Func<SearchResult, int>) (p => p.StartOffset)).ToList<SearchResult>();
        int num = 0;
        if (list.Count <= 0)
          return;
        this._textArea.Document.BeginUpdate();
        try
        {
          foreach (SearchResult searchResult in list)
          {
            this._textArea.Document.Replace(searchResult.StartOffset + num, searchResult.EndOffset - searchResult.StartOffset, this._replaceTextBox.Text);
            num += this._replaceTextBox.Text.Length - (searchResult.EndOffset - searchResult.StartOffset);
          }
        }
        finally
        {
          this._textArea.Document.EndUpdate();
        }
      }
    }

    private void _replaceTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      this._textArea_PreviewKeyDown(sender, e);
      if (e.Key != Key.Return)
        return;
      this._buttonReplaceSingle_Click(sender, (RoutedEventArgs) null);
      e.Handled = true;
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/avalon/searchpanel.xaml", UriKind.Relative));
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
          this._buttonFancyMode = (FancyButton) target;
          this._buttonFancyMode.Click += new RoutedEventHandler(this._buttonFancyMode_Click);
          break;
        case 2:
          this._border1 = (Border) target;
          break;
        case 3:
          this._cbSubMenu = (LeftComboBox) target;
          break;
        case 4:
          this._searchTextBox = (TextBox) target;
          this._searchTextBox.PreviewKeyDown += new KeyEventHandler(this._textArea_PreviewKeyDown);
          break;
        case 5:
          this._labelFind = (TextBlock) target;
          break;
        case 6:
          this._buttonOpenSubMenu = (FancyButton) target;
          this._buttonOpenSubMenu.Click += new RoutedEventHandler(this._buttonOpenSubMenu_Click);
          break;
        case 7:
          this._buttonPrev = (FancyButton) target;
          this._buttonPrev.Click += new RoutedEventHandler(this._buttonPrev_Click);
          break;
        case 8:
          this._buttonNext = (FancyButton) target;
          this._buttonNext.Click += new RoutedEventHandler(this._buttonNext_Click);
          break;
        case 9:
          this._border2 = (Border) target;
          break;
        case 10:
          this._replaceTextBox = (TextBox) target;
          this._replaceTextBox.PreviewKeyDown += new KeyEventHandler(this._replaceTextBox_PreviewKeyDown);
          break;
        case 11:
          this._labelReplace = (TextBlock) target;
          break;
        case 12:
          this._buttonReplaceSingle = (FancyButton) target;
          this._buttonReplaceSingle.Click += new RoutedEventHandler(this._buttonReplaceSingle_Click);
          break;
        case 13:
          this._buttonReplaceAll = (FancyButton) target;
          this._buttonReplaceAll.Click += new RoutedEventHandler(this._buttonReplaceAll_Click);
          break;
        case 14:
          this._buttonClose = (FancyButton) target;
          this._buttonClose.Click += new RoutedEventHandler(this._buttonClose_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public class SearchPanelAdorner : Adorner
    {
      private readonly SearchPanel _panel;

      public SearchPanelAdorner(TextArea textArea, SearchPanel panel)
        : base((UIElement) textArea)
      {
        this._panel = panel;
        this.AddVisualChild((Visual) panel);
      }

      protected override int VisualChildrenCount => 1;

      protected override Visual GetVisualChild(int index)
      {
        if (index != 0)
          throw new ArgumentOutOfRangeException();
        return (Visual) this._panel;
      }

      protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
      {
        this._panel.Arrange(new Rect(new Point(0.0, 0.0), finalSize));
        return new System.Windows.Size(this._panel.ActualWidth, this._panel.ActualHeight);
      }
    }

    public class SearchResultBackgroundRenderer : IBackgroundRenderer
    {
      private readonly TextSegmentCollection<SearchResult> _currentResults = new TextSegmentCollection<SearchResult>();
      private Brush _markerBrush;
      private Pen _markerPen;

      public SearchResultBackgroundRenderer()
      {
        this._markerBrush = (Brush) Brushes.LightGreen;
        this._markerPen = new Pen(this._markerBrush, 1.0);
      }

      public TextSegmentCollection<SearchResult> CurrentResults => this._currentResults;

      public Brush MarkerBrush
      {
        get => this._markerBrush;
        set
        {
          this._markerBrush = value;
          this._markerPen = new Pen(this._markerBrush, 1.0);
        }
      }

      public KnownLayer Layer => KnownLayer.Selection;

      public void Draw(TextView textView, DrawingContext drawingContext)
      {
        if (textView == null)
          throw new ArgumentNullException(nameof (textView));
        if (drawingContext == null)
          throw new ArgumentNullException(nameof (drawingContext));
        if (this._currentResults == null || !textView.VisualLinesValid)
          return;
        ReadOnlyCollection<VisualLine> visualLines = textView.VisualLines;
        if (visualLines.Count == 0)
          return;
        int offset = visualLines.First<VisualLine>().FirstDocumentLine.Offset;
        int endOffset = visualLines.Last<VisualLine>().LastDocumentLine.EndOffset;
        foreach (SearchResult overlappingSegment in this._currentResults.FindOverlappingSegments(offset, endOffset - offset))
        {
          BackgroundGeometryBuilder backgroundGeometryBuilder = new BackgroundGeometryBuilder();
          backgroundGeometryBuilder.AlignToMiddleOfPixels = true;
          backgroundGeometryBuilder.AddSegment(textView, (ISegment) overlappingSegment);
          Geometry geometry = backgroundGeometryBuilder.CreateGeometry();
          if (geometry != null)
            drawingContext.DrawGeometry(this._markerBrush, this._markerPen, geometry);
        }
      }
    }
  }
}
