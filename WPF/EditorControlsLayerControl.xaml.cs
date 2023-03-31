// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.LayerControl
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ActEditor.Core.WPF.GenericControls;
using ColorPicker.Sliders;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.Image;
using GrfToWpfBridge;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Utilities;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class LayerControl : UserControl, IComponentConnector
  {
    public const string PreviewSelectedBrush = "PreviewSelectedBrush";
    public const string SelectedBrush = "SelectedBrush";
    public static double ActualHeightBuffered;
    private static Size _bufferedSize;
    private readonly List<ClickSelectTextBox> _boxes = new List<ClickSelectTextBox>(8);
    private readonly FramePreview _frameEditor;
    private readonly bool _isReference;
    private readonly string _name;
    private readonly StackPanel _sp;
    private readonly ScrollViewer _sv;
    private Act _act;
    private int _actionIndex;
    private bool _eventsEnabled = true;
    private int _frameIndex;
    private bool _hasBuffered;
    private bool _isPreviewSelected;
    private bool _isSelected;
    private int _layerIndex;
    internal Grid _grid;
    internal TextBlock _tbSpriteId;
    internal ClickSelectTextBox _tbSpriteNumber;
    internal ClickSelectTextBox _tbOffsetX;
    internal ClickSelectTextBox _tbOffsetY;
    internal ClickSelectTextBox _tbMirror;
    internal CheckBox _cbMirror;
    internal QuickColorSelector _color;
    internal ClickSelectTextBox _tbScaleX;
    internal ClickSelectTextBox _tbScaleY;
    internal ClickSelectTextBox _tbRotation;
    private bool _contentLoaded;

    static LayerControl()
    {
      GrfColor previewSelectedColor = new GrfColor(byte.MaxValue, byte.MaxValue, (byte) 237, (byte) 122);
      GrfColor selectedColor = new GrfColor(byte.MaxValue, (byte) 222, (byte) 251, (byte) 169);
      BufferedBrushes.Register(nameof (PreviewSelectedBrush), (Func<GrfColor>) (() => previewSelectedColor));
      BufferedBrushes.Register(nameof (SelectedBrush), (Func<GrfColor>) (() => selectedColor));
    }

    public LayerControl(ActEditorWindow actEditor, LayerControlHeader header, string name)
    {
      LayerControl layerControl = this;
      this._frameEditor = actEditor._framePreview;
      this.InitializeComponent();
      this._commonInit();
      this._initReferenceEvents();
      this._isReference = true;
      this._name = name;
      header.SizeChanged += (SizeChangedEventHandler) delegate
      {
        for (int index = 2; index < layerControl._grid.ColumnDefinitions.Count; ++index)
          layerControl._grid.ColumnDefinitions[index].Width = new GridLength(header.Grid.ColumnDefinitions[index].ActualWidth);
      };
      this._saveReference();
      this.Loaded += (RoutedEventHandler) delegate
      {
        LayerControl.ActualHeightBuffered = this.ActualHeight;
      };
    }

    public LayerControl(Act act, ActEditorWindow actEditor, int layerIndex)
    {
      this._act = act;
      this._frameEditor = actEditor._framePreview;
      this._sv = actEditor._layerEditor._sv;
      this._sp = actEditor._layerEditor._sp;
      this.InitializeComponent();
      this._commonInit();
      this._layerIndex = layerIndex;
      this._tbSpriteId.Text = this._layerIndex.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      this._initEvents();
    }

    public Grid Grid => this._grid;

    private Layer _layer => this._act.TryGetLayer(this._actionIndex, this._frameIndex, this._layerIndex);

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        if (this._isSelected == value)
          return;
        this._isSelected = value;
        this._grid.Background = this._isSelected ? BufferedBrushes.GetBrush("SelectedBrush") : (Brush) Brushes.White;
      }
    }

    public bool IsPreviewSelected
    {
      get => this._isPreviewSelected;
      set
      {
        if (this._isPreviewSelected == value)
          return;
        this._isPreviewSelected = value;
        if (this.IsSelected)
          return;
        this._grid.Background = this._isPreviewSelected ? BufferedBrushes.GetBrush("PreviewSelectedBrush") : (Brush) Brushes.White;
      }
    }

    private void _saveReference()
    {
      GrfEditorConfiguration.ConfigAsker.IsAutomaticSaveEnabled = false;
      List<string> list = Methods.StringToList(GrfEditorConfiguration.ConfigAsker["[ActEditor - " + this._name + "]", "0,0,0,false,#FFFFFFFF,1,1,0"]);
      this._tbOffsetX.Text = list[1];
      this._tbOffsetY.Text = list[2];
      this._cbMirror.IsChecked = new bool?(bool.Parse(list[3]));
      this._color.Color = new GrfColor(list[4]).ToColor();
      this._tbScaleX.Text = list[5];
      this._tbScaleY.Text = list[6];
      this._tbRotation.Text = list[7];
      GrfEditorConfiguration.ConfigAsker.IsAutomaticSaveEnabled = true;
    }

    public event LayerControl.LayerPropertyChangedDelegate LayerPropertyChanged;

    public void OnLayerPropertyChanged()
    {
      LayerControl.LayerPropertyChangedDelegate layerPropertyChanged = this.LayerPropertyChanged;
      if (layerPropertyChanged == null)
        return;
      layerPropertyChanged((object) this);
    }

    private void _initReferenceEvents()
    {
      TextChangedEventHandler changedEventHandler1 = (TextChangedEventHandler) ((sender, args) =>
      {
        if (!this._eventsEnabled || !LayerControl._getVal(sender as TextBox, out int _))
          return;
        this._applyCommands();
      });
      TextChangedEventHandler changedEventHandler2 = (TextChangedEventHandler) ((sender, args) =>
      {
        if (!this._eventsEnabled || !LayerControl._getDecimalVal(((TextBox) sender).Text, out float _))
          return;
        this._applyCommands();
      });
      RoutedEventHandler update = (RoutedEventHandler) delegate
      {
        if (!this._eventsEnabled)
          return;
        this._applyCommands();
      };
      this._tbOffsetX.TextChanged += changedEventHandler1;
      this._tbOffsetY.TextChanged += changedEventHandler1;
      this._cbMirror.Checked += update;
      this._cbMirror.Unchecked += update;
      this._color.ColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((e, a) => update((object) null, (RoutedEventArgs) null));
      this._color.PreviewColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((e, a) => update((object) null, (RoutedEventArgs) null));
      this._tbScaleX.TextChanged += changedEventHandler2;
      this._tbScaleY.TextChanged += changedEventHandler2;
      this._tbRotation.TextChanged += changedEventHandler1;
    }

    private void _applyCommands(bool save = true)
    {
      try
      {
        if (this._act == null)
          return;
        this._act.Commands.UndoAll();
        this._act.Commands.Translate(int.Parse(this._tbOffsetX.Text), int.Parse(this._tbOffsetY.Text));
        this._act.Commands.Scale(LayerControl._getDecimalVal(this._tbScaleX.Text), LayerControl._getDecimalVal(this._tbScaleY.Text));
        this._act.Commands.Rotate(int.Parse(this._tbRotation.Text));
        bool? isChecked1 = this._cbMirror.IsChecked;
        if ((!isChecked1.GetValueOrDefault() ? 0 : (isChecked1.HasValue ? 1 : 0)) != 0)
        {
          CommandsHolder commands = this._act.Commands;
          bool? isChecked2 = this._cbMirror.IsChecked;
          int num = !isChecked2.GetValueOrDefault() ? 0 : (isChecked2.HasValue ? 1 : 0);
          commands.SetMirror(num != 0);
        }
        if (!this._color.Color.ToGrfColor().Equals((object) GrfColor.White))
          this._act.Commands.SetColor(this._color.Color.ToGrfColor());
        if (save)
          GrfEditorConfiguration.ConfigAsker["[ActEditor - " + this._name + "]"] = Methods.ListToString(new List<string>()
          {
            "",
            this._tbOffsetX.Text,
            this._tbOffsetY.Text,
            this._cbMirror.IsChecked.ToString(),
            this._color.Color.ToGrfColor().ToHexString(),
            this._tbScaleX.Text,
            this._tbScaleY.Text,
            this._tbRotation.Text
          });
      }
      catch
      {
      }
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update();
    }

    private static float _getDecimalVal(string text)
    {
      float dval;
      LayerControl._getDecimalVal(text, out dval);
      return dval;
    }

    private static bool _getDecimalVal(string text, out float dval)
    {
      if (float.TryParse(text, out dval))
        return true;
      text = text.Replace(",", ".");
      if (float.TryParse(text, out dval))
        return true;
      text = text.Replace(".", ",");
      return float.TryParse(text, out dval);
    }

    private void _commonInit()
    {
      this.IsEnabledChanged += (DependencyPropertyChangedEventHandler) delegate
      {
        this._color.IsEnabled = this.IsEnabled;
      };
      if (this._isReference)
        return;
      double pixels = this._frameEditor.Editor.LayerEditor._sv.ViewportWidth / (double) this._grid.ColumnDefinitions.Count;
      for (int index = 0; index < this._grid.ColumnDefinitions.Count; ++index)
        this._grid.ColumnDefinitions[index].Width = new GridLength(pixels);
    }

    private void _initEvents()
    {
      this._boxes.Add(this._tbSpriteNumber);
      this._boxes.Add(this._tbOffsetX);
      this._boxes.Add(this._tbOffsetY);
      this._boxes.Add(this._tbScaleX);
      this._boxes.Add(this._tbScaleY);
      this._boxes.Add(this._tbRotation);
      this._tbSpriteId.MouseUp += new MouseButtonEventHandler(this._tbSpriteId_MouseUp);
      this._tbSpriteNumber.TextChanged += new TextChangedEventHandler(this._tbSpriteNumber_TextChanged);
      this._tbOffsetX.TextChanged += new TextChangedEventHandler(this._tbOffsetX_TextChanged);
      this._tbOffsetY.TextChanged += new TextChangedEventHandler(this._tbOffsetY_TextChanged);
      this._cbMirror.Checked += new RoutedEventHandler(this._cbMirror_Checked);
      this._cbMirror.Unchecked += new RoutedEventHandler(this._cbMirror_Unchecked);
      this._color.ColorChanged += new SliderGradient.GradientPickerColorEventHandler(this._color_ColorChanged);
      this._color.PreviewColorChanged += new SliderGradient.GradientPickerColorEventHandler(this._color_PreviewColorChanged);
      this._tbScaleX.TextChanged += new TextChangedEventHandler(this._tbScaleX_TextChanged);
      this._tbScaleY.TextChanged += new TextChangedEventHandler(this._tbScaleY_TextChanged);
      this._tbRotation.TextChanged += new TextChangedEventHandler(this._tbRotation_TextChanged);
    }

    private void _tbRotation_TextChanged(object sender, TextChangedEventArgs e)
    {
      int ival;
      if (!this._eventsEnabled || !LayerControl._getVal(sender as TextBox, out ival))
        return;
      this._act.Commands.SetRotation(this._actionIndex, this._frameIndex, this._layerIndex, ival);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _tbScaleX_TextChanged(object sender, TextChangedEventArgs e)
    {
      float dval;
      if (!this._eventsEnabled || !LayerControl._getDecimalVal(((TextBox) sender).Text, out dval))
        return;
      this._act.Commands.SetScaleX(this._actionIndex, this._frameIndex, this._layerIndex, dval);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _tbScaleY_TextChanged(object sender, TextChangedEventArgs e)
    {
      float dval;
      if (!this._eventsEnabled || !LayerControl._getDecimalVal(((TextBox) sender).Text, out dval))
        return;
      this._act.Commands.SetScaleY(this._actionIndex, this._frameIndex, this._layerIndex, dval);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _color_PreviewColorChanged(object sender, Color color)
    {
      if (!this._eventsEnabled)
        return;
      this._act[this._actionIndex, this._frameIndex, this._layerIndex].Color = color.ToGrfColor();
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _color_ColorChanged(object sender, Color value)
    {
      if (!this._eventsEnabled)
        return;
      this._act[this._actionIndex, this._frameIndex, this._layerIndex].Color = this._color.InitialColor;
      this._act.Commands.SetColor(this._actionIndex, this._frameIndex, this._layerIndex, this._color.Color.ToGrfColor());
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _cbMirror_Unchecked(object sender, RoutedEventArgs e)
    {
      if (!this._eventsEnabled)
        return;
      this._act.Commands.SetMirror(this._actionIndex, this._frameIndex, this._layerIndex, false);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _cbMirror_Checked(object sender, RoutedEventArgs e)
    {
      if (!this._eventsEnabled)
        return;
      this._act.Commands.SetMirror(this._actionIndex, this._frameIndex, this._layerIndex, true);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _tbOffsetY_TextChanged(object sender, TextChangedEventArgs e)
    {
      int ival;
      if (!this._eventsEnabled || !LayerControl._getVal(sender as TextBox, out ival))
        return;
      this._act.Commands.SetOffsetY(this._actionIndex, this._frameIndex, this._layerIndex, ival);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _tbOffsetX_TextChanged(object sender, TextChangedEventArgs e)
    {
      int ival;
      if (!this._eventsEnabled || !LayerControl._getVal(sender as TextBox, out ival))
        return;
      this._act.Commands.SetOffsetX(this._actionIndex, this._frameIndex, this._layerIndex, ival);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _tbSpriteNumber_TextChanged(object sender, TextChangedEventArgs e)
    {
      int ival;
      if (!this._eventsEnabled || !LayerControl._getVal(sender as TextBox, out ival))
        return;
      this._act.Commands.SetAbsoluteSpriteId(this._actionIndex, this._frameIndex, this._layerIndex, ival);
      if (this._frameEditor == null)
        return;
      this._frameEditor.Update(this._layerIndex);
    }

    private void _tbSpriteId_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this._frameEditor == null || this._frameEditor.MainDrawingComponent == null || this._frameEditor.MainDrawingComponent == null || e.ChangedButton != MouseButton.Left)
        return;
      LayerDraw layerDraw = this._frameEditor.MainDrawingComponent.Get(this._layerIndex);
      if (layerDraw == null)
        return;
      if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
      {
        this._frameEditor.Editor.SelectionEngine.SelectUpToFromShift(this._layerIndex);
      }
      else
      {
        layerDraw.IsSelected = !layerDraw.IsSelected;
        layerDraw.QuickRender((IPreview) this._frameEditor);
      }
      Keyboard.Focus((IInputElement) this._frameEditor.Editor.GridPrimary);
    }

    private static bool _getVal(TextBox box, out int ival) => int.TryParse(box.Text, out ival);

    public void Update()
    {
      this._update();
      this.OnLayerPropertyChanged();
    }

    public void InternalUpdate(bool ignoreVisibility = false)
    {
      if (!ignoreVisibility && !this._isVisible())
        return;
      this._update();
    }

    private void _update(bool disableEvents = true)
    {
      Layer layer = this._layer;
      if (layer == null)
      {
        this.SetNull();
      }
      else
      {
        if (disableEvents)
          this._eventsEnabled = false;
        this._tbSpriteId.Text = this._layerIndex.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        this._tbSpriteNumber.Text = !layer.IsIndexed8() ? (this._act.Sprite.NumberOfIndexed8Images + layer.SpriteIndex).ToString((IFormatProvider) CultureInfo.InvariantCulture) : layer.SpriteIndex.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        this._tbOffsetX.Text = layer.OffsetX.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        this._tbOffsetY.Text = layer.OffsetY.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        this._cbMirror.IsChecked = new bool?(layer.Mirror);
        this._color.Color = layer.Color.ToColor();
        this._tbScaleX.Text = layer.ScaleX.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        this._tbScaleY.Text = layer.ScaleY.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        this._tbRotation.Text = layer.Rotation.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        if (!disableEvents)
          return;
        this._eventsEnabled = true;
      }
    }

    private void __update()
    {
      ClickSelectTextBox.EventsEnabled = false;
      this._update();
      ClickSelectTextBox.EventsEnabled = true;
    }

    private void _updateReference(bool disableEvents = true)
    {
      if (disableEvents)
        this._eventsEnabled = false;
      this._tbSpriteNumber.Text = "";
      this._tbSpriteNumber.IsHitTestVisible = false;
      this._tbSpriteNumber.IsReadOnly = true;
      this._saveReference();
      if (!disableEvents)
        return;
      this._eventsEnabled = true;
    }

    public void Set(Act act, int actionIndex, int frameIndex, int layerIndex, bool partialUpdate)
    {
      this._act = act;
      this._actionIndex = actionIndex;
      this._frameIndex = frameIndex;
      this._layerIndex = layerIndex;
      this._cbMirror.Visibility = Visibility.Visible;
      this._color.Visibility = Visibility.Visible;
      if (this._isReference)
        this._updateReference();
      else if (partialUpdate)
        this.__update();
      else
        this._update();
    }

    private bool _isVisible()
    {
      if (this._sp == null || this._sv == null)
        return true;
      int num1 = this._sp.Children.IndexOf((UIElement) this);
      if (this._layerIndex < 0 || this._layerIndex >= this._sp.Children.Count)
        return false;
      double num2 = (double) num1 * this.ActualHeight;
      double num3 = (double) (num1 + 1) * this.ActualHeight;
      return this._sv.VerticalOffset < num2 && num2 < this._sv.VerticalOffset + this._sv.ViewportHeight || this._sv.VerticalOffset < num3 && num3 < this._sv.VerticalOffset + this._sv.ViewportHeight;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      if (!this._isVisible())
        return;
      base.OnRender(drawingContext);
    }

    protected override Size MeasureOverride(Size constraint)
    {
      if (this._hasBuffered)
        return LayerControl._bufferedSize;
      if (this._isReference)
        return base.MeasureOverride(constraint);
      LayerControl._bufferedSize = base.MeasureOverride(constraint);
      this._hasBuffered = true;
      return LayerControl._bufferedSize;
    }

    public void ReferenceSetAndUpdate(
      Act act,
      int actionIndex,
      int frameIndex,
      int layerIndex,
      bool partialUpdate)
    {
      if (!this._isReference)
        return;
      this._act = act;
      this._actionIndex = actionIndex;
      this._frameIndex = frameIndex;
      this._layerIndex = layerIndex;
      this._cbMirror.Visibility = Visibility.Visible;
      this._color.Visibility = Visibility.Visible;
      this._updateReference();
      this._applyCommands(false);
    }

    public void SetNull()
    {
      if (this._isReference)
      {
        this._updateReference();
      }
      else
      {
        this._eventsEnabled = false;
        ClickSelectTextBox.EventsEnabled = false;
        this._tbSpriteId.Text = "";
        this._boxes.ForEach((Action<ClickSelectTextBox>) (p => p.Text = ""));
        this._cbMirror.Visibility = Visibility.Hidden;
        this._color.Visibility = Visibility.Hidden;
        this._eventsEnabled = true;
        ClickSelectTextBox.EventsEnabled = true;
      }
    }

    public void RefreshPreviewBackground() => this._boxes.ForEach((Action<ClickSelectTextBox>) (p => p.UpdateBackground()));

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/layercontrol.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._grid = (Grid) target;
          break;
        case 2:
          this._tbSpriteId = (TextBlock) target;
          break;
        case 3:
          this._tbSpriteNumber = (ClickSelectTextBox) target;
          break;
        case 4:
          this._tbOffsetX = (ClickSelectTextBox) target;
          break;
        case 5:
          this._tbOffsetY = (ClickSelectTextBox) target;
          break;
        case 6:
          this._tbMirror = (ClickSelectTextBox) target;
          break;
        case 7:
          this._cbMirror = (CheckBox) target;
          break;
        case 8:
          this._color = (QuickColorSelector) target;
          break;
        case 9:
          this._tbScaleX = (ClickSelectTextBox) target;
          break;
        case 10:
          this._tbScaleY = (ClickSelectTextBox) target;
          break;
        case 11:
          this._tbRotation = (ClickSelectTextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void LayerPropertyChangedDelegate(object sender);
  }
}
