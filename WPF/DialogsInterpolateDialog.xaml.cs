// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.InterpolateDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ActEditor.Core.Scripts;
using ActEditor.Core.WPF.EditorControls;
using ActEditor.Core.WPF.GenericControls;
using ColorPicker.Sliders;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities;
using Utilities.Tools;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class InterpolateDialog : TkWindow, IPreview, IComponentConnector
  {
    public static float EaseRange = 100f;
    private readonly Act _act;
    internal RadioButton _mode0;
    internal RadioButton _mode1;
    internal Label _labelStartIndex;
    internal Label _labelEndIndex;
    internal Label _labelRange;
    internal Label _labelLayerIndexes;
    internal Border _b1;
    internal ClickSelectTextBox _tbIndexStart;
    internal Border _b2;
    internal ClickSelectTextBox _tbIndexEnd;
    internal Border _b3;
    internal ClickSelectTextBox _tbRange;
    internal Border _b4;
    internal ClickSelectTextBox _tbLayerIndexes;
    internal FrameSelector _asIndexStart;
    internal FrameSelector _asIndexEnd;
    internal SliderColor _gpEase;
    internal ClickSelectTextBox _tbEase;
    internal Label _labelEaseInOrOut;
    internal CheckBox _cbOffsets;
    internal CheckBox _cbAngle;
    internal CheckBox _cbMirror;
    internal CheckBox _cbScale;
    internal CheckBox _cbColor;
    internal SliderColor _gpTolerance;
    internal ClickSelectTextBox _tbTolerance;
    internal ReadonlyFramePreview _rfp;
    internal ReadonlyPlaySelector _rps;
    internal Grid _gridActionPresenter;
    internal Button _buttonApply;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public InterpolateDialog.EditMode Mode
    {
      get => this.Dispatch<InterpolateDialog.EditMode>((Func<InterpolateDialog.EditMode>) (() =>
      {
        bool? isChecked1 = this._mode0.IsChecked;
        if ((!isChecked1.GetValueOrDefault() ? 0 : (isChecked1.HasValue ? 1 : 0)) != 0)
          return InterpolateDialog.EditMode.Frame;
        bool? isChecked2 = this._mode1.IsChecked;
        return (!isChecked2.GetValueOrDefault() ? 0 : (isChecked2.HasValue ? 1 : 0)) == 0 ? InterpolateDialog.EditMode.None : InterpolateDialog.EditMode.Layers;
      }));
      set
      {
        switch (value)
        {
          case InterpolateDialog.EditMode.Frame:
            this._mode0.IsChecked = new bool?(true);
            this._frameMode();
            break;
          case InterpolateDialog.EditMode.Layers:
            this._mode1.IsChecked = new bool?(true);
            this._layersMode();
            break;
        }
      }
    }

    private void _layersMode()
    {
      this._setAllVisible();
      this._b3.Visibility = Visibility.Collapsed;
      this._labelRange.Visibility = Visibility.Collapsed;
    }

    private void _frameMode()
    {
      this._setAllVisible();
      this._b2.Visibility = Visibility.Collapsed;
      this._b4.Visibility = Visibility.Collapsed;
      this._labelEndIndex.Visibility = Visibility.Collapsed;
      this._labelLayerIndexes.Visibility = Visibility.Collapsed;
      this._asIndexEnd.Visibility = Visibility.Collapsed;
    }

    private void _setAllVisible()
    {
      this._labelStartIndex.Visibility = Visibility.Visible;
      this._labelEndIndex.Visibility = Visibility.Visible;
      this._labelLayerIndexes.Visibility = Visibility.Visible;
      this._labelRange.Visibility = Visibility.Visible;
      this._b1.Visibility = Visibility.Visible;
      this._b2.Visibility = Visibility.Visible;
      this._b3.Visibility = Visibility.Visible;
      this._b4.Visibility = Visibility.Visible;
      this._asIndexStart.Visibility = Visibility.Visible;
      this._asIndexEnd.Visibility = Visibility.Visible;
    }

    public int ActionIndex { get; set; }

    public int StartIndex
    {
      get
      {
        int result;
        return int.TryParse(this._tbIndexStart.Dispatch<ClickSelectTextBox, string>((Func<ClickSelectTextBox, string>) (p => p.Text)), out result) ? result : -1;
      }
      set => this._tbIndexStart.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public int EndIndex
    {
      get
      {
        int result;
        return int.TryParse(this._tbIndexEnd.Dispatch<ClickSelectTextBox, string>((Func<ClickSelectTextBox, string>) (p => p.Text)), out result) ? result : -1;
      }
      set
      {
        this._tbIndexEnd.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        if (int.Parse(this._tbIndexEnd.Text) == value)
          return;
        this._tbIndexEnd.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      }
    }

    public int Range
    {
      get
      {
        int result;
        return int.TryParse(this._tbRange.Dispatch<ClickSelectTextBox, string>((Func<ClickSelectTextBox, string>) (p => p.Text)), out result) ? result : -1;
      }
      set
      {
        this._tbRange.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        if (int.Parse(this._tbRange.Text) == value)
          return;
        this._tbRange.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      }
    }

    public string LayerIndexes
    {
      get => this._tbLayerIndexes.Dispatch<ClickSelectTextBox, string>((Func<ClickSelectTextBox, string>) (p => p.Text));
      set => this._tbLayerIndexes.Text = value;
    }

    public InterpolateDialog()
      : base("Interpolation", "advanced.png")
    {
      this.InitializeComponent();
      GrfEditorConfiguration.Bind(this._cbOffsets, (Func<bool>) (() => GrfEditorConfiguration.InterpolateOffsets), (Action<bool>) (v => GrfEditorConfiguration.InterpolateOffsets = v), new System.Action(this._updatePreview));
      GrfEditorConfiguration.Bind(this._cbAngle, (Func<bool>) (() => GrfEditorConfiguration.InterpolateAngle), (Action<bool>) (v => GrfEditorConfiguration.InterpolateAngle = v), new System.Action(this._updatePreview));
      GrfEditorConfiguration.Bind(this._cbScale, (Func<bool>) (() => GrfEditorConfiguration.InterpolateScale), (Action<bool>) (v => GrfEditorConfiguration.InterpolateScale = v), new System.Action(this._updatePreview));
      GrfEditorConfiguration.Bind(this._cbColor, (Func<bool>) (() => GrfEditorConfiguration.InterpolateColor), (Action<bool>) (v => GrfEditorConfiguration.InterpolateColor = v), new System.Action(this._updatePreview));
      GrfEditorConfiguration.Bind(this._cbMirror, (Func<bool>) (() => GrfEditorConfiguration.InterpolateMirror), (Action<bool>) (v => GrfEditorConfiguration.InterpolateMirror = v), new System.Action(this._updatePreview));
      GrfEditorConfiguration.Bind<int>((TextBox) this._tbRange, (Func<int>) (() => GrfEditorConfiguration.InterpolateRange), (Action<int>) (v => GrfEditorConfiguration.InterpolateRange = v), new Func<string, int>(FormatConverters.IntConverter), new System.Action(this._updatePreview));
      WpfUtilities.AddFocus((FrameworkElement) this._tbIndexStart, (FrameworkElement) this._tbIndexEnd, (FrameworkElement) this._tbLayerIndexes, (FrameworkElement) this._tbRange, (FrameworkElement) this._tbEase, (FrameworkElement) this._tbTolerance);
      WpfUtilities.PreviewLabel((TextBox) this._tbLayerIndexes, "Example : 1,2,5-9;12;");
      this._asIndexStart.FrameChanged += new ActIndexSelector.FrameIndexChangedDelegate(this._asIndexStart_ActionChanged);
      this._asIndexEnd.FrameChanged += new ActIndexSelector.FrameIndexChangedDelegate(this._asIndexEnd_ActionChanged);
      this._tbIndexEnd.TextChanged += (TextChangedEventHandler) delegate
      {
        int result;
        if (!int.TryParse(this._tbIndexEnd.Text, out result))
          return;
        this._asIndexEnd.SelectedFrame = result;
        this._updatePreview();
      };
      this._tbIndexStart.TextChanged += (TextChangedEventHandler) delegate
      {
        int result;
        if (!int.TryParse(this._tbIndexStart.Text, out result))
          return;
        this._asIndexStart.SelectedFrame = result;
        this._updatePreview();
      };
      bool eventsEnabled = true;
      this._gpEase.ValueChanged += (SliderGradient.GradientPickerEventHandler) delegate
      {
        ((GradientBrush) this._gpEase.GradientBackground).GradientStops[1].Offset = this._gpEase.Position;
        if (!eventsEnabled)
          return;
        this._tbEase.Text = ((int) (this._gpEase.Position * (2.0 * (double) InterpolateDialog.EaseRange) - (double) InterpolateDialog.EaseRange)).ToString((IFormatProvider) CultureInfo.InvariantCulture);
      };
      this._tbEase.TextChanged += (TextChangedEventHandler) delegate
      {
        eventsEnabled = false;
        int result;
        if (int.TryParse(this._tbEase.Text, out result))
        {
          this._gpEase.SetPosition(((double) result + (double) InterpolateDialog.EaseRange) / (2.0 * (double) InterpolateDialog.EaseRange), false);
          this._labelEaseInOrOut.Content = result < 0 ? (object) "In" : (result > 0 ? (object) "Out" : (object) "");
          GrfEditorConfiguration.InterpolateEase = result;
          this._updatePreview();
        }
        eventsEnabled = true;
      };
      this._gpEase.Loaded += (RoutedEventHandler) delegate
      {
        this._tbEase.Text = GrfEditorConfiguration.InterpolateEase.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      };
      this._gpTolerance.ValueChanged += (SliderGradient.GradientPickerEventHandler) delegate
      {
        GrfEditorConfiguration.InterpolateTolerance = this._gpTolerance.Position;
        this._updatePreview();
      };
      this._gpTolerance.SetPosition(GrfEditorConfiguration.InterpolateTolerance, true);
    }

    public InterpolateDialog(Act act, int actionIndex)
      : this()
    {
      this.ActionIndex = actionIndex;
      this._asIndexStart.Set(act, actionIndex);
      this._asIndexEnd.Set(act, actionIndex);
      this._act = act;
      this._rps.Init(this._act, this.ActionIndex);
      this._rfp.Init((ISelector) this._rps);
      this._updatePreview();
    }

    private void _updatePreview()
    {
      if (this._act == null)
        return;
      LazyAction.Execute((System.Action) (() =>
      {
        Act act = new Act(this._act.Sprite);
        foreach (GRF.FileFormats.ActFormat.Action action in this._act)
          act.AddAction(new GRF.FileFormats.ActFormat.Action(action));
        if (this.CanExecute(act))
        {
          this.Execute(act);
          this.BeginDispatch<InterpolateDialog>((System.Action) (() => this._rps.SelectedFrame = this.StartIndex));
          this._rps.Play();
        }
        else
          this._rps.Stop();
        this._rps.Init(act, this.ActionIndex);
      }), this.GetHashCode());
    }

    private void _asIndexEnd_ActionChanged(object sender, int actionindex) => this._tbIndexEnd.Text = actionindex.ToString((IFormatProvider) CultureInfo.InvariantCulture);

    private void _asIndexStart_ActionChanged(object sender, int actionindex) => this._tbIndexStart.Text = actionindex.ToString((IFormatProvider) CultureInfo.InvariantCulture);

    protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
        this.DialogResult = new bool?(true);
      base.GRFEditorWindowKeyDown(sender, e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      bool? dialogResult = this.DialogResult;
      if ((!dialogResult.GetValueOrDefault() ? 0 : (dialogResult.HasValue ? 1 : 0)) != 0 && !this.CanExecute(this._act))
      {
        this.DialogResult = new bool?();
        e.Cancel = true;
      }
      if (e.Cancel)
        return;
      this._rps.Stop();
    }

    public bool CanExecute(Act act)
    {
      if (this.Mode == InterpolateDialog.EditMode.None)
        return false;
      try
      {
        this.Execute(act, false);
        return true;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
        return false;
      }
    }

    public void Execute(Act act, bool executeCommands = true)
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      InterpolateDialog.\u003C\u003Ec__DisplayClass38 cDisplayClass38_1 = new InterpolateDialog.\u003C\u003Ec__DisplayClass38();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass38_1.act = act;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass38_1.\u003C\u003E4__this = this;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass38_1.easeMethod = InterpolationAnimation.GetEaseMethod(GrfEditorConfiguration.InterpolateEase);
      switch (this.Mode)
      {
        case InterpolateDialog.EditMode.None:
          throw new Exception("No command selected.");
        case InterpolateDialog.EditMode.Frame:
          if (this.StartIndex < 0)
            this.StartIndex = 0;
          // ISSUE: reference to a compiler-generated field
          if (this.StartIndex >= cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames)
          {
            // ISSUE: reference to a compiler-generated field
            this.StartIndex = cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames - 1;
          }
          if (this.Range <= 0)
            this.Range = 1;
          if (this.Range > 100)
            throw new Exception("The number of frames must be below 100.");
          int startIndex = this.StartIndex;
          int actionIndex = this.ActionIndex;
          int range = this.Range;
          if (!executeCommands)
            break;
          try
          {
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.Begin();
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.Backup((Action<Act>) (actInput =>
            {
              int frameIndex = startIndex + 1;
              if (frameIndex >= act[actionIndex].Frames.Count)
                frameIndex = 0;
              GRF.FileFormats.ActFormat.Frame frame1 = act[actionIndex, startIndex];
              List<Layer> layers = act[actionIndex, frameIndex].Layers;
              List<GRF.FileFormats.ActFormat.Frame> collection = new List<GRF.FileFormats.ActFormat.Frame>();
              HashSet<int> intSet = new HashSet<int>();
              int num = range;
              InterpolationAnimation.UseInterpolateSettings = true;
              for (int index1 = 0; index1 < num; ++index1)
              {
                GRF.FileFormats.ActFormat.Frame frame2 = new GRF.FileFormats.ActFormat.Frame(frame1);
                List<Layer> startLayers = frame2.Layers;
                float degree = (float) (((double) index1 + 1.0) / ((double) num + 1.0));
                // ISSUE: variable of a compiler-generated type
                InterpolateDialog.\u003C\u003Ec__DisplayClass38 cDisplayClass38 = cDisplayClass38_1;
                for (int layer = 0; layer < frame2.NumberOfLayers; ++layer)
                {
                  if (layer < layers.Count)
                  {
                    if (layers[layer].SpriteIndex == startLayers[layer].SpriteIndex || this._similarEnough(act, layers[layer], startLayers[layer]))
                    {
                      startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], layers[layer], degree, easeMethod);
                      intSet.Add(layer);
                    }
                    else if (startLayers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1 && layers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1)
                    {
                      int index2 = layers.FindIndex((Predicate<Layer>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex));
                      Layer sub2 = layers[index2];
                      startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], sub2, degree, easeMethod);
                      intSet.Add(index2);
                    }
                    else
                      startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], (Layer) null, degree, easeMethod);
                  }
                  else if (startLayers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1 && layers.Count<Layer>((Func<Layer, bool>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex)) == 1)
                  {
                    int index3 = layers.FindIndex((Predicate<Layer>) (p => p.SpriteIndex == startLayers[layer].SpriteIndex));
                    Layer sub2 = layers[index3];
                    startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], sub2, degree, easeMethod);
                    intSet.Add(index3);
                  }
                  else
                    startLayers[layer] = InterpolationAnimation.Interpolate(startLayers[layer], (Layer) null, degree, easeMethod);
                }
                for (int index4 = 0; index4 < layers.Count; ++index4)
                {
                  if (!intSet.Contains(index4))
                    startLayers.Add(InterpolationAnimation.Interpolate((Layer) null, layers[index4], degree, easeMethod));
                }
                collection.Add(frame2);
              }
              act[actionIndex].Frames.InsertRange(startIndex + 1, (IEnumerable<GRF.FileFormats.ActFormat.Frame>) collection);
            }), "Interpolate frames");
            break;
          }
          catch (Exception ex)
          {
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.End();
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.InvalidateVisual();
          }
        case InterpolateDialog.EditMode.Layers:
          if (this.StartIndex < 0)
            this.StartIndex = 0;
          // ISSUE: reference to a compiler-generated field
          if (this.StartIndex >= cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames)
          {
            // ISSUE: reference to a compiler-generated field
            this.StartIndex = cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames - 1;
          }
          if (this.EndIndex < 0)
            this.EndIndex = 0;
          // ISSUE: reference to a compiler-generated field
          if (this.EndIndex >= cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames)
          {
            // ISSUE: reference to a compiler-generated field
            this.EndIndex = cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames - 1;
          }
          // ISSUE: reference to a compiler-generated field
          if (this.EndIndex > cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames)
          {
            // ISSUE: reference to a compiler-generated field
            this.EndIndex = cDisplayClass38_1.act[this.ActionIndex].NumberOfFrames;
          }
          // ISSUE: reference to a compiler-generated field
          GRF.FileFormats.ActFormat.Frame start = cDisplayClass38_1.act[this.ActionIndex, this.StartIndex];
          HashSet<int> layerIndexes = new HashSet<int>();
          foreach (int num in Methods.GetRange(this.LayerIndexes, start.NumberOfLayers))
          {
            if (num > -1 && num < start.NumberOfLayers)
              layerIndexes.Add(num);
          }
          if (layerIndexes.Count == 0)
          {
            for (int index = 0; index < start.NumberOfLayers; ++index)
              layerIndexes.Add(index);
          }
          if (!executeCommands)
            break;
          try
          {
            int endIndex = this.EndIndex;
            int startIndex2 = this.StartIndex;
            int actionIndex2 = this.ActionIndex;
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.Begin();
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.Backup((Action<Act>) (actInput =>
            {
              GRF.FileFormats.ActFormat.Frame frame = act[actionIndex2, endIndex];
              InterpolationAnimation.UseInterpolateSettings = true;
              foreach (int num in layerIndexes)
              {
                Layer layer = start.Layers[num];
                Layer subEnd = (Layer) null;
                if (num < frame.NumberOfLayers && frame.Layers[num].SpriteIndex == layer.SpriteIndex)
                {
                  subEnd = frame.Layers[num];
                }
                else
                {
                  for (int index = 0; index < frame.NumberOfLayers; ++index)
                  {
                    if (frame.Layers[index].SpriteIndex == layer.SpriteIndex)
                      subEnd = frame.Layers[index];
                  }
                }
                InterpolationAnimation.Interpolate(act, actionIndex2, num, layer, subEnd, startIndex2, endIndex, easeMethod);
              }
            }), "Interpolate selected layers");
            break;
          }
          catch (Exception ex)
          {
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.Commands.End();
            // ISSUE: reference to a compiler-generated field
            cDisplayClass38_1.act.InvalidateVisual();
          }
      }
    }

    private bool _similarEnough(Act act, Layer finalLayer, Layer startLayer)
    {
      if (GrfEditorConfiguration.InterpolateTolerance >= 1.0)
        return false;
      GrfImage image1 = startLayer.GetImage(act.Sprite);
      GrfImage image2 = finalLayer.GetImage(act.Sprite);
      return image1 != null && image2 != null && image1.SimilarityWith(image2) > GrfEditorConfiguration.InterpolateTolerance;
    }

    private void _buttonCancel_Click(object sender, RoutedEventArgs e) => this.Close();

    private void _mode_Checked(object sender, RoutedEventArgs e)
    {
      InterpolateDialog.EditMode editMode = InterpolateDialog.EditMode.None;
      if (sender == this._mode0)
        editMode = InterpolateDialog.EditMode.Frame;
      else if (sender == this._mode1)
        editMode = InterpolateDialog.EditMode.Layers;
      this.Mode = editMode;
      this._updatePreview();
    }

    private void _buttonApply_Click(object sender, RoutedEventArgs e)
    {
      if (!this.CanExecute(this._act))
        return;
      this.Execute(this._act);
      this._asIndexStart.Set(this._act, this.ActionIndex);
      this._asIndexEnd.Set(this._act, this.ActionIndex);
    }

    public Canvas Canva { get; private set; }

    public int CenterX { get; private set; }

    public int CenterY { get; private set; }

    public ZoomEngine ZoomEngine { get; private set; }

    public Act Act => this._act;

    public int SelectedAction => this.ActionIndex;

    public int SelectedFrame { get; private set; }

    public List<DrawingComponent> Components { get; private set; }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/interpolatedialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._mode0 = (RadioButton) target;
          this._mode0.Checked += new RoutedEventHandler(this._mode_Checked);
          break;
        case 2:
          this._mode1 = (RadioButton) target;
          this._mode1.Checked += new RoutedEventHandler(this._mode_Checked);
          break;
        case 3:
          this._labelStartIndex = (Label) target;
          break;
        case 4:
          this._labelEndIndex = (Label) target;
          break;
        case 5:
          this._labelRange = (Label) target;
          break;
        case 6:
          this._labelLayerIndexes = (Label) target;
          break;
        case 7:
          this._b1 = (Border) target;
          break;
        case 8:
          this._tbIndexStart = (ClickSelectTextBox) target;
          break;
        case 9:
          this._b2 = (Border) target;
          break;
        case 10:
          this._tbIndexEnd = (ClickSelectTextBox) target;
          break;
        case 11:
          this._b3 = (Border) target;
          break;
        case 12:
          this._tbRange = (ClickSelectTextBox) target;
          break;
        case 13:
          this._b4 = (Border) target;
          break;
        case 14:
          this._tbLayerIndexes = (ClickSelectTextBox) target;
          break;
        case 15:
          this._asIndexStart = (FrameSelector) target;
          break;
        case 16:
          this._asIndexEnd = (FrameSelector) target;
          break;
        case 17:
          this._gpEase = (SliderColor) target;
          break;
        case 18:
          this._tbEase = (ClickSelectTextBox) target;
          break;
        case 19:
          this._labelEaseInOrOut = (Label) target;
          break;
        case 20:
          this._cbOffsets = (CheckBox) target;
          break;
        case 21:
          this._cbAngle = (CheckBox) target;
          break;
        case 22:
          this._cbMirror = (CheckBox) target;
          break;
        case 23:
          this._cbScale = (CheckBox) target;
          break;
        case 24:
          this._cbColor = (CheckBox) target;
          break;
        case 25:
          this._gpTolerance = (SliderColor) target;
          break;
        case 26:
          this._tbTolerance = (ClickSelectTextBox) target;
          break;
        case 27:
          this._rfp = (ReadonlyFramePreview) target;
          break;
        case 28:
          this._rps = (ReadonlyPlaySelector) target;
          break;
        case 29:
          this._gridActionPresenter = (Grid) target;
          break;
        case 30:
          this._buttonApply = (Button) target;
          this._buttonApply.Click += new RoutedEventHandler(this._buttonApply_Click);
          break;
        case 31:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    Point IPreview.PointToScreen([In] Point obj0) => this.PointToScreen(obj0);

    public enum EditMode
    {
      None,
      Frame,
      Layers,
    }
  }
}
