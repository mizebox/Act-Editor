// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.IPreviewEditor
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.EditorControls;
using GRF.FileFormats.ActFormat;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ActEditor.Core
{
  public interface IPreviewEditor
  {
    UIElement Element { get; }

    Act Act { get; }

    int SelectedAction { get; }

    int SelectedFrame { get; }

    SelectionEngine SelectionEngine { get; }

    List<ReferenceControl> References { get; }

    IActIndexSelector FrameSelector { get; }

    event ActEditorWindow.ActEditorEventDelegate ReferencesChanged;

    event ActEditorWindow.ActEditorEventDelegate ActLoaded;

    Grid GridPrimary { get; }

    LayerEditor LayerEditor { get; }

    SpriteSelector SpriteSelector { get; }

    FramePreview FramePreview { get; }

    SpriteManager SpriteManager { get; }
  }
}
