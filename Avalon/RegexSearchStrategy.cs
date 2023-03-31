// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Avalon.RegexSearchStrategy
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ActEditor.Core.Avalon
{
  public class RegexSearchStrategy : ISearchStrategy, IEquatable<ISearchStrategy>
  {
    private readonly bool _matchWholeWords;
    private readonly Regex _searchPattern;

    public RegexSearchStrategy(Regex searchPattern, bool matchWholeWords)
    {
      this._searchPattern = searchPattern != null ? searchPattern : throw new ArgumentNullException(nameof (searchPattern));
      this._matchWholeWords = matchWholeWords;
    }

    public IEnumerable<ISearchResult> FindAll(ITextSource document, int offset, int length)
    {
      int endOffset = offset + length;
      foreach (Match result in this._searchPattern.Matches(document.Text))
      {
        int resultEndOffset = result.Length + result.Index;
        if (offset <= result.Index && endOffset >= resultEndOffset && (!this._matchWholeWords || AvalonLoader.IsWordBorder(document, result.Index) && AvalonLoader.IsWordBorder(document, resultEndOffset)))
        {
          SearchResult searchResult = new SearchResult();
          searchResult.StartOffset = result.Index;
          searchResult.Length = result.Length;
          searchResult.Data = result;
          yield return (ISearchResult) searchResult;
        }
      }
    }

    public ISearchResult FindNext(ITextSource document, int offset, int length) => this.FindAll(document, offset, length).FirstOrDefault<ISearchResult>();

    public bool Equals(ISearchStrategy other) => other == this;
  }
}
