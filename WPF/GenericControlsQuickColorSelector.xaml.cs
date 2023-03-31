// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.GenericControls.QuickColorSelector
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ColorPicker;
using ColorPicker.Sliders;
using GRF.Image;
using GrfToWpfBridge;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;

namespace ActEditor.Core.WPF.GenericControls
{
  public partial class QuickColorSelector : UserControl, IComponentConnector
  {
    private static bool _isShown;
    private static readonly Brush _sharedGridBackground;
    private static readonly HashSet<char> _allowed = new HashSet<char>()
    {
      'a',
      'b',
      'c',
      'd',
      'e',
      'f'
    };
    private Point _oldPosition;
    private ConfigAskerSetting _setting;
    internal Border _border;
    internal Grid _grid;
    internal Rectangle _previewPanelBg;
    internal Border _borderEnabled;
    internal FancyButton _reset;
    private bool _contentLoaded;

    static QuickColorSelector()
    {
      VisualBrush visualBrush1 = new VisualBrush();
      visualBrush1.Viewport = new Rect(0.0, 0.0, 16.0, 16.0);
      visualBrush1.ViewportUnits = BrushMappingMode.Absolute;
      visualBrush1.TileMode = TileMode.Tile;
      VisualBrush visualBrush2 = visualBrush1;
      System.Windows.Controls.Image image = new System.Windows.Controls.Image()
      {
        Source = (ImageSource) ApplicationManager.PreloadResourceImage("background.png")
      };
      image.SetValue(RenderOptions.BitmapScalingModeProperty, (object) BitmapScalingMode.NearestNeighbor);
      image.Width = 256.0;
      image.Height = 256.0;
      visualBrush2.Visual = (Visual) image;
      QuickColorSelector._sharedGridBackground = (Brush) visualBrush2;
    }

    public QuickColorSelector()
    {
      this.InitializeComponent();
      this._border.MouseEnter += new MouseEventHandler(this._quickColorSelector_MouseEnter);
      this._border.MouseLeave += new MouseEventHandler(this._quickColorSelector_MouseLeave);
      this._previewPanelBg.Fill = (Brush) new SolidColorBrush(Colors.White);
      this._border.MouseDown += new MouseButtonEventHandler(this._quickColorSelector_MouseDown);
      this._border.MouseMove += new MouseEventHandler(this._quickColorSelector_MouseMove);
      this._border.DragEnter += new DragEventHandler(this._quickColorSelector_DragEnter);
      this._border.DragOver += new DragEventHandler(this._quickColorSelector_DragEnter);
      this._border.DragLeave += new DragEventHandler(this._quickColorSelector_DragLeave);
      this._border.Drop += new DragEventHandler(this._quickColorSelector_Drop);
      this._grid.Background = QuickColorSelector._sharedGridBackground;
    }

    public new bool IsEnabled
    {
      get => base.IsEnabled;
      set
      {
        if (value)
        {
          this._borderEnabled.BorderBrush = (Brush) Brushes.Transparent;
          this._borderEnabled.Background = (Brush) Brushes.Transparent;
        }
        else
        {
          SolidColorBrush controlBrush = SystemColors.ControlBrush;
          SolidColorBrush solidColorBrush1 = new SolidColorBrush(Color.FromArgb((byte) 150, controlBrush.Color.R, controlBrush.Color.G, controlBrush.Color.B));
          SolidColorBrush solidColorBrush2 = new SolidColorBrush(Color.FromArgb((byte) 230, controlBrush.Color.R, controlBrush.Color.G, controlBrush.Color.B));
          this._borderEnabled.BorderBrush = (Brush) solidColorBrush1;
          this._borderEnabled.Background = (Brush) solidColorBrush2;
        }
        base.IsEnabled = value;
      }
    }

    public GrfColor InitialColor { get; set; }

    public Color Color
    {
      get => ((SolidColorBrush) this._previewPanelBg.Fill).Color;
      set
      {
        if (value == ((SolidColorBrush) this._previewPanelBg.Fill).Color)
          return;
        this._previewPanelBg.Fill = (Brush) new SolidColorBrush(value);
        this.OnColorChanged(value);
      }
    }

    public event SliderGradient.GradientPickerColorEventHandler ColorChanged;

    public event SliderGradient.GradientPickerColorEventHandler PreviewColorChanged;

    public void OnPreviewColorChanged(Color value)
    {
      SliderGradient.GradientPickerColorEventHandler previewColorChanged = this.PreviewColorChanged;
      if (previewColorChanged == null)
        return;
      previewColorChanged((object) this, value);
    }

    public void OnColorChanged(Color value)
    {
      SliderGradient.GradientPickerColorEventHandler colorChanged = this.ColorChanged;
      if (colorChanged == null)
        return;
      colorChanged((object) this, value);
    }

    private void _quickColorSelector_DragLeave(object sender, DragEventArgs e) => this._border.BorderBrush = (Brush) Brushes.Black;

