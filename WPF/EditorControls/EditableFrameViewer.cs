// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.EditableFrameViewer
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.DrawingComponents;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.Graphics;
using GrfToWpfBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TokeiLibrary;

namespace ActEditor.Core.WPF.EditorControls
{
  public class EditableFrameViewer : FrameViewer
  {
    protected EditableFrameViewerSettings _editableSettings;
    protected Window _parentWnd;
    private bool _hasKeyboardTranslated;
    private bool _operationsEnabled;
    private bool _hasRotated;
    private bool _hasScaled;
    private bool _hasTranslated;
    private System.Windows.Point _recentPosition;
    private FramePreview.ScaleDirection? _favoriteOrientation;

    public override void InitComponent(FrameViewerSettings settings)
    {
      base.InitComponent(settings);
      this._editableSettings = settings is EditableFrameViewerSettings ? (EditableFrameViewerSettings) settings : throw new Exception("Invalid setting type. " + (object) typeof (EditableFrameViewer) + " required.");
      this.Loaded += (RoutedEventHandler) delegate
      {
        this._parentWnd = WpfUtilities.FindDirectParentControl<Window>((FrameworkElement) this);
        this._parentWnd.KeyDown += new KeyEventHandler(this._framePreview_KeyDown);
        this._parentWnd.KeyUp += new KeyEventHandler(this._framePreview_KeyUp);
      };
    }

    public Layer[] GetSelectedLayers()
    {
      if (this.MainDrawingComponent == null)
        return new Layer[0];
      ActDraw drawingComponent = this.MainDrawingComponent;
      List<Layer> layerList = new List<Layer>();
      for (int index = 0; index < drawingComponent.Components.Count; ++index)
      {
        if (drawingComponent.Components[index].IsSelected)
          layerList.Add(this.Act[this.SelectedAction, this.SelectedFrame].Layers[index]);
      }
      return layerList.ToArray();
    }

    public void Copy()
    {
      if (this.GetSelectedLayers().Length <= 0)
        return;
      Clipboard.SetDataObject((object) new DataObject("Layers", (object) ((IEnumerable<Layer>) this.GetSelectedLayers()).ToList<Layer>().Select<Layer, Layer>((Func<Layer, Layer>) (p => new Layer(p))).ToArray<Layer>()));
    }

    public void Cut()
    {
      Layer[] selectedLayers = this._editableSettings.SelectionEngine.SelectedLayers;
      if (selectedLayers.Length <= 0)
        return;
      Clipboard.SetDataObject((object) new DataObject("Layers", (object) ((IEnumerable<Layer>) selectedLayers).ToList<Layer>().Select<Layer, Layer>((Func<Layer, Layer>) (p => new Layer(p))).ToArray<Layer>()));
      try
      {
        this.Act.Commands.Begin();
        foreach (int layerIndex in (IEnumerable<int>) this._editableSettings.SelectionEngine.SelectedItems.OrderByDescending<int, int>((Func<int, int>) (p => p)))
          this.Act.Commands.LayerDelete(this.SelectedAction, this.SelectedFrame, layerIndex);
        this._editableSettings.SelectionEngine.ClearSelection();
      }
      catch (Exception ex)
      {
        this.Act.Commands.CancelEdit();
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this.Act.Commands.End();
        this.Act.InvalidateVisual();
      }
    }

    private double _getDistance(System.Windows.Point point1, System.Windows.Point point2) => GRF.Graphics.Point.CalculateDistance(point1.ToGrfPoint(), point2.ToGrfPoint());

