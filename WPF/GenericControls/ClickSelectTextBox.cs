// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.GenericControls.ClickSelectTextBox
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TokeiLibrary.Shortcuts;

namespace ActEditor.Core.WPF.GenericControls
{
  public class ClickSelectTextBox : TextBox
  {
    private static readonly Thickness _sharedThickness = new Thickness(3.0, 0.0, 3.0, 1.0);
    private readonly QuickTextPreviewAdorner _adorner;
    private readonly TextBlock _tblock = new TextBlock();

    static ClickSelectTextBox() => ClickSelectTextBox.EventsEnabled = true;

    public ClickSelectTextBox()
    {
      this.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, (Delegate) new MouseButtonEventHandler(ClickSelectTextBox._selectivelyIgnoreMouseButton), true);
      this.AddHandler(UIElement.GotKeyboardFocusEvent, (Delegate) new RoutedEventHandler(ClickSelectTextBox._selectAllText), true);
      this.AddHandler(Control.MouseDoubleClickEvent, (Delegate) new RoutedEventHandler(ClickSelectTextBox._selectAllText), true);
      this.IsUndoEnabled = false;
      this.PreviewKeyDown += new KeyEventHandler(this._clickSelectTextBox_KeyDown);
      this._tblock.Padding = ClickSelectTextBox._sharedThickness;
      this._tblock.IsHitTestVisible = false;
      this._tblock.Visibility = Visibility.Collapsed;
      this._tblock.Background = (Brush) Brushes.White;
      this._tblock.VerticalAlignment = VerticalAlignment.Center;
      this._tblock.TextAlignment = TextAlignment.Right;
      this._tblock.ClipToBounds = true;
      this._adorner = new QuickTextPreviewAdorner(this._tblock, (UIElement) this);
      this.Loaded += (RoutedEventHandler) delegate
      {
        AdornerLayer.GetAdornerLayer((Visual) this)?.Add((Adorner) this._adorner);
        if (ClickSelectTextBox.EventsEnabled)
          return;
        this.Text = this._tblock.Text;
      };
    }

    public static bool EventsEnabled { get; set; }

    public new string Text
    {
      get => (string) this.GetValue(TextBox.TextProperty);
      set
      {
        if (ClickSelectTextBox.EventsEnabled)
        {
          this._tblock.Visibility = Visibility.Collapsed;
          this.SetValue(TextBox.TextProperty, (object) value);
        }
        else
        {
          if (!this.IsLoaded)
            return;
          this._setText(value);
        }
      }
    }

    private void _setText(string value)
    {
      if (this._adorner.Parent == null)
      {
        this._tblock.Text = value;
      }
      else
      {
        if (this._tblock.Visibility != Visibility.Visible || this._tblock.Width <= 0.0)
        {
          this._tblock.Visibility = Visibility.Visible;
          this._tblock.Width = this.ActualWidth;
          this._tblock.Background = ((Panel) this.Parent).Background;
        }
        this._tblock.Text = value;
      }
    }

    private void _clickSelectTextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (!ApplicationShortcut.Is(ApplicationShortcut.Undo) && !ApplicationShortcut.Is(ApplicationShortcut.Redo))
        return;
      UIElement reference = (UIElement) this;
      int num = 0;
      do
      {
        reference = VisualTreeHelper.GetParent((DependencyObject) reference) as UIElement;
        ++num;
      }
      while (reference == null && num < 1000);
      if (reference == null)
        return;
      KeyEventArgs e1 = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, e.Key);
      e1.RoutedEvent = UIElement.KeyDownEvent;
      e1.Source = sender;
      reference.RaiseEvent((RoutedEventArgs) e1);
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
      if (!ClickSelectTextBox.EventsEnabled)
        return;
      base.OnTextChanged(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
      if (e.Data.GetData("ImageIndex") != null)
        e.Effects = DragDropEffects.All;
      else
        base.OnDragOver(e);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
      if (e.Data.GetData("ImageIndex") != null)
        e.Effects = DragDropEffects.All;
      else
        base.OnDragEnter(e);
    }

    private static void _selectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
    {
      DependencyObject reference = (DependencyObject) (e.OriginalSource as UIElement);
      while (true)
      {
        switch (reference)
        {
          case null:
          case TextBox _:
            goto label_3;
          default:
            reference = VisualTreeHelper.GetParent(reference);
            continue;
        }
      }
label_3:
      if (reference == null)
        return;
      TextBox textBox = (TextBox) reference;
      if (textBox.IsKeyboardFocusWithin)
        return;
      textBox.Focus();
      e.Handled = true;
    }

    private static void _selectAllText(object sender, RoutedEventArgs e)
    {
      if (!(e.OriginalSource is TextBox originalSource))
        return;
      originalSource.SelectAll();
    }

    public void UpdateBackground()
    {
      if (this._tblock == null)
        return;
      this._tblock.Background = ((Panel) this.Parent).Background;
    }
  }
}
