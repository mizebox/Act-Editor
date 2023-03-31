// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.DrawingComponents.AnchorDraw
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ActEditor.Core.DrawingComponents
{
  public class AnchorDraw : DrawingComponent
  {
    public const string AnchorBrushName = "AnchorDrawBrush";
    private Rectangle _line0;
    private Rectangle _line1;
    private Point _point;
    private bool _visible;

    static AnchorDraw() => BufferedBrushes.Register("AnchorDrawBrush", (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorAnchorColor));

    public AnchorDraw(Anchor anchor) => this._point = new Point((double) anchor.OffsetX, (double) anchor.OffsetY);

    public AnchorDraw(Point point) => this._point = point;

    public bool Visible
    {
      get => this._visible;
      set
      {
        if (value != this._visible)
        {
          if (this._line0 != null)
            this._line0.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
          if (this._line1 != null)
            this._line1.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
        this._visible = value;
      }
    }

    public void RenderOffsets(IPreview frameEditor, Point point)
    {
      this._point = point;
      this._initLines(frameEditor);
      this.QuickRender(frameEditor);
    }

    private void _initLines(IPreview frameEditor)
    {
      if (this._line0 == null)
      {
        this._line0 = new Rectangle();
        frameEditor.Canva.Children.Add((UIElement) this._line0);
        this._line0.StrokeThickness = 1.0;
        this._line0.SnapsToDevicePixels = true;
        this._line0.SetValue(RenderOptions.EdgeModeProperty, (object) EdgeMode.Aliased);
        this._line0.Height = 2.0;
        this._line0.Width = 20.0;
        this._line0.Visibility = this.Visible ? Visibility.Visible : Visibility.Collapsed;
        this._line0.RenderTransform = (Transform) new TranslateTransform()
        {
          X = (-this._line0.Width / 2.0),
          Y = (-this._line0.Height / 2.0)
        };
      }
      if (this._line1 != null)
        return;
      this._line1 = new Rectangle();
      frameEditor.Canva.Children.Add((UIElement) this._line1);
      this._line1.StrokeThickness = 1.0;
      this._line1.SnapsToDevicePixels = true;
      this._line1.SetValue(RenderOptions.EdgeModeProperty, (object) EdgeMode.Aliased);
      this._line1.Height = 20.0;
      this._line1.Width = 2.0;
      this._line1.RenderTransformOrigin = new Point(0.0, 0.0);
      this._line1.Visibility = this.Visible ? Visibility.Visible : Visibility.Collapsed;
      this._line1.RenderTransform = (Transform) new TranslateTransform()
      {
        X = (-this._line1.Width / 2.0),
        Y = (-this._line1.Height / 2.0)
      };
    }

    public override void Render(IPreview frameEditor)
    {
      this._initLines(frameEditor);
      if (!frameEditor.Canva.Children.Contains((UIElement) this._line0))
      {
        frameEditor.Canva.Children.Add((UIElement) this._line0);
        frameEditor.Canva.Children.Add((UIElement) this._line1);
      }
      this.QuickRender(frameEditor);
    }

    public override void QuickRender(IPreview frameEditor)
    {
      Thickness thickness = new Thickness(this._point.X * frameEditor.ZoomEngine.Scale + (double) frameEditor.CenterX, this._point.Y * frameEditor.ZoomEngine.Scale + (double) frameEditor.CenterY, 0.0, 0.0);
      this._line0.Margin = thickness;
      this._line1.Margin = thickness;
      this._line0.Stroke = BufferedBrushes.GetBrush("AnchorDrawBrush");
      this._line1.Stroke = BufferedBrushes.GetBrush("AnchorDrawBrush");
    }

    public override void Remove(IPreview frameEditor)
    {
      if (this._line0 != null)
        frameEditor.Canva.Children.Remove((UIElement) this._line0);
      if (this._line1 == null)
        return;
      frameEditor.Canva.Children.Remove((UIElement) this._line1);
    }
  }
}
