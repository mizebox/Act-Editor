// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.UsageDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class UsageDialog : TkWindow, IComponentConnector
  {
    private readonly IPreviewEditor _actEditor;
    internal System.Windows.Controls.ListView _listView;
    internal Grid _gridActionPresenter;
    internal Button _buttonOk;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public UsageDialog() => this.InitializeComponent();

    public UsageDialog(IPreviewEditor actEditor, IEnumerable<ActIndex> indexes)
      : base("Usage", "help.ico")
    {
      this._actEditor = actEditor;
      this.InitializeComponent();
      this.Owner = WpfUtilities.TopWindow;
      this._listView.ItemsSource = (IEnumerable) indexes;
      ListViewDataTemplateHelper.GenerateListViewTemplateNew(this._listView, new ListViewDataTemplateHelper.GeneralColumnInfo[3]
      {
        new ListViewDataTemplateHelper.GeneralColumnInfo()
        {
          Header = "Action",
          DisplayExpression = "ActionIndex",
          SearchGetAccessor = "ActionIndex",
          TextAlignment = TextAlignment.Right,
          ToolTipBinding = "ActionIndex",
          FixedWidth = 70.0
        },
        new ListViewDataTemplateHelper.GeneralColumnInfo()
        {
          Header = "Frame",
          DisplayExpression = "FrameIndex",
          SearchGetAccessor = "FrameIndex",
          TextAlignment = TextAlignment.Right,
          ToolTipBinding = "FrameIndex",
          FixedWidth = 70.0
        },
        new ListViewDataTemplateHelper.GeneralColumnInfo()
        {
          Header = "Layer",
          DisplayExpression = "LayerIndex",
          SearchGetAccessor = "LayerIndex",
          TextAlignment = TextAlignment.Right,
          ToolTipBinding = "LayerIndex",
          FixedWidth = 70.0
        }
      }, (ListViewCustomComparer) new DefaultListViewComparer<ActIndex>(), (IList<string>) new string[2]
      {
        "Default",
        "Black"
      });
      this._listView.SelectionChanged += new SelectionChangedEventHandler(this._listView_SelectionChanged);
    }

    private void _listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!(this._listView.SelectedItem is ActIndex selectedItem))
        return;
      this._actEditor.FrameSelector.SetAction(selectedItem.ActionIndex);
      this._actEditor.FrameSelector.SetFrame(selectedItem.FrameIndex);
      this._actEditor.SelectionEngine.DeselectAll();
      this._actEditor.SelectionEngine.Select(selectedItem.LayerIndex);
    }

    protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape && e.Key != Key.Return)
        return;
      this.Close();
    }

    private void _buttonOk_Click(object sender, RoutedEventArgs e) => this.Close();

    private void _buttonCancel_Click(object sender, RoutedEventArgs e) => this.Close();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/usagedialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._listView = (System.Windows.Controls.ListView) target;
          break;
        case 2:
          this._gridActionPresenter = (Grid) target;
          break;
        case 3:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        case 4:
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
