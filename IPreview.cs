// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.IPreview
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.DrawingComponents;
using GRF.FileFormats.ActFormat;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Utilities.Tools;

namespace ActEditor.Core
{
  public interface IPreview
  {
    Canvas Canva { get; }

    int CenterX { get; }

    int CenterY { get; }

    ZoomEngine ZoomEngine { get; }

    Act Act { get; }

    int SelectedAction { get; }

    int SelectedFrame { get; }

    List<DrawingComponent> Components { get; }

    Point PointToScreen(Point point);
  }
}
