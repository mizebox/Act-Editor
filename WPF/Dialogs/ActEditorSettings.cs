// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.ActEditorSettings
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.GenericControls;
using ColorPicker.Sliders;
using ErrorManager;
using GRF.Image;
using GrfToWpfBridge;
using GrfToWpfBridge.MultiGrf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using TokeiLibrary;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles;
using Utilities;
using Utilities.Services;

namespace ActEditor.Core.WPF.Dialogs
{
  public class ActEditorSettings : TkWindow, IComponentConnector
  {
    private readonly ActEditorWindow _actEditor;
    internal CheckBox _gridReopenLastest;
    internal CheckBox _gridHVisible;
    internal CheckBox _gridVVisible;
    internal CheckBox _cbRefresh;
    internal CheckBox _cbAliasing;
    internal ComboBox _comboBoxEncoding;
    internal QuickColorSelector _colorPreviewPanelBakground;
    internal QuickColorSelector _colorGridLH;
    internal QuickColorSelector _colorGridLV;
    internal QuickColorSelector _colorSpriteBorder;
    internal QuickColorSelector _colorSpriteOverlay;
    internal QuickColorSelector _colorSelectionBorder;
    internal QuickColorSelector _colorSelectionOverlay;
    internal QuickColorSelector _colorAnchor;
    internal QuickColorSelector _colorSpritePanelBackground;
    internal ComboBox _mz1;
    internal ComboBox _mz2;
    internal Grid _resourceGrfs;
    internal CheckBox _cbUniform;
    internal QuickColorSelector _colorBackground;
    internal QuickColorSelector _colorGuildelines;
    internal TextBox _tbDelayFactor;
    internal TextBox _tbMargin;
    internal CheckBox _cbHideGifDialog;
    internal CheckBox _assAct;
    internal CheckBox _debuggerLogAnyExceptions;
    internal Grid _gridActionPresenter;
    internal Button _buttonOk;
    private bool _contentLoaded;

    public ActEditorSettings() => this.InitializeComponent();

