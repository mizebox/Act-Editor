// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Avalon.AvalonLoader
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ActEditor.Core.Avalon
{
  public class AvalonLoader
  {
    public static void Load(ICSharpCode.AvalonEdit.TextEditor editor) => new AvalonDefaultLoading().Attach(editor);

    public static void Select(string preset, ComboBox box) => box.Dispatcher.Invoke((Delegate) (() =>
    {
      try
      {
        object obj = box.Items.Cast<object>().FirstOrDefault<object>((Func<object, bool>) (p => p.ToString() == preset));
        if (obj == null)
          return;
        box.SelectedItem = obj;
      }
      catch
      {
      }
    }));

    public static bool IsWordBorder(ITextSource document, int offset) => TextUtilities.GetNextCaretPosition(document, offset - 1, LogicalDirection.Forward, CaretPositioningMode.WordBorder) == offset;

    public static bool IsWholeWord(ITextSource document, int offsetStart, int offsetEnd) => TextUtilities.GetNextCaretPosition(document, offsetStart - 1, LogicalDirection.Forward, CaretPositioningMode.WordBorder) == offsetStart && TextUtilities.GetNextCaretPosition(document, offsetStart, LogicalDirection.Forward, CaretPositioningMode.WordBorder) == offsetEnd;

    public static string GetWholeWord(TextDocument document, ICSharpCode.AvalonEdit.TextEditor textEditor)
    {
      TextArea textArea = textEditor.TextArea;
      TextView textView = textArea.TextView;
      if (textView == null)
        return (string) null;
      Point point = textArea.TextView.GetVisualPosition(textArea.Caret.Position, VisualYPosition.LineMiddle) - textArea.TextView.ScrollOffset;
      VisualLine visualLine = textView.GetVisualLine(textEditor.TextArea.Caret.Position.Line);
      if (visualLine == null)
        return (string) null;
      int visualColumn1 = visualLine.GetVisualColumn(point, textArea.Selection.EnableVirtualSpace);
      int visualColumn2 = visualLine.VisualLength != visualColumn1 ? visualLine.GetNextCaretPosition(visualColumn1 + 1, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol, textArea.Selection.EnableVirtualSpace) : visualLine.GetNextCaretPosition(visualColumn1, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol, textArea.Selection.EnableVirtualSpace);
      if (visualColumn2 == -1)
        visualColumn2 = 0;
      int visualColumn3 = visualLine.GetNextCaretPosition(visualColumn2, LogicalDirection.Forward, CaretPositioningMode.WordBorderOrSymbol, textArea.Selection.EnableVirtualSpace);
      if (visualColumn3 == -1)
        visualColumn3 = visualLine.VisualLength;
      if (visualColumn1 < visualColumn2 || visualColumn1 > visualColumn3)
        return "";
      int offset1 = visualLine.FirstDocumentLine.Offset;
      int offset2 = visualLine.GetRelativeOffset(visualColumn2) + offset1;
      int num = visualLine.GetRelativeOffset(visualColumn3) + offset1;
      return textEditor.TextArea.Document.GetText(offset2, num - offset2);
    }

    public static ISegment GetWholeWordSegment(TextDocument document, ICSharpCode.AvalonEdit.TextEditor textEditor)
    {
      TextArea textArea = textEditor.TextArea;
      TextView textView = textArea.TextView;
      if (textView == null)
        return (ISegment) null;
      Point point = textArea.TextView.GetVisualPosition(textArea.Caret.Position, VisualYPosition.LineMiddle) - textArea.TextView.ScrollOffset;
      VisualLine visualLine = textView.GetVisualLine(textEditor.TextArea.Caret.Position.Line);
      if (visualLine == null)
        return (ISegment) null;
      int visualColumn1 = visualLine.GetVisualColumn(point, textArea.Selection.EnableVirtualSpace);
      int visualColumn2 = visualLine.VisualLength != visualColumn1 ? visualLine.GetNextCaretPosition(visualColumn1 + 1, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol, textArea.Selection.EnableVirtualSpace) : visualLine.GetNextCaretPosition(visualColumn1, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol, textArea.Selection.EnableVirtualSpace);
      if (visualColumn2 == -1)
        visualColumn2 = 0;
      int visualColumn3 = visualLine.GetNextCaretPosition(visualColumn2, LogicalDirection.Forward, CaretPositioningMode.WordBorderOrSymbol, textArea.Selection.EnableVirtualSpace);
      if (visualColumn3 == -1)
        visualColumn3 = visualLine.VisualLength;
      if (visualColumn1 < visualColumn2 || visualColumn1 > visualColumn3)
        return (ISegment) new SimpleSegment();
      int offset1 = visualLine.FirstDocumentLine.Offset;
      int offset2 = visualLine.GetRelativeOffset(visualColumn2) + offset1;
      int num = visualLine.GetRelativeOffset(visualColumn3) + offset1;
      return (ISegment) new SimpleSegment(offset2, num - offset2);
    }
  }
}
