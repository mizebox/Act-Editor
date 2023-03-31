// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.TestSheetDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class TestSheetDialog : TkWindow, IComponentConnector
  {
    internal System.Windows.Controls.ListView _listview;
    private bool _contentLoaded;

    public TestSheetDialog()
      : base("add.png", "add.png", SizeToContent.Manual, ResizeMode.CanResize)
    {
      this.InitializeComponent();
      ListViewDataTemplateHelper.GenerateListViewTemplateNew(this._listview, new ListViewDataTemplateHelper.GeneralColumnInfo[1]
      {
        new ListViewDataTemplateHelper.GeneralColumnInfo()
        {
          Header = "File name",
          DisplayExpression = "DisplayName",
          SearchGetAccessor = "DisplayName",
          IsFill = true,
          TextAlignment = TextAlignment.Left,
          ToolTipBinding = "DisplayName"
        }
      }, (ListViewCustomComparer) new DefaultListViewComparer<TestSheetDialog.ActReferenceView>(), (IList<string>) new string[2]
      {
        "Default",
        "Black"
      }, "generateHeader", "true", "overrideSizeRedraw", "true");
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/testsheetdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this._listview = (System.Windows.Controls.ListView) target;
      else
        this._contentLoaded = true;
    }

    public class ActReferenceView
    {
      private readonly string _name;

      public int Index { get; set; }

      public bool Default => true;

      public ActReferenceView(string name, int index)
      {
        this.Index = index;
        this._name = name;
      }

      public string DisplayName => this._name;

      public override string ToString() => this._name;
    }
  }
}
