// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.CompactActIndexSelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.GenericControls;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.Image;
using GRF.Threading;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles;
using Utilities.Commands;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class CompactActIndexSelector : UserControl, IComponentConnector
  {
    private readonly List<FancyButton> _fancyButtons;
    private readonly object _lock = new object();
    private bool _eventsEnabled = true;
    private bool _frameChangedEventEnabled = true;
    private bool _handlersEnabled = true;
    private int _pending;
    private Act _act;
    internal FancyButton _fancyButton3;
    internal FancyButton _fancyButton4;
    internal FancyButton _fancyButton5;
    internal FancyButton _fancyButton2;
    internal FancyButton _fancyButton6;
    internal FancyButton _fancyButton1;
    internal FancyButton _fancyButton0;
    internal FancyButton _fancyButton7;
    internal FancyButton _play;
    internal ComboBox _comboBoxAnimationIndex;
    internal ComboBox _comboBoxActionIndex;
    internal ScrollBar _sbFrameIndex;
    internal Border _borderFrameIndex;
    internal ClickSelectTextBox _tbFrameIndex;
    internal TextBox _labelFrameIndex;
    private bool _contentLoaded;

    public CompactActIndexSelector()
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
        this._sbFrameIndex.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this._sbFrameIndex_MouseLeftButtonDown);
        this._sbFrameIndex.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this._sbFrameIndex_MouseLeftButtonUp);
      }
      catch
      {
      }
      try
      {
        this._updatePlay();
        this._play.Click += new RoutedEventHandler(this._play_Click);
        WpfUtilities.AddFocus((FrameworkElement) this._tbFrameIndex);
      }
      catch
      {
      }
      this.MouseDown += new MouseButtonEventHandler(this._actIndexSelector_MouseDown);
      this.Loaded += (RoutedEventHandler) delegate { };
      this.MouseEnter += (MouseEventHandler) delegate
      {
        this.Opacity = 1.0;
      };
      this.MouseLeave += (MouseEventHandler) delegate
      {
        this.Opacity = 0.699999988079071;
      };
    }

    private void _actIndexSelector_MouseDown(object sender, MouseButtonEventArgs e)
    {
    }

    public int SelectedAction { get; set; }

    public int SelectedFrame { get; set; }

    public event CompactActIndexSelector.FrameIndexChangedDelegate ActionChanged;

    public event CompactActIndexSelector.FrameIndexChangedDelegate FrameChanged;

    public event CompactActIndexSelector.FrameIndexChangedDelegate SpecialFrameChanged;

    public void OnSpecialFrameChanged(int actionindex)
    {
      if (!this._handlersEnabled)
        return;
      CompactActIndexSelector.FrameIndexChangedDelegate specialFrameChanged = this.SpecialFrameChanged;
      if (specialFrameChanged == null)
        return;
      specialFrameChanged((object) this, actionindex);
    }

    public event CompactActIndexSelector.FrameIndexChangedDelegate AnimationPlaying;

    public void OnAnimationPlaying(int actionindex)
    {
      CompactActIndexSelector.FrameIndexChangedDelegate animationPlaying = this.AnimationPlaying;
      if (animationPlaying == null)
        return;
      animationPlaying((object) this, actionindex);
    }

    public void OnFrameChanged(int actionindex)
    {
      if (!this._handlersEnabled)
        return;
      if (!this._frameChangedEventEnabled)
      {
        this.OnSpecialFrameChanged(actionindex);
      }
      else
      {
        CompactActIndexSelector.FrameIndexChangedDelegate frameChanged = this.FrameChanged;
        if (frameChanged == null)
          return;
        frameChanged((object) this, actionindex);
      }
    }

    public void OnActionChanged(int actionindex)
    {
      if (!this._handlersEnabled)
        return;
      CompactActIndexSelector.FrameIndexChangedDelegate actionChanged = this.ActionChanged;
      if (actionChanged == null)
        return;
      actionChanged((object) this, actionindex);
    }

    private void _sbFrameIndex_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      lock (this._lock)
        ++this._pending;
      this.OnAnimationPlaying(0);
      GrfThread.Start((System.Action) (() =>
      {
        int num = 20;
        while (num-- > 0)
        {
          if (e.LeftButton == MouseButtonState.Pressed)
            return;
          Thread.Sleep(100);
        }
        lock (this._lock)
          this._pending = 0;
      }));
    }

    public void SetAction(int index)
    {
      if (index >= this._comboBoxActionIndex.Items.Count || index <= -1)
        return;
      this._comboBoxActionIndex.SelectedIndex = index;
    }

    public void SetFrame(int index) => this._sbFrameIndex.Value = (double) index;

    private void _sbFrameIndex_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (this._act == null)
      {
        lock (this._lock)
          --this._pending;
      }
      else
      {
        Point position = e.GetPosition((IInputElement) this._sbFrameIndex);
        bool isLeft = position.X > 0.0 && position.Y > 0.0 && position.Y < this._sbFrameIndex.ActualHeight && position.X < SystemParameters.HorizontalScrollBarButtonWidth;
        bool isRight = position.X > this._sbFrameIndex.ActualWidth - SystemParameters.HorizontalScrollBarButtonWidth && position.Y > 0.0 && position.Y < this._sbFrameIndex.ActualHeight && position.X < this._sbFrameIndex.ActualWidth;
        if (position.X > 0.0 && position.Y > 0.0 && position.X < this._sbFrameIndex.ActualWidth && position.Y < this._sbFrameIndex.ActualHeight)
          this.OnAnimationPlaying(2);
        if (!isLeft && !isRight)
        {
          lock (this._lock)
            --this._pending;
        }
        else
        {
          GrfThread.Start((System.Action) (() =>
          {
            int num = 0;
            while (this.Dispatch<MouseButtonState>((Func<MouseButtonState>) (() => Mouse.LeftButton)) == MouseButtonState.Pressed)
            {
              this._sbFrameIndex.Dispatch<ScrollBar>((Action<ScrollBar>) delegate
              {
                position = e.GetPosition((IInputElement) this._sbFrameIndex);
                isLeft = position.X > 0.0 && position.Y > 0.0 && position.Y < this._sbFrameIndex.ActualHeight && position.X < SystemParameters.HorizontalScrollBarButtonWidth;
                isRight = position.X > this._sbFrameIndex.ActualWidth - SystemParameters.HorizontalScrollBarButtonWidth && position.Y > 0.0 && position.Y < this._sbFrameIndex.ActualHeight && position.X < this._sbFrameIndex.ActualWidth;
              });
              if (isLeft)
              {
                --this.SelectedFrame;
                if (this.SelectedFrame < 0)
                  this.SelectedFrame = this._act[this.SelectedAction].NumberOfFrames - 1;
              }
              if (isRight)
              {
                ++this.SelectedFrame;
                if (this.SelectedFrame >= this._act[this.SelectedAction].NumberOfFrames)
                  this.SelectedFrame = 0;
              }
              this._sbFrameIndex.Dispatch<ScrollBar, double>((Func<ScrollBar, double>) (p => p.Value = (double) this.SelectedFrame));
              Thread.Sleep(num == 0 ? 400 : 50);
              lock (this._lock)
              {
                if (this._pending > 0)
                {
                  --this._pending;
                  break;
                }
              }
              ++num;
            }
          }));
          e.Handled = true;
        }
      }
    }

    private void _play_Click(object sender, RoutedEventArgs e)
    {
      this._play.Dispatch<FancyButton>((Action<FancyButton>) delegate
      {
        this._play.IsPressed = !this._play.IsPressed;
        this._sbFrameIndex.IsEnabled = !this._play.IsPressed;
        this._updatePlay();
      });
      if (!this._play.Dispatch<bool>((Func<bool>) (() => this._play.IsPressed)))
        return;
      GrfThread.Start(new System.Action(this._playAnimation));
    }

    private void _playAnimation()
    {
      Act act = this._act;
      if (act == null)
        this._play_Click((object) null, (RoutedEventArgs) null);
      else if (act[this.SelectedAction].NumberOfFrames <= 1)
        this._play_Click((object) null, (RoutedEventArgs) null);
      else if ((double) act[this.SelectedAction].AnimationSpeed < 0.800000011920929)
      {
        this._play_Click((object) null, (RoutedEventArgs) null);
        ErrorHandler.HandleException("The animation speed is too fast and might cause issues. The animation will not be displayed.", ErrorLevel.NotSpecified);
      }
      else
      {
        Stopwatch stopwatch = new Stopwatch();
        --this.SelectedFrame;
        int num1 = (int) ((double) act[this.SelectedAction].AnimationSpeed * 25.0);
        int num2 = 1;
        int num3 = 0;
        if (num1 <= 50)
        {
          num2 = 1;
          num3 = 1;
        }
        if (num1 <= 25)
        {
          num2 = 1;
          num3 = 2;
        }
        if (num2 + num3 == act[this.SelectedAction].NumberOfFrames)
          ++num2;
        int num4 = -num3;
        try
        {
          this.OnAnimationPlaying(2);
          while (this._play.Dispatch<FancyButton, bool>((Func<FancyButton, bool>) (p => p.IsPressed)))
          {
            stopwatch.Reset();
            stopwatch.Start();
            int millisecondsTimeout = (int) ((double) act[this.SelectedAction].AnimationSpeed * 25.0);
            if ((double) act[this.SelectedAction].AnimationSpeed < 0.800000011920929)
            {
              this._play_Click((object) null, (RoutedEventArgs) null);
              ErrorHandler.HandleException("The animation speed is too fast and might cause issues. The animation will not be displayed.", ErrorLevel.NotSpecified);
              break;
            }
            ++this.SelectedFrame;
            if (this.SelectedFrame >= act[this.SelectedAction].NumberOfFrames)
              this.SelectedFrame = 0;
            if (num4 < 0)
            {
              this._frameChangedEventEnabled = false;
              this.Dispatch<double>((Func<double>) (() => this._sbFrameIndex.Value = (double) this.SelectedFrame));
              this._frameChangedEventEnabled = true;
            }
            else
              this.Dispatch<double>((Func<double>) (() => this._sbFrameIndex.Value = (double) this.SelectedFrame));
            ++num4;
            if (num4 >= num2)
              num4 = -num3;
            if (!this._play.Dispatch<FancyButton, bool>((Func<FancyButton, bool>) (p => p.IsPressed)))
              break;
            stopwatch.Stop();
            Thread.Sleep(millisecondsTimeout);
          }
        }
        finally
        {
          this._frameChangedEventEnabled = true;
          this.OnAnimationPlaying(0);
        }
      }
    }

    private void _updatePlay()
    {
      if (this._play.IsPressed)
      {
        this._play.ImagePath = "stop2.png";
        this._play.ImageIcon.Width = 16.0;
        this._play.ImageIcon.Stretch = Stretch.Fill;
      }
      else
      {
        this._play.ImagePath = "play.png";
        this._play.ImageIcon.Width = 16.0;
        this._play.ImageIcon.Stretch = Stretch.Fill;
      }
    }

    public void Init()
    {
      this.ActionChanged += new CompactActIndexSelector.FrameIndexChangedDelegate(this._frameSelector_ActionChanged);
      this._sbFrameIndex.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._sbFrameIndex_ValueChanged);
      this._tbFrameIndex.TextChanged += new TextChangedEventHandler(this._tbFrameIndex_TextChanged);
      this._sbFrameIndex.SmallChange = 1.0;
      this._sbFrameIndex.LargeChange = 1.0;
    }

    private void _tbFrameIndex_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (!this._eventsEnabled || this._act == null)
        return;
      int result;
      int.TryParse(this._tbFrameIndex.Text, out result);
      if (result > this._act[this.SelectedAction].NumberOfFrames || result < 0)
        result = 0;
      this._sbFrameIndex.Value = (double) result;
    }

    private void _sbFrameIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!this._eventsEnabled || this._act == null)
        return;
      int actionindex = (int) Math.Round(this._sbFrameIndex.Value);
      this._eventsEnabled = false;
      this._sbFrameIndex.Value = (double) actionindex;
      this._tbFrameIndex.Text = actionindex.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      this._eventsEnabled = true;
      this.SelectedFrame = actionindex;
      this.OnFrameChanged(actionindex);
    }

    private void _updateAction()
    {
      if (this._act == null || this.SelectedAction >= this._act.NumberOfActions)
        return;
      this._eventsEnabled = false;
      while (this.SelectedFrame >= this._act[this.SelectedAction].NumberOfFrames && this.SelectedFrame > 0)
        --this.SelectedFrame;
      this._eventsEnabled = true;
      int num1 = this._act[this.SelectedAction].NumberOfFrames - 1;
      int num2 = num1 < 0 ? 0 : num1;
      this._sbFrameIndex.Minimum = 0.0;
      this._sbFrameIndex.Maximum = (double) num2;
      this._labelFrameIndex.Text = "/ " + (object) num2 + " frame" + (num2 > 1 ? (object) "s" : (object) "");
    }

    private void _frameSelector_ActionChanged(object sender, int actionindex)
    {
      this._updateAction();
      this._sbFrameIndex.Value = 0.0;
    }

    public void Load(Act act)
    {
      this._act = act;
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      int selectedIndex = this._comboBoxActionIndex.SelectedIndex;
      this._comboBoxActionIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxActionIndex.Items.Clear();
      this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxAnimationIndex.Items.Clear();
      this._eventsEnabled = false;
      this._eventsEnabled = true;
      int numberOfActions = this._act.NumberOfActions;
      this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) CompactActIndexSelector.GetAnimations(numberOfActions);
      this._comboBoxActionIndex.ItemsSource = (IEnumerable) Enumerable.Range(0, numberOfActions);
      if (numberOfActions != 0)
        this._comboBoxActionIndex.SelectedIndex = 0;
      this._act.VisualInvalidated += (Act.InvalidateVisualDelegate) (s => this.Update());
      this._act.Commands.CommandIndexChanged += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._commands_CommandUndo);
      if (selectedIndex >= this._act.NumberOfActions)
        return;
      this._comboBoxActionIndex.SelectedIndex = selectedIndex;
    }

    private void _commands_CommandUndo(object sender, IActCommand command)
    {
      try
      {
        ActionCommand command1 = this._getCommand<ActionCommand>(command);
        if (command1 != null)
        {
          if (command1.Executed && (command1.Edit == ActionCommand.ActionEdit.CopyAt || command1.Edit == ActionCommand.ActionEdit.InsertAt || command1.Edit == ActionCommand.ActionEdit.ReplaceTo || command1.Edit == ActionCommand.ActionEdit.InsertAt))
            this.SelectedAction = command1.ActionIndexTo;
          if (this.SelectedAction < 0)
            this.SelectedAction = 0;
          if (this.SelectedAction >= this._act.NumberOfActions)
            this.SelectedAction = this._act.NumberOfActions - 1;
          this._updateActionSelection();
        }
        FrameCommand command2 = this._getCommand<FrameCommand>(command);
        if (command2 != null && command2.Executed)
        {
          if (command2.ActionIndexTo == this.SelectedAction && command2.Edit == FrameCommand.FrameEdit.ReplaceTo || command2.ActionIndexTo == this.SelectedAction && command2.Edit == FrameCommand.FrameEdit.Switch || command2.ActionIndexTo == this.SelectedAction && command2.Edit == FrameCommand.FrameEdit.CopyTo)
            this.SelectedFrame = command2.FrameIndexTo;
          else if (command2.ActionIndex == this.SelectedAction && command2.Edit == FrameCommand.FrameEdit.InsertTo)
            this.SelectedFrame = command2.FrameIndex;
          if (this.SelectedFrame != (int) this._sbFrameIndex.Value)
            this._sbFrameIndex.Value = (double) this.SelectedFrame;
        }
        this._updateInfo();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private T _getCommand<T>(IActCommand command) where T : class, IActCommand
    {
      switch (command)
      {
        case ActGroupCommand actGroupCommand:
          return actGroupCommand.Commands.FirstOrDefault<IActCommand>((Func<IActCommand, bool>) (p => p.GetType() == typeof (T))) as T;
        case T _:
          return command as T;
        default:
          return default (T);
      }
    }

    private void _updateActionSelection()
    {
      try
      {
        int selectedAction = this.SelectedAction;
        this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) null;
        this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) CompactActIndexSelector.GetAnimations(this._act.NumberOfActions);
        this._comboBoxActionIndex.ItemsSource = (IEnumerable) null;
        this._comboBoxActionIndex.ItemsSource = (IEnumerable) Enumerable.Range(0, this._act.NumberOfActions);
        if (selectedAction >= this._comboBoxActionIndex.Items.Count)
          this._comboBoxActionIndex.SelectedIndex = this._comboBoxActionIndex.Items.Count - 1;
        this._comboBoxActionIndex.SelectedIndex = selectedAction;
        this.Update();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public static RangeObservableCollection<string> GetAnimations(int actionsCount)
    {
      switch (actionsCount)
      {
        case 8:
          RangeObservableCollection<string> animations1 = new RangeObservableCollection<string>();
          animations1.Add("0 - Idle");
          return animations1;
        case 16:
          RangeObservableCollection<string> animations2 = new RangeObservableCollection<string>();
          animations2.Add("0 - Idle");
          animations2.Add("1 - Walking");
          return animations2;
        case 40:
          RangeObservableCollection<string> animations3 = new RangeObservableCollection<string>();
          animations3.Add("0 - Idle");
          animations3.Add("1 - Walk");
          animations3.Add("2 - Attack");
          animations3.Add("3 - Receiving damage");
          animations3.Add("4 - Die");
          return animations3;
        case 64:
          RangeObservableCollection<string> animations4 = new RangeObservableCollection<string>();
          animations4.Add("0 - Idle");
          animations4.Add("1 - Walking");
          animations4.Add("2 - Attacking1");
          animations4.Add("3 - Receiving damage");
          animations4.Add("4 - Dead");
          animations4.Add("5 - Attacking2");
          animations4.Add("6 - Attacking3");
          animations4.Add("7 - Action");
          return animations4;
        case 72:
          RangeObservableCollection<string> animations5 = new RangeObservableCollection<string>();
          animations5.Add("0 - Idle");
          animations5.Add("1 - Walking");
          animations5.Add("2 - Attacking");
          animations5.Add("3 - Receiving damage");
          animations5.Add("4 - Dead");
          animations5.Add("5 - Special");
          animations5.Add("6 - Perf1");
          animations5.Add("7 - Perf2");
          animations5.Add("8 - Perf3");
          return animations5;
        case 104:
          RangeObservableCollection<string> animations6 = new RangeObservableCollection<string>();
          animations6.Add("0 - Idle");
          animations6.Add("1 - Walking");
          animations6.Add("2 - Sitting");
          animations6.Add("3 - Picking item");
          animations6.Add("4 - After receiving damage");
          animations6.Add("5 - Attacking1");
          animations6.Add("6 - Receiving damage");
          animations6.Add("7 - Freeze1");
          animations6.Add("8 - Dead");
          animations6.Add("9 - Freeze2");
          animations6.Add("10 - Attacking2 (no weapon)");
          animations6.Add("11 - Attacking3 (weapon)");
          animations6.Add("12 - Casting spell");
          return animations6;
        default:
          int num = (int) Math.Ceiling((double) actionsCount / 8.0);
          RangeObservableCollection<string> animations7 = new RangeObservableCollection<string>();
          for (int index = 0; index < num; ++index)
            animations7.Add(index.ToString((IFormatProvider) CultureInfo.InvariantCulture));
          return animations7;
      }
    }

    private void _updateInfo()
    {
      this._play.IsPressed = false;
      this._updatePlay();
      this._updateAction();
    }

    private void _fancyButton_Click(object sender, RoutedEventArgs e)
    {
      int num = this._comboBoxActionIndex.SelectedIndex / 8;
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      ((FancyButton) sender).IsPressed = true;
      this._comboBoxActionIndex.SelectedIndex = num * 8 + int.Parse(((FrameworkElement) sender).Tag.ToString());
    }

    private void _setDisabledButtons() => this.Dispatcher.Invoke((Delegate) (() =>
    {
      int num1 = this._comboBoxActionIndex.SelectedIndex / 8;
      if ((num1 + 1) * 8 > this._act.NumberOfActions)
      {
        this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsButtonEnabled = true));
        int num2 = (num1 + 1) * 8 - this._act.NumberOfActions;
        for (int index = 0; index < num2; ++index)
        {
          int disabledIndex = 7 - index;
          this._fancyButtons.First<FancyButton>((Func<FancyButton, bool>) (p => int.Parse(p.Tag.ToString()) == disabledIndex)).IsButtonEnabled = false;
        }
      }
      else
        this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsButtonEnabled = true));
    }));

    private void _comboBoxAnimationIndex_SelectionChanged(
      object sender,
      SelectionChangedEventArgs e)
    {
      if (this._comboBoxAnimationIndex.SelectedIndex < 0)
        return;
      int num = this._comboBoxActionIndex.SelectedIndex % 8;
      if (8 * this._comboBoxAnimationIndex.SelectedIndex + num >= this._act.NumberOfActions)
        this._comboBoxActionIndex.SelectedIndex = 8 * this._comboBoxAnimationIndex.SelectedIndex;
      else
        this._comboBoxActionIndex.SelectedIndex = 8 * this._comboBoxAnimationIndex.SelectedIndex + num;
    }

    private void _comboBoxActionIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._comboBoxActionIndex.SelectedIndex < 0 || this._comboBoxActionIndex.SelectedIndex >= this._act.NumberOfActions)
        return;
      int actionIndex = this._comboBoxActionIndex.SelectedIndex;
      int num = actionIndex / 8;
      this._disableEvents();
      this._comboBoxAnimationIndex.SelectedIndex = num;
      this._fancyButton_Click((object) this._fancyButtons.First<FancyButton>((Func<FancyButton, bool>) (p => p.Tag.ToString() == (actionIndex % 8).ToString((IFormatProvider) CultureInfo.InvariantCulture))), (RoutedEventArgs) null);
      this._setDisabledButtons();
      this.SelectedAction = this._comboBoxActionIndex.SelectedIndex;
      this.SelectedFrame = 0;
      this.OnActionChanged(this.SelectedAction);
      this._enableEvents();
    }

    public void Update()
    {
      try
      {
        int selectedFrame = this.SelectedFrame;
        bool flag = selectedFrame != 0;
        if (flag)
          this._handlersEnabled = false;
        this._comboBoxActionIndex_SelectionChanged((object) null, (SelectionChangedEventArgs) null);
        if (this._act == null || this.SelectedAction < 0 || this.SelectedAction >= this._act.NumberOfActions || selectedFrame >= this._act[this.SelectedAction].NumberOfFrames || !flag)
          return;
        this._handlersEnabled = true;
        this.SelectedFrame = selectedFrame;
        if (this._sbFrameIndex.Value == (double) selectedFrame)
          this.OnFrameChanged(selectedFrame);
        else
          this._sbFrameIndex.Value = (double) selectedFrame;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
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

    public void Reset()
    {
      this._eventsEnabled = false;
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsButtonEnabled = false));
      this._comboBoxActionIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxActionIndex.Items.Clear();
      this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxAnimationIndex.Items.Clear();
      this._sbFrameIndex.Value = 0.0;
      this._tbFrameIndex.Text = "0";
      this._labelFrameIndex.Text = "/ 0 frame";
      this._sbFrameIndex.Maximum = 0.0;
      this._play.IsPressed = false;
      this._updatePlay();
      this._eventsEnabled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/compactactindexselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
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
          this._play = (FancyButton) target;
          break;
        case 10:
          this._comboBoxAnimationIndex = (ComboBox) target;
          this._comboBoxAnimationIndex.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxAnimationIndex_SelectionChanged);
          break;
        case 11:
          this._comboBoxActionIndex = (ComboBox) target;
          this._comboBoxActionIndex.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxActionIndex_SelectionChanged);
          break;
        case 12:
          this._sbFrameIndex = (ScrollBar) target;
          break;
        case 13:
          this._borderFrameIndex = (Border) target;
          break;
        case 14:
          this._tbFrameIndex = (ClickSelectTextBox) target;
          break;
        case 15:
          this._labelFrameIndex = (TextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void FrameIndexChangedDelegate(object sender, int actionIndex);
  }
}
