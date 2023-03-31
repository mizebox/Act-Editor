// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.DrawingComponents.GridLine
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using GRF.Image;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ActEditor.Core.DrawingComponents
{
  public class GridLine : DrawingComponent
  {
    public const string GridLineHorizontalBrush = "Horizontal";
    public const string GridLineVerticalBrush = "Vertical";
    private readonly Orientation _orientation;
    private Rectangle _line;
    private bool _visible = true;

    static GridLine()
    {
      BufferedBrushes.Register("Horizontal", (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorGridLineHorizontal));
      BufferedBrushes.Register("Vertical", (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorGridLineVertical));
    }

    public GridLine(Orientation orientation)
    {
      this._orientation = orientation;
      this.IsHitTestVisible = false;
    }

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
      if (this._line != null)
      {
        if (this._orientation == Orientation.Horizontal)
          this._line.Visibility = GrfEditorConfiguration.ActEditorGridLineHVisible ? Visibility.Visible : Visibility.Hidden;
        else
          this._line.Visibility = GrfEditorConfiguration.ActEditorGridLineVVisible ? Visibility.Visible : Visibility.Hidden;
      }
      if (this._line == null)
      {
        this._line = new Rectangle();
        frameEditor.Canva.Children.Add((UIElement) this._line);
        this._line.StrokeThickness = 1.0;
        this._line.SnapsToDevicePixels = true;
        this._line.SetValue(RenderOptions.EdgeModeProperty, (object) EdgeMode.Aliased);
        this._line.Height = 1.0;
        this._line.Width = 1.0;
      }
      if (this._orientation == Orientation.Horizontal)
      {
        this._line.Margin = new Thickness(0.0, (double) frameEditor.CenterY, 0.0, 0.0);
        this._line.Width = frameEditor.Canva.ActualWidth + 50.0;
        this._line.Stroke = this._getColor();
      }
      else
      {
        if (this._orientation != Orientation.Vertical)
          return;
        this._line.Margin = new Thickness((double) frameEditor.CenterX, 0.0, 0.0, 0.0);
        this._line.Height = frameEditor.Canva.ActualHeight + 50.0;
        this._line.Stroke = this._getColor();
      }
    }

    public override void QuickRender(IPreview frameEditor)
    {
      if (this._line == null)
      {
        this.Render(frameEditor);
      }
      else
      {
        if (this._orientation == Orientation.Horizontal)
        {
          this._line.Margin = new Thickness(0.0, (double) frameEditor.CenterY, 0.0, 0.0);
          this._line.Width = frameEditor.Canva.ActualWidth + 50.0;
        }
        else if (this._orientation == Orientation.Vertical)
        {
          this._line.Margin = new Thickness((double) frameEditor.CenterX, 0.0, 0.0, 0.0);
          this._line.Height = frameEditor.Canva.ActualHeight + 50.0;
        }
        this._line.Stroke = this._getColor();
      }
    }

    private Brush _getColor() => BufferedBrushes.GetBrush(this._orientation == Orientation.Horizontal ? "Horizontal" : "Vertical");

    public override void Remove(IPreview frameEditor)
    {
      if (this._line == null)
        return;
      frameEditor.Canva.Children.Remove((UIElement) this._line);
    }
  }
}
