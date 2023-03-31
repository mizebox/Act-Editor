// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.ActionSelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TokeiLibrary;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class ActionSelector : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty ShowInsertBarProperty = DependencyProperty.Register(nameof (ShowInsertBar), typeof (bool), typeof (ActionSelector));
    private readonly ManualResetEvent _actThreadHandle = new ManualResetEvent(false);
    private readonly List<FancyButton> _fancyButtons;
    private readonly object _lockAnimation = new object();
    private readonly Stopwatch _watch = new Stopwatch();
    private int _actThreadSleepDelay = 100;
    private bool _changedAnimationIndex;
    private int _frameIndex;
    private bool _isRunning = true;
    private int _selectedAction;
    private bool _threadIsEnabled = true;
    internal FancyButton _fancyButton3;
    internal FancyButton _fancyButton4;
    internal FancyButton _fancyButton5;
    internal FancyButton _fancyButton2;
    internal FancyButton _fancyButton6;
    internal FancyButton _fancyButton1;
    internal FancyButton _fancyButton0;
    internal FancyButton _fancyButton7;
    internal Line _line3;
    internal Line _line2;
    internal Line _line1;
    internal Line _line0;
    internal Line _line7;
    internal Line _line6;
    internal Line _line5;
    internal Line _line4;
    internal ComboBox _comboBoxAnimationIndex;
    internal ComboBox _comboBoxActionIndex;
    internal ScrollViewer _scrollViewer;
    internal DockPanel _dockPanelImages;
    internal System.Windows.Controls.Image _imagePreview;
    private bool _contentLoaded;

    public ActionSelector()
    {
      this.InitializeComponent();
      try
      {
        this._fancyButtons = ((IEnumerable<FancyButton>) new FancyButton[8]
        {
          this._fancyButton0,
          this._fancyButton1,
          this._fancyButton2,
          this._fancyButton3,
          this._fancyButton4,
          this._fancyButton5,
          this._fancyButton6,
          this._fancyButton7
        }).ToList<FancyButton>();
        byte[] resource1 = ApplicationManager.GetResource("arrow.png");
        BitmapSource bitmapSource1 = new GrfImage(ref resource1).Cast<BitmapSource>();
        byte[] resource2 = ApplicationManager.GetResource("arrowoblique.png");
        BitmapSource bitmapSource2 = new GrfImage(ref resource2).Cast<BitmapSource>();
        this._fancyButton0.ImageIcon.Source = (ImageSource) bitmapSource1;
        this._fancyButton0.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton0.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 90.0
        };
        this._fancyButton1.ImageIcon.Source = (ImageSource) bitmapSource2;
        this._fancyButton1.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton1.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 90.0
        };
        this._fancyButton2.ImageIcon.Source = (ImageSource) bitmapSource1;
        this._fancyButton2.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton2.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 180.0
        };
        this._fancyButton3.ImageIcon.Source = (ImageSource) bitmapSource2;
        this._fancyButton3.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton3.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 180.0
        };
        this._fancyButton4.ImageIcon.Source = (ImageSource) bitmapSource1;
        this._fancyButton4.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton4.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 270.0
        };
        this._fancyButton5.ImageIcon.Source = (ImageSource) bitmapSource2;
        this._fancyButton5.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton5.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 270.0
        };
        this._fancyButton6.ImageIcon.Source = (ImageSource) bitmapSource1;
        this._fancyButton6.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton6.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 360.0
        };
        this._fancyButton7.ImageIcon.Source = (ImageSource) bitmapSource2;
        this._fancyButton7.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
        this._fancyButton7.ImageIcon.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 360.0
        };
        this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsEnabled = false));
        new Thread(new ThreadStart(this._actAnimationThread))
        {
          Name = "GrfEditor - Sprite animation update thread"
        }.Start();
        this._scrollViewer.Visibility = Visibility.Collapsed;
        this.SizeChanged += (SizeChangedEventHandler) delegate
        {
          if (this.ActualHeight <= 0.0)
            return;
          this._scrollViewer.MaxHeight = this.ActualHeight;
          this._scrollViewer.Visibility = Visibility.Visible;
        };
        this.Loaded += (RoutedEventHandler) delegate
        {
          WpfUtilities.FindParentControl<Window>((DependencyObject) this).Closed += (EventHandler) delegate
          {
            this._isRunning = false;
            this._enableActThread = true;
          };
          if (this.SelectedAction >= this._comboBoxActionIndex.Items.Count)
            return;
          this._comboBoxActionIndex_SelectionChanged((object) null, (SelectionChangedEventArgs) null);
        };
      }
      catch
      {
      }
    }

    public bool ShowInsertBar
    {
      get => (bool) this.GetValue(ActionSelector.ShowInsertBarProperty);
      set => this.SetValue(ActionSelector.ShowInsertBarProperty, (object) value);
    }

    public int SelectedAction
    {
      get => this._selectedAction;
      set
      {
        this._comboBoxActionIndex.SelectedIndex = value;
        this._selectedAction = this._comboBoxActionIndex.SelectedIndex;
      }
    }

    private bool _enableActThread
    {
      set
      {
        if (value)
        {
          if (this._threadIsEnabled)
            return;
          this._actThreadHandle.Set();
        }
        else
        {
          if (!this._threadIsEnabled)
            return;
          this._threadIsEnabled = false;
          this._actThreadHandle.Reset();
        }
      }
    }

    public Act Act { get; set; }

    public void Show(int index)
    {
      this._line0.Visibility = Visibility.Hidden;
      this._line1.Visibility = Visibility.Hidden;
      this._line2.Visibility = Visibility.Hidden;
      this._line3.Visibility = Visibility.Hidden;
      this._line4.Visibility = Visibility.Hidden;
      this._line5.Visibility = Visibility.Hidden;
      this._line6.Visibility = Visibility.Hidden;
      this._line7.Visibility = Visibility.Hidden;
      switch (index % 8)
      {
        case 0:
          this._line0.Visibility = Visibility.Visible;
          break;
        case 1:
          this._line1.Visibility = Visibility.Visible;
          break;
        case 2:
          this._line2.Visibility = Visibility.Visible;
          break;
        case 3:
          this._line3.Visibility = Visibility.Visible;
          break;
        case 4:
          this._line4.Visibility = Visibility.Visible;
          break;
        case 5:
          this._line5.Visibility = Visibility.Visible;
          break;
        case 6:
          this._line6.Visibility = Visibility.Visible;
          break;
        case 7:
          this._line7.Visibility = Visibility.Visible;
          break;
      }
    }

    public event ActIndexSelector.FrameIndexChangedDelegate ActionChanged;

    public void OnActionChanged(int actionindex)
    {
      ActIndexSelector.FrameIndexChangedDelegate actionChanged = this.ActionChanged;
      if (actionChanged == null)
        return;
      actionChanged((object) this, actionindex);
    }

    private void _actAnimationThread()
    {
      while (this._isRunning)
      {
        this._watch.Reset();
        this._watch.Start();
        lock (this._lockAnimation)
          this._displayNextFrame();
        this._watch.Stop();
        int num = (int) ((long) this._actThreadSleepDelay - this._watch.ElapsedMilliseconds);
        Thread.Sleep(num < 0 ? 0 : num);
        if (!this._threadIsEnabled)
        {
          this._actThreadHandle.WaitOne();
          if (!this._threadIsEnabled)
            this._threadIsEnabled = true;
        }
      }
    }

    private void _displayNextFrame()
    {
      try
      {
        if (this.SelectedAction < 0)
          this._enableActThread = false;
        else if (this.Act == null)
        {
          this._enableActThread = false;
        }
        else
        {
          ++this._frameIndex;
          this._frameIndex = this._frameIndex >= this.Act[this.SelectedAction].NumberOfFrames ? 0 : this._frameIndex;
          if (this.Act[this.SelectedAction].Frames[this._frameIndex % this.Act[this.SelectedAction].NumberOfFrames].NumberOfLayers <= 0)
            this._imagePreview.Dispatch<System.Windows.Controls.Image, ImageSource>((Func<System.Windows.Controls.Image, ImageSource>) (p => p.Source = (ImageSource) null));
          else if (this.Act[this.SelectedAction].Frames[this._frameIndex % this.Act[this.SelectedAction].NumberOfFrames].Layers.Where<Layer>((Func<Layer, bool>) (p => p.SpriteIndex >= 0)).ToList<Layer>().Count <= 0)
            this._imagePreview.Dispatch<System.Windows.Controls.Image, ImageSource>((Func<System.Windows.Controls.Image, ImageSource>) (p => p.Source = (ImageSource) null));
          else if (!(bool) this._imagePreview.Dispatcher.Invoke((Delegate) (() =>
          {
            try
            {
              this.Dispatcher.Invoke((Delegate) (() =>
              {
                try
                {
                  if (this._changedAnimationIndex)
                  {
                    this._frameIndex = 0;
                    this._changedAnimationIndex = false;
                  }
                  foreach (GrfImage image in this.Act.Sprite.Images)
                  {
                    if (image.GrfImageType == GrfImageType.Indexed8)
                      image.Palette[3] = (byte) 0;
                  }
                  ImageSource source = ActImaging.Imaging.GenerateImage(this.Act, this.SelectedAction, this._frameIndex);
                  this._imagePreview.Margin = new Thickness((double) (int) (this._scrollViewer.ActualWidth / 2.0 - (double) source.Dispatcher.Invoke((Delegate) (() => source.Width)) / 2.0), (double) (int) (this._scrollViewer.ActualHeight / 2.0 - (double) source.Dispatcher.Invoke((Delegate) (() => source.Height)) / 2.0), 0.0, 0.0);
                  this._imagePreview.Source = source;
                }
                catch
                {
                  this._enableActThread = false;
                }
                finally
                {
                  foreach (GrfImage image in this.Act.Sprite.Images)
                  {
                    if (image.GrfImageType == GrfImageType.Indexed8)
                      image.Palette[3] = byte.MaxValue;
                  }
                }
              }));
              return true;
            }
            catch
            {
              return false;
            }
          })))
            throw new Exception("Unable to load the animation.");
        }
      }
      catch (Exception ex)
      {
        this._enableActThread = false;
      }
    }

    public static RangeObservableCollection<string> GetAnimations(Act act) => new RangeObservableCollection<string>((IEnumerable<string>) act.GetAnimationStrings());

    public void SetAct(Act act)
    {
      this.Act = act;
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      this._comboBoxActionIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxActionIndex.Items.Clear();
      this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxAnimationIndex.Items.Clear();
      int num = (int) Math.Ceiling((double) this.Act.NumberOfActions / 8.0);
      int numberOfActions = this.Act.NumberOfActions;
      this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) ActionSelector.GetAnimations(this.Act);
      if (this.ShowInsertBar)
      {
        ++numberOfActions;
        if ((int) Math.Ceiling((double) numberOfActions / 8.0) > num)
          ((Collection<string>) this._comboBoxAnimationIndex.ItemsSource).Add("Append");
      }
      for (int newItem = 0; newItem < numberOfActions; ++newItem)
        this._comboBoxActionIndex.Items.Add((object) newItem);
      if (numberOfActions == 0)
        return;
      this._comboBoxActionIndex.SelectedIndex = 0;
    }

    private void _fancyButton_Click(object sender, RoutedEventArgs e)
    {
      int num = this._comboBoxActionIndex.SelectedIndex / 8;
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      ((FancyButton) sender).IsPressed = true;
      this._comboBoxActionIndex.SelectedIndex = num * 8 + int.Parse(((FrameworkElement) sender).Tag.ToString());
    }

    private void _comboBoxAnimationIndex_SelectionChanged(
      object sender,
      SelectionChangedEventArgs e)
    {
      if (this._comboBoxAnimationIndex.SelectedIndex < 0)
        return;
      int num = this._comboBoxActionIndex.SelectedIndex % 8;
      if (8 * this._comboBoxAnimationIndex.SelectedIndex + num >= this.Act.NumberOfActions)
        this._comboBoxActionIndex.SelectedIndex = 8 * this._comboBoxAnimationIndex.SelectedIndex;
      else
        this._comboBoxActionIndex.SelectedIndex = 8 * this._comboBoxAnimationIndex.SelectedIndex + num;
    }

    private void _comboBoxActionIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._comboBoxActionIndex.SelectedIndex < 0)
        return;
      int actionIndex = this._comboBoxActionIndex.SelectedIndex;
      int num = actionIndex / 8;
      if (this.ShowInsertBar)
        this.Show(actionIndex);
      this._disableEvents();
      this._comboBoxAnimationIndex.SelectedIndex = num;
      this._fancyButton_Click((object) this._fancyButtons.First<FancyButton>((Func<FancyButton, bool>) (p => p.Tag.ToString() == (actionIndex % 8).ToString((IFormatProvider) CultureInfo.InvariantCulture))), (RoutedEventArgs) null);
      this._setDisabledButtons();
      this.SelectedAction = this._comboBoxActionIndex.SelectedIndex;
      if (this.ShowInsertBar && actionIndex == this.Act.NumberOfActions)
      {
        this.OnActionChanged(this.SelectedAction);
        this._enableEvents();
      }
      else if (actionIndex < 0 || actionIndex >= this.Act.NumberOfActions)
      {
        this._enableEvents();
      }
      else
      {
        if ((int) this.Act[actionIndex].AnimationSpeed * 25 == 0 || float.IsNaN(this.Act[actionIndex].AnimationSpeed))
        {
          if (this.Act[actionIndex].Frames[0].Layers[0].SpriteIndex < 0)
          {
            this._imagePreview.Source = (ImageSource) null;
            return;
          }
          this._imagePreview.Source = (ImageSource) this.Act.Sprite.Images[this.Act[actionIndex].Frames[0].Layers[0].SpriteIndex].Cast<BitmapSource>();
        }
        else
          this._actThreadSleepDelay = (int) ((double) this.Act[actionIndex].AnimationSpeed * 25.0);
        this._changedAnimationIndex = true;
        this._enableActThread = true;
        this.OnActionChanged(this.SelectedAction);
        this._enableEvents();
      }
    }

    private void _disableEvents()
    {
      this._comboBoxAnimationIndex.SelectionChanged -= new SelectionChangedEventHandler(this._comboBoxAnimationIndex_SelectionChanged);
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.Click -= new RoutedEventHandler(this._fancyButton_Click)));
    }

    private void _enableEvents()
    {
      this._comboBoxAnimationIndex.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxAnimationIndex_SelectionChanged);
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.Click += new RoutedEventHandler(this._fancyButton_Click)));
    }

    private void _setDisabledButtons() => this.Dispatcher.Invoke((Delegate) (() =>
    {
      int num1 = this._comboBoxActionIndex.SelectedIndex / 8;
      if ((num1 + 1) * 8 > this.Act.NumberOfActions)
      {
        this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsButtonEnabled = true));
        int num2 = (num1 + 1) * 8 - this.Act.NumberOfActions;
        for (int index = 0; index < num2; ++index)
        {
          int disabledIndex = 7 - index;
          this._fancyButtons.First<FancyButton>((Func<FancyButton, bool>) (p => int.Parse(p.Tag.ToString()) == disabledIndex)).IsButtonEnabled = false;
        }
      }
      else
        this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsButtonEnabled = true));
    }));

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/actionselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._fancyButton3 = (FancyButton) target;
          this._fancyButton3.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 2:
          this._fancyButton4 = (FancyButton) target;
          this._fancyButton4.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 3:
          this._fancyButton5 = (FancyButton) target;
          this._fancyButton5.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 4:
          this._fancyButton2 = (FancyButton) target;
          this._fancyButton2.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 5:
          this._fancyButton6 = (FancyButton) target;
          this._fancyButton6.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 6:
          this._fancyButton1 = (FancyButton) target;
          this._fancyButton1.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 7:
          this._fancyButton0 = (FancyButton) target;
          this._fancyButton0.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 8:
          this._fancyButton7 = (FancyButton) target;
          this._fancyButton7.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 9:
          this._line3 = (Line) target;
          break;
        case 10:
          this._line2 = (Line) target;
          break;
        case 11:
          this._line1 = (Line) target;
          break;
        case 12:
          this._line0 = (Line) target;
          break;
        case 13:
          this._line7 = (Line) target;
          break;
        case 14:
          this._line6 = (Line) target;
          break;
        case 15:
          this._line5 = (Line) target;
          break;
        case 16:
          this._line4 = (Line) target;
          break;
        case 17:
          this._comboBoxAnimationIndex = (ComboBox) target;
          this._comboBoxAnimationIndex.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxAnimationIndex_SelectionChanged);
          break;
        case 18:
          this._comboBoxActionIndex = (ComboBox) target;
          this._comboBoxActionIndex.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxActionIndex_SelectionChanged);
          break;
        case 19:
          this._scrollViewer = (ScrollViewer) target;
          break;
        case 20:
          this._dockPanelImages = (DockPanel) target;
          break;
        case 21:
          this._imagePreview = (System.Windows.Controls.Image) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
