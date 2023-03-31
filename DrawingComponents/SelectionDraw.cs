// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.DrawingComponents.SelectionDraw
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using GRF.Image;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ActEditor.Core.DrawingComponents
{
  public class SelectionDraw : DrawingComponent
  {
    public const string SelectionBorder = "Selection_Border";
    public const string SelectionOverlay = "Selection_Overlay";
    private Rectangle _line;
    private bool _visible = true;

    static SelectionDraw()
    {
      BufferedBrushes.Register("Selection_Border", (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSelectionBorder));
      BufferedBrushes.Register("Selection_Overlay", (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSelectionBorderOverlay));
    }

    public SelectionDraw() => this.IsHitTestVisible = false;

    public bool Visible
    {
      get => this._visible;
      set
      {
        this._visible = value;
        if (this._line == null)
          return;
        this._line.Visibility = value ? Visibility.Visible : Visibility.Hidden;
      }
    }

    public override void Render(IPreview frameEditor)
    {
    }

    public void Render(IPreview frameEditor, Rect rect)
    {
      if (this._line == null)
      {
        this._line = new Rectangle();
        frameEditor.Canva.Children.Add((UIElement) this._line);
        this._line.StrokeThickness = 1.0;
        this._line.SnapsToDevicePixels = true;
        this._line.SetValue(RenderOptions.EdgeModeProperty, (object) EdgeMode.Aliased);
      }
      this._line.Fill = BufferedBrushes.GetBrush("Selection_Overlay");
      this._line.Height = (double) (int) rect.Height;
      this._line.Width = (double) (int) rect.Width;
      this._line.Margin = new Thickness((double) (int) rect.X, (double) (int) rect.Y, 0.0, 0.0);
      this._line.Stroke = BufferedBrushes.GetBrush("Selection_Border");
    }

    public override void QuickRender(IPreview frameEditor)
    {
    }

    public override void Remove(IPreview frameEditor)
    {
      if (this._line == null)
        return;
      frameEditor.Canva.Children.Remove((UIElement) this._line);
    }
  }
}
