// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.FrameSelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActImaging;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class FrameSelector : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty ShowPreviewProperty = DependencyProperty.Register(nameof (ShowPreview), typeof (bool), typeof (FrameSelector));
    public static readonly DependencyProperty AllowLastIndexProperty = DependencyProperty.Register(nameof (AllowLastIndex), typeof (bool), typeof (FrameSelector));
    private readonly Line _linePreview = new Line();
    private Act _act;
    private int _actionIndex;
    internal Grid _gridBlocks;
    internal ScrollBar _sbFrameIndex;
    internal ScrollViewer _scrollViewer;
    internal DockPanel _dockPanelImages;
    internal System.Windows.Controls.Image _imagePreview;
    private bool _contentLoaded;

    public FrameSelector()
    {
      this.InitializeComponent();
      this._sbFrameIndex.SmallChange = 1.0;
      this._sbFrameIndex.LargeChange = 1.0;
      this._sbFrameIndex.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._sbFrameIndex_ValueChanged);
      this.Loaded += (RoutedEventHandler) delegate
      {
        if ((double) this.SelectedFrame >= this._sbFrameIndex.Maximum)
          return;
        this._sbFrameIndex_ValueChanged((object) null, (RoutedPropertyChangedEventArgs<double>) null);
      };
    }

    public int SelectedFrame
    {
      get => (int) this._sbFrameIndex.Value;
      set
      {
        this._sbFrameIndex.Value = (double) value;
        if (value != this.Action.NumberOfFrames)
          return;
        this._linePreview.SetValue(Grid.ColumnProperty, (object) (value + 1));
      }
    }

    public GRF.FileFormats.ActFormat.Action Action { get; set; }

    public bool ShowPreview
    {
      get => (bool) this.GetValue(FrameSelector.ShowPreviewProperty);
      set => this.SetValue(FrameSelector.ShowPreviewProperty, (object) value);
    }

    public bool AllowLastIndex
    {
      get => (bool) this.GetValue(FrameSelector.AllowLastIndexProperty);
      set => this.SetValue(FrameSelector.AllowLastIndexProperty, (object) value);
    }

    public event ActIndexSelector.FrameIndexChangedDelegate FrameChanged;

    public void OnFrameChanged(int actionindex)
    {
      ActIndexSelector.FrameIndexChangedDelegate frameChanged = this.FrameChanged;
      if (frameChanged == null)
        return;
      frameChanged((object) this, actionindex);
    }

    private void _sbFrameIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (this.Action == null)
        return;
      int actionindex = (int) Math.Round(this._sbFrameIndex.Value);
      this.SelectedFrame = actionindex;
      if (this.SelectedFrame < this.Action.NumberOfFrames)
      {
        try
        {
          foreach (GrfImage image in this._act.Sprite.Images)
          {
            if (image.GrfImageType == GrfImageType.Indexed8)
              image.Palette[3] = (byte) 0;
          }
          ImageSource source = Imaging.GenerateImage(this._act, this._actionIndex, this.SelectedFrame);
          this._imagePreview.Margin = new Thickness((double) (int) (this._scrollViewer.ActualWidth / 2.0 - (double) source.Dispatcher.Invoke((Delegate) (() => source.Width)) / 2.0), (double) (int) (this._scrollViewer.ActualHeight / 2.0 - (double) source.Dispatcher.Invoke((Delegate) (() => source.Height)) / 2.0), 0.0, 0.0);
          this._imagePreview.Source = source;
        }
        finally
        {
          foreach (GrfImage image in this._act.Sprite.Images)
          {
            if (image.GrfImageType == GrfImageType.Indexed8)
              image.Palette[3] = byte.MaxValue;
          }
        }
      }
      else
        this._imagePreview.Source = (ImageSource) null;
      this._linePreview.SetValue(Grid.ColumnProperty, (object) (actionindex + 1));
      this.OnFrameChanged(actionindex);
    }

    public void Set(Act act, int actionIndex)
    {
      this._act = act;
      this._actionIndex = actionIndex;
      this.Action = act[actionIndex];
      this._sbFrameIndex.Maximum = (double) (this.Action.NumberOfFrames - 1 + (this.AllowLastIndex ? 1 : 0));
      this._gridBlocks.ColumnDefinitions.Clear();
      this._gridBlocks.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(SystemParameters.HorizontalScrollBarButtonWidth)
      });
      for (int index = 0; index < this.Action.NumberOfFrames; ++index)
      {
        Rectangle element = new Rectangle();
        double num = (double) (index % 5) / 10.0 + 0.5;
        this._gridBlocks.ColumnDefinitions.Add(new ColumnDefinition());
        element.Fill = (Brush) new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, (byte) (num * (double) byte.MaxValue), (byte) 0));
        element.VerticalAlignment = VerticalAlignment.Bottom;
        element.Height = 16.0;
        element.SetValue(Grid.ColumnProperty, (object) (index + 1));
        if (this.Action.NumberOfFrames > 1 && (index == 0 || index == this.Action.NumberOfFrames - 1))
          this._gridBlocks.ColumnDefinitions.Last<ColumnDefinition>().Width = new GridLength((166.0 - 2.0 * SystemParameters.HorizontalScrollBarButtonWidth) / ((double) (this.Action.NumberOfFrames - 1) * 2.0));
        this._gridBlocks.Children.Add((UIElement) element);
      }
      this._linePreview.Stretch = Stretch.Fill;
      this._linePreview.Y2 = 1.0;
      this._linePreview.HorizontalAlignment = HorizontalAlignment.Left;
      this._linePreview.Stroke = (Brush) Brushes.Blue;
      this._linePreview.StrokeStartLineCap = PenLineCap.Square;
      this._linePreview.StrokeEndLineCap = PenLineCap.Square;
      if (this._gridBlocks.Children.Contains((UIElement) this._linePreview))
        this._gridBlocks.Children.Remove((UIElement) this._linePreview);
      this._gridBlocks.Children.Add((UIElement) this._linePreview);
      this._linePreview.Visibility = this.ShowPreview ? Visibility.Visible : Visibility.Hidden;
      this._gridBlocks.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(SystemParameters.HorizontalScrollBarButtonWidth)
      });
      this._sbFrameIndex_ValueChanged((object) null, (RoutedPropertyChangedEventArgs<double>) null);
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/frameselector.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._gridBlocks = (Grid) target;
          break;
        case 2:
          this._sbFrameIndex = (ScrollBar) target;
          break;
        case 3:
          this._scrollViewer = (ScrollViewer) target;
          break;
        case 4:
          this._dockPanelImages = (DockPanel) target;
          break;
        case 5:
          this._imagePreview = (System.Windows.Controls.Image) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
