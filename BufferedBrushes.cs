// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.BufferedBrushes
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.Image;
using GrfToWpfBridge;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ActEditor.Core
{
  public static class BufferedBrushes
  {
    private static readonly Dictionary<string, Func<GrfColor>> _getters = new Dictionary<string, Func<GrfColor>>();
    private static readonly Dictionary<string, GrfColor> _current = new Dictionary<string, GrfColor>();
    private static readonly Dictionary<string, Brush> _brushes = new Dictionary<string, Brush>();

    public static Brush GetBrush(string brushName)
    {
      GrfColor color = BufferedBrushes._getters[brushName]();
      GrfColor grfColor = BufferedBrushes._current[brushName];
      if (!(grfColor == (GrfColor) null) && color.Equals((object) grfColor))
        return BufferedBrushes._brushes[brushName];
      BufferedBrushes._current[brushName] = color;
      SolidColorBrush brush = new SolidColorBrush(color.ToColor());
      BufferedBrushes._brushes[brushName] = (Brush) brush;
      brush.Freeze();
      return (Brush) brush;
    }

    public static void Register(string brushName, Func<GrfColor> getter)
    {
      if (BufferedBrushes._current.ContainsKey(brushName))
        return;
      GrfColor color = getter();
      BufferedBrushes._getters[brushName] = getter;
      BufferedBrushes._current[brushName] = getter();
      SolidColorBrush solidColorBrush = new SolidColorBrush(color.ToColor());
      solidColorBrush.Freeze();
      BufferedBrushes._brushes[brushName] = (Brush) solidColorBrush;
    }
  }
}