    public ActEditorSettings(ActEditorWindow actEditor, MetaGrfResourcesViewer resource)
      : base("Settings", "settings.png")
    {
      this._actEditor = actEditor;
      this.InitializeComponent();
      GrfEditorConfiguration.ConfigAsker.AdvancedSettingEnabled = true;
      this._colorPreviewPanelBakground.Color = GrfEditorConfiguration.ActEditorBackgroundColor;
      this._colorPreviewPanelBakground.Init(GrfEditorConfiguration.ConfigAsker.RetrieveSetting((Func<object>) (() => (object) GrfEditorConfiguration.ActEditorBackgroundColor)));
      this._colorPreviewPanelBakground.ColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((sender, value) =>
      {
        GrfEditorConfiguration.ActEditorBackgroundColor = value;
        this._actEditor._framePreview._primary.Background = (Brush) new SolidColorBrush(value);
      });
      this._colorPreviewPanelBakground.PreviewColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((sender, value) =>
      {
        GrfEditorConfiguration.ActEditorBackgroundColor = value;
        this._actEditor._framePreview._primary.Background = (Brush) new SolidColorBrush(value);
      });
      this._set(this._colorSpritePanelBackground, (Func<Color>) (() => GrfEditorConfiguration.ActEditorSpriteBackgroundColor), (Action<Color>) (v => GrfEditorConfiguration.ActEditorSpriteBackgroundColor = v));
      this._set(this._colorGridLH, (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorGridLineHorizontal), (Action<GrfColor>) (v => GrfEditorConfiguration.ActEditorGridLineHorizontal = v));
      this._set(this._colorGridLV, (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorGridLineVertical), (Action<GrfColor>) (v => GrfEditorConfiguration.ActEditorGridLineVertical = v));
      this._set(this._colorSpriteBorder, (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSpriteSelectionBorder), (Action<GrfColor>) (v => GrfEditorConfiguration.ActEditorSpriteSelectionBorder = v));
      this._set(this._colorSpriteOverlay, (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSpriteSelectionBorderOverlay), (Action<GrfColor>) (v => GrfEditorConfiguration.ActEditorSpriteSelectionBorderOverlay = v));
      this._set(this._colorSelectionBorder, (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSelectionBorder), (Action<GrfColor>) (v => GrfEditorConfiguration.ActEditorSelectionBorder = v));
      this._set(this._colorSelectionOverlay, (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSelectionBorderOverlay), (Action<GrfColor>) (v => GrfEditorConfiguration.ActEditorSelectionBorderOverlay = v));
      this._set(this._colorAnchor, (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorAnchorColor), (Action<GrfColor>) (v => GrfEditorConfiguration.ActEditorAnchorColor = v));
      GrfEditorConfiguration.Bind(this._gridHVisible, (Func<bool>) (() => GrfEditorConfiguration.ActEditorGridLineHVisible), (Action<bool>) (v => GrfEditorConfiguration.ActEditorGridLineHVisible = v), (Action) (() => this._actEditor._framePreview.UpdateAndSelect()));
      GrfEditorConfiguration.Bind(this._gridVVisible, (Func<bool>) (() => GrfEditorConfiguration.ActEditorGridLineVVisible), (Action<bool>) (v => GrfEditorConfiguration.ActEditorGridLineVVisible = v), (Action) (() => this._actEditor._framePreview.UpdateAndSelect()));
      GrfEditorConfiguration.Bind(this._cbRefresh, (Func<bool>) (() => GrfEditorConfiguration.ActEditorRefreshLayerEditor), (Action<bool>) (v => GrfEditorConfiguration.ActEditorRefreshLayerEditor = v));
      GrfEditorConfiguration.Bind(this._cbAliasing, (Func<bool>) (() => GrfEditorConfiguration.UseAliasing), (Action<bool>) (v => GrfEditorConfiguration.UseAliasing = v), (Action) (() => this._actEditor._framePreview.UpdateAndSelect()));
      GrfEditorConfiguration.Bind(this._debuggerLogAnyExceptions, (Func<bool>) (() => Configuration.LogAnyExceptions), (Action<bool>) (v => Configuration.LogAnyExceptions = v));
      GrfEditorConfiguration.Bind(this._cbUniform, (Func<bool>) (() => GrfEditorConfiguration.ActEditorGifUniform), (Action<bool>) (v => GrfEditorConfiguration.ActEditorGifUniform = v));
      GrfEditorConfiguration.Bind(this._colorBackground, (Func<Color>) (() => GrfEditorConfiguration.ActEditorGifBackgroundColor), (Action<Color>) (v => GrfEditorConfiguration.ActEditorGifBackgroundColor = v));
      GrfEditorConfiguration.Bind(this._colorGuildelines, (Func<Color>) (() => GrfEditorConfiguration.ActEditorGifGuidelinesColor), (Action<Color>) (v => GrfEditorConfiguration.ActEditorGifGuidelinesColor = v));
      GrfEditorConfiguration.Bind(this._cbHideGifDialog, (Func<bool>) (() => GrfEditorConfiguration.ActEditorGifHideDialog), (Action<bool>) (v => GrfEditorConfiguration.ActEditorGifHideDialog = v));
      GrfEditorConfiguration.Bind<float>(this._tbDelayFactor, (Func<float>) (() => GrfEditorConfiguration.ActEditorGifDelayFactor), (Action<float>) (v => GrfEditorConfiguration.ActEditorGifDelayFactor = v), new Func<string, float>(FormatConverters.SingleConverter));
      GrfEditorConfiguration.Bind<int>(this._tbMargin, (Func<int>) (() => GrfEditorConfiguration.ActEditorGifMargin), (Action<int>) (v => GrfEditorConfiguration.ActEditorGifMargin = v), new Func<string, int>(FormatConverters.IntConverter));
      GrfEditorConfiguration.Bind(this._gridReopenLastest, (Func<bool>) (() => GrfEditorConfiguration.ReopenLatestFile), (Action<bool>) (v => GrfEditorConfiguration.ReopenLatestFile = v));
      this._mz1.SelectedIndex = (double) GrfEditorConfiguration.ActEditorZoomInMultiplier > 0.0 ? 0 : 1;
      this._mz2.SelectedIndex = (double) GrfEditorConfiguration.ActEditorZoomInMultiplier > 0.0 ? 1 : 0;
      bool enableEvents = true;
      this._mz1.SelectionChanged += (SelectionChangedEventHandler) delegate
      {
        if (!enableEvents)
          return;
        GrfEditorConfiguration.ActEditorZoomInMultiplier = this._mz1.SelectedIndex == 0 ? 1f : -1f;
        enableEvents = false;
        this._mz2.SelectedIndex = this._mz1.SelectedIndex == 0 ? 1 : 0;
        enableEvents = true;
      };
      this._mz2.SelectionChanged += (SelectionChangedEventHandler) delegate
      {
        if (!enableEvents)
          return;
        GrfEditorConfiguration.ActEditorZoomInMultiplier = this._mz2.SelectedIndex == 0 ? -1f : 1f;
        enableEvents = false;
        this._mz1.SelectedIndex = this._mz2.SelectedIndex == 0 ? 1 : 0;
        enableEvents = true;
      };
      this._resourceGrfs.Children.Add((UIElement) resource);
      resource.Height = 150.0;
      this._comboBoxEncoding.Items.Add((object) "Default (codeage 1252 - Western European [Windows])");
      this._comboBoxEncoding.Items.Add((object) "Korean (codepage 949 - ANSI/OEM Korean [Unified Hangul Code])");
      this._comboBoxEncoding.Items.Add((object) "Other...");
      switch (GrfEditorConfiguration.EncodingCodepage)
      {
        case 949:
          this._comboBoxEncoding.SelectedIndex = 1;
          break;
        case 1252:
          this._comboBoxEncoding.SelectedIndex = 0;
          break;
        default:
          this._comboBoxEncoding.Items[2] = (object) (GrfEditorConfiguration.EncodingCodepage.ToString() + "...");
          this._comboBoxEncoding.SelectedIndex = 2;
          break;
      }
      this._comboBoxEncoding.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxEncoding_SelectionChanged);
      GrfEditorConfiguration.Bind(this._assAct, (Func<bool>) (() => GrfEditorConfiguration.ShellAssociateAct), (Action<bool>) (v => GrfEditorConfiguration.ShellAssociateAct = v), (Action) (() =>
      {
        if (GrfEditorConfiguration.ShellAssociateAct)
        {
          try
          {
            ApplicationManager.AddExtension(Methods.ApplicationFullPath, "Act", ".act", true);
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException("Failed to associate the file extension\n\n" + ex.Message, ErrorLevel.NotSpecified);
          }
        }
        else
        {
          try
          {
            ApplicationManager.RemoveExtension("grfeditor", ".act");
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException("Failed to remove the association of the file extension\n\n" + ex.Message, ErrorLevel.NotSpecified);
          }
        }
      }));
    }

