// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.GifSavingDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.GenericControls;
using ErrorManager;
using GRF.FileFormats.ActFormat;
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
using TokeiLibrary.WPF.Styles;
using Utilities;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class GifSavingDialog : TkWindow, IComponentConnector
  {
    private readonly List<string> _extra = new List<string>();
    internal TextBox _tbIndexFrom;
    internal TextBox _tbIndexTo;
    internal CheckBox _cbUniform;
    internal QuickColorSelector _colorBackground;
    internal QuickColorSelector _colorGuildelines;
    internal TextBox _tbDelay;
    internal TextBox _tbDelayFactor;
    internal TextBox _tbMargin;
    internal Grid _gridActionPresenter;
    internal CheckBox _cbDoNotShow;
    internal Button _buttonOk;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public GifSavingDialog() => this.InitializeComponent();

    public GifSavingDialog(Act act, int selectedIndex)
      : base("Gif saving", "app.ico")
    {
      this.InitializeComponent();
      if (act == null)
      {
        this.Loaded += (RoutedEventHandler) delegate
        {
          ErrorHandler.HandleException("No Act loaded.");
          this.Close();
        };
      }
      else
      {
        this._tbIndexFrom.Text = "0";
        this._tbIndexTo.Text = act[selectedIndex].NumberOfFrames.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        GrfEditorConfiguration.Bind(this._cbUniform, (Func<bool>) (() => GrfEditorConfiguration.ActEditorGifUniform), (Action<bool>) (v => GrfEditorConfiguration.ActEditorGifUniform = v));
        GrfEditorConfiguration.Bind(this._colorBackground, (Func<Color>) (() => GrfEditorConfiguration.ActEditorGifBackgroundColor), (Action<Color>) (v => GrfEditorConfiguration.ActEditorGifBackgroundColor = v));
        GrfEditorConfiguration.Bind(this._colorGuildelines, (Func<Color>) (() => GrfEditorConfiguration.ActEditorGifGuidelinesColor), (Action<Color>) (v => GrfEditorConfiguration.ActEditorGifGuidelinesColor = v));
        GrfEditorConfiguration.Bind(this._cbDoNotShow, (Func<bool>) (() => GrfEditorConfiguration.ActEditorGifHideDialog), (Action<bool>) (v => GrfEditorConfiguration.ActEditorGifHideDialog = v));
        this._tbDelay.Text = ((int) Math.Ceiling((double) act[selectedIndex].AnimationSpeed * 25.0)).ToString((IFormatProvider) CultureInfo.InvariantCulture);
        GrfEditorConfiguration.Bind<float>(this._tbDelayFactor, (Func<float>) (() => GrfEditorConfiguration.ActEditorGifDelayFactor), (Action<float>) (v => GrfEditorConfiguration.ActEditorGifDelayFactor = v), new Func<string, float>(FormatConverters.SingleConverter));
        GrfEditorConfiguration.Bind<int>(this._tbMargin, (Func<int>) (() => GrfEditorConfiguration.ActEditorGifMargin), (Action<int>) (v => GrfEditorConfiguration.ActEditorGifMargin = v), new Func<string, int>(FormatConverters.IntConverter));
      }
    }

    public string[] Extra
    {
      get
      {
        this._extra.Clear();
        this._extra.Add("indexFrom");
        this._extra.Add(this._tbIndexFrom.Text);
        this._extra.Add("indexTo");
        this._extra.Add(this._tbIndexTo.Text);
        this._extra.Add("uniform");
        this._extra.Add(GrfEditorConfiguration.ActEditorGifUniform.ToString());
        this._extra.Add("background");
        this._extra.Add(GrfEditorConfiguration.ActEditorGifBackgroundColor.ToGrfColor().ToHexString().Replace("0x", "#"));
        this._extra.Add("guideLinesColor");
        this._extra.Add(GrfEditorConfiguration.ActEditorGifGuidelinesColor.ToGrfColor().ToHexString().Replace("0x", "#"));
        this._extra.Add("scaling");
        this._extra.Add(GrfEditorConfiguration.ActEditorScalingMode.ToString());
        this._extra.Add("delay");
        this._extra.Add(this._tbDelay.Text);
        this._extra.Add("delayFactor");
        this._extra.Add(this._tbDelayFactor.Text);
        this._extra.Add("margin");
        this._extra.Add(GrfEditorConfiguration.ActEditorGifMargin.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        return this._extra.ToArray();
      }
    }

    protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.Close();
    }

    private void _buttonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(true);
      this.Close();
    }

    private void _buttonCancel_Click(object sender, RoutedEventArgs e) => this.Close();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/gifsavingdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._tbIndexFrom = (TextBox) target;
          break;
        case 2:
          this._tbIndexTo = (TextBox) target;
          break;
        case 3:
          this._cbUniform = (CheckBox) target;
          break;
        case 4:
          this._colorBackground = (QuickColorSelector) target;
          break;
        case 5:
          this._colorGuildelines = (QuickColorSelector) target;
          break;
        case 6:
          this._tbDelay = (TextBox) target;
          break;
        case 7:
          this._tbDelayFactor = (TextBox) target;
          break;
        case 8:
          this._tbMargin = (TextBox) target;
          break;
        case 9:
          this._gridActionPresenter = (Grid) target;
          break;
        case 10:
          this._cbDoNotShow = (CheckBox) target;
          break;
        case 11:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        case 12:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
