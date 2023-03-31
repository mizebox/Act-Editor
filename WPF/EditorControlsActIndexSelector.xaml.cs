// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.ActIndexSelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
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
using Utilities.Extension;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class ActIndexSelector : UserControl, IActIndexSelector, IComponentConnector
  {
    private readonly List<FancyButton> _fancyButtons;
    private readonly object _lock = new object();
    private readonly SoundEffect _se = new SoundEffect();
    private ActEditorWindow _actEditor;
    private bool _eventsEnabled = true;
    private bool _frameChangedEventEnabled = true;
    private bool _handlersEnabled = true;
    private int _pending;
    internal FancyButton _fancyButton3;
    internal FancyButton _fancyButton4;
    internal FancyButton _fancyButton5;
    internal FancyButton _fancyButton2;
    internal FancyButton _fancyButton6;
    internal FancyButton _fancyButton1;
    internal FancyButton _fancyButton0;
    internal FancyButton _fancyButton7;
    internal ComboBox _comboBoxAnimationIndex;
    internal ComboBox _comboBoxActionIndex;
    internal ScrollBar _sbFrameIndex;
    internal Border _borderFrameIndex;
    internal ClickSelectTextBox _tbFrameIndex;
    internal TextBox _labelFrameIndex;
    internal FancyButton _cbSoundEnable;
    internal FancyButton _buttonRenderMode;
    internal ComboBox _cbSound;
    internal FancyButton _play;
    internal Border _borderAnimSpeed;
    internal ClickSelectTextBox _interval;
    private bool _contentLoaded;

    public ActIndexSelector()
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
        this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsButtonEnabled = false));
        this._sbFrameIndex.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this._sbFrameIndex_MouseLeftButtonDown);
        this._sbFrameIndex.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this._sbFrameIndex_MouseLeftButtonUp);
      }
      catch
      {
      }
      try
      {
        this._updatePlay();
        this._cbSound.SelectionChanged += new SelectionChangedEventHandler(this._cbSound_SelectionChanged);
        this._play.Click += new RoutedEventHandler(this._play_Click);
        this._cbSoundEnable.IsPressed = !GrfEditorConfiguration.ActEditorPlaySound;
        System.Action action = (System.Action) (() =>
        {
          GrfEditorConfiguration.ActEditorPlaySound = !this._cbSoundEnable.IsPressed;
          this._cbSoundEnable.ImagePath = GrfEditorConfiguration.ActEditorPlaySound ? "soundOn.png" : "soundOff.png";
          this._cbSoundEnable.ToolTip = GrfEditorConfiguration.ActEditorPlaySound ? (object) "Sounds are currently enabled." : (object) "Sounds are currenty disabled.";
        });
        this._cbSoundEnable.Click += (RoutedEventHandler) delegate
        {
          this._cbSoundEnable.IsPressed = !this._cbSoundEnable.IsPressed;
          action();
        };
        action();
        ((FrameworkElement) this._buttonRenderMode.FindName("_tbIdentifier")).Margin = new Thickness(3.0, 0.0, 0.0, 3.0);
        ((FrameworkElement) ((Panel) ((Decorator) this._buttonRenderMode.FindName("_border")).Child).Children[2]).HorizontalAlignment = HorizontalAlignment.Left;
        ((FrameworkElement) ((Panel) ((Decorator) this._buttonRenderMode.FindName("_border")).Child).Children[2]).Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
        System.Action action2 = (System.Action) (() =>
        {
          bool flag = GrfEditorConfiguration.ActEditorScalingMode == BitmapScalingMode.NearestNeighbor;
          this._buttonRenderMode.ImagePath = flag ? "editor.png" : "ingame.png";
          this._buttonRenderMode.TextHeader = flag ? "Editor" : "Ingame";
          this._buttonRenderMode.IsPressed = !flag;
          this._buttonRenderMode.ToolTip = flag ? (object) "Render mode is currently set to \"Editor\"." : (object) "Render mode is currently set to \"Ingame\".";
        });
        this._buttonRenderMode.Click += (RoutedEventHandler) delegate
        {
          GrfEditorConfiguration.ActEditorScalingMode = GrfEditorConfiguration.ActEditorScalingMode == BitmapScalingMode.NearestNeighbor ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor;
          action2();
          this._actEditor._framePreview.UpdateAndSelect();
        };
        action2();
        WpfUtilities.AddFocus((FrameworkElement) this._tbFrameIndex, (FrameworkElement) this._interval);
      }
      catch
      {
      }
    }

    public int SelectedAction { get; set; }

    public int SelectedFrame { get; set; }

    public event ActIndexSelector.FrameIndexChangedDelegate ActionChanged;

    public event ActIndexSelector.FrameIndexChangedDelegate FrameChanged;

    public event ActIndexSelector.FrameIndexChangedDelegate SpecialFrameChanged;

    public bool IsPlaying { get; private set; }

    public void OnSpecialFrameChanged(int actionindex)
    {
      if (!this._handlersEnabled)
        return;
      ActIndexSelector.FrameIndexChangedDelegate specialFrameChanged = this.SpecialFrameChanged;
      if (specialFrameChanged == null)
        return;
      specialFrameChanged((object) this, actionindex);
    }

    public event ActIndexSelector.FrameIndexChangedDelegate AnimationPlaying;

    public void OnAnimationPlaying(int actionindex)
    {
      ActIndexSelector.FrameIndexChangedDelegate animationPlaying = this.AnimationPlaying;
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
        ActIndexSelector.FrameIndexChangedDelegate frameChanged = this.FrameChanged;
        if (frameChanged == null)
          return;
        frameChanged((object) this, actionindex);
      }
    }

    public void OnActionChanged(int actionindex)
    {
      if (!this._handlersEnabled)
        return;
      ActIndexSelector.FrameIndexChangedDelegate actionChanged = this.ActionChanged;
      if (actionChanged == null)
        return;
      actionChanged((object) this, actionindex);
    }

    private void _cbSound_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!this._eventsEnabled)
        return;
      if (this._cbSound.SelectedIndex == this._cbSound.Items.Count - 1)
      {
        InputDialog inputDialog = new InputDialog("New sound file name", "New sound", "atk", false);
        inputDialog.Owner = WpfUtilities.TopWindow;
        bool? nullable = inputDialog.ShowDialog();
        if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
        {
          if (inputDialog.Input == "")
            return;
          this._actEditor.Act.Commands.InsertSoundId(inputDialog.Input, this._actEditor.Act.SoundFiles.Count);
          this._eventsEnabled = false;
          this._reloadSound();
          this._eventsEnabled = true;
          this._cbSound.SelectedIndex = this._actEditor.Act.SoundFiles.Count;
        }
        else
          this._cbSound.SelectedIndex = this._actEditor.Act[this.SelectedAction, this.SelectedFrame].SoundId + 1;
      }
      else
        this._actEditor.Act.Commands.SetSoundId(this._actEditor.SelectedAction, this._actEditor.SelectedFrame, this._cbSound.SelectedIndex - 1);
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
      if (this._actEditor.Act == null)
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
                  this.SelectedFrame = this._actEditor.Act[this.SelectedAction].NumberOfFrames - 1;
              }
              if (isRight)
              {
                ++this.SelectedFrame;
                if (this.SelectedFrame >= this._actEditor.Act[this.SelectedAction].NumberOfFrames)
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
      Act act = this._actEditor.Act;
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
          this.Dispatch<ActIndexSelector, bool>((Func<ActIndexSelector, bool>) (p => p._actEditor._layerEditor.IsHitTestVisible = false));
          this.OnAnimationPlaying(2);
          this.IsPlaying = true;
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
            if (!this._cbSoundEnable.Dispatch<FancyButton, bool>((Func<FancyButton, bool>) (p => p.IsPressed)))
            {
              int soundId = act[this.SelectedAction, this.SelectedFrame].SoundId;
              if (soundId > -1 && soundId < act.SoundFiles.Count)
              {
                string soundFile = act.SoundFiles[soundId];
                if (soundFile.GetExtension() == null)
                  soundFile += ".wav";
                byte[] data = this._actEditor.MetaGrf.GetData("data\\wav\\" + soundFile);
                if (data != null)
                {
                  try
                  {
                    this._se.Play(data);
                  }
                  catch (Exception ex)
                  {
                    this._cbSoundEnable.Dispatch<FancyButton>((Action<FancyButton>) (p => p.OnClick((RoutedEventArgs) null)));
                    ErrorHandler.HandleException(ex);
                  }
                }
              }
            }
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
          this.IsPlaying = false;
          this._frameChangedEventEnabled = true;
          this.Dispatch<ActIndexSelector, bool>((Func<ActIndexSelector, bool>) (p => p._actEditor._layerEditor.IsHitTestVisible = true));
          this.OnAnimationPlaying(0);
          this._sbFrameIndex.Dispatch<ScrollBar, bool>((Func<ScrollBar, bool>) (p => p.IsEnabled = true));
        }
      }
    }

    private void _updatePlay()
    {
      ((FrameworkElement) this._play.FindName("_tbIdentifier")).Margin = new Thickness(3.0, 0.0, 0.0, 3.0);
      ((FrameworkElement) ((Panel) ((Decorator) this._play.FindName("_border")).Child).Children[2]).HorizontalAlignment = HorizontalAlignment.Left;
      ((FrameworkElement) ((Panel) ((Decorator) this._play.FindName("_border")).Child).Children[2]).Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
      if (this._play.IsPressed)
      {
        this._play.ImagePath = "stop2.png";
        this._play.TextHeader = "Stop";
      }
      else
      {
        this._play.ImagePath = "play.png";
        this._play.TextHeader = "Play";
      }
    }

    public void Init(ActEditorWindow actEditor)
    {
      this._actEditor = actEditor;
      this._actEditor.ActLoaded += new ActEditorWindow.ActEditorEventDelegate(this._actEditor_ActLoaded);
      this.ActionChanged += new ActIndexSelector.FrameIndexChangedDelegate(this._frameSelector_ActionChanged);
      this._sbFrameIndex.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._sbFrameIndex_ValueChanged);
      this._tbFrameIndex.TextChanged += new TextChangedEventHandler(this._tbFrameIndex_TextChanged);
      this._sbFrameIndex.SmallChange = 1.0;
      this._sbFrameIndex.LargeChange = 1.0;
    }

    private void _tbFrameIndex_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (!this._eventsEnabled)
        return;
      int result;
      int.TryParse(this._tbFrameIndex.Text, out result);
      if (result > this._actEditor.Act[this.SelectedAction].NumberOfFrames || result < 0)
        result = 0;
      this._sbFrameIndex.Value = (double) result;
    }

    private void _sbFrameIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!this._eventsEnabled || this._actEditor.Act == null)
        return;
      int num = (int) Math.Round(this._sbFrameIndex.Value);
      this._eventsEnabled = false;
      this._sbFrameIndex.Value = (double) num;
      this._tbFrameIndex.Text = num.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      this._cbSound.SelectedIndex = this._actEditor.Act[this._actEditor.SelectedAction, num].SoundId + 1;
      this._eventsEnabled = true;
      this.SelectedFrame = num;
      this.OnFrameChanged(num);
    }

    private void _updateAction()
    {
      if (this._actEditor.Act == null || this.SelectedAction >= this._actEditor.Act.NumberOfActions)
        return;
      this._eventsEnabled = false;
      while (this._actEditor.SelectedFrame >= this._actEditor.Act[this._actEditor.SelectedAction].NumberOfFrames && this._actEditor.SelectedFrame > 0)
        --this._actEditor.SelectedFrame;
      int num1 = this._actEditor.Act[this._actEditor.SelectedAction, this._actEditor.SelectedFrame].SoundId;
      if (num1 >= this._actEditor.Act.SoundFiles.Count)
        num1 = -1;
      this._reloadSound();
      this._cbSound.SelectedIndex = num1 + 1;
      this._eventsEnabled = true;
      int num2 = this._actEditor.Act[this.SelectedAction].NumberOfFrames - 1;
      int num3 = num2 < 0 ? 0 : num2;
      this._sbFrameIndex.Minimum = 0.0;
      this._sbFrameIndex.Maximum = (double) num3;
      this._labelFrameIndex.Text = "/ " + (object) num3 + " frame" + (num3 > 1 ? (object) "s" : (object) "");
    }

    private void _frameSelector_ActionChanged(object sender, int actionindex)
    {
      this._updateAction();
      this._sbFrameIndex.Value = 0.0;
    }

    private void _actEditor_ActLoaded(object sender)
    {
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      this._comboBoxActionIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxActionIndex.Items.Clear();
      this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) null;
      this._comboBoxAnimationIndex.Items.Clear();
      this._eventsEnabled = false;
      this._reloadSound();
      this._eventsEnabled = true;
      int numberOfActions = this._actEditor.Act.NumberOfActions;
      this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) ActionSelector.GetAnimations(this._actEditor.Act);
      this._comboBoxActionIndex.ItemsSource = (IEnumerable) Enumerable.Range(0, numberOfActions);
      if (numberOfActions != 0)
        this._comboBoxActionIndex.SelectedIndex = 0;
      this._actEditor.Act.VisualInvalidated += (Act.InvalidateVisualDelegate) (s => this.Update());
      this._actEditor.Act.Commands.CommandIndexChanged += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._commands_CommandUndo);
    }

    private void _reloadSound()
    {
      List<DummyStringView> items = new List<DummyStringView>();
      items.Add(new DummyStringView("None"));
      this._actEditor.Act.SoundFiles.ForEach((Action<string>) (p => items.Add(new DummyStringView(p))));
      items.Add(new DummyStringView("Add new..."));
      this._cbSound.ItemsSource = (IEnumerable) items;
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
          if (this.SelectedAction >= this._actEditor.Act.NumberOfActions)
            this.SelectedAction = this._actEditor.Act.NumberOfActions - 1;
          this._updateActionSelection(false, true);
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
        if (this._getCommand<BackupCommand>(command) != null)
          this._updateActionSelection(true, false);
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

    private void _updateActionSelection(bool keepFrameIndex, bool update)
    {
      try
      {
        int selectedAction = this.SelectedAction;
        int selectedFrame = this.SelectedFrame;
        this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) null;
        this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) ActionSelector.GetAnimations(this._actEditor.Act);
        this._comboBoxActionIndex.ItemsSource = (IEnumerable) null;
        this._comboBoxActionIndex.ItemsSource = (IEnumerable) Enumerable.Range(0, this._actEditor.Act.NumberOfActions);
        if (selectedAction >= this._comboBoxActionIndex.Items.Count)
          this._comboBoxActionIndex.SelectedIndex = this._comboBoxActionIndex.Items.Count - 1;
        this._comboBoxActionIndex.SelectedIndex = selectedAction;
        if (keepFrameIndex)
          this._sbFrameIndex.Value = (double) selectedFrame;
        if (!update)
          return;
        this.Update();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _updateInfo()
    {
      this._play.IsPressed = false;
      this._updatePlay();
      this._updateAction();
      this._updateInterval();
    }

    private void _updateInterval() => this._interval.Text = (this._actEditor.Act[this.SelectedAction].AnimationSpeed * 25f).ToString((IFormatProvider) CultureInfo.InvariantCulture);

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
      if ((num1 + 1) * 8 > this._actEditor.Act.NumberOfActions)
      {
        this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsButtonEnabled = true));
        int num2 = (num1 + 1) * 8 - this._actEditor.Act.NumberOfActions;
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
      if (8 * this._comboBoxAnimationIndex.SelectedIndex + num >= this._actEditor.Act.NumberOfActions)
        this._comboBoxActionIndex.SelectedIndex = 8 * this._comboBoxAnimationIndex.SelectedIndex;
      else
        this._comboBoxActionIndex.SelectedIndex = 8 * this._comboBoxAnimationIndex.SelectedIndex + num;
    }

    private void _comboBoxActionIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._comboBoxActionIndex.SelectedIndex < 0 || this._comboBoxActionIndex.SelectedIndex >= this._actEditor.Act.NumberOfActions)
        return;
      int actionIndex = this._comboBoxActionIndex.SelectedIndex;
      int num = actionIndex / 8;
      this._disableEvents();
      this._comboBoxAnimationIndex.SelectedIndex = num;
      this._fancyButton_Click((object) this._fancyButtons.First<FancyButton>((Func<FancyButton, bool>) (p => p.Tag.ToString() == (actionIndex % 8).ToString((IFormatProvider) CultureInfo.InvariantCulture))), (RoutedEventArgs) null);
      this._setDisabledButtons();
      this.SelectedAction = this._comboBoxActionIndex.SelectedIndex;
      this.SelectedFrame = 0;
      this._updateInterval();
      this.OnActionChanged(this.SelectedAction);
      this._enableEvents();
    }

    public void Update()
    {
      try
      {
        int selectedFrame = this.SelectedFrame;
        bool flag1 = this.SelectedAction == this._comboBoxActionIndex.SelectedIndex;
        HashSet<int> selection = new HashSet<int>();
        bool flag2 = selectedFrame != 0;
        if (flag2)
          this._handlersEnabled = false;
        if (flag1)
          selection = new HashSet<int>((IEnumerable<int>) this._actEditor.SelectionEngine.SelectedItems);
        this._comboBoxActionIndex_SelectionChanged((object) null, (SelectionChangedEventArgs) null);
        if (this._actEditor.Act == null)
          return;
        if (this.SelectedAction >= 0 && this.SelectedAction < this._actEditor.Act.NumberOfActions && selectedFrame < this._actEditor.Act[this.SelectedAction].NumberOfFrames && flag2)
        {
          this._handlersEnabled = true;
          this.SelectedFrame = selectedFrame;
          if (this._sbFrameIndex.Value == (double) selectedFrame)
            this.OnFrameChanged(selectedFrame);
          else
            this._sbFrameIndex.Value = (double) selectedFrame;
        }
        if (!flag1)
          return;
        this._actEditor.SelectionEngine.SetSelection(selection);
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

    private void _interval_TextChanged(object sender, TextChangedEventArgs e)
    {
      float result;
      if (this._actEditor.Act == null || this._actEditor.Act.Commands.IsLocked || !float.TryParse(this._interval.Text, out result) || (double) result <= 0.0)
        return;
      this._actEditor.Act.Commands.SetInterval(this.SelectedAction, result / 25f);
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
      this._cbSound.ItemsSource = (IEnumerable) null;
      this._cbSound.Items.Clear();
      this._sbFrameIndex.Value = 0.0;
      this._tbFrameIndex.Text = "0";
      this._interval.Text = "";
      this._labelFrameIndex.Text = "/ 0 frame";
      this._sbFrameIndex.Maximum = 0.0;
      this._play.IsPressed = false;
      this._updatePlay();
      this._eventsEnabled = true;
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/actindexselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
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
          this._comboBoxAnimationIndex = (ComboBox) target;
          this._comboBoxAnimationIndex.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxAnimationIndex_SelectionChanged);
          break;
        case 10:
          this._comboBoxActionIndex = (ComboBox) target;
          this._comboBoxActionIndex.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxActionIndex_SelectionChanged);
          break;
        case 11:
          this._sbFrameIndex = (ScrollBar) target;
          break;
        case 12:
          this._borderFrameIndex = (Border) target;
          break;
        case 13:
          this._tbFrameIndex = (ClickSelectTextBox) target;
          break;
        case 14:
          this._labelFrameIndex = (TextBox) target;
          break;
        case 15:
          this._cbSoundEnable = (FancyButton) target;
          break;
        case 16:
          this._buttonRenderMode = (FancyButton) target;
          break;
        case 17:
          this._cbSound = (ComboBox) target;
          break;
        case 18:
          this._play = (FancyButton) target;
          break;
        case 19:
          this._borderAnimSpeed = (Border) target;
          break;
        case 20:
          this._interval = (ClickSelectTextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void FrameIndexChangedDelegate(object sender, int actionIndex);
  }
}