    private void _set(QuickColorSelector qcs, Func<GrfColor> get, Action<GrfColor> set)
    {
      qcs.Color = get().ToColor();
      qcs.Init(GrfEditorConfiguration.ConfigAsker.RetrieveSetting((Func<object>) (() => (object) get())));
      qcs.ColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((sender, value) =>
      {
        set(value.ToGrfColor());
        this._actEditor._framePreview.SizeUpdate();
      });
      qcs.PreviewColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((sender, value) =>
      {
        GrfEditorConfiguration.ConfigAsker.IsAutomaticSaveEnabled = false;
        set(value.ToGrfColor());
        GrfEditorConfiguration.ConfigAsker.IsAutomaticSaveEnabled = true;
        this._actEditor._framePreview.SizeUpdate();
      });
    }

    private void _set(QuickColorSelector qcs, Func<Color> get, Action<Color> set)
    {
      qcs.Color = get();
      qcs.Init(GrfEditorConfiguration.ConfigAsker.RetrieveSetting((Func<object>) (() => (object) get())));
      qcs.ColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((sender, value) =>
      {
        set(value);
        this._actEditor._spriteSelector._dp.Background = (Brush) new SolidColorBrush(value);
      });
      qcs.PreviewColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((sender, value) =>
      {
        GrfEditorConfiguration.ConfigAsker.IsAutomaticSaveEnabled = false;
        set(value);
        GrfEditorConfiguration.ConfigAsker.IsAutomaticSaveEnabled = true;
        this._actEditor._spriteSelector._dp.Background = (Brush) new SolidColorBrush(value);
      });
    }

