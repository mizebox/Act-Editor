// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.FramePreview
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ActEditor.Core.Scripts;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.Graphics;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;
using Utilities.Commands;
using Utilities.Tools;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class FramePreview : UserControl, IPreview, IComponentConnector
  {
    private readonly List<DrawingComponent> _components = new List<DrawingComponent>();
    private readonly ZoomEngine _zoomEngine = new ZoomEngine()
    {
      ZoomInMultiplier = (Func<double>) (() => (double) GrfEditorConfiguration.ActEditorZoomInMultiplier)
    };
    private bool _anchorEdit;
    private FramePreview.ScaleDirection? _favoriteOrientation;
    private bool _hasKeyboardTranslated;
    private bool _hasMoved;
    private bool _hasRotated;
    private bool _hasScaled;
    private bool _isAnyDown;
    private System.Windows.Point _oldAnchorPoint;
    private System.Windows.Point _oldPosition;
    private bool _operationsEnabled;
    private System.Windows.Point _recentPosition;
    private System.Windows.Point _relativeCenter = new System.Windows.Point(0.5, 0.8);
    public Action<List<DrawingComponent>> CustomUpdate;
    private bool _hasTranslated;
    internal TkMenuItem _miDelete;
    internal TkMenuItem _miInvert;
    internal TkMenuItem _miFront;
    internal TkMenuItem _miBack;
    internal TkMenuItem _miActionFront;
    internal TkMenuItem _miActionBack;
    internal TkMenuItem _miCopy;
    internal TkMenuItem _miCut;
    internal TkMenuItem _miSelect;
    internal Grid _gridBackground;
    internal Canvas _primary;
    internal Grid _gridZoom;
    internal ComboBox _cbZoom;
    private bool _contentLoaded;

    public IEditorInteractionEngine InteractionEngine { get; set; }

    public event FramePreview.ConstructorCalledDelegate ConstructorCalled;

    public event FramePreview.FramePreviewEventDelegate PreviewRender;

    protected virtual void OnPreviewRender()
    {
      FramePreview.FramePreviewEventDelegate previewRender = this.PreviewRender;
      if (previewRender == null)
        return;
      previewRender((object) this);
    }

    public virtual void OnConstructorCalled()
    {
      FramePreview.ConstructorCalledDelegate constructorCalled = this.ConstructorCalled;
      if (constructorCalled == null)
        return;
      constructorCalled((object) this);
    }

    public FramePreview()
    {
      this.InitializeComponent();
      this._primary.Background = (Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorBackgroundColor);
      this._components.Add((DrawingComponent) new GridLine(Orientation.Horizontal));
      this._components.Add((DrawingComponent) new GridLine(Orientation.Vertical));
      this.SizeChanged += new SizeChangedEventHandler(this._framePreview_SizeChanged);
      this.MouseMove += new MouseEventHandler(this._framePreview_MouseMove);
      this.MouseDown += new MouseButtonEventHandler(this._framePreview_MouseDown);
      this.MouseUp += new MouseButtonEventHandler(this._framePreview_MouseUp);
      this.MouseWheel += new MouseWheelEventHandler(this._framePreview_MouseWheel);
      this.Drop += new DragEventHandler(this._framePreview_Drop);
      this.Loaded += (RoutedEventHandler) delegate
      {
        if (this.Editor == null)
          return;
        this.Editor.Element.KeyDown += new KeyEventHandler(this.FramePreview_KeyDown);
        this.Editor.Element.KeyUp += new KeyEventHandler(this._framePreview_KeyUp);
      };
    }

    public List<DrawingComponent> Components => this._components;

    public IPreviewEditor Editor { get; set; }

    public System.Windows.Point RelativeCenter
    {
      get => this._relativeCenter;
      set => this._relativeCenter = value;
    }

    public ActDraw MainDrawingComponent => this._components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary));

    public Grid GridBackground => this._gridBackground;

    public Func<bool> ImageExists => new Func<bool>(this._imageExists);

    public Act Act => this.Editor.Act;

    public int SelectedAction => this.Editor.SelectedAction;

    public int SelectedFrame => this.Editor.SelectedFrame;

    public ZoomEngine ZoomEngine => this._zoomEngine;

    public Canvas Canva => this._primary;

    public int CenterX => (int) (this._primary.ActualWidth * this._relativeCenter.X);

    public int CenterY => (int) (this._primary.ActualHeight * this._relativeCenter.Y);

    public event DrawingComponent.DrawingComponentDelegate Selected;

    public void OnSelected(int index, bool selected)
    {
      DrawingComponent.DrawingComponentDelegate selected1 = this.Selected;
      if (selected1 == null)
        return;
      selected1((object) this, index, selected);
    }

    private void _framePreview_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Left && e.Key != Key.Up && e.Key != Key.Right && e.Key != Key.Down || !this._hasKeyboardTranslated)
        return;
      this._setTranslate();
      this._hasKeyboardTranslated = false;
    }

    public void Copy() => this.InteractionEngine.Copy();

    public void Paste() => this.InteractionEngine.Paste();

    public void Cut() => this.InteractionEngine.Cut();

    public void FramePreview_KeyDown(object sender, KeyEventArgs e)
    {
      if (this.Editor.FrameSelector.IsPlaying)
        return;
      if (ApplicationShortcut.Is(ApplicationShortcut.Copy))
        this.Copy();
      if (ApplicationShortcut.Is(ApplicationShortcut.Cut))
        this.Cut();
      int modifiers = (int) Keyboard.Modifiers;
      if (e.Key == Key.Delete)
        this.InteractionEngine.Delete();
      if (ApplicationShortcut.Is(ApplicationShortcut.Paste))
        this.Paste();
      if (e.Key != Key.Left && e.Key != Key.Up && e.Key != Key.Right && e.Key != Key.Down || this.Editor == null)
        return;
      this._framePreview_MouseUp((object) this, (MouseButtonEventArgs) null);
      if (this._components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary)) != null)
      {
        List<LayerDraw> selectedLayerDraws = this.Editor.SelectionEngine.SelectedLayerDraws;
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

    private void _framePreview_Drop(object sender, DragEventArgs e)
    {
      object data = e.Data.GetData("ImageIndex");
      if (data == null)
        return;
      this.Editor.Act.Commands.LayerAdd(this.Editor.SelectedAction, this.Editor.SelectedFrame, (int) data);
      int num = this.Editor.Act[this.Editor.SelectedAction, this.Editor.SelectedFrame].NumberOfLayers - 1;
      Layer layer = this.Editor.Act[this.Editor.SelectedAction, this.Editor.SelectedFrame, num];
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      layer.OffsetX = (int) ((position.X - this._relativeCenter.X * this.ActualWidth) / this.ZoomEngine.Scale);
      layer.OffsetY = (int) ((position.Y - this._relativeCenter.Y * this.ActualHeight) / this.ZoomEngine.Scale);
      this.Editor.FrameSelector.OnFrameChanged(this.Editor.SelectedFrame);
      this.Editor.SelectionEngine.SetSelection(num);
      Keyboard.Focus((IInputElement) this.Editor.GridPrimary);
    }

    private void _framePreview_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
        return;
      this.ZoomEngine.Zoom((double) e.Delta);
      System.Windows.Point position = e.GetPosition((IInputElement) this._primary);
      double num1 = position.X / this._primary.ActualWidth - this._relativeCenter.X;
      double num2 = position.Y / this._primary.ActualHeight - this._relativeCenter.Y;
      this._relativeCenter.X = position.X / this._primary.ActualWidth - num1 / this.ZoomEngine.OldScale * this.ZoomEngine.Scale;
      this._relativeCenter.Y = position.Y / this._primary.ActualHeight - num2 / this.ZoomEngine.OldScale * this.ZoomEngine.Scale;
      this._cbZoom.SelectedIndex = -1;
      this._cbZoom.Text = this._zoomEngine.ScaleText;
      this.SizeUpdate();
    }

    private void _framePreview_MouseUp(object sender, MouseButtonEventArgs e)
    {
      try
      {
        bool flag = true;
        this._isAnyDown = false;
        if (!this.Editor.FrameSelector.IsPlaying)
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
              bool? nullable = this.Editor.SelectionEngine.IsUnderMouse(this._oldPosition);
              if ((nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0 && this._getDistance(this._oldPosition, e.GetPosition((IInputElement) this)) < 6.0)
              {
                this.Editor.SelectionEngine.DeselectAll();
                goto label_38;
              }
            }
            if (e.ChangedButton == MouseButton.Left && this._componentsUnderMouse((MouseEventArgs) e) && flag)
            {
              SelectionDraw selectionDraw = this._components.OfType<SelectionDraw>().FirstOrDefault<SelectionDraw>();
              if (selectionDraw == null || !selectionDraw.Visible)
                this.Editor.SelectionEngine.SelectUnderMouse(this._oldPosition, (MouseEventArgs) e);
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
                this.Editor.SelectionEngine.LatestSelected = num;
              this.ContextMenu.IsOpen = true;
            }
label_38:
            if (this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) != this._cbZoom)
              e.Handled = true;
          }
          this.Editor.FrameSelector.OnAnimationPlaying(0);
        }
        else if (this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) != this._cbZoom)
          e.Handled = true;
        this._operationsEnabled = false;
        this.ReleaseMouseCapture();
        this.UpdateSelection(new Rect(), false);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private double _getDistance(System.Windows.Point point1, System.Windows.Point point2) => GRF.Graphics.Point.CalculateDistance(point1.ToGrfPoint(), point2.ToGrfPoint());

    private void _setTranslate()
    {
      this.Editor.Act.Commands.Begin();
      try
      {
        foreach (LayerDraw selectedLayerDraw in this.Editor.SelectionEngine.SelectedLayerDraws)
          selectedLayerDraw.Translate();
      }
      finally
      {
        this.Editor.Act.Commands.EndEdit();
      }
    }

    private void _setScale()
    {
      this.Editor.Act.Commands.Begin();
      try
      {
        foreach (LayerDraw selectedLayerDraw in this.Editor.SelectionEngine.SelectedLayerDraws)
          selectedLayerDraw.Scale();
      }
      finally
      {
        this.Editor.Act.Commands.EndEdit();
      }
    }

    private void _setRotated()
    {
      this.Editor.Act.Commands.Begin();
      try
      {
        foreach (LayerDraw selectedLayerDraw in this.Editor.SelectionEngine.SelectedLayerDraws)
          selectedLayerDraw.Rotate();
      }
      finally
      {
        this.Editor.Act.Commands.EndEdit();
      }
    }

    private void _framePreview_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        this._isAnyDown = true;
        this._hasMoved = false;
        if (Keyboard.FocusedElement != this._cbZoom)
          Keyboard.Focus((IInputElement) this.Editor.GridPrimary);
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
          if (e.LeftButton == MouseButtonState.Pressed && !this.Editor.FrameSelector.IsPlaying)
          {
            if (this.Editor.SelectionEngine.SelectedItems.Count > 0)
            {
              if (this._noSelectedComponentsUnderMouse((MouseEventArgs) e) && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
              {
                this._operationsEnabled = false;
                return;
              }
              foreach (LayerDraw selectedLayerDraw in this.Editor.SelectionEngine.SelectedLayerDraws)
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

    private void _framePreview_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released || this.ContextMenu.IsOpen)
          return;
        System.Windows.Point position = e.GetPosition((IInputElement) this);
        double deltaX = position.X - this._oldPosition.X;
        double deltaY = position.Y - this._oldPosition.Y;
        if (deltaX == 0.0 && deltaY == 0.0 || this.GetObjectAtPoint<ComboBox>(e.GetPosition((IInputElement) this)) == this._cbZoom && !this.IsMouseCaptured)
          return;
        GRF.Graphics.Point point1 = position.ToGrfPoint() - this._recentPosition.ToGrfPoint();
        if (!this._favoriteOrientation.HasValue)
          this._favoriteOrientation = new FramePreview.ScaleDirection?((double) point1.X * (double) point1.X > (double) point1.Y * (double) point1.Y ? FramePreview.ScaleDirection.Horizontal : FramePreview.ScaleDirection.Vertical);
        if (e.RightButton == MouseButtonState.Pressed && this._isAnyDown)
        {
          this._relativeCenter.X += deltaX / this.Canva.ActualWidth;
          this._relativeCenter.Y += deltaY / this.Canva.ActualHeight;
          this._oldPosition = position;
          this.SizeUpdate();
          this._hasMoved = true;
        }
        if (this.Editor.FrameSelector.IsPlaying)
          return;
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
            this.Editor.FrameSelector.OnAnimationPlaying(1);
            List<LayerDraw> selectedLayerDraws = this.Editor.SelectionEngine.SelectedLayerDraws;
            List<Layer> list = ((IEnumerable<Layer>) this.Editor.SelectionEngine.SelectedLayers).ToList<Layer>();
            GRF.Graphics.Point point2 = new GRF.Graphics.Point(0.0f, 0.0f);
            foreach (Layer layer in list)
            {
              point2.X += (float) layer.OffsetX;
              point2.Y += (float) layer.OffsetY;
            }
            point2.X = (float) ((double) point2.X / (double) list.Count * this.ZoomEngine.Scale) + (float) this.CenterX;
            point2.Y = (float) ((double) point2.Y / (double) list.Count * this.ZoomEngine.Scale) + (float) this.CenterY;
            Vertex diffVector = new Vertex(this._oldPosition.ToGrfPoint() - point2);
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
                  double x = (position.ToGrfPoint() - point2).Lenght / (this._oldPosition.ToGrfPoint() - point2).Lenght;
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
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    public void UpdateSelection(Rect rect, bool show)
    {
      if (this.Editor.FrameSelector.IsPlaying)
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
        this.Editor.SelectionEngine.Select(rect, this.ZoomEngine, new System.Windows.Point((double) this.CenterX, (double) this.CenterY));
      }
      else
      {
        if (selectionDraw == null)
          return;
        selectionDraw.Visible = false;
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

    private void _framePreview_SizeChanged(object sender, SizeChangedEventArgs e) => this.SizeUpdate();

    public void Init(IPreviewEditor editor)
    {
      this.Editor = editor;
      this.Editor.ReferencesChanged += (ActEditorWindow.ActEditorEventDelegate) (s => this.UpdateAndSelect());
      this.Editor.FrameSelector.FrameChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
      this.Editor.FrameSelector.SpecialFrameChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
      this.Editor.FrameSelector.ActionChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
      this.Editor.ActLoaded += new ActEditorWindow.ActEditorEventDelegate(this._actEditor_ActLoaded);
      this.InteractionEngine = (IEditorInteractionEngine) new DefaultInteractionEngine(this, editor);
    }

    public void Init(Act act)
    {
    }

    private void _actEditor_ActLoaded(object sender)
    {
      if (this.Editor.Act == null)
        return;
      Act act = this.Editor.Act;
      act.Commands.CommandUndo += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) ((s, e) => this.Update());
      act.Commands.CommandRedo += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) ((s, e) => this.Update());
    }

    public void Update(int layerIndex) => this._components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary))?.Render((IPreview) this, layerIndex);

    public void Update()
    {
      this._updateBackground();
      if (this.Editor == null)
        return;
      while (this._components.Count > 2)
      {
        this._components[2].Remove((IPreview) this);
        this._components.RemoveAt(2);
      }
      if (this.CustomUpdate != null)
      {
        this.CustomUpdate(this._components);
      }
      else
      {
        foreach (ReferenceControl referenceControl in this.Editor.References.Where<ReferenceControl>((Func<ReferenceControl, bool>) (p => p.ShowReference && p.Mode == ZMode.Back)))
          this._components.Add((DrawingComponent) new ActDraw(referenceControl.Act, this.Editor));
        if (this.Editor.Act != null)
        {
          ActDraw actDraw = new ActDraw(this.Editor.Act, this.Editor);
          actDraw.Selected += new DrawingComponent.DrawingComponentDelegate(this._primary_Selected);
          this._components.Add((DrawingComponent) actDraw);
        }
        foreach (ReferenceControl referenceControl in this.Editor.References.Where<ReferenceControl>((Func<ReferenceControl, bool>) (p => p.ShowReference && p.Mode == ZMode.Front)))
          this._components.Add((DrawingComponent) new ActDraw(referenceControl.Act, this.Editor));
        Act act = this.Editor.Act;
        if (act != null)
        {
          int selectedAction = this.Editor.SelectedAction;
          int selectedFrame = this.Editor.SelectedFrame;
          GRF.FileFormats.ActFormat.Frame frame = act.TryGetFrame(selectedAction, selectedFrame);
          if (frame != null && GrfEditorConfiguration.ShowAnchors && frame.Anchors.Count > 0)
          {
            foreach (Anchor anchor in frame.Anchors)
              this._components.Add((DrawingComponent) new AnchorDraw(anchor)
              {
                Visible = true
              });
          }
        }
      }
      foreach (DrawingComponent component in this._components)
        component.Render((IPreview) this);
    }

    public void UpdateAndSelect()
    {
      this.Update();
      this.Editor.SelectionEngine.RefreshSelection();
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
          layerList.Add(this.Editor.Act[this.Editor.SelectedAction, this.Editor.SelectedFrame].Layers[index]);
      }
      return layerList.ToArray();
    }

    private void _primary_Selected(object sender, int index, bool selected)
    {
      if (selected)
        this.Editor.SelectionEngine.AddSelection(index);
      else
        this.Editor.SelectionEngine.RemoveSelection(index);
      this.OnSelected(index, selected);
    }

    public void SizeUpdate()
    {
      this._updateBackground();
      if (this.Editor == null)
        return;
      foreach (DrawingComponent component in this._components)
        component.QuickRender((IPreview) this);
    }

    private void _updateBackground()
    {
      try
      {
        if (this._gridBackground.Background is VisualBrush)
        {
          if (this.ZoomEngine.Scale < 0.45)
            ((TileBrush) this._gridBackground.Background).Viewport = new Rect(this.RelativeCenter.X, this.RelativeCenter.Y, 16.0 / this._gridBackground.ActualWidth, 16.0 / this._gridBackground.ActualHeight);
          else
            ((TileBrush) this._gridBackground.Background).Viewport = new Rect(this.RelativeCenter.X, this.RelativeCenter.Y, 16.0 / (this._gridBackground.ActualWidth / this.ZoomEngine.Scale), 16.0 / (this._gridBackground.ActualHeight / this.ZoomEngine.Scale));
        }
        else
        {
          if (!(this._gridBackground.Background is ImageBrush background))
            return;
          background.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
          double pixelWidth = (double) ((BitmapSource) background.ImageSource).PixelWidth;
          double pixelHeight = (double) ((BitmapSource) background.ImageSource).PixelHeight;
          background.Viewport = new Rect(this.RelativeCenter.X + pixelWidth / (this._gridBackground.ActualWidth / this.ZoomEngine.Scale) / 2.0, this.RelativeCenter.Y + pixelHeight / (this._gridBackground.ActualHeight / this.ZoomEngine.Scale) / 2.0, pixelWidth / (this._gridBackground.ActualWidth / this.ZoomEngine.Scale), pixelHeight / (this._gridBackground.ActualHeight / this.ZoomEngine.Scale));
        }
      }
      catch
      {
      }
    }

    public void Reset()
    {
      this.Editor.SelectionEngine.ClearSelection();
      this.Update();
    }

    private void _cbZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._cbZoom.SelectedIndex < 0)
        return;
      this._zoomEngine.SetZoom(double.Parse(((string) ((ContentControl) this._cbZoom.SelectedItem).Content).Replace(" %", "")) / 100.0);
      this._cbZoom.Text = this._zoomEngine.ScaleText;
      this.SizeUpdate();
    }

    private void _cbZoom_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      try
      {
        string s = this._cbZoom.Text.Replace(" ", "").Replace("%", "");
        this._cbZoom.SelectedIndex = -1;
        this._zoomEngine.SetZoom(double.Parse(s) / 100.0);
        this._cbZoom.Text = this._zoomEngine.ScaleText;
        this.SizeUpdate();
        e.Handled = true;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _cbZoom_MouseEnter(object sender, MouseEventArgs e)
    {
      this._cbZoom.Opacity = 1.0;
      this._cbZoom.StaysOpenOnEdit = true;
    }

    private void _cbZoom_MouseLeave(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Released)
        return;
      this._cbZoom.Opacity = 0.7;
    }

    private void _miDelete_Click(object sender, RoutedEventArgs e)
    {
      if (this.Editor.LayerEditor == null)
        return;
      this.Editor.LayerEditor.Delete();
    }

    private void _miFront_Click(object sender, RoutedEventArgs e)
    {
      if (this.Editor.LayerEditor == null)
        return;
      this.Editor.LayerEditor.BringToFront();
    }

    private void _miBack_Click(object sender, RoutedEventArgs e)
    {
      if (this.Editor.LayerEditor == null)
        return;
      this.Editor.LayerEditor.BringToBack();
    }

    private void _miActionFront_Click(object sender, RoutedEventArgs e)
    {
      ActionLayerMove actionLayerMove = new ActionLayerMove(ActionLayerMove.MoveDirection.Down, this.Editor);
      if (!actionLayerMove.CanExecute(this.Act, this.SelectedAction, this.SelectedFrame, this.Editor.SelectionEngine.SelectedItems.ToArray<int>()))
        return;
      actionLayerMove.Execute(this.Act, this.SelectedAction, this.SelectedFrame, this.Editor.SelectionEngine.SelectedItems.ToArray<int>());
    }

    private void _miActionBack_Click(object sender, RoutedEventArgs e)
    {
      ActionLayerMove actionLayerMove = new ActionLayerMove(ActionLayerMove.MoveDirection.Up, this.Editor);
      if (!actionLayerMove.CanExecute(this.Act, this.SelectedAction, this.SelectedFrame, this.Editor.SelectionEngine.SelectedItems.ToArray<int>()))
        return;
      actionLayerMove.Execute(this.Act, this.SelectedAction, this.SelectedFrame, this.Editor.SelectionEngine.SelectedItems.ToArray<int>());
    }

    private void _miCopy_Click(object sender, RoutedEventArgs e) => this.Copy();

    private void _miCut_Click(object sender, RoutedEventArgs e) => this.Cut();

    private void _miInvert_Click(object sender, RoutedEventArgs e) => this.Editor.SelectionEngine.SelectReverse(new HashSet<int>((IEnumerable<int>) this.Editor.SelectionEngine.CurrentlySelected));

    private void _miSelect_Click(object sender, RoutedEventArgs e)
    {
      ActDraw main = this.Editor.SelectionEngine.Main;
      if (main == null)
        return;
      int latestSelected = this.Editor.SelectionEngine.LatestSelected;
      if (latestSelected <= -1 || latestSelected >= main.Components.Count)
        return;
      Layer layer = ((LayerDraw) main.Components[latestSelected]).Layer;
      if (this.Editor.Act.Sprite.GetImage(layer) == null)
        return;
      this.Editor.SpriteSelector.Select(layer.GetAbsoluteSpriteId(this.Editor.Act.Sprite));
    }

    private bool _imageExists()
    {
      ActDraw main = this.Editor.SelectionEngine.Main;
      if (main != null)
      {
        int latestSelected = this.Editor.SelectionEngine.LatestSelected;
        if (latestSelected > -1 && latestSelected < main.Components.Count)
          return this.Editor.Act.Sprite.GetImage(((LayerDraw) main.Components[latestSelected]).Layer) != null;
      }
      return false;
    }

    public void EditAnchors()
    {
      if (this.Editor.FrameSelector.IsPlaying || this._gridBackground.IsMouseCaptured || this._anchorEdit)
        return;
      if (!GrfEditorConfiguration.ShowAnchors)
      {
        ErrorHandler.HandleException("You must turn on the anchors by going in Anchors > Show anchors before editing them.");
      }
      else
      {
        this._anchorEdit = true;
        if (this.AnchorIndex >= this.Editor.Act[this.Editor.SelectedAction, this.Editor.SelectedFrame].Anchors.Count)
        {
          this.Editor.Act.Commands.SetAnchorPosition(this.Editor.SelectedAction, this.Editor.SelectedFrame, 0, 0, this.AnchorIndex);
          this.Update();
        }
        Anchor anchor = this.Editor.Act[this.Editor.SelectedAction, this.Editor.SelectedFrame].Anchors[this.AnchorIndex];
        this._oldAnchorPoint = new System.Windows.Point((double) anchor.OffsetX, (double) anchor.OffsetY);
        this._gridBackground.CaptureMouse();
        this.Editor.Element.PreviewKeyDown += new KeyEventHandler(this._actEditor_PreviewKeyDown);
        this._gridBackground.PreviewMouseMove += new MouseEventHandler(this._gridBackground_MouseMove);
        this._gridBackground.PreviewMouseUp += new MouseButtonEventHandler(this._gridBackground_MouseUp);
        this._gridBackground.PreviewMouseDown += new MouseButtonEventHandler(this._gridBackground_MouseDown);
        this._setAnchorPosition(Mouse.GetPosition((IInputElement) this._gridBackground));
      }
    }

    private void _actEditor_PreviewKeyDown(object sender, KeyEventArgs e) => this._gridBackground.ReleaseMouseCapture();

    private void _gridBackground_MouseDown(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void _gridBackground_MouseUp(object sender, MouseButtonEventArgs e)
    {
      this._setAnchorPosition(e.GetPosition((IInputElement) this._gridBackground));
      this._gridBackground.ReleaseMouseCapture();
      e.Handled = true;
    }

    private void _setAnchorPosition(System.Windows.Point absolutePosition)
    {
      if (!this._anchorEdit)
        return;
      int x = (int) ((absolutePosition.X - (double) this.CenterX) / this.ZoomEngine.Scale);
      int y = (int) ((absolutePosition.Y - (double) this.CenterY) / this.ZoomEngine.Scale);
      System.Windows.Point point = new System.Windows.Point((double) x, (double) y);
      Anchor anchor = this.Editor.Act[this.Editor.SelectedAction, this.Editor.SelectedFrame].Anchors[this.AnchorIndex];
      anchor.OffsetX = x;
      anchor.OffsetY = y;
      this._components.OfType<AnchorDraw>().ToList<AnchorDraw>()[this.AnchorIndex]?.RenderOffsets((IPreview) this, point);
      foreach (DrawingComponent drawingComponent in this._components.OfType<ActDraw>().Where<ActDraw>((Func<ActDraw, bool>) (p => !p.Primary)))
        drawingComponent.Render((IPreview) this);
    }

    private void _gridBackground_MouseMove(object sender, MouseEventArgs e)
    {
      if (!this._anchorEdit)
        return;
      this._setAnchorPosition(e.GetPosition((IInputElement) this._gridBackground));
      e.Handled = true;
    }

    private void _gridBackground_LostMouseCapture(object sender, MouseEventArgs e)
    {
      if (!this._anchorEdit)
        return;
      try
      {
        this.Editor.Element.PreviewKeyDown -= new KeyEventHandler(this._actEditor_PreviewKeyDown);
        this._gridBackground.PreviewMouseMove -= new MouseEventHandler(this._gridBackground_MouseMove);
        this._gridBackground.PreviewMouseUp -= new MouseButtonEventHandler(this._gridBackground_MouseUp);
        this._gridBackground.PreviewMouseDown -= new MouseButtonEventHandler(this._gridBackground_MouseDown);
        Anchor anchor = this.Editor.Act[this.Editor.SelectedAction, this.Editor.SelectedFrame].Anchors[this.AnchorIndex];
        int offsetX = anchor.OffsetX;
        int offsetY = anchor.OffsetY;
        anchor.OffsetX = (int) this._oldAnchorPoint.X;
        anchor.OffsetY = (int) this._oldAnchorPoint.Y;
        this.Editor.Act.Commands.SetAnchorPosition(this.Editor.SelectedAction, this.Editor.SelectedFrame, offsetX, offsetY, this.AnchorIndex);
      }
      catch
      {
      }
      finally
      {
        this._anchorEdit = false;
      }
    }

    public int AnchorIndex { get; set; }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/framepreview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._miDelete = (TkMenuItem) target;
          this._miDelete.Click += new RoutedEventHandler(this._miDelete_Click);
          break;
        case 2:
          this._miInvert = (TkMenuItem) target;
          this._miInvert.Click += new RoutedEventHandler(this._miInvert_Click);
          break;
        case 3:
          this._miFront = (TkMenuItem) target;
          this._miFront.Click += new RoutedEventHandler(this._miFront_Click);
          break;
        case 4:
          this._miBack = (TkMenuItem) target;
          this._miBack.Click += new RoutedEventHandler(this._miBack_Click);
          break;
        case 5:
          this._miActionFront = (TkMenuItem) target;
          this._miActionFront.Click += new RoutedEventHandler(this._miActionFront_Click);
          break;
        case 6:
          this._miActionBack = (TkMenuItem) target;
          this._miActionBack.Click += new RoutedEventHandler(this._miActionBack_Click);
          break;
        case 7:
          this._miCopy = (TkMenuItem) target;
          this._miCopy.Click += new RoutedEventHandler(this._miCopy_Click);
          break;
        case 8:
          this._miCut = (TkMenuItem) target;
          this._miCut.Click += new RoutedEventHandler(this._miCut_Click);
          break;
        case 9:
          this._miSelect = (TkMenuItem) target;
          this._miSelect.Click += new RoutedEventHandler(this._miSelect_Click);
          break;
        case 10:
          this._gridBackground = (Grid) target;
          this._gridBackground.LostMouseCapture += new MouseEventHandler(this._gridBackground_LostMouseCapture);
          break;
        case 11:
          this._primary = (Canvas) target;
          break;
        case 12:
          this._gridZoom = (Grid) target;
          break;
        case 13:
          this._cbZoom = (ComboBox) target;
          this._cbZoom.SelectionChanged += new SelectionChangedEventHandler(this._cbZoom_SelectionChanged);
          this._cbZoom.PreviewKeyDown += new KeyEventHandler(this._cbZoom_PreviewKeyDown);
          this._cbZoom.MouseLeave += new MouseEventHandler(this._cbZoom_MouseLeave);
          this._cbZoom.MouseEnter += new MouseEventHandler(this._cbZoom_MouseEnter);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    System.Windows.Point IPreview.PointToScreen([In] System.Windows.Point obj0) => this.PointToScreen(obj0);

    public enum ScaleDirection
    {
      Horizontal,
      Vertical,
      Both,
    }

    public delegate void ConstructorCalledDelegate(object sender);

    public delegate void FramePreviewEventDelegate(object sender);
  }
}
