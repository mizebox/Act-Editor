// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.LayerEditor
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ActEditor.Core.Scripts;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.Threading;
using GrfToWpfBridge;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using Utilities.Commands;
using Utilities.Extension;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class LayerEditor : System.Windows.Controls.UserControl, IComponentConnector
  {
    private readonly DispatcherTimer _timer;
    private readonly LayerEditor.UpdateThread _updateThread = new LayerEditor.UpdateThread();
    private readonly Stopwatch _watch = new Stopwatch();
    private ActEditorWindow _actEditor;
    private bool _hasMoved;
    private int _layerMouseDown = -1;
    private System.Windows.Point _oldPosition;
    private int _previousMouseDown = -1;
    private LayerControlProvider _provider;
    internal TkMenuItem _miDelete;
    internal TkMenuItem _miInvert;
    internal TkMenuItem _miFront;
    internal TkMenuItem _miBack;
    internal TkMenuItem _miActionFront;
    internal TkMenuItem _miActionBack;
    internal TkMenuItem _miCopy;
    internal TkMenuItem _miCut;
    internal TkMenuItem _miSelect;
    internal Grid _displayGrid;
    internal LayerControlHeader _sfch;
    internal ScrollViewer _sv;
    internal StackPanel _sp;
    internal System.Windows.Controls.ListView _listView;
    internal Line _lineMoveLayer;
    private bool _contentLoaded;

    public LayerEditor()
    {
      this.InitializeComponent();
      this._displayGrid.ColumnDefinitions[1] = new ColumnDefinition()
      {
        Width = new GridLength(SystemParameters.VerticalScrollBarWidth)
      };
      this._timer = new DispatcherTimer();
      this._timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
      this._timer.Tick += new EventHandler(this._timer_Tick);
      this._updateThread.Start(this);
      this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this._layerEditor_PreviewKeyDown);
    }

    public LayerControlProvider Provider => this._provider;

    public bool DoNotRemove { get; set; }

    public bool IgnoreUpdate { get; set; }

    private string _hasBeenDrawn { get; set; }

    public int SelectedAction => this._actEditor._frameSelector.SelectedAction;

    public int SelectedFrame => this._actEditor._frameSelector.SelectedFrame;

    public Func<bool> ImageExists => new Func<bool>(this._imageExists);

    private void _layerEditor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      int num = (int) (Keyboard.Modifiers & ModifierKeys.Shift);
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
      if (this._isUnderViewport())
        this._sv.ScrollToVerticalOffset(this._sv.VerticalOffset + this._sfch.ActualHeight);
      else if (this._isAboveViewport())
        this._sv.ScrollToVerticalOffset(this._sv.VerticalOffset - this._sfch.ActualHeight);
      else
        this._timer.Stop();
    }

    private bool _isUnderViewport()
    {
      System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;
      System.Windows.Point point = this.PointFromScreen(new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y));
      return point.Y > this.ActualHeight && point.Y < this.ActualHeight + 50.0;
    }

    private bool _isAboveViewport()
    {
      System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;
      return this.GetObjectAtPoint<LayerControlHeader>(this.PointFromScreen(new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y))) != null;
    }

    public void Init(ActEditorWindow actEditor)
    {
      this._actEditor = actEditor;
      this._actEditor._frameSelector.FrameChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
      this._actEditor._frameSelector.ActionChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this.Update());
      this._actEditor.ActLoaded += (ActEditorWindow.ActEditorEventDelegate) delegate
      {
        if (actEditor.Act == null)
          return;
        this._hasBeenDrawn = (string) null;
        actEditor.Act.Commands.CommandRedo += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) ((s, e) => this._fieldsUpdate());
        actEditor.Act.Commands.CommandUndo += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) ((s, e) => this._fieldsUpdate());
      };
      this._actEditor._framePreview.Selected += new DrawingComponent.DrawingComponentDelegate(this._framePreview_Selected);
      this._actEditor._frameSelector.AnimationPlaying += new ActIndexSelector.FrameIndexChangedDelegate(this._frameSelector_AnimationPlaying);
      this._actEditor.Loaded += (RoutedEventHandler) delegate
      {
        this._provider = new LayerControlProvider(this._actEditor);
      };
      this.PreviewMouseDown += new MouseButtonEventHandler(this._layerEditor_MouseDown);
      this.PreviewMouseUp += new MouseButtonEventHandler(this._layerEditor_MouseUp);
      this.MouseMove += new System.Windows.Input.MouseEventHandler(this._layerEditor_MouseMove);
      this.MouseLeave += new System.Windows.Input.MouseEventHandler(this._layerEditor_MouseLeave);
      this.DragOver += new System.Windows.DragEventHandler(this._layerEditor_DragOver);
      this.DragEnter += new System.Windows.DragEventHandler(this._layerEditor_DragEnter);
      this.DragLeave += new System.Windows.DragEventHandler(this._layerEditor_DragLeave);
      this.Drop += new System.Windows.DragEventHandler(this._layerEditor_Drop);
    }

    private void _frameSelector_AnimationPlaying(object sender, int actionindex)
    {
      if (actionindex == 0)
      {
        this._watch.Stop();
        this.DoNotRemove = false;
        this.IgnoreUpdate = false;
        int selectedAction = this._actEditor._frameSelector.SelectedAction;
        int selectedFrame = this._actEditor._frameSelector.SelectedFrame;
        if (this._hasBeenDrawn == null || this._hasBeenDrawn != selectedAction.ToString() + "," + (object) selectedFrame)
          this.Dispatch<LayerEditor>((Action<LayerEditor>) (p => p.Update()));
        this.Dispatch<LayerEditor, bool>((Func<LayerEditor, bool>) (p => p.IsEnabled = true));
      }
      else
      {
        if (!GrfEditorConfiguration.ActEditorRefreshLayerEditor)
          this.Dispatch<LayerEditor, bool>((Func<LayerEditor, bool>) (p => p.IsEnabled = false));
        this.DoNotRemove = true;
      }
    }

    private void _layerEditor_DragLeave(object sender, System.Windows.DragEventArgs e) => this._mouseLeave(true);

    private void _layerEditor_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
      if (e.Data.GetData("ImageIndex") != null)
        this._enter(e.GetPosition((IInputElement) this));
      else
        e.Effects = System.Windows.DragDropEffects.None;
    }

    private void _layerEditor_DragOver(object sender, System.Windows.DragEventArgs e)
    {
      if (e.Data.GetData("ImageIndex") != null)
        this._move(e.GetPosition((IInputElement) this), true);
      else
        e.Effects = System.Windows.DragDropEffects.None;
    }

    private void _layerEditor_Drop(object sender, System.Windows.DragEventArgs e)
    {
      try
      {
        object data = e.Data.GetData("ImageIndex");
        if (data == null)
          return;
        int absoluteSpriteIndex = (int) data;
        int indexDrop = this._getIndexDrop(e.GetPosition((IInputElement) this), true);
        if (indexDrop <= -1)
          indexDrop = this._getIndexDrop(e.GetPosition((IInputElement) this), true);
        if (indexDrop <= -1)
          return;
        List<Tuple<Layer, bool>> selection = this.GetSelection();
        if (this._actEditor.Act == null)
          return;
        this._actEditor.Act.Commands.LayerAdd(this._actEditor.SelectedAction, this._actEditor.SelectedFrame, indexDrop, absoluteSpriteIndex);
        this.Update();
        this._actEditor._framePreview.Update();
        this._actEditor.SelectionEngine.SetSelection(this.GenerateSelection(selection));
        this.UpdateSelection();
        e.Handled = true;
      }
      finally
      {
        this._lineMoveLayer.Visibility = Visibility.Hidden;
        this._hasMoved = false;
        this.ReleaseMouseCapture();
      }
    }

    private void _layerEditor_MouseDown(object sender, MouseButtonEventArgs e) => this._enter(e.GetPosition((IInputElement) this));

    private void _layerEditor_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => this._mouseLeave();

    private void _layerEditor_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) => this._move(e.GetPosition((IInputElement) this));

    private void _layerEditor_MouseUp(object sender, MouseButtonEventArgs e)
    {
      try
      {
        int indexDrop = this._getIndexDrop(e.GetPosition((IInputElement) this));
        if (indexDrop > -1)
        {
          List<Tuple<Layer, bool>> selection = this.GetSelection();
          if (this._actEditor.Act == null)
            return;
          int layerMouseDown = this._layerMouseDown;
          int count = 1;
          LayerControl layerControl = this._provider.Get(layerMouseDown);
          if (layerMouseDown >= this._sp.Children.Count)
            return;
          if (layerControl.IsSelected)
          {
            for (int index = layerMouseDown - 1; index >= 0 && this._provider.Get(index).IsSelected; --index)
              --layerMouseDown;
            for (int index = layerMouseDown + 1; index < this._sp.Children.Count && this._provider.Get(index).IsSelected; ++index)
              ++count;
          }
          if (this._actEditor.Act.Commands.LayerSwitchRange(this._actEditor._frameSelector.SelectedAction, this._actEditor._frameSelector.SelectedFrame, layerMouseDown, count, indexDrop))
          {
            this.Update();
            this._actEditor._framePreview.Update();
            this._actEditor.SelectionEngine.SetSelection(this.GenerateSelection(selection));
            this.UpdateSelection();
            e.Handled = true;
          }
        }
        if (e.ChangedButton != MouseButton.Right)
          return;
        LayerControl objectAtPoint = this.GetObjectAtPoint<LayerControl>(e.GetPosition((IInputElement) this));
        bool flag = false;
        if (objectAtPoint != null)
        {
          int index = this._sp.Children.IndexOf((UIElement) objectAtPoint);
          if (index > -1)
          {
            this._actEditor.SelectionEngine.Select(index);
            flag = true;
            this._actEditor.SelectionEngine.LatestSelected = index;
          }
        }
        this._miDelete.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
        if (!flag)
        {
          e.Handled = true;
        }
        else
        {
          this.ContextMenu.IsOpen = true;
          e.Handled = true;
        }
      }
      finally
      {
        this._lineMoveLayer.Visibility = Visibility.Hidden;
        this._hasMoved = false;
        this.ReleaseMouseCapture();
      }
    }

    private void _enter(System.Windows.Point position)
    {
      this._layerMouseDown = -1;
      LayerControl objectAtPoint = this.GetObjectAtPoint<LayerControl>(position);
      if (objectAtPoint != null)
        this._layerMouseDown = this.GetObjectAtPoint<System.Windows.Controls.TextBox>(position) != null ? -1 : this._sp.Children.IndexOf((UIElement) objectAtPoint);
      this._oldPosition = position;
      this._previousMouseDown = -1;
    }

    private void _move(System.Windows.Point current, bool overrideMouse = false)
    {
      if (!overrideMouse && (current == this._oldPosition || GRF.Graphics.Point.CalculateDistance(current.ToGrfPoint(), this._oldPosition.ToGrfPoint()) <= 5.0))
        return;
      if (Mouse.LeftButton == MouseButtonState.Pressed || overrideMouse)
      {
        this._hasMoved = true;
        if (this._layerMouseDown <= -1 && !overrideMouse)
          return;
        if (!overrideMouse && this._provider.Get(this._layerMouseDown) != null)
          this._provider.Get(this._layerMouseDown).IsPreviewSelected = true;
        int indexDrop = this._getIndexDrop(current, overrideMouse);
        if (indexDrop <= -1)
          return;
        if (!this.IsMouseCaptured)
          this.CaptureMouse();
        this._lineMoveLayer.Stroke = (System.Windows.Media.Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorSpriteSelectionBorder.ToColor());
        this._lineMoveLayer.Visibility = Visibility.Visible;
        double actualHeight = this._sfch.ActualHeight;
        for (int index = 0; index < indexDrop; ++index)
          actualHeight += LayerControl.ActualHeightBuffered;
        double num = actualHeight - this._sv.VerticalOffset;
        if (num >= this.ActualHeight - 1.0)
          num = this.ActualHeight - 1.0;
        if (this._isAboveViewport())
        {
          if (this._sv.VerticalOffset > 0.0)
            this._lineMoveLayer.Visibility = Visibility.Hidden;
          else
            num = this._sfch.ActualHeight;
        }
        if (this._isUnderViewport())
        {
          if (this._sv.VerticalOffset < this._sv.ScrollableHeight)
            this._lineMoveLayer.Visibility = Visibility.Hidden;
          else if (this._sv.ScrollableHeight > 0.0)
            num = this.ActualHeight - 1.0;
        }
        if (num < this._sfch.ActualHeight)
          this._lineMoveLayer.Visibility = Visibility.Hidden;
        this._lineMoveLayer.Margin = new Thickness(0.0, num - 2.0, SystemParameters.VerticalScrollBarWidth, 0.0);
      }
      else
      {
        LayerControl controlUnderMouse = this._getControlUnderMouse();
        for (int index = 0; index < this._sp.Children.Count; ++index)
        {
          LayerControl layerControl = this._provider.Get(index);
          if (layerControl != controlUnderMouse)
            layerControl.IsPreviewSelected = false;
        }
        if (controlUnderMouse == null)
          return;
        controlUnderMouse.IsPreviewSelected = true;
      }
    }

    private void _mouseLeave(bool hideBar = false)
    {
      for (int index = 0; index < this._sp.Children.Count; ++index)
        this._provider.Get(index).IsPreviewSelected = false;
      if (!hideBar)
        return;
      this._lineMoveLayer.Visibility = Visibility.Hidden;
      this._hasMoved = false;
    }

    private LayerControl _getControlUnderMouse() => this.GetObjectAtPoint<LayerControl>(Mouse.GetPosition((IInputElement) this));

    private int _getIndexDrop(System.Windows.Point point, bool overrideMouse = false)
    {
      LayerControl objectAtPoint1 = this.GetObjectAtPoint<LayerControl>(point);
      LayerControlHeader objectAtPoint2 = this.GetObjectAtPoint<LayerControlHeader>(point);
      Line objectAtPoint3 = this.GetObjectAtPoint<Line>(point);
      if ((this._isAboveViewport() || objectAtPoint2 != null) && this._sv.VerticalOffset != 0.0 && !this._timer.IsEnabled)
      {
        this._timer_Tick((object) null, (EventArgs) null);
        this._timer.Start();
        return this._previousMouseDown;
      }
      if (this._isUnderViewport() && this._sv.VerticalOffset != this._sv.ScrollableHeight && !this._timer.IsEnabled)
      {
        this._timer_Tick((object) null, (EventArgs) null);
        this._timer.Start();
        return this._previousMouseDown;
      }
      if (!this._hasMoved || this._layerMouseDown <= -1 && !overrideMouse)
        return -1;
      System.Windows.Point point1 = new System.Windows.Point(point.X, point.Y - this._sfch.ActualHeight);
      bool flag = point1.X < 0.0 || point1.Y < 0.0 || point1.X > this._sv.ViewportWidth || point1.Y > this._sv.ViewportHeight;
      int indexDrop1 = -1;
      if (this._isAboveViewport() && this._sv.VerticalOffset == 0.0)
      {
        int indexDrop2 = 0;
        this._previousMouseDown = indexDrop2;
        return indexDrop2;
      }
      if (objectAtPoint2 != null)
        return this._previousMouseDown;
      if (objectAtPoint1 != null)
        indexDrop1 = this._sp.Children.IndexOf((UIElement) objectAtPoint1);
      if (this._isUnderViewport() && this._sv.VerticalOffset == this._sv.ScrollableHeight)
      {
        int count = this._sp.Children.Count;
        this._previousMouseDown = count;
        return count;
      }
      if (flag || indexDrop1 < 0 && objectAtPoint3 != null)
      {
        if (this._previousMouseDown < 0 && objectAtPoint3 != null)
          this._previousMouseDown = this._getIndexDrop(new System.Windows.Point(point.X, point.Y + 6.0), overrideMouse);
        return this._previousMouseDown;
      }
      if (indexDrop1 < 0)
        indexDrop1 = this._sp.Children.Count;
      this._previousMouseDown = indexDrop1;
      return indexDrop1;
    }

    public HashSet<int> GenerateSelection(List<Tuple<Layer, bool>> selection)
    {
      HashSet<int> selection1 = new HashSet<int>();
      if (this._actEditor.Act == null)
        return selection1;
      GRF.FileFormats.ActFormat.Frame frame = this._actEditor.Act[this._actEditor._frameSelector.SelectedAction, this._actEditor._frameSelector.SelectedFrame];
      for (int index = 0; index < selection.Count; ++index)
      {
        if (selection[index].Item2)
        {
          int num = frame.Layers.IndexOf(selection[index].Item1);
          if (num > -1)
            selection1.Add(num);
        }
      }
      return selection1;
    }

    public List<Tuple<Layer, bool>> GetSelection()
    {
      List<Tuple<Layer, bool>> selection = new List<Tuple<Layer, bool>>();
      if (this._actEditor.Act == null)
        return selection;
      for (int index = 0; index < this._sp.Children.Count; ++index)
      {
        Layer layer = this._actEditor.Act[this._actEditor._frameSelector.SelectedAction, this._actEditor._frameSelector.SelectedFrame, index];
        selection.Add(new Tuple<Layer, bool>(layer, ((LayerControl) this._sp.Children[index]).IsSelected));
      }
      return selection;
    }

    public LayerControl Get(int layerIndex) => this._sp.Children[layerIndex] as LayerControl;

    public void Update()
    {
      if (this.DoNotRemove)
      {
        if (!GrfEditorConfiguration.ActEditorRefreshLayerEditor)
          return;
        this._updateThread.Add(new LayerEditor.UpdateInfo(this.SelectedAction, this.SelectedFrame));
      }
      else
        this.ThreadUpdate(this.SelectedFrame);
    }

    internal void ThreadUpdate(int selectedFrame)
    {
      Act act = this._actEditor.Act;
      if (act == null)
        return;
      if (this.DoNotRemove)
      {
        this._hasBeenDrawn = (string) null;
        this._specialUpdate(selectedFrame);
      }
      else
      {
        int numberOfLayers = this._actEditor.Frame.NumberOfLayers;
        int selectedAction = this._actEditor._frameSelector.SelectedAction;
        int selectedFrame1 = this._actEditor._frameSelector.SelectedFrame;
        this._hasBeenDrawn = selectedAction.ToString() + "," + (object) selectedFrame1;
        if (numberOfLayers < this._sp.Children.Count)
        {
          this._sp.Children.RemoveRange(numberOfLayers, this._sp.Children.Count - numberOfLayers);
          if (selectedFrame != this.SelectedFrame)
            return;
        }
        for (int index = 0; index < this._sp.Children.Count; ++index)
        {
          this._provider.Get(index).Set(act, selectedAction, selectedFrame1, index, false);
          if (selectedFrame != this.SelectedFrame)
            return;
        }
        for (int count = this._sp.Children.Count; count < numberOfLayers; ++count)
        {
          LayerControl element = this._provider.Get(count);
          element.Set(act, selectedAction, selectedFrame1, count, false);
          element.IsSelected = false;
          this._sp.Children.Add((UIElement) element);
          if (selectedFrame != this.SelectedFrame)
            break;
        }
      }
    }

    private void _specialUpdate(int selectedFrame)
    {
      if (!this.DoNotRemove)
        return;
      Act act = this._actEditor.Act;
      if (act == null || selectedFrame != this.Dispatch<int>((Func<int>) (() => this.SelectedFrame)))
        return;
      int numberOfLayers = this._actEditor.Frame.NumberOfLayers;
      int action = this._actEditor._frameSelector.SelectedAction;
      int frame = this._actEditor._frameSelector.SelectedFrame;
      int num1 = numberOfLayers;
      double num2 = 0.0;
      double verticalOffset = this._sv.VerticalOffset;
      double num3 = verticalOffset + this._sv.ViewportHeight;
      int i = 0;
      for (int index = num1; i < index; ++i)
      {
        if (num2 > num3)
          return;
        LayerControl ctr = this._provider.Get(i);
        this._sp.Dispatch<StackPanel>((Action<StackPanel>) delegate
        {
          if (i < this._sp.Children.Count)
            return;
          ctr.IsSelected = false;
          this._sp.Children.Add((UIElement) ctr);
        });
        if (num2 < verticalOffset)
        {
          num2 += LayerControl.ActualHeightBuffered;
          if (num2 < verticalOffset)
            continue;
        }
        else
          num2 += LayerControl.ActualHeightBuffered;
        if (!this.DoNotRemove)
          return;
        ctr.Dispatch((System.Action) (() => ctr.Set(act, action, frame, i, true)));
        if (selectedFrame != this.Dispatch<int>((Func<int>) (() => this.SelectedFrame)))
          return;
      }
      int num4 = this.Dispatch<int>((Func<int>) (() => this._sp.Children.Count));
      if (num4 <= 0)
        return;
      double num5 = this._provider.Get(0).ActualHeight * (double) num1;
      for (int index = num1; index < num4 && num5 <= num3; ++index)
      {
        LayerControl source = this._provider.Get(index);
        source.Dispatch(new System.Action(source.SetNull));
        num5 += LayerControl.ActualHeightBuffered;
        if (selectedFrame != this.Dispatch<int>((Func<int>) (() => this.SelectedFrame)) || !this.DoNotRemove)
          break;
      }
    }

    public void UpdateSelection()
    {
      for (int index = 0; index < this._sp.Children.Count; ++index)
        this._provider.Get(index).IsSelected = this._actEditor.SelectionEngine.SelectedItems.Contains(index);
    }

    private void _framePreview_Selected(object sender, int index, bool selected)
    {
      if (index < 0 || index >= this._sp.Children.Count)
        return;
      ((LayerControl) this._sp.Children[index]).IsSelected = selected;
    }

    private void _fieldsUpdate()
    {
      Act act = this._actEditor.Act;
      if (act == null)
        return;
      int selectedAction = this._actEditor._frameSelector.SelectedAction;
      int selectedFrame = this._actEditor._frameSelector.SelectedFrame;
      if (selectedAction >= act.NumberOfActions || selectedFrame >= act[selectedAction].NumberOfFrames)
        return;
      if (act[selectedAction, selectedFrame].NumberOfLayers != this._sp.Children.Count)
        this.Update();
      for (int index = 0; index < this._sp.Children.Count; ++index)
        ((LayerControl) this._sp.Children[index]).Set(act, selectedAction, selectedFrame, index, false);
    }

    public void Delete()
    {
      if (this._actEditor.Act == null)
        return;
      try
      {
        this._actEditor.Act.Commands.Begin();
        for (int index = this._sp.Children.Count - 1; index >= 0; --index)
        {
          if (this._provider.Get(index).IsSelected)
            this._actEditor.Act.Commands.LayerDelete(this._actEditor.SelectedAction, this._actEditor.SelectedFrame, index);
        }
      }
      catch (Exception ex)
      {
        this._actEditor.Act.Commands.CancelEdit();
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._actEditor.Act.Commands.End();
      }
      this._actEditor.SelectionEngine.DeselectAll();
      this._actEditor.Act.InvalidateVisual();
    }

    private void _miDelete_Click(object sender, RoutedEventArgs e) => this.Delete();

    public void Reset() => this._sp.Children.Clear();

    private void _miFront_Click(object sender, RoutedEventArgs e) => this.BringToFront();

    private void _miActionFront_Click(object sender, RoutedEventArgs e)
    {
      ActionLayerMove actionLayerMove = new ActionLayerMove(ActionLayerMove.MoveDirection.Down, (IPreviewEditor) this._actEditor);
      if (!actionLayerMove.CanExecute(this._actEditor.Act, this.SelectedAction, this.SelectedFrame, this._actEditor.SelectionEngine.SelectedItems.ToArray<int>()))
        return;
      actionLayerMove.Execute(this._actEditor.Act, this.SelectedAction, this.SelectedFrame, this._actEditor.SelectionEngine.SelectedItems.ToArray<int>());
    }

    private void _miActionBack_Click(object sender, RoutedEventArgs e)
    {
      ActionLayerMove actionLayerMove = new ActionLayerMove(ActionLayerMove.MoveDirection.Up, (IPreviewEditor) this._actEditor);
      if (!actionLayerMove.CanExecute(this._actEditor.Act, this.SelectedAction, this.SelectedFrame, this._actEditor.SelectionEngine.SelectedItems.ToArray<int>()))
        return;
      actionLayerMove.Execute(this._actEditor.Act, this.SelectedAction, this.SelectedFrame, this._actEditor.SelectionEngine.SelectedItems.ToArray<int>());
    }

    public void BringToFront()
    {
      if (this._actEditor.Act == null)
        return;
      this._bringTo(-1);
    }

    private void _bringTo(int index)
    {
      Layer[] selectedLayers = this._actEditor.SelectionEngine.SelectedLayers;
      Act act = this._actEditor.Act;
      if (selectedLayers.Length == 0 || act == null || act[this.SelectedAction, this.SelectedFrame].NumberOfLayers <= 1)
        return;
      int start = act[this.SelectedAction, this.SelectedFrame].NumberOfLayers - selectedLayers.Length;
      try
      {
        act.Commands.BeginNoDelay();
        foreach (int layerIndex in new List<int>((IEnumerable<int>) this._actEditor.SelectionEngine.CurrentlySelected).OrderByDescending<int, int>((Func<int, int>) (p => p)).ToList<int>())
          act.Commands.LayerDelete(this.SelectedAction, this.SelectedFrame, layerIndex);
        if (index < 0)
          act.Commands.LayerAdd(this.SelectedAction, this.SelectedFrame, selectedLayers);
        else
          act.Commands.LayerAdd(this.SelectedAction, this.SelectedFrame, selectedLayers, index);
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        act.Commands.End();
        act.InvalidateVisual();
        if (index < 0)
          this._actEditor.SelectionEngine.SetSelection(start, selectedLayers.Length);
        else
          this._actEditor.SelectionEngine.SetSelection(0, selectedLayers.Length);
      }
    }

    private void _miBack_Click(object sender, RoutedEventArgs e)
    {
      if (this._actEditor.Act == null)
        return;
      this._bringTo(0);
    }

    public void BringToBack()
    {
      if (this._actEditor.Act == null)
        return;
      this._bringTo(0);
    }

    private void _miCut_Click(object sender, RoutedEventArgs e) => this._actEditor._framePreview.Cut();

    private void _miCopy_Click(object sender, RoutedEventArgs e) => this._actEditor._framePreview.Copy();

    private void _miInvert_Click(object sender, RoutedEventArgs e) => this._actEditor.SelectionEngine.SelectReverse(new HashSet<int>((IEnumerable<int>) this._actEditor.SelectionEngine.CurrentlySelected));

    private void _miSelect_Click(object sender, RoutedEventArgs e)
    {
      ActDraw main = this._actEditor.SelectionEngine.Main;
      if (main == null)
        return;
      int latestSelected = this._actEditor.SelectionEngine.LatestSelected;
      if (latestSelected <= -1 || latestSelected >= main.Components.Count)
        return;
      Layer layer = ((LayerDraw) main.Components[latestSelected]).Layer;
      if (this._actEditor.Act.Sprite.GetImage(layer) == null)
        return;
      this._actEditor._spriteSelector.Select(layer.GetAbsoluteSpriteId(this._actEditor.Act.Sprite));
    }

    private bool _imageExists()
    {
      ActDraw main = this._actEditor.SelectionEngine.Main;
      if (main != null)
      {
        int latestSelected = this._actEditor.SelectionEngine.LatestSelected;
        if (latestSelected > -1 && latestSelected < main.Components.Count)
          return this._actEditor.Act.Sprite.GetImage(((LayerDraw) main.Components[latestSelected]).Layer) != null;
      }
      return false;
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/layereditor.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
          this._displayGrid = (Grid) target;
          break;
        case 11:
          this._sfch = (LayerControlHeader) target;
          break;
        case 12:
          this._sv = (ScrollViewer) target;
          break;
        case 13:
          this._sp = (StackPanel) target;
          break;
        case 14:
          this._listView = (System.Windows.Controls.ListView) target;
          break;
        case 15:
          this._lineMoveLayer = (Line) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public class UpdateInfo
    {
      public int ActionIndex;
      public int FrameIndex;

      public UpdateInfo(int actionIndex, int frameIndex)
      {
        this.ActionIndex = actionIndex;
        this.FrameIndex = frameIndex;
      }
    }

    public class UpdateThread : PausableThread
    {
      private readonly object _updateLock = new object();
      private readonly Queue<LayerEditor.UpdateInfo> _updateQueue = new Queue<LayerEditor.UpdateInfo>();
      private LayerEditor _layerEditor;

      public void Start(LayerEditor layerEditor)
      {
        this._layerEditor = layerEditor;
        this.IsPaused = true;
        GrfThread.StartSTA(new System.Action(this._start));
      }

      private void _start()
      {
        while (true)
        {
          LayerEditor.UpdateInfo updateInfo;
          do
          {
            if (this.IsPaused)
              this.Pause();
            bool flag = true;
            lock (this._updateLock)
            {
              if (this._updateQueue.Count == 0)
                flag = false;
            }
            if (!flag)
            {
              this.IsPaused = true;
            }
            else
            {
              updateInfo = (LayerEditor.UpdateInfo) null;
              lock (this._updateLock)
              {
                if (this._updateQueue.Count == 1)
                  updateInfo = this._updateQueue.Dequeue();
                else if (this._updateQueue.Count > 1)
                  this._updateQueue.Dequeue();
              }
            }
          }
          while (updateInfo == null);
          if (this._layerEditor.DoNotRemove)
            this._layerEditor.ThreadUpdate(updateInfo.FrameIndex);
          Thread.Sleep(20);
        }
      }

      public void Add(LayerEditor.UpdateInfo updateInfo)
      {
        lock (this._updateLock)
          this._updateQueue.Enqueue(updateInfo);
        if (!this.IsPaused)
          return;
        this.IsPaused = false;
      }
    }
  }
}
