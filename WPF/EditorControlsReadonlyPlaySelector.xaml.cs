// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.ReadonlyPlaySelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Threading;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class ReadonlyPlaySelector : UserControl, ISelector, IComponentConnector
  {
    private bool _eventsEnabled = true;
    private bool _frameChangedEventEnabled = true;
    private bool _handlersEnabled = true;
    private int _frameIndex;
    internal FancyButton _play;
    private bool _contentLoaded;

    public int SelectedFrame
    {
      get => this._frameIndex;
      set
      {
        this._frameIndex = value;
        if (this._frameIndex >= this.Act[this.SelectedAction].NumberOfFrames)
          this._frameIndex = 0;
        if (!this._eventsEnabled || this.Act == null)
          return;
        this.OnFrameChanged(value);
      }
    }

    public Act Act { get; private set; }

    public ReadonlyPlaySelector()
    {
      this.InitializeComponent();
      try
      {
        this._updatePlay();
        this._play.Click += new RoutedEventHandler(this._play_Click);
      }
      catch
      {
      }
      this.MouseEnter += (MouseEventHandler) delegate
      {
        this.Opacity = 1.0;
      };
      this.MouseLeave += (MouseEventHandler) delegate
      {
        this.Opacity = 0.699999988079071;
      };
    }

    public int SelectedAction { get; set; }

    public event ActIndexSelector.FrameIndexChangedDelegate ActionChanged;

    public event ActIndexSelector.FrameIndexChangedDelegate FrameChanged;

    public event ActIndexSelector.FrameIndexChangedDelegate SpecialFrameChanged;

    public void OnSpecialFrameChanged(int actionindex)
    {
      if (!this._handlersEnabled)
        return;
      ActIndexSelector.FrameIndexChangedDelegate specialFrameChanged = this.SpecialFrameChanged;
      if (specialFrameChanged == null)
        return;
      specialFrameChanged((object) this, actionindex);
    }

    public event ReadonlyPlaySelector.FrameIndexChangedDelegate AnimationPlaying;

    public void OnAnimationPlaying(int actionindex)
    {
      ReadonlyPlaySelector.FrameIndexChangedDelegate animationPlaying = this.AnimationPlaying;
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

    private void _play_Click(object sender, RoutedEventArgs e)
    {
      this._play.Dispatch<FancyButton>((Action<FancyButton>) delegate
      {
        this._play.IsPressed = !this._play.IsPressed;
        this._updatePlay();
      });
      if (!this._play.Dispatch<bool>((Func<bool>) (() => this._play.IsPressed)))
        return;
      GrfThread.Start(new System.Action(this._playAnimation));
    }

    public bool IsPlaying() => this._play.Dispatch<bool>((Func<bool>) (() => this._play.IsPressed));

    public void Play()
    {
      if (this.IsPlaying())
        return;
      this._play.Dispatch<FancyButton>((Action<FancyButton>) delegate
      {
        this._play.IsPressed = true;
        this._updatePlay();
      });
      if (!this._play.Dispatch<bool>((Func<bool>) (() => this._play.IsPressed)))
        return;
      GrfThread.Start(new System.Action(this._playAnimation));
    }

    public void Stop() => this._play.Dispatch<FancyButton>((Action<FancyButton>) delegate
    {
      this._play.IsPressed = false;
      this._updatePlay();
    });

    private void _playAnimation()
    {
      Act act = this.Act;
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
            ++num4;
            if (num4 >= num2)
              num4 = -num3;
            if (!this._play.Dispatch<FancyButton, bool>((Func<FancyButton, bool>) (p => p.IsPressed)))
              break;
            this.Dispatch<ReadonlyPlaySelector, int>((Func<ReadonlyPlaySelector, int>) (p => this.SelectedFrame++));
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

    public void Init(Act act, int selectedAction)
    {
      this.SelectedAction = selectedAction;
      this.Act = act;
    }

    public void Update()
    {
      try
      {
        int selectedFrame = this.SelectedFrame;
        bool flag = selectedFrame != 0;
        if (flag)
          this._handlersEnabled = false;
        if (this.Act == null || this.SelectedAction < 0 || this.SelectedAction >= this.Act.NumberOfActions || selectedFrame >= this.Act[this.SelectedAction].NumberOfFrames || !flag)
          return;
        this._handlersEnabled = true;
        this.SelectedFrame = selectedFrame;
        if (this.SelectedFrame == selectedFrame)
          this.OnFrameChanged(selectedFrame);
        else
          this.SelectedFrame = selectedFrame;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/readonlyplayselector.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this._play = (FancyButton) target;
      else
        this._contentLoaded = true;
    }

    public delegate void FrameIndexChangedDelegate(object sender, int actionIndex);
  }
}
