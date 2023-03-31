// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.LayerControlProvider
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using TokeiLibrary;

namespace ActEditor.Core.WPF.EditorControls
{
  public class LayerControlProvider
  {
    public const int MinimumAmountToGenerateAndLoad = 20;
    public const int MinimumAmountToGenerate = 100;
    private static readonly System.Action _emptyDelegate = (System.Action) (() => { });
    private readonly ActEditorWindow _actEditor;
    private readonly Dictionary<int, LayerControl> _controls = new Dictionary<int, LayerControl>();

    public LayerControlProvider(ActEditorWindow actEditor)
    {
      this._actEditor = actEditor;
      for (int index = 0; index < 20; ++index)
      {
        LayerControl element = new LayerControl((Act) null, actEditor, index);
        actEditor._preloader.Children.Add((UIElement) element);
        element.Dispatcher.Invoke(DispatcherPriority.Render, (Delegate) LayerControlProvider._emptyDelegate);
        this._controls[index] = element;
      }
      for (int index = 20; index < 100; ++index)
        this._controls[index] = new LayerControl((Act) null, actEditor, index);
    }

    public LayerControl Get(int index)
    {
      if (!this._controls.ContainsKey(index))
        this._controls[index] = new LayerControl((Act) null, this._actEditor, index);
      LayerControl ctr = this._controls[index];
      if (ctr.Parent == this._actEditor._preloader)
        this._actEditor.Dispatch<ActEditorWindow>((Action<ActEditorWindow>) (p => p._preloader.Children.Remove((UIElement) ctr)));
      return ctr;
    }
  }
}
