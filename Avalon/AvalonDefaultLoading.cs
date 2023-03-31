// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Avalon.AvalonDefaultLoading
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TokeiLibrary;
using Utilities.Extension;

namespace ActEditor.Core.Avalon
{
  public class AvalonDefaultLoading
  {
    private readonly object _lock = new object();
    private readonly List<string> _toIgnore = new List<string>()
    {
      "\t",
      Environment.NewLine,
      "\n",
      "\r",
      " ",
      ",",
      ".",
      "!",
      "\"",
      "?"
    };
    private string _currentWord;
    private SearchPanel.SearchResultBackgroundRenderer _renderer;
    private TextArea _textArea;
    private TextEditor _textEditor;

    public void Attach(TextEditor editor)
    {
      this._textEditor = editor;
      this._loadAvalon();
    }

    private void _loadAvalon()
    {
      new DispatcherTimer()
      {
        Interval = TimeSpan.FromSeconds(2.0)
      }.Start();
      this._textEditor.Dispatch<TextEditor, double>((Func<TextEditor, double>) (p => p.TextArea.SelectionCornerRadius = 0.0));
      this._textEditor.Dispatch<TextEditor, Pen>((Func<TextEditor, Pen>) (p => p.TextArea.SelectionBorder = new Pen(this._textEditor.TextArea.SelectionBrush, 0.0)));
      this._textEditor.TextArea.SelectionBrush = (Brush) new SolidColorBrush(Color.FromArgb((byte) 160, (byte) 172, (byte) 213, (byte) 254));
      this._textEditor.TextArea.SelectionBorder = new Pen(this._textEditor.TextArea.SelectionBrush, 1.0);
      this._textEditor.TextArea.SelectionForeground = (Brush) new SolidColorBrush(Colors.Black);
      new SearchPanel().Attach(this._textEditor.TextArea, this._textEditor);
      FontFamily fontFamily = this._textEditor.FontFamily;
      double fontSize = this._textEditor.FontSize;
      this._renderer = new SearchPanel.SearchResultBackgroundRenderer()
      {
        MarkerBrush = (Brush) new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 143, byte.MaxValue, (byte) 143))
      };
      this._textEditor.TextArea.Caret.PositionChanged += new EventHandler(this._caret_PositionChanged);
      try
      {
        this._textEditor.FontFamily = new FontFamily("Consolas");
        this._textEditor.FontSize = 12.0;
        if (this._textEditor.FontFamily == null)
        {
          this._textEditor.FontFamily = fontFamily;
          this._textEditor.FontSize = fontSize;
        }
      }
      catch
      {
        this._textEditor.FontFamily = fontFamily;
        this._textEditor.FontSize = fontSize;
      }
      this._textEditor.TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._renderer);
      this._textEditor.TextArea.KeyDown += new KeyEventHandler(this._textArea_KeyDown);
      this._textArea = this._textEditor.TextArea;
    }

    private void _textArea_KeyDown(object sender, KeyEventArgs e)
    {
      if (Keyboard.Modifiers.HasFlags((Enum) (ModifierKeys.Control | ModifierKeys.Shift)) && e.Key == Key.Up)
      {
        this.FindPrevious();
        e.Handled = true;
      }
      if (!Keyboard.Modifiers.HasFlags((Enum) (ModifierKeys.Control | ModifierKeys.Shift)) || e.Key != Key.Down)
        return;
      this.FindNext();
      e.Handled = true;
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
      this._textEditor.TextArea.Caret.PositionChanged -= new EventHandler(this._caret_PositionChanged);
      this._textArea.Caret.Offset = result.StartOffset;
      this._textArea.Selection = Selection.Create(this._textArea, result.StartOffset, result.EndOffset);
      this._textArea.Caret.BringCaretToView();
      this._textEditor.TextArea.Caret.PositionChanged += new EventHandler(this._caret_PositionChanged);
    }

    private void _caret_PositionChanged(object sender, EventArgs e)
    {
      try
      {
        string currentWord = AvalonLoader.GetWholeWord(this._textEditor.TextArea.Document, this._textEditor);
        if (this._textEditor.CaretOffset > 0 && (currentWord.Length <= 0 || !char.IsLetterOrDigit(currentWord[0])))
        {
          char[] chArray = new char[6]
          {
            '{',
            '}',
            '(',
            ')',
            '[',
            ']'
          };
          foreach (char c in chArray)
          {
            if (this._isBefore(c) || this._isAfter(c))
              currentWord = string.Concat((object) c);
          }
        }
        if (this._currentWord != currentWord)
        {
          this._renderer.CurrentResults.Clear();
          this._textArea.TextView.InvalidateLayer(KnownLayer.Selection);
        }
        this._currentWord = currentWord;
        this._updateCurrentWord(currentWord);
      }
      catch
      {
      }
    }

    private bool _isBefore(char c) => (int) this._textEditor.Text[this._textEditor.CaretOffset - 1] == (int) c;

    private bool _isAfter(char c) => this._textEditor.CaretOffset < this._textEditor.Text.Length && (int) this._textEditor.Text[this._textEditor.CaretOffset] == (int) c;

    private int _findBefore(char back, char forward, int indexStart, int current)
    {
      for (int index = indexStart; current != 0 && index > -1; --index)
      {
        if ((int) this._textEditor.Text[index] == (int) forward)
          ++current;
        else if ((int) this._textEditor.Text[index] == (int) back)
          --current;
        if (current == 0)
          return index;
      }
      return -1;
    }

    private int _findAfter(char back, char forward, int indexStart, int current)
    {
      for (int index = indexStart; current != 0 && index < this._textEditor.Text.Length; ++index)
      {
        if ((int) this._textEditor.Text[index] == (int) forward)
          --current;
        else if ((int) this._textEditor.Text[index] == (int) back)
          ++current;
        if (current == 0)
          return index;
      }
      return -1;
    }

    private bool _bracketMatch(char current, char back, char forward)
    {
      if ((int) current == (int) forward && (this._isAfter(current) || this._isBefore(current)))
      {
        this._renderer.CurrentResults.Clear();
        if (this._isAfter(current))
        {
          TextSegmentCollection<SearchResult> currentResults1 = this._renderer.CurrentResults;
          SearchResult searchResult1 = new SearchResult();
          searchResult1.StartOffset = this._textEditor.CaretOffset;
          searchResult1.Length = 1;
          SearchResult searchResult2 = searchResult1;
          currentResults1.Add(searchResult2);
          int before = this._findBefore(back, forward, this._textEditor.CaretOffset - 1, 1);
          if (before >= 0)
          {
            TextSegmentCollection<SearchResult> currentResults2 = this._renderer.CurrentResults;
            SearchResult searchResult3 = new SearchResult();
            searchResult3.StartOffset = before;
            searchResult3.Length = 1;
            SearchResult searchResult4 = searchResult3;
            currentResults2.Add(searchResult4);
          }
        }
        if (this._isBefore(current))
        {
          TextSegmentCollection<SearchResult> currentResults3 = this._renderer.CurrentResults;
          SearchResult searchResult5 = new SearchResult();
          searchResult5.StartOffset = this._textEditor.CaretOffset - 1;
          searchResult5.Length = 1;
          SearchResult searchResult6 = searchResult5;
          currentResults3.Add(searchResult6);
          int before = this._findBefore(back, forward, this._textEditor.CaretOffset - 2, 1);
          if (before >= 0)
          {
            TextSegmentCollection<SearchResult> currentResults4 = this._renderer.CurrentResults;
            SearchResult searchResult7 = new SearchResult();
            searchResult7.StartOffset = before;
            searchResult7.Length = 1;
            SearchResult searchResult8 = searchResult7;
            currentResults4.Add(searchResult8);
          }
        }
        this._textArea.TextView.InvalidateLayer(KnownLayer.Selection);
        return true;
      }
      if ((int) current != (int) back || !this._isAfter(current) && !this._isBefore(current))
        return false;
      this._renderer.CurrentResults.Clear();
      if (this._isBefore(current))
      {
        TextSegmentCollection<SearchResult> currentResults5 = this._renderer.CurrentResults;
        SearchResult searchResult9 = new SearchResult();
        searchResult9.StartOffset = this._textEditor.CaretOffset - 1;
        searchResult9.Length = 1;
        SearchResult searchResult10 = searchResult9;
        currentResults5.Add(searchResult10);
        int after = this._findAfter(back, forward, this._textEditor.CaretOffset, 1);
        if (after >= 0)
        {
          TextSegmentCollection<SearchResult> currentResults6 = this._renderer.CurrentResults;
          SearchResult searchResult11 = new SearchResult();
          searchResult11.StartOffset = after;
          searchResult11.Length = 1;
          SearchResult searchResult12 = searchResult11;
          currentResults6.Add(searchResult12);
        }
      }
      if (this._isAfter(current))
      {
        TextSegmentCollection<SearchResult> currentResults7 = this._renderer.CurrentResults;
        SearchResult searchResult13 = new SearchResult();
        searchResult13.StartOffset = this._textEditor.CaretOffset;
        searchResult13.Length = 1;
        SearchResult searchResult14 = searchResult13;
        currentResults7.Add(searchResult14);
        int after = this._findAfter(back, forward, this._textEditor.CaretOffset + 1, 1);
        if (after >= 0)
        {
          TextSegmentCollection<SearchResult> currentResults8 = this._renderer.CurrentResults;
          SearchResult searchResult15 = new SearchResult();
          searchResult15.StartOffset = after;
          searchResult15.Length = 1;
          SearchResult searchResult16 = searchResult15;
          currentResults8.Add(searchResult16);
        }
      }
      this._textArea.TextView.InvalidateLayer(KnownLayer.Selection);
      return true;
    }

    private void _updateCurrentWord(string currentWord)
    {
      this._renderer.CurrentResults.Clear();
      if (currentWord == null || this._textEditor.CaretOffset == 0 || this._toIgnore.Any<string>((Func<string, bool>) (p => currentWord.Contains(p))))
        return;
      if (currentWord.Length == 1)
      {
        char current = currentWord[0];
        if (this._bracketMatch(current, '{', '}') || this._bracketMatch(current, '(', ')') || this._bracketMatch(current, '[', ']'))
          return;
      }
      if (currentWord == ">")
      {
        int startIndex = this._textEditor.Text.LastIndexOf('<', this._textEditor.CaretOffset);
        int num = this._textEditor.Text.IndexOf('>', startIndex + 1) + 1;
        if (num != this._textEditor.CaretOffset || num <= startIndex)
        {
          this._renderer.CurrentResults.Clear();
          return;
        }
        currentWord = this._textEditor.Text.Substring(startIndex, num - startIndex);
        this._currentWord = currentWord;
      }
      if (currentWord == "<")
      {
        int num1 = this._textEditor.Text.IndexOf('>', this._textEditor.CaretOffset);
        int startIndex = this._textEditor.Text.LastIndexOf('<', num1 - 1);
        if (startIndex != this._textEditor.CaretOffset || num1 <= startIndex)
        {
          this._renderer.CurrentResults.Clear();
          return;
        }
        int num2 = num1 + 1;
        currentWord = this._textEditor.Text.Substring(startIndex, num2 - startIndex);
        this._currentWord = currentWord;
      }
      new Thread((ThreadStart) (() =>
      {
        lock (this._lock)
        {
          try
          {
            if (currentWord != this._currentWord)
              return;
            Thread.Sleep(300);
            if (currentWord != this._currentWord)
              return;
            RegexSearchStrategy strategy = new RegexSearchStrategy(new Regex(Regex.Escape(currentWord), RegexOptions.Compiled), true);
            this._textEditor.Dispatch<TextEditor>((Action<TextEditor>) delegate
            {
              try
              {
                this._renderer.CurrentResults.Clear();
                if (!string.IsNullOrEmpty(currentWord))
                {
                  foreach (SearchResult searchResult in strategy.FindAll((ITextSource) this._textArea.Document, 0, this._textArea.Document.TextLength))
                    this._renderer.CurrentResults.Add(searchResult);
                }
                this._textArea.TextView.InvalidateLayer(KnownLayer.Selection);
              }
              catch
              {
              }
            });
          }
          catch (ArgumentException ex)
          {
            throw new SearchPatternException(ex.Message, (Exception) ex);
          }
        }
      })).Start();
    }
  }
}