    private void _comboBoxEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      object obj = (object) null;
      bool flag = false;
      if (e.RemovedItems.Count > 0)
        obj = e.RemovedItems[0];
      switch (this._comboBoxEncoding.SelectedIndex)
      {
        case 0:
          if (!this._setEncoding(1252))
          {
            flag = true;
            break;
          }
          break;
        case 1:
          if (!this._setEncoding(949))
          {
            flag = true;
            break;
          }
          break;
        case 2:
          InputDialog inputDialog = WindowProvider.ShowWindow<InputDialog>((TkWindow) new InputDialog("Using an unsupported encoding may cause unexpected results, make a copy of your GRF file before saving!\nEnter the codepage number for the encoding :", "Encoding", this._comboBoxEncoding.Items[2].ToString().IndexOf(' ') > 0 ? this._comboBoxEncoding.Items[2].ToString().Substring(0, this._comboBoxEncoding.Items[2].ToString().IndexOf(' ')) : EncodingService.DisplayEncoding.CodePage.ToString((IFormatProvider) CultureInfo.InvariantCulture)), (Window) this);
          bool? dialogResult = inputDialog.DialogResult;
          if ((!dialogResult.GetValueOrDefault() ? 0 : (dialogResult.HasValue ? 1 : 0)) != 0)
          {
            if (EncodingService.EncodingExists(inputDialog.Input))
            {
              this._comboBoxEncoding.SelectionChanged -= new SelectionChangedEventHandler(this._comboBoxEncoding_SelectionChanged);
              this._comboBoxEncoding.Items[2] = (object) (inputDialog.Input + "...");
              this._comboBoxEncoding.SelectedIndex = 2;
              this._comboBoxEncoding.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxEncoding_SelectionChanged);
              if (!this._setEncoding(int.Parse(inputDialog.Input)))
              {
                flag = true;
                break;
              }
              break;
            }
            flag = true;
            break;
          }
          flag = true;
          break;
        default:
          if (!this._setEncoding(1252))
          {
            flag = true;
            break;
          }
          break;
      }
      if (!flag)
        return;
      this._comboBoxEncoding.SelectionChanged -= new SelectionChangedEventHandler(this._comboBoxEncoding_SelectionChanged);
      if (obj != null)
        this._comboBoxEncoding.SelectedItem = obj;
      this._comboBoxEncoding.SelectionChanged += new SelectionChangedEventHandler(this._comboBoxEncoding_SelectionChanged);
    }

    private bool _setEncoding(int encoding)
    {
      if (!EncodingService.SetDisplayEncoding(encoding))
        return false;
      GrfEditorConfiguration.EncodingCodepage = encoding;
      return true;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this._resourceGrfs.Children.Clear();
      base.OnClosing(e);
    }

    private void _buttonOk_Click(object sender, RoutedEventArgs e) => this.Close();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/acteditorsettingsdialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._gridReopenLastest = (CheckBox) target;
          break;
        case 2:
          this._gridHVisible = (CheckBox) target;
          break;
        case 3:
          this._gridVVisible = (CheckBox) target;
          break;
        case 4:
          this._cbRefresh = (CheckBox) target;
          break;
        case 5:
          this._cbAliasing = (CheckBox) target;
          break;
        case 6:
          this._comboBoxEncoding = (ComboBox) target;
          break;
        case 7:
          this._colorPreviewPanelBakground = (QuickColorSelector) target;
          break;
        case 8:
          this._colorGridLH = (QuickColorSelector) target;
          break;
        case 9:
          this._colorGridLV = (QuickColorSelector) target;
          break;
        case 10:
          this._colorSpriteBorder = (QuickColorSelector) target;
          break;
        case 11:
          this._colorSpriteOverlay = (QuickColorSelector) target;
          break;
        case 12:
          this._colorSelectionBorder = (QuickColorSelector) target;
          break;
        case 13:
          this._colorSelectionOverlay = (QuickColorSelector) target;
          break;
        case 14:
          this._colorAnchor = (QuickColorSelector) target;
          break;
        case 15:
          this._colorSpritePanelBackground = (QuickColorSelector) target;
          break;
        case 16:
          this._mz1 = (ComboBox) target;
          break;
        case 17:
          this._mz2 = (ComboBox) target;
          break;
        case 18:
          this._resourceGrfs = (Grid) target;
          break;
        case 19:
          this._cbUniform = (CheckBox) target;
          break;
        case 20:
          this._colorBackground = (QuickColorSelector) target;
          break;
        case 21:
          this._colorGuildelines = (QuickColorSelector) target;
          break;
        case 22:
          this._tbDelayFactor = (TextBox) target;
          break;
        case 23:
          this._tbMargin = (TextBox) target;
          break;
        case 24:
          this._cbHideGifDialog = (CheckBox) target;
          break;
        case 25:
          this._assAct = (CheckBox) target;
          break;
        case 26:
          this._debuggerLogAnyExceptions = (CheckBox) target;
          break;
        case 27:
          this._gridActionPresenter = (Grid) target;
          break;
        case 28:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
