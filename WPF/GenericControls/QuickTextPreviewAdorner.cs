// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.GenericControls.QuickTextPreviewAdorner
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ActEditor.Core.WPF.GenericControls
{
  public class QuickTextPreviewAdorner : Adorner
  {
    private readonly TextBlock _block;

    public QuickTextPreviewAdorner(TextBlock block, UIElement adornedElement)
      : base(adornedElement)
    {
      this._block = block;
      this.AddVisualChild((Visual) this._block);
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
      if (index != 0)
        throw new ArgumentOutOfRangeException();
      return (Visual) this._block;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      this._block.Arrange(new Rect(new Point(0.0, 0.0), finalSize));
      return new Size(this._block.ActualWidth, this._block.ActualHeight);
    }
  }
}
