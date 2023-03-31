// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.GenericControls.LinkControl
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace ActEditor.Core.WPF.GenericControls
{
  public partial class LinkControl : UserControl, IComponentConnector
  {
    public static DependencyProperty NormalBrushProperty = DependencyProperty.Register(nameof (NormalBrush), typeof (Brush), typeof (LinkControl), new PropertyMetadata((object) new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 0, (byte) 0, (byte) 254)), new PropertyChangedCallback(LinkControl.OnNormalBrushChanged)));
    public static DependencyProperty MouseOverBrushProperty = DependencyProperty.Register(nameof (MouseOverBrush), typeof (Brush), typeof (LinkControl), new PropertyMetadata((object) new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 0, (byte) 91, byte.MaxValue)), new PropertyChangedCallback(LinkControl.OnMouseOverBrushChanged)));
    public static DependencyProperty HeaderProperty = DependencyProperty.Register(nameof (Header), typeof (string), typeof (LinkControl), new PropertyMetadata(new PropertyChangedCallback(LinkControl.OnHeaderChanged)));
    internal TextBlock _label;
    private bool _contentLoaded;

    public LinkControl()
    {
      this.InitializeComponent();
      this._label.Foreground = this.NormalBrush;
      this.MouseLeftButtonUp += (MouseButtonEventHandler) ((e, a) => this.OnClick());
    }

    public Brush NormalBrush
    {
      get => (Brush) this.GetValue(LinkControl.NormalBrushProperty);
      set => this.SetValue(LinkControl.NormalBrushProperty, (object) value);
    }

    public Brush MouseOverBrush
    {
      get => (Brush) this.GetValue(LinkControl.MouseOverBrushProperty);
      set => this.SetValue(LinkControl.MouseOverBrushProperty, (object) value);
    }

    public string Header
    {
      get => (string) this.GetValue(LinkControl.HeaderProperty);
      set => this.SetValue(LinkControl.HeaderProperty, (object) value);
    }

    public event LinkControl.LinkControlDelegate Click;

    public void OnClick()
    {
      LinkControl.LinkControlDelegate click = this.Click;
      if (click == null)
        return;
      click((object) this);
    }

    private static void OnNormalBrushChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is LinkControl linkControl))
        return;
      linkControl._label.Foreground = (Brush) e.NewValue;
    }

    private static void OnMouseOverBrushChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is LinkControl linkControl))
        return;
      linkControl._label.Text = e.NewValue.ToString();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      Mouse.OverrideCursor = Cursors.Hand;
      this._label.Foreground = this.MouseOverBrush;
      this._label.SetValue(TextBlock.TextDecorationsProperty, (object) TextDecorations.Underline);
      base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      Mouse.OverrideCursor = (Cursor) null;
      this._label.Foreground = this.NormalBrush;
      this._label.SetValue(TextBlock.TextDecorationsProperty, (object) null);
      base.OnMouseLeave(e);
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/genericcontrols/linkcontrol.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this._label = (TextBlock) target;
      else
        this._contentLoaded = true;
    }

    public delegate void LinkControlDelegate(object sender);
  }
}
