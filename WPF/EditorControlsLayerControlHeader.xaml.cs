// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.LayerControlHeader
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class LayerControlHeader : UserControl, IComponentConnector
  {
    internal Grid _grid;
    internal Label _lab0;
    internal Label _lab1;
    private bool _contentLoaded;

    public LayerControlHeader()
    {
      this.InitializeComponent();
      this.SizeChanged += (SizeChangedEventHandler) delegate
      {
        for (int index = 0; index < this._grid.ColumnDefinitions.Count; ++index)
        {
          double actualWidth = this._grid.ColumnDefinitions[index].ActualWidth;
          this._grid.ColumnDefinitions[index].MaxWidth = actualWidth;
          this._grid.ColumnDefinitions[index].MinWidth = actualWidth;
        }
      };
    }

    public Grid Grid => this._grid;

    public void HideStart()
    {
      this._grid.ColumnDefinitions[0].MinWidth = 0.0;
      this._grid.ColumnDefinitions[0].Width = new GridLength(0.0);
      this._grid.ColumnDefinitions[1].MinWidth = 0.0;
      this._grid.ColumnDefinitions[1].Width = new GridLength(0.0);
      this._lab0.Visibility = Visibility.Hidden;
      this._lab1.Visibility = Visibility.Hidden;
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/layercontrolheader.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._grid = (Grid) target;
          break;
        case 2:
          this._lab0 = (Label) target;
          break;
        case 3:
          this._lab1 = (Label) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