    private void _quickColorSelector_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetData("GrfColor") != null)
      {
        GrfColor data = e.Data.GetData("GrfColor") as GrfColor;
        if (!(data != (GrfColor) null))
          return;
        this.InitialColor = this.Color.ToGrfColor();
        this._previewPanelBg.Fill = (Brush) new SolidColorBrush(data.ToColor());
        this.OnPreviewColorChanged(data.ToColor());
        this.OnColorChanged(data.ToColor());
      }
      else
      {
        string data = e.Data.GetData("System.String") as string;
        if (!this._isColorFormat(data))
          return;
        GrfColor color = new GrfColor(data);
        this.InitialColor = this.Color.ToGrfColor();
        this._previewPanelBg.Fill = (Brush) new SolidColorBrush(color.ToColor());
        this.OnPreviewColorChanged(color.ToColor());
        this.OnColorChanged(color.ToColor());
      }
    }

    private void _quickColorSelector_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetData("GrfColor") != null)
      {
        e.Effects = DragDropEffects.All;
        this._border.BorderBrush = (Brush) Brushes.Green;
      }
      else if (this._isColorFormat(e.Data.GetData("System.String") as string))
      {
        e.Effects = DragDropEffects.All;
        this._border.BorderBrush = (Brush) Brushes.Green;
      }
      else
        e.Effects = DragDropEffects.None;
    }

    private bool _isValidCharacter(char c) => char.IsDigit(c) || QuickColorSelector._allowed.Contains(char.ToLower(c));

    private bool _isColorFormat(string txt)
    {
      if (txt == null)
        return false;
      if (txt.StartsWith("#"))
        txt = txt.Substring(1);
      else if (txt.StartsWith("0x") || txt.StartsWith("0X"))
        txt = txt.Substring(2);
      return (txt.Length == 6 || txt.Length == 8) && txt.All<char>(new Func<char, bool>(this._isValidCharacter));
    }

    private void _quickColorSelector_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || (e.GetPosition((IInputElement) this).ToGrfPoint() - this._oldPosition.ToGrfPoint()).Lenght <= 4.0)
        return;
      DataObject data = new DataObject();
      data.SetData("GrfColor", (object) this.Color.ToGrfColor());
      data.SetText(this.Color.ToGrfColor().ToHexString());
      int num = (int) DragDrop.DoDragDrop((DependencyObject) this, (object) data, DragDropEffects.All);
    }

    private void _quickColorSelector_MouseDown(object sender, MouseButtonEventArgs e) => this._oldPosition = e.GetPosition((IInputElement) this);

    public void SetColor(Color color) => this._previewPanelBg.Fill = (Brush) new SolidColorBrush(color);

    private void _quickColorSelector_MouseLeave(object sender, MouseEventArgs e)
    {
      this._border.BorderBrush = (Brush) Brushes.Black;
      Mouse.OverrideCursor = (Cursor) null;
    }

    private void _quickColorSelector_MouseEnter(object sender, MouseEventArgs e)
    {
      this._border.BorderBrush = (Brush) Brushes.Blue;
      Mouse.OverrideCursor = Cursors.Hand;
    }

    private void _previewPanelBg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => this._previewBackground(this._previewPanelBg);

    private void _previewBackground(Rectangle previewPanel)
    {
      if (QuickColorSelector._isShown)
        return;
      QuickColorSelector._isShown = true;
      PickerDialog dialog = new PickerDialog(this.Color);
      dialog.Owner = WpfUtilities.TopWindow;
      this.InitialColor = this.Color.ToGrfColor();
      Rectangle previewPanelClosure = previewPanel;
      dialog.PickerControl.ColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((s, newColor) =>
      {
        previewPanelClosure.Fill = (Brush) new SolidColorBrush(newColor);
        this.OnPreviewColorChanged(newColor);
      });
      dialog.Closed += (EventHandler) delegate
      {
        QuickColorSelector._isShown = false;
        if (!dialog.DialogResult)
        {
          previewPanel.Fill = (Brush) new SolidColorBrush(dialog.PickerControl.InitialColor);
          this.OnPreviewColorChanged(dialog.PickerControl.InitialColor);
        }
        else
        {
          if (!dialog.DialogResult)
            return;
          this.OnColorChanged(dialog.PickerControl.SelectedColor);
        }
      };
      dialog.Show();
    }

    public void Init(ConfigAskerSetting setting)
    {
      this._setting = setting;
      if (this._setting == null)
        return;
      this._reset.Visibility = this._setting.Get().Replace("0x", "#") == this._setting.Default.Replace("0x", "#") ? Visibility.Collapsed : Visibility.Visible;
      this._setting.PreviewPropertyChanged += new ConfigAskerSetting.ConfigAskerSettingEventHandler(this._setting_PreviewPropertyChanged);
    }

    private void _setting_PreviewPropertyChanged(object sender, string oldvalue, string newvalue) => this._reset.Visibility = this._setting.Default.Replace("0x", "#") == newvalue.Replace("0x", "#") ? Visibility.Collapsed : Visibility.Visible;

    private void _reset_Click(object sender, RoutedEventArgs e)
    {
      if (this._setting == null)
        return;
      this._setting.Set(this._setting.Default);
      this._reset.Visibility = this._setting.IsDefault ? Visibility.Collapsed : Visibility.Visible;
      this.Color = new GrfColor(this._setting.Get()).ToColor();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/genericcontrols/quickcolorselector.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._border = (Border) target;
          break;
        case 2:
          this._grid = (Grid) target;
          break;
        case 3:
          this._previewPanelBg = (Rectangle) target;
          this._previewPanelBg.MouseLeftButtonUp += new MouseButtonEventHandler(this._previewPanelBg_MouseLeftButtonUp);
          break;
        case 4:
          this._borderEnabled = (Border) target;
          break;
        case 5:
          this._reset = (FancyButton) target;
          this._reset.Click += new RoutedEventHandler(this._reset_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
