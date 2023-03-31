// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.GrfShellExplorer.PreviewTabs.PreviewAct
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF;
using GRF.Core;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace ActEditor.Tools.GrfShellExplorer.PreviewTabs
{
  public partial class PreviewAct : FilePreviewTab, IDisposable, IComponentConnector
  {
    private readonly ManualResetEvent _actThreadHandle = new ManualResetEvent(false);
    private readonly List<FancyButton> _fancyButtons;
    private readonly object _lockAnimation = new object();
    private readonly Stopwatch _watch = new Stopwatch();
    private Act _act;
    private int _actThreadSleepDelay = 100;
    private int _actionIndex = -1;
    private bool _changedAnimationIndex;
    private int _frameIndex;
    private bool _isRunning = true;
    private bool _stopAnimation;
    private bool _threadIsEnabled = true;
    internal Label _labelHeader;
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
    internal ScrollViewer _scrollViewer;
    internal DockPanel _dockPanelImages;
    internal System.Windows.Controls.Image _imagePreview;
    internal MenuItem _menuItemImageExport;
    private bool _contentLoaded;

    public PreviewAct()
    {
      this.InitializeComponent();
      this._imagePreview.Dispatch<System.Windows.Controls.Image>((Action<System.Windows.Controls.Image>) (p => p.SetValue(RenderOptions.BitmapScalingModeProperty, (object) Configuration.BestAvailableScaleMode)));
      this._imagePreview.Dispatch<System.Windows.Controls.Image>((Action<System.Windows.Controls.Image>) (p => p.SetValue(RenderOptions.EdgeModeProperty, (object) EdgeMode.Aliased)));
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
        this.IsVisibleChanged += (DependencyPropertyChangedEventHandler) ((e, a) => this._enableActThread = this.IsVisible);
        this.Dispatcher.ShutdownStarted += (EventHandler) delegate
        {
          this._isRunning = false;
          this._enableActThread = true;
        };
        new Thread(new ThreadStart(this._actAnimationThread))
        {
          Name = "GrfEditor - Sprite animation update thread"
        }.Start();
      }
      catch
      {
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

    public ScrollViewer ScrollViewer => this._scrollViewer;

    private void _actAnimationThread()
    {
      while (this._isRunning)
      {
        this._watch.Reset();
        this._watch.Start();
        lock (this._lockAnimation)
        {
          if (!this._stopAnimation)
            this._displayNextFrame();
        }
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

    protected override void _load(FileEntry entry)
    {
      byte[] decompressedData1 = entry.GetDecompressedData();
      string actRelativePath = entry.RelativePath;
      this._labelHeader.Dispatch<Label, object>((Func<Label, object>) (p => p.Content = (object) ("Animation : " + Path.GetFileName(actRelativePath))));
      byte[] decompressedData2;
      try
      {
        try
        {
          decompressedData2 = this._grfData.FileTable[actRelativePath.Replace(".act", ".spr")].GetDecompressedData();
        }
        catch
        {
          List<string> list = this._grfData.FileTable.Files.Where<string>((Func<string, bool>) (p => p.StartsWith(actRelativePath.Replace(Path.GetExtension(actRelativePath), "")))).ToList<string>();
          list.Remove(actRelativePath);
          if (list.Count == 1)
            decompressedData2 = this._grfData.FileTable[list[0]].GetDecompressedData();
          else
            throw;
        }
      }
      catch
      {
        ErrorHandler.HandleException("Couldn't find the corresponding spr file : \n" + actRelativePath.Replace(".act", ".spr"), ErrorLevel.Low);
        return;
      }
      if (this._isCancelRequired())
        return;
      Act act = new Act((MultiType) decompressedData1, (MultiType) decompressedData2);
      if (this._isCancelRequired())
        return;
      List<int> actions = new List<int>();
      for (int index = 0; index < act.NumberOfActions; ++index)
      {
        int num = index;
        actions.Add(num);
      }
      lock (this._lockAnimation)
        this._stopAnimation = true;
      lock (this._lockAnimation)
      {
        this._act = act;
        this._changedAnimationIndex = true;
        this._stopAnimation = false;
      }
      this._comboBoxActionIndex.Dispatcher.Invoke((Delegate) (() => this._comboBoxActionIndex.ItemsSource = (IEnumerable) actions));
      this._comboBoxAnimationIndex.Dispatcher.Invoke((Delegate) (() => this._comboBoxAnimationIndex.ItemsSource = (IEnumerable) act.GetAnimationStrings()));
      this._setDisabledButtons();
      if (this._isCancelRequired())
        return;
      int num1 = (int) this._imagePreview.Dispatch<System.Windows.Controls.Image, VerticalAlignment>((Func<System.Windows.Controls.Image, VerticalAlignment>) (p => p.VerticalAlignment = VerticalAlignment.Top));
      int num2 = (int) this._imagePreview.Dispatch<System.Windows.Controls.Image, HorizontalAlignment>((Func<System.Windows.Controls.Image, HorizontalAlignment>) (p => p.HorizontalAlignment = HorizontalAlignment.Left));
      this._comboBoxActionIndex.Dispatch<ComboBox, int>((Func<ComboBox, int>) (p => p.SelectedIndex = 0));
      int num3 = (int) this._comboBoxActionIndex.Dispatch<ComboBox, Visibility>((Func<ComboBox, Visibility>) (p => p.Visibility = Visibility.Visible));
      int num4 = (int) this._imagePreview.Dispatch<System.Windows.Controls.Image, Visibility>((Func<System.Windows.Controls.Image, Visibility>) (p => p.Visibility = Visibility.Visible));
      int num5 = (int) this._scrollViewer.Dispatch<ScrollViewer, Visibility>((Func<ScrollViewer, Visibility>) (p => p.Visibility = Visibility.Visible));
      int actionIndex = (int) this._comboBoxActionIndex.Dispatcher.Invoke((Delegate) (() => this._comboBoxActionIndex.SelectedIndex));
      if (actionIndex < 0)
        return;
      if ((int) this._act[actionIndex].AnimationSpeed * 25 == 0 || float.IsNaN(this._act[actionIndex].AnimationSpeed))
      {
        if (this._act[actionIndex].Frames[0].Layers[0].SpriteIndex < 0)
        {
          this._imagePreview.Dispatch<System.Windows.Controls.Image, ImageSource>((Func<System.Windows.Controls.Image, ImageSource>) (p => p.Source = (ImageSource) null));
          return;
        }
        this._imagePreview.Dispatch<System.Windows.Controls.Image, ImageSource>((Func<System.Windows.Controls.Image, ImageSource>) (p => p.Source = (ImageSource) this._act.Sprite.Images[this._act[actionIndex].Frames[0].Layers[0].SpriteIndex].Cast<BitmapSource>()));
      }
      else
        this._actThreadSleepDelay = (int) ((double) this._act[actionIndex].AnimationSpeed * 25.0);
      this._enableActThread = true;
    }

    private void _displayNextFrame()
    {
      try
      {
        if (this._actionIndex < 0)
        {
          this._enableActThread = false;
        }
        else
        {
          ++this._frameIndex;
          this._frameIndex = this._frameIndex >= this._act[this._actionIndex].NumberOfFrames ? 0 : this._frameIndex;
          if (this._act[this._actionIndex].Frames[this._frameIndex % this._act[this._actionIndex].NumberOfFrames].NumberOfLayers <= 0)
            this._imagePreview.Dispatch<System.Windows.Controls.Image, ImageSource>((Func<System.Windows.Controls.Image, ImageSource>) (p => p.Source = (ImageSource) null));
          else if (this._act[this._actionIndex].Frames[this._frameIndex % this._act[this._actionIndex].NumberOfFrames].Layers.Where<Layer>((Func<Layer, bool>) (p => p.SpriteIndex >= 0)).ToList<Layer>().Count <= 0)
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
                  ImageSource source = ActImaging.Imaging.GenerateImage(this._act, this._actionIndex, this._frameIndex);
                  this._imagePreview.Margin = new Thickness((double) (int) (10.0 + this._scrollViewer.ActualWidth / 2.0 - (double) source.Dispatcher.Invoke((Delegate) (() => source.Width)) / 2.0), (double) (int) (10.0 + this._scrollViewer.ActualHeight / 2.0 - (double) source.Dispatcher.Invoke((Delegate) (() => source.Height)) / 2.0), 0.0, 0.0);
                  this._imagePreview.Source = source;
                }
                catch
                {
                  this._enableActThread = false;
                  ErrorHandler.HandleException("Unable to load the animation.");
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
        ErrorHandler.HandleException(ex, ErrorLevel.Warning);
      }
      finally
      {
        if (this._stopAnimation)
          this._enableActThread = false;
      }
    }

    private void _comboBoxActionIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        if (this._isCancelRequired() || this._comboBoxActionIndex.SelectedIndex < 0 || this._stopAnimation)
          return;
        int actionIndex = this._comboBoxActionIndex.SelectedIndex;
        int num = actionIndex / 8;
        this._disableEvents();
        this._comboBoxAnimationIndex.SelectedIndex = num;
        this._fancyButton_Click((object) this._fancyButtons.First<FancyButton>((Func<FancyButton, bool>) (p => p.Tag.ToString() == (actionIndex % 8).ToString((IFormatProvider) CultureInfo.InvariantCulture))), (RoutedEventArgs) null);
        this._setDisabledButtons();
        this._enableEvents();
        if (actionIndex < 0)
          return;
        if ((int) this._act[actionIndex].AnimationSpeed * 25 == 0 || float.IsNaN(this._act[actionIndex].AnimationSpeed))
        {
          if (this._act[actionIndex].Frames[0].Layers[0].SpriteIndex < 0)
          {
            this._imagePreview.Source = (ImageSource) null;
            return;
          }
          this._imagePreview.Source = (ImageSource) this._act.Sprite.Images[this._act[actionIndex].Frames[0].Layers[0].SpriteIndex].Cast<BitmapSource>();
        }
        else
          this._actThreadSleepDelay = (int) ((double) this._act[actionIndex].AnimationSpeed * 25.0);
        this._actionIndex = actionIndex;
        this._changedAnimationIndex = true;
        this._enableActThread = true;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex, ErrorLevel.Warning);
      }
    }

    private void _fancyButton_Click(object sender, RoutedEventArgs e)
    {
      int num = this._comboBoxActionIndex.SelectedIndex / 8;
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      ((FancyButton) sender).IsPressed = true;
      this._comboBoxActionIndex.SelectedIndex = num * 8 + int.Parse(((FrameworkElement) sender).Tag.ToString());
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

    private void _setDisabledButtons() => this.Dispatcher.Invoke((Delegate) (() =>
    {
      int num1 = this._comboBoxActionIndex.SelectedIndex / 8;
      if ((num1 + 1) * 8 > this._act.NumberOfActions)
      {
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

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~PreviewAct() => this.Dispose(false);

    protected void Dispose(bool disposing)
    {
      if (!disposing || this._actThreadHandle == null)
        return;
      this._actThreadHandle.Close();
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/tools/grfshellexplorer/previewtabs/previewact.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._labelHeader = (Label) target;
          break;
        case 2:
          this._fancyButton3 = (FancyButton) target;
          this._fancyButton3.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 3:
          this._fancyButton4 = (FancyButton) target;
          this._fancyButton4.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 4:
          this._fancyButton5 = (FancyButton) target;
          this._fancyButton5.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 5:
          this._fancyButton2 = (FancyButton) target;
          this._fancyButton2.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 6:
          this._fancyButton6 = (FancyButton) target;
          this._fancyButton6.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 7:
          this._fancyButton1 = (FancyButton) target;
          this._fancyButton1.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 8:
          this._fancyButton0 = (FancyButton) target;
          this._fancyButton0.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 9:
          this._fancyButton7 = (FancyButton) target;
          this._fancyButton7.Click += new RoutedEventHandler(this._fancyButton_Click);
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
          this._scrollViewer = (ScrollViewer) target;
          break;
        case 13:
          this._dockPanelImages = (DockPanel) target;
          break;
        case 14:
          this._imagePreview = (System.Windows.Controls.Image) target;
          break;
        case 15:
          this._menuItemImageExport = (MenuItem) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
