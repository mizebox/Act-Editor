// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.DrawingComponents.LayerDraw
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.EditorControls;
using GRF.FileFormats.ActFormat;
using GRF.Graphics;
using GRF.Image;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ActEditor.Core.DrawingComponents
{
  public class LayerDraw : DrawingComponent
  {
    public const string SelectionBorderBrush = "SelectionBorderBrush";
    public const string SelectionOverlayBrush = "SelectionOverlayBrush";
    private static readonly Thickness _bufferedThickness = new Thickness(1.0);
    private readonly IPreviewEditor _actEditor;
    private readonly TransformGroup _borderTransformGroup = new TransformGroup();
    private readonly RotateTransform _rotate = new RotateTransform();
    private readonly ScaleTransform _scale = new ScaleTransform();
    private readonly TransformGroup _transformGroup = new TransformGroup();
    private readonly TranslateTransform _translateFrame = new TranslateTransform();
    private readonly TranslateTransform _translateToCenter = new TranslateTransform();
    private readonly IPreview _preview;
    private Act _act;
    private readonly ActDraw _parent;
    private Border _border;
    private System.Windows.Controls.Image _image;
    private Layer _layer;
    private Layer _layerCopy;
    private ScaleTransform _scalePreview = new ScaleTransform();
    private TranslateTransform _translatePreview = new TranslateTransform();

    static LayerDraw()
    {
      BufferedBrushes.Register(nameof (SelectionBorderBrush), (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSpriteSelectionBorder));
      BufferedBrushes.Register(nameof (SelectionOverlayBrush), (Func<GrfColor>) (() => GrfEditorConfiguration.ActEditorSpriteSelectionBorderOverlay));
    }

    public LayerDraw()
    {
      this._transformGroup.Children.Add((Transform) this._translateToCenter);
      this._transformGroup.Children.Add((Transform) this._scale);
      this._transformGroup.Children.Add((Transform) this._rotate);
      this._transformGroup.Children.Add((Transform) this._translateFrame);
      this._transformGroup.Children.Add((Transform) this._scalePreview);
      this._transformGroup.Children.Add((Transform) this._translatePreview);
      this._borderTransformGroup.Children.Add((Transform) this._translateToCenter);
      this._borderTransformGroup.Children.Add((Transform) this._scale);
      this._borderTransformGroup.Children.Add((Transform) this._rotate);
      this._borderTransformGroup.Children.Add((Transform) this._translateFrame);
      this._borderTransformGroup.Children.Add((Transform) this._scalePreview);
      this._borderTransformGroup.Children.Add((Transform) this._translatePreview);
    }

    public LayerDraw(IPreviewEditor actEditor, Act act, int layerIndex, ActDraw parent)
      : this()
    {
      this._actEditor = actEditor;
      this._preview = (IPreview) actEditor.FramePreview;
      this._act = act;
      this._parent = parent;
      this.LayerIndex = layerIndex;
    }

    public LayerDraw(IPreview preview, Act act, int layerIndex, ActDraw parent)
      : this()
    {
      this._preview = preview;
      this._act = act;
      this._parent = parent;
      this.LayerIndex = layerIndex;
    }

    public int LayerIndex { get; private set; }

    public Layer Layer => this._act.TryGetLayer(this._actEditor.SelectedAction, this._actEditor.SelectedFrame, this.LayerIndex);

    public override bool IsSelected
    {
      get => base.IsSelected;
      set
      {
        base.IsSelected = value;
        this._border.Visibility = this.IsSelected ? Visibility.Visible : Visibility.Hidden;
      }
    }

    public override bool IsSelectable
    {
      get => base.IsSelectable;
      set
      {
        if (base.IsSelectable && base.IsSelectable == value || !base.IsSelectable && base.IsSelectable == value)
          return;
        base.IsSelectable = value;
        this._initBorder();
      }
    }

    private bool _canInternalUpdate => this._actEditor != null && this._actEditor.LayerEditor != null;

    private LayerControl _subControl => this._actEditor.LayerEditor.Get(this.LayerIndex);

    private Brush _getBorderBrush() => BufferedBrushes.GetBrush("SelectionBorderBrush");

    private Brush _getBorderBackgroundBrush() => BufferedBrushes.GetBrush("SelectionOverlayBrush");

    public void Init(Act act, int layerIndex)
    {
      this._act = act;
      this.LayerIndex = layerIndex;
    }

    public override void OnSelected(int index, bool isSelected) => base.OnSelected(this.LayerIndex, isSelected);

    public override void Select()
    {
      this._initBorder();
      if (!this.IsSelectable)
        this.IsSelected = false;
      else
        this.IsSelected = true;
    }

    private void _initBorder()
    {
      if (this._border == null)
      {
        this._border = new Border();
        this._border.BorderThickness = LayerDraw._bufferedThickness;
        this._border.BorderBrush = this._getBorderBrush();
        this._border.Background = this._getBorderBackgroundBrush();
        this._border.SnapsToDevicePixels = true;
        this._border.IsHitTestVisible = false;
        this._border.Visibility = Visibility.Hidden;
      }
      if (this.IsSelectable)
        return;
      this.IsSelected = false;
      this.IsHitTestVisible = false;
      this._border.IsHitTestVisible = false;
      if (this._image == null)
        return;
      this._image.IsHitTestVisible = false;
    }

    private void _initImage()
    {
      if (this._image != null)
        return;
      this._image = new System.Windows.Controls.Image();
      this._image.SnapsToDevicePixels = true;
      if (!this.IsSelectable)
        this._image.IsHitTestVisible = false;
      else
        this._image.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this._image_MouseLeftButtonUp);
    }

    private void _initDc(IPreview frameEditor)
    {
      if (!frameEditor.Canva.Children.Contains((UIElement) this._image))
        frameEditor.Canva.Children.Add((UIElement) this._image);
      if (frameEditor.Canva.Children.Contains((UIElement) this._border))
        return;
      frameEditor.Canva.Children.Add((UIElement) this._border);
    }

    private bool _valideMouseOperation()
    {
      if (this.IsSelectable)
        return true;
      this.IsSelected = false;
      return false;
    }

    private void _image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (!this._valideMouseOperation())
        return;
      this.IsSelected = !this.IsSelected;
      this.ReleaseMouseCapture();
      e.Handled = true;
    }

    public override void Render(IPreview frameEditor)
    {
      this._initBorder();
      this._initImage();
      this._initDc(frameEditor);
      Act act = this._act ?? frameEditor.Act;
      int selectedAction = frameEditor.SelectedAction;
      int frameIndex = frameEditor.SelectedFrame;
      int? nullable = new int?();
      if (selectedAction >= act.NumberOfActions)
        return;
      if (act.Name == "Head" || act.Name == "Body")
      {
        bool flag = false;
        if ((act[selectedAction].NumberOfFrames == 3 && 0 <= selectedAction && selectedAction < 8 || 16 <= selectedAction && selectedAction < 24) && frameEditor.Act != null)
        {
          int num = frameEditor.Act[selectedAction].NumberOfFrames / 3;
          if (num != 0)
          {
            nullable = new int?(frameIndex);
            if (frameIndex < num)
            {
              frameIndex = 0;
              flag = true;
            }
            else if (frameIndex < 2 * num)
            {
              frameIndex = 1;
              flag = true;
            }
            else if (frameIndex < 3 * num)
            {
              frameIndex = 2;
              flag = true;
            }
            else
            {
              frameIndex = 2;
              flag = true;
            }
          }
        }
        if (!flag && frameIndex >= act[selectedAction].NumberOfFrames)
        {
          if (act[selectedAction].NumberOfFrames > 0)
            frameIndex %= act[selectedAction].NumberOfFrames;
          else
            frameIndex = 0;
        }
      }
      else if (frameIndex >= act[selectedAction].NumberOfFrames)
      {
        if (act[selectedAction].NumberOfFrames > 0)
          frameIndex %= act[selectedAction].NumberOfFrames;
        else
          frameIndex = 0;
      }
      GRF.FileFormats.ActFormat.Frame frame1 = act[selectedAction, frameIndex];
      if (this.LayerIndex >= frame1.NumberOfLayers)
        return;
      this._layer = act[selectedAction, frameIndex, this.LayerIndex];
      if (this._layer.SpriteIndex < 0)
      {
        this._image.Source = (ImageSource) null;
      }
      else
      {
        int index = this._layer.IsBgra32() ? this._layer.SpriteIndex + act.Sprite.NumberOfIndexed8Images : this._layer.SpriteIndex;
        if (index < 0 || index >= act.Sprite.Images.Count)
        {
          this._image.Source = (ImageSource) null;
        }
        else
        {
          GrfImage grfImage1 = act.Sprite.Images[index];
          if (grfImage1.GrfImageType == GrfImageType.Indexed8)
          {
            grfImage1 = grfImage1.Copy();
            grfImage1.Palette[3] = (byte) 0;
          }
          int num1 = 0;
          int num2 = 0;
          if (act.AnchoredTo != null && frame1.Anchors.Count > 0)
          {
            GRF.FileFormats.ActFormat.Frame frame2 = !nullable.HasValue || act.Name == null || act.AnchoredTo.Name == null ? act.AnchoredTo.TryGetFrame(selectedAction, nullable ?? frameIndex) : act.AnchoredTo.TryGetFrame(selectedAction, frameIndex) ?? act.AnchoredTo.TryGetFrame(selectedAction, nullable.Value);
            if (frame2 != null && frame2.Anchors.Count > 0)
            {
              num1 = frame2.Anchors[0].OffsetX - frame1.Anchors[0].OffsetX;
              num2 = frame2.Anchors[0].OffsetY - frame1.Anchors[0].OffsetY;
              if (act.AnchoredTo.AnchoredTo != null)
              {
                GRF.FileFormats.ActFormat.Frame frame3 = act.AnchoredTo.AnchoredTo.TryGetFrame(selectedAction, nullable ?? frameIndex);
                if (frame3 != null && frame3.Anchors.Count > 0)
                {
                  num1 = frame3.Anchors[0].OffsetX - frame1.Anchors[0].OffsetX;
                  num2 = frame3.Anchors[0].OffsetY - frame1.Anchors[0].OffsetY;
                }
              }
            }
          }
          int num3 = this._layer.Mirror ? -(grfImage1.Width + 1) % 2 : 0;
          this._translateToCenter.X = (double) (-((grfImage1.Width + 1) / 2) + num3);
          this._translateToCenter.Y = (double) -((grfImage1.Height + 1) / 2);
          this._translateFrame.X = (double) (this._layer.OffsetX + num1);
          this._translateFrame.Y = (double) (this._layer.OffsetY + num2);
          this._scale.ScaleX = (double) this._layer.ScaleX * (this._layer.Mirror ? -1.0 : 1.0);
          this._scale.ScaleY = (double) this._layer.ScaleY;
          this._rotate.Angle = (double) this._layer.Rotation;
          this._image.RenderTransform = (Transform) this._transformGroup;
          this._image.SetValue(RenderOptions.BitmapScalingModeProperty, (object) GrfEditorConfiguration.ActEditorScalingMode);
          GrfImage grfImage2 = grfImage1.Copy();
          grfImage2.ApplyChannelColor(this._layer.Color);
          this._image.Source = (ImageSource) grfImage2.Cast<BitmapSource>();
          this._image.VerticalAlignment = VerticalAlignment.Top;
          this._image.HorizontalAlignment = HorizontalAlignment.Left;
          this._border.Width = (double) grfImage2.Width;
          this._border.Height = (double) grfImage2.Height;
          this._border.RenderTransform = (Transform) this._borderTransformGroup;
          this.QuickRender(frameEditor);
        }
      }
    }

    public override void QuickRender(IPreview frameEditor)
    {
      if (this._scalePreview == null)
        this._scalePreview = new ScaleTransform();
      this._scalePreview.CenterX = (double) frameEditor.CenterX;
      this._scalePreview.CenterY = (double) frameEditor.CenterY;
      this._scalePreview.ScaleX = frameEditor.ZoomEngine.Scale;
      this._scalePreview.ScaleY = frameEditor.ZoomEngine.Scale;
      if (this._translatePreview == null)
        this._translatePreview = new TranslateTransform();
      this._translatePreview.X = (double) frameEditor.CenterX * frameEditor.ZoomEngine.Scale;
      this._translatePreview.Y = (double) frameEditor.CenterY * frameEditor.ZoomEngine.Scale;
      if (this._border == null)
        return;
      this._border.SetValue(RenderOptions.EdgeModeProperty, (object) (EdgeMode) (GrfEditorConfiguration.UseAliasing ? 1 : 0));
      this._border.BorderBrush = this._getBorderBrush();
      this._border.Background = this._getBorderBackgroundBrush();
      if (this._image.Source == null)
      {
        this._border.BorderThickness = new Thickness(0.0);
        this._border.Width = 0.0;
        this._border.Height = 0.0;
      }
      else
      {
        double num1 = Math.Abs(1.0 / (frameEditor.ZoomEngine.Scale * this._scale.ScaleX));
        double num2 = Math.Abs(1.0 / (frameEditor.ZoomEngine.Scale * this._scale.ScaleY));
        if (double.IsInfinity(num1) || double.IsNaN(num1) || double.IsInfinity(num2) || double.IsNaN(num2))
        {
          this._border.Width = 0.0;
          this._border.Height = 0.0;
        }
        else
          this._border.BorderThickness = new Thickness(num1, num2, num1, num2);
      }
    }

    public override void Remove(IPreview frameEditor)
    {
      if (this._image != null)
        frameEditor.Canva.Children.Remove((UIElement) this._image);
      if (this._border == null)
        return;
      frameEditor.Canva.Children.Remove((UIElement) this._border);
    }

    public void SaveInitialData() => this._layerCopy = new Layer(this._layer);

    public bool IsMouseUnder(MouseEventArgs e)
    {
      this._initImage();
      try
      {
        return this._scale.ScaleX != 0.0 && this._scale.ScaleY != 0.0 && object.ReferenceEquals((object) this._image.InputHitTest(e.GetPosition((IInputElement) this._image)), (object) this._image);
      }
      catch
      {
        return false;
      }
    }

    public bool IsMouseUnder(System.Windows.Point point)
    {
      this._initImage();
      try
      {
        return this._scale.ScaleX != 0.0 && this._scale.ScaleY != 0.0 && object.ReferenceEquals((object) this._image.InputHitTest(this._image.PointFromScreen(point)), (object) this._image);
      }
      catch
      {
        return false;
      }
    }

    public void PreviewScale(double scale)
    {
      if (this._layerCopy == null)
        return;
      double num1;
      double num2;
      if (this._layer.Width == 0)
      {
        num1 = 0.0;
        num2 = 0.0;
      }
      else
      {
        num1 = (double) this._layerCopy.ScaleX * scale;
        num2 = (double) this._layerCopy.ScaleY * scale;
      }
      this._layer.ScaleX = (float) num1;
      this._layer.ScaleY = (float) num2;
      if (this._canInternalUpdate)
        this._subControl.InternalUpdate();
      this.Render(this._preview);
    }

    public void PreviewScale(Vertex diffVector, double deltaX, double deltaY)
    {
      if (this._layerCopy == null)
        return;
      double num1 = 2.0 * Math.PI - this._layer.RotationRadian;
      if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
      {
        if (this._actEditor.SelectionEngine.CurrentlySelected.Count > 1)
        {
          double num2 = deltaX * Math.Cos(this._layer.RotationRadian) + deltaY * Math.Sin(this._layer.RotationRadian);
          double num3 = deltaX * Math.Sin(num1) + deltaY * Math.Cos(num1);
          deltaX = num2;
          deltaY = num3;
        }
        else
        {
          Vertex vertex1 = diffVector;
          Vertex vertex2 = new Vertex(vertex1.X + (float) deltaX, vertex1.Y + (float) deltaY);
          vertex1.RotateZ((float) this._layer.Rotation);
          vertex2.RotateZ((float) this._layer.Rotation);
          this._layer.ScaleX = this._layerCopy.ScaleX * (vertex2.X / vertex1.X);
          this._layer.ScaleY = this._layerCopy.ScaleY * (vertex2.Y / vertex1.Y);
          if (this._canInternalUpdate)
            this._subControl.InternalUpdate();
          this.Render(this._preview);
          return;
        }
      }
      double num4 = deltaX * 2.0 / this._preview.ZoomEngine.Scale;
      double num5 = deltaY * 2.0 / this._preview.ZoomEngine.Scale;
      double num6;
      double num7;
      if (this._layer.Width == 0 || this._layer.Height == 0)
      {
        num6 = 0.0;
        num7 = 0.0;
      }
      else if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
      {
        double num8 = (double) Math.Max(this._layer.Width, this._layer.Height);
        double num9 = (num8 + num4) / num8;
        num6 = (double) this._layerCopy.ScaleX * num9;
        num7 = (double) this._layerCopy.ScaleY * num9;
      }
      else
      {
        num6 = (double) this._layerCopy.ScaleX * ((double) this._layer.Width + num4) / (double) this._layer.Width;
        num7 = (double) this._layerCopy.ScaleY * ((double) this._layer.Height + num5) / (double) this._layer.Height;
      }
      this._layer.ScaleX = (float) num6;
      this._layer.ScaleY = (float) num7;
      if (this._canInternalUpdate)
        this._subControl.InternalUpdate();
      this.Render(this._preview);
    }

    public void Scale()
    {
      if (this._layerCopy == null)
        return;
      if (this._canInternalUpdate)
        this._subControl.InternalUpdate(true);
      float scaleX = this._layer.ScaleX;
      float scaleY = this._layer.ScaleY;
      this._layer.ScaleX = this._layerCopy.ScaleX;
      this._layer.ScaleY = this._layerCopy.ScaleY;
      this._act.Commands.SetScale(this._preview.SelectedAction, this._preview.SelectedFrame, this.LayerIndex, scaleX, scaleY);
    }

    public void PreviewRotate(System.Windows.Point initialPoint, float deltaX, float deltaY)
    {
      if (this._layerCopy == null)
        return;
      int[] array = this._actEditor.SelectionEngine.CurrentlySelected.OrderBy<int, int>((Func<int, int>) (p => p)).ToArray<int>();
      if (array.Length > 1 && array[0] != this.LayerIndex && this._preview.Components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary)).Components[array[0]] is LayerDraw component)
      {
        int amount = component._layer.Rotation - component._layerCopy.Rotation;
        this._layer.Rotation = this._layerCopy.Rotation;
        this._layer.Rotate(amount);
        if (this._canInternalUpdate)
          this._subControl.InternalUpdate();
        this.Render(this._preview);
      }
      else
      {
        System.Windows.Point point1 = new System.Windows.Point((double) this._preview.CenterX + (double) this._layer.OffsetX * this._preview.ZoomEngine.Scale, (double) this._preview.CenterY + (double) this._layer.OffsetY * this._preview.ZoomEngine.Scale);
        GRF.Graphics.Point v = new GRF.Graphics.Point(1f, 0.0f);
        System.Windows.Point point2 = new System.Windows.Point(initialPoint.X - point1.X, initialPoint.Y - point1.Y);
        System.Windows.Point point3 = new System.Windows.Point(point2.X + (double) deltaX, point2.Y + (double) deltaY);
        double num1 = GRF.Graphics.Point.CalculateAngle(new GRF.Graphics.Point(point2.X, point2.Y), v);
        double num2 = GRF.Graphics.Point.CalculateAngle(new GRF.Graphics.Point(point3.X, point3.Y), v);
        if (point2.Y < 0.0)
          num1 = 2.0 * Math.PI - num1;
        if (point3.Y < 0.0)
          num2 = 2.0 * Math.PI - num2;
        int amount = (int) ((num2 - num1) * 360.0 / (2.0 * Math.PI));
        this._layer.Rotation = this._layerCopy.Rotation;
        this._layer.Rotate(amount);
        if (this._canInternalUpdate)
          this._subControl.InternalUpdate();
        this.Render(this._preview);
      }
    }

    public void Rotate()
    {
      if (this._layerCopy == null)
        return;
      if (this._canInternalUpdate)
        this._subControl.InternalUpdate(true);
      int rotation = this._layer.Rotation;
      this._layer.Rotation = this._layerCopy.Rotation;
      this._act.Commands.SetRotation(this._preview.SelectedAction, this._preview.SelectedFrame, this.LayerIndex, rotation);
    }

    public void PreviewTranslate(double deltaX, double deltaY)
    {
      if (this._layerCopy == null)
        return;
      int num1 = (int) (deltaX / this._preview.ZoomEngine.Scale);
      int num2 = (int) (deltaY / this._preview.ZoomEngine.Scale);
      this._layer.OffsetX = this._layerCopy.OffsetX + num1;
      this._layer.OffsetY = this._layerCopy.OffsetY + num2;
      if (this._canInternalUpdate)
        this._subControl.InternalUpdate();
      this.Render(this._preview);
    }

    public void PreviewTranslateRaw(int x, int y)
    {
      if (this._layerCopy == null)
        return;
      this._layer.OffsetX += x;
      this._layer.OffsetY += y;
      if (this._canInternalUpdate)
        this._subControl.InternalUpdate();
      this.Render(this._preview);
    }

    public void Translate()
    {
      if (this._layerCopy == null)
        return;
      if (this._canInternalUpdate)
        this._subControl.InternalUpdate(true);
      int offsetX = this._layer.OffsetX - this._layerCopy.OffsetX;
      int offsetY = this._layer.OffsetY - this._layerCopy.OffsetY;
      this._layer.OffsetX = this._layerCopy.OffsetX;
      this._layer.OffsetY = this._layerCopy.OffsetY;
      if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
        this._act.Commands.Translate(this._preview.SelectedAction, offsetX, offsetY);
      else
        this._act.Commands.Translate(this._preview.SelectedAction, this._preview.SelectedFrame, this.LayerIndex, offsetX, offsetY);
    }
  }
}