    public override void OnFrameMouseUp(MouseButtonEventArgs e)
    {
      bool flag = true;
      this._isAnyDown = false;
      if (!this._editableSettings.IsPlaying())
      {
        if (this._hasScaled)
        {
          this._setScale();
          this._hasScaled = false;
          flag = false;
        }
        if (this._hasRotated)
        {
          this._setRotated();
          this._hasRotated = false;
          flag = false;
        }
        if (this._hasTranslated)
        {
          this._setTranslate();
          this._hasTranslated = false;
        }
        else if (e != null)
        {
          if (this._noSelectedComponentsUnderMouse((MouseEventArgs) e) && e.ChangedButton == MouseButton.Left)
          {
            bool? nullable = this._editableSettings.SelectionEngine.IsUnderMouse(this._oldPosition);
            if ((nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0 && this._getDistance(this._oldPosition, e.GetPosition((IInputElement) this)) < 6.0)
            {
              this._editableSettings.SelectionEngine.DeselectAll();
              goto label_38;
            }
          }
          if (e.ChangedButton == MouseButton.Left && this._componentsUnderMouse((MouseEventArgs) e) && flag)
          {
            SelectionDraw selectionDraw = this._components.OfType<SelectionDraw>().FirstOrDefault<SelectionDraw>();
            if (selectionDraw == null || !selectionDraw.Visible)
              this._editableSettings.SelectionEngine.SelectUnderMouse(this._oldPosition, (MouseEventArgs) e);
          }
          else if (e.ChangedButton == MouseButton.Right && flag && !this._hasMoved && this._componentsUnderMouse((MouseEventArgs) e) && this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) != this._cbZoom)
          {
            List<DrawingComponent> drawingComponentList = new List<DrawingComponent>((IEnumerable<DrawingComponent>) this.MainDrawingComponent.Components);
            drawingComponentList.Reverse();
            int num = -1;
            if (this._noSelectedComponentsUnderMouse((MouseEventArgs) e))
            {
              foreach (LayerDraw layerDraw in drawingComponentList)
              {
                if (layerDraw.IsMouseUnder((MouseEventArgs) e))
                {
                  layerDraw.IsSelected = true;
                  num = layerDraw.LayerIndex;
                  break;
                }
              }
            }
            else
            {
              foreach (LayerDraw layerDraw in drawingComponentList)
              {
                if (layerDraw.IsSelected && layerDraw.IsMouseUnder((MouseEventArgs) e))
                {
                  num = layerDraw.LayerIndex;
                  break;
                }
              }
            }
            if (num < 0)
            {
              foreach (LayerDraw layerDraw in drawingComponentList)
              {
                if (layerDraw.IsMouseUnder((MouseEventArgs) e))
                {
                  num = layerDraw.LayerIndex;
                  break;
                }
              }
            }
            if (num > -1)
              this._editableSettings.SelectionEngine.LatestSelected = num;
            this.ContextMenu.IsOpen = true;
          }
label_38:
          if (this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) != this._cbZoom)
            e.Handled = true;
        }
        this._editableSettings.SetAnimation(0);
      }
      else if (this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) != this._cbZoom)
        e.Handled = true;
      this._operationsEnabled = false;
      this.ReleaseMouseCapture();
      this.UpdateSelection(new Rect(), false);
    }

    public override void OnFrameMouseMoved(MouseEventArgs e)
    {
      base.OnFrameMouseMoved(e);
      if (this._editableSettings.IsPlaying())
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      double deltaX = position.X - this._oldPosition.X;
      double deltaY = position.Y - this._oldPosition.Y;
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if (!this.IsMouseCaptured)
          this.CaptureMouse();
        if (!this._operationsEnabled)
        {
          if (!this._isAnyDown)
            return;
          if (this._getDistance(this._oldPosition, e.GetPosition((IInputElement) this)) > 5.0)
          {
            this.UpdateSelection(new Rect(this._oldPosition, e.GetPosition((IInputElement) this)), true);
            return;
          }
          this.UpdateSelection(new Rect(), false);
          return;
        }
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control || this._hasScaled)
          this._hasScaled = true;
        else if (Keyboard.Modifiers == ModifierKeys.Shift || this._hasRotated)
        {
          this._hasRotated = true;
        }
        else
        {
          if (!this._hasTranslated && !this._hasScaled && !this._hasRotated && this._noSelectedComponentsUnderMouse(e))
            return;
          this._hasTranslated = true;
        }
        if (this._components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary)) != null)
        {
          this._editableSettings.SetAnimation(1);
          List<LayerDraw> selectedLayerDraws = this._editableSettings.SelectionEngine.SelectedLayerDraws;
          List<Layer> list = ((IEnumerable<Layer>) this._editableSettings.SelectionEngine.SelectedLayers).ToList<Layer>();
          GRF.Graphics.Point point = new GRF.Graphics.Point(0.0f, 0.0f);
          foreach (Layer layer in list)
          {
            point.X += (float) layer.OffsetX;
            point.Y += (float) layer.OffsetY;
          }
          point.X = (float) ((double) point.X / (double) list.Count * this.ZoomEngine.Scale) + (float) this.CenterX;
          point.Y = (float) ((double) point.Y / (double) list.Count * this.ZoomEngine.Scale) + (float) this.CenterY;
          Vertex diffVector = new Vertex(this._oldPosition.ToGrfPoint() - point);
          foreach (LayerDraw layerDraw in (IEnumerable<LayerDraw>) selectedLayerDraws.OrderBy<LayerDraw, int>((Func<LayerDraw, int>) (p => p.LayerIndex)))
          {
            if (this._hasScaled)
            {
              if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
              {
                if (this._favoriteOrientation.HasValue)
                {
                  if (this._favoriteOrientation.Value == FramePreview.ScaleDirection.Horizontal)
                    deltaY = 0.0;
                  else if (this._favoriteOrientation.Value == FramePreview.ScaleDirection.Vertical)
                    deltaX = 0.0;
                }
                layerDraw.PreviewScale(diffVector, deltaX, deltaY);
              }
              else if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
              {
                double x = (position.ToGrfPoint() - point).Lenght / (this._oldPosition.ToGrfPoint() - point).Lenght;
                layerDraw.PreviewScale(Math.Pow(x, 1.2));
              }
              else
              {
                this._favoriteOrientation = new FramePreview.ScaleDirection?();
                layerDraw.PreviewScale(diffVector, deltaX, deltaY);
              }
            }
            if (this._hasTranslated)
              layerDraw.PreviewTranslate(deltaX, deltaY);
            if (this._hasRotated)
              layerDraw.PreviewRotate(this._oldPosition, (float) deltaX, (float) deltaY);
          }
        }
      }
      this._recentPosition = position;
    }

    protected override void _framePreview_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        this._isAnyDown = true;
        this._hasMoved = false;
        if (Keyboard.FocusedElement != this._cbZoom)
          Keyboard.Focus((IInputElement) this);
        this._oldPosition = e.GetPosition((IInputElement) this);
        this._favoriteOrientation = new FramePreview.ScaleDirection?();
        if (this._hasTranslated || this._hasScaled || this._hasRotated)
        {
          e.Handled = true;
        }
        else
        {
          if (e.RightButton == MouseButtonState.Pressed)
          {
            this._operationsEnabled = true;
            this.CaptureMouse();
          }
          if (e.LeftButton == MouseButtonState.Pressed && !this._editableSettings.IsPlaying())
          {
            if (this._editableSettings.SelectionEngine.SelectedItems.Count > 0)
            {
              if (this._noSelectedComponentsUnderMouse((MouseEventArgs) e) && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
              {
                this._operationsEnabled = false;
                return;
              }
              foreach (LayerDraw selectedLayerDraw in this._editableSettings.SelectionEngine.SelectedLayerDraws)
                selectedLayerDraw.SaveInitialData();
              this._operationsEnabled = true;
            }
            else
              this._operationsEnabled = false;
          }
          this._recentPosition = this._oldPosition;
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private bool _noSelectedComponentsUnderMouse(MouseEventArgs e)
    {
      if (this.MainDrawingComponent == null)
        return true;
      foreach (LayerDraw component in this.MainDrawingComponent.Components)
      {
        if (component.IsMouseUnder(e) && component.IsSelected)
          return false;
      }
      return true;
    }

    private bool _componentsUnderMouse(MouseEventArgs e)
    {
      if (this.MainDrawingComponent == null)
        return false;
      foreach (LayerDraw component in this.MainDrawingComponent.Components)
      {
        if (component.IsMouseUnder(e))
          return true;
      }
      return false;
    }

    protected override void _primary_Selected(object sender, int index, bool selected)
    {
      if (selected)
        this._editableSettings.SelectionEngine.AddSelection(index);
      else
        this._editableSettings.SelectionEngine.RemoveSelection(index);
    }

    private void _framePreview_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Left && e.Key != Key.Up && e.Key != Key.Right && e.Key != Key.Down || !this._hasKeyboardTranslated)
        return;
      this._setTranslate();
      this._hasKeyboardTranslated = false;
    }

    public void UpdateSelection(Rect rect, bool show)
    {
      if (this._editableSettings.IsPlaying())
        return;
      SelectionDraw selectionDraw = this._components.OfType<SelectionDraw>().FirstOrDefault<SelectionDraw>();
      if (show)
      {
        if (selectionDraw == null)
        {
          selectionDraw = new SelectionDraw();
          this._components.Add((DrawingComponent) selectionDraw);
        }
        selectionDraw.Render((IPreview) this, rect);
        selectionDraw.Visible = true;
        this._editableSettings.SelectionEngine.Select(rect, this.ZoomEngine, new System.Windows.Point((double) this.CenterX, (double) this.CenterY));
      }
      else
      {
        if (selectionDraw == null)
          return;
        selectionDraw.Visible = false;
      }
    }

    private void _framePreview_KeyDown(object sender, KeyEventArgs e)
    {
      if (this._editableSettings.IsPlaying() || e.Key != Key.Left && e.Key != Key.Up && e.Key != Key.Right && e.Key != Key.Down || this._editableSettings.Act == null)
        return;
      this._framePreview_MouseUp((object) this, (MouseButtonEventArgs) null);
      if (this._components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary)) != null)
      {
        List<LayerDraw> selectedLayerDraws = this._editableSettings.SelectionEngine.SelectedLayerDraws;
        if (!this._hasKeyboardTranslated)
        {
          foreach (LayerDraw layerDraw in selectedLayerDraws)
            layerDraw.SaveInitialData();
          this._hasKeyboardTranslated = true;
        }
        foreach (LayerDraw layerDraw in selectedLayerDraws)
        {
          if (this._hasKeyboardTranslated)
            layerDraw.PreviewTranslateRaw((Keyboard.IsKeyDown(Key.Left) ? -1 : 0) + (Keyboard.IsKeyDown(Key.Right) ? 1 : 0), (Keyboard.IsKeyDown(Key.Up) ? -1 : 0) + (Keyboard.IsKeyDown(Key.Down) ? 1 : 0));
        }
      }
      e.Handled = true;
    }

    private void _setTranslate()
    {
      this.Act.Commands.Begin();
      try
      {
        foreach (LayerDraw selectedLayerDraw in this._editableSettings.SelectionEngine.SelectedLayerDraws)
          selectedLayerDraw.Translate();
      }
      finally
      {
        this.Act.Commands.EndEdit();
      }
    }

    private void _setScale()
    {
      this.Act.Commands.Begin();
      try
      {
        foreach (LayerDraw selectedLayerDraw in this._editableSettings.SelectionEngine.SelectedLayerDraws)
          selectedLayerDraw.Scale();
      }
      finally
      {
        this.Act.Commands.EndEdit();
      }
    }

    private void _setRotated()
    {
      this.Act.Commands.Begin();
      try
      {
        foreach (LayerDraw selectedLayerDraw in this._editableSettings.SelectionEngine.SelectedLayerDraws)
          selectedLayerDraw.Rotate();
      }
      finally
      {
        this.Act.Commands.EndEdit();
      }
    }
  }
}
