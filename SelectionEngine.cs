// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.SelectionEngine
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.DrawingComponents;
using ActEditor.Core.WPF.EditorControls;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Utilities.Commands;
using Utilities.Tools;

namespace ActEditor.Core
{
  public class SelectionEngine
  {
    public const double SelectionRange = 0.8;
    private IPreviewEditor _actEditor;
    private IPreview _framePreview;

    public SelectionEngine() => this.SelectedItems = new HashSet<int>();

    public HashSet<int> SelectedItems { get; private set; }

    public List<DrawingComponent> Components { get; private set; }

    public ActDraw Main => this.Components.OfType<ActDraw>().FirstOrDefault<ActDraw>((Func<ActDraw, bool>) (p => p.Primary));

    public int LatestSelected { get; internal set; }

    public List<LayerDraw> SelectedLayerDraws
    {
      get
      {
        ActDraw main = this.Main;
        return main != null ? this.SelectedItems.Where<int>((Func<int, bool>) (p => p < main.Components.Count)).Select<int, LayerDraw>((Func<int, LayerDraw>) (p => (LayerDraw) main.Components[p])).ToList<LayerDraw>() : new List<LayerDraw>();
      }
    }

    public Layer[] SelectedLayers
    {
      get
      {
        if (this.Main == null)
          return new Layer[0];
        List<Layer> layerList = new List<Layer>();
        List<Layer> layers = this._act[this._framePreview.SelectedAction, this._framePreview.SelectedFrame].Layers;
        foreach (int index in (IEnumerable<int>) this.SelectedItems.OrderBy<int, int>((Func<int, int>) (p => p)))
        {
          if (index < layers.Count)
            layerList.Add(layers[index]);
        }
        return layerList.ToArray();
      }
    }

    public HashSet<int> CurrentlySelected
    {
      get
      {
        if (this.Main == null)
          return new HashSet<int>();
        HashSet<int> currentlySelected = new HashSet<int>();
        int numberOfLayers = this._actEditor.Act[this._actEditor.SelectedAction, this._actEditor.SelectedFrame].NumberOfLayers;
        foreach (int selectedItem in this.SelectedItems)
        {
          if (selectedItem < numberOfLayers)
            currentlySelected.Add(selectedItem);
        }
        return currentlySelected;
      }
    }

    private IEnumerable<Layer> _allLayers => this.Main != null ? (IEnumerable<Layer>) this._act[this._framePreview.SelectedAction, this._framePreview.SelectedFrame].Layers.ToArray() : (IEnumerable<Layer>) new Layer[0];

    private Act _act => this._actEditor == null ? this._framePreview.Act : this._actEditor.Act;

    public void Init(IPreviewEditor actEditor)
    {
      this._actEditor = actEditor;
      this._framePreview = (IPreview) this._actEditor.FramePreview;
      this.Components = this._framePreview.Components;
      this._actEditor.FrameSelector.FrameChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this._refreshSelection());
      this._actEditor.FrameSelector.SpecialFrameChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this._refreshSelection());
      this._actEditor.FrameSelector.ActionChanged += (ActIndexSelector.FrameIndexChangedDelegate) ((s, e) => this._internalFullClearSelection());
      this._actEditor.ActLoaded += (ActEditorWindow.ActEditorEventDelegate) delegate
      {
        if (this._actEditor.Act == null)
          return;
        this._actEditor.Act.Commands.CommandUndo += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._onCommandsOnCommandUndo);
        this._actEditor.Act.Commands.CommandRedo += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._onCommandsOnCommandUndo);
      };
    }

    public static SelectionEngine DummyEngine(IPreview preview)
    {
      SelectionEngine selectionEngine = new SelectionEngine();
      selectionEngine._init(preview);
      return selectionEngine;
    }

    private void _init(IPreview preview)
    {
      this._framePreview = preview;
      this.Components = this._framePreview.Components;
      if (this._framePreview.Act == null)
        return;
      this._framePreview.Act.Commands.CommandUndo += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._onCommandsOnCommandUndo);
      this._framePreview.Act.Commands.CommandRedo += new AbstractCommand<IActCommand>.AbstractCommandsEventHandler(this._onCommandsOnCommandUndo);
    }

    private void _onCommandsOnCommandUndo(object sender, IActCommand command)
    {
      if (this._getCommand<ActionCommand>(command) != null)
        this._internalFullClearSelection();
      else
        this._internalCleanSelection();
    }

    public void ClearSelection()
    {
      this.SelectedItems.Clear();
      this.Main?.Deselect();
    }

    public void Select(Rect rect, ZoomEngine zoom, Point absoluteCenter)
    {
      if (this.Main == null)
        return;
      rect = new Rect(rect.X - absoluteCenter.X, rect.Y - absoluteCenter.Y, rect.Width, rect.Height);
      Rect[] selectionAreas = this._getSelectionAreas(zoom);
      for (int index = 0; index < selectionAreas.Length; ++index)
      {
        if (rect.IntersectsWith(selectionAreas[index]))
          this.Select(index);
        else
          this.RemoveSelection(index);
      }
    }

    public void Select(int index)
    {
      ActDraw main = this.Main;
      if (main == null)
        return;
      main.Select(index);
      this.SelectedItems.Add(index);
    }

    public void SelectAll()
    {
      if (this._actEditor != null && this._actEditor.LayerEditor != null && this._actEditor.LayerEditor.DoNotRemove)
        return;
      ActDraw main = this.Main;
      if (main == null)
        return;
      this.SelectedItems.Clear();
      main.Select();
      this._internalSetSelection(0, main.Components.Count);
    }

    public void DeselectAll()
    {
      if (this._actEditor != null && this._actEditor.LayerEditor != null && this._actEditor.LayerEditor.DoNotRemove)
        return;
      ActDraw main = this.Main;
      if (main == null)
        return;
      this.SelectedItems.Clear();
      main.Deselect();
    }

    public void SetSelection(int start, int length)
    {
      ActDraw main = this.Main;
      if (main == null)
        return;
      this.SelectedItems.Clear();
      for (int index = 0; index < main.Components.Count; ++index)
      {
        if (start <= index && index < start + length)
        {
          main.Components[index].IsSelected = true;
          this.SelectedItems.Add(index);
        }
        else
          main.Components[index].IsSelected = false;
      }
    }

    public void SetSelection(int index)
    {
      ActDraw main = this.Main;
      if (main == null)
        return;
      this.SelectedItems.Clear();
      for (int index1 = 0; index1 < main.Components.Count; ++index1)
      {
        if (index1 == index)
        {
          main.Components[index1].IsSelected = true;
          this.SelectedItems.Add(index1);
        }
        else
          main.Components[index1].IsSelected = false;
      }
    }

    public void SelectUnderMouse(Point oldPosition, MouseEventArgs e)
    {
      ActDraw main = this.Main;
      if (main == null)
        return;
      List<DrawingComponent> drawingComponentList = new List<DrawingComponent>((IEnumerable<DrawingComponent>) main.Components);
      drawingComponentList.Reverse();
      foreach (LayerDraw layerDraw in drawingComponentList)
      {
        if (layerDraw.IsMouseUnder(this._framePreview.PointToScreen(oldPosition)) && layerDraw.IsMouseUnder(e))
        {
          layerDraw.IsSelected = !layerDraw.IsSelected;
          break;
        }
      }
    }

    public bool? IsUnderMouse(Point position)
    {
      ActDraw main = this.Main;
      if (main == null)
        return new bool?();
      foreach (LayerDraw layerDraw in new List<DrawingComponent>((IEnumerable<DrawingComponent>) main.Components))
      {
        if (layerDraw.IsMouseUnder(this._framePreview.PointToScreen(position)))
          return new bool?(true);
      }
      return new bool?(false);
    }

    public void AddSelection(int index)
    {
      this.SelectedItems.Add(index);
      ActDraw main = this.Main;
      if (main == null)
        return;
      this.LatestSelected = index;
      main.Select(index);
    }

    public void RemoveSelection(int index)
    {
      this.SelectedItems.Remove(index);
      this.Main?.Deselect(index);
    }

    public void RefreshSelection() => this._refreshSelection();

    public void SetSelection(HashSet<int> selection)
    {
      this.SelectedItems = selection;
      this._refreshSelection();
    }

    public void SelectReverse(HashSet<int> selected)
    {
      this.SelectedItems.Clear();
      ActDraw main = this.Main;
      if (main == null)
        return;
      for (int index = 0; index < main.Components.Count; ++index)
      {
        if (selected.Contains(index))
        {
          main.Components[index].IsSelected = false;
        }
        else
        {
          main.Components[index].IsSelected = true;
          this.SelectedItems.Add(index);
        }
      }
    }

    public void SelectUpToFromShift(int layerIndex)
    {
      ActDraw main = this.Main;
      int latestSelected = this.LatestSelected;
      if (main == null)
        return;
      if (latestSelected == layerIndex)
      {
        main.Components[layerIndex].IsSelected = !main.Components[layerIndex].IsSelected;
      }
      else
      {
        int num1 = latestSelected < layerIndex ? latestSelected : layerIndex;
        int num2 = latestSelected < layerIndex ? layerIndex : latestSelected;
        for (int index = num1; index <= num2 && index < main.Components.Count; ++index)
          main.Components[index].IsSelected = true;
      }
    }

    private void _internalSetSelection(int from, int count)
    {
      for (int index = from; index < from + count; ++index)
        this.SelectedItems.Add(index);
    }

    private Rect[] _getSelectionAreas(ZoomEngine zoom)
    {
      List<Rect> rectList = new List<Rect>();
      Spr sprite = this._actEditor == null ? this._framePreview.Act.Sprite : this._actEditor.Act.Sprite;
      foreach (Layer allLayer in this._allLayers)
      {
        if (allLayer.SpriteIndex > -1)
        {
          GrfImage image = allLayer.GetImage(sprite);
          double num1 = 0.0;
          double num2 = 0.0;
          if (image != null)
          {
            num1 = (double) image.Width;
            num2 = (double) image.Height;
          }
          double num3 = num1 * zoom.Scale * (double) allLayer.ScaleX;
          double num4 = num2 * zoom.Scale * (double) allLayer.ScaleY;
          double num5 = (double) allLayer.OffsetX * zoom.Scale;
          double num6 = (double) allLayer.OffsetY * zoom.Scale;
          double x = num5 - num3 * 0.8 * 0.5;
          double y = num6 - num4 * 0.8 * 0.5;
          rectList.Add(new Rect(new Point(x, y), new Point(x + num3 * 0.8, y + num4 * 0.8)));
        }
        else
          rectList.Add(new Rect((double) allLayer.OffsetX * zoom.Scale, (double) allLayer.OffsetY * zoom.Scale, 0.0, 0.0));
      }
      return rectList.ToArray();
    }

    private T _getCommand<T>(IActCommand command) where T : class, IActCommand
    {
      switch (command)
      {
        case ActGroupCommand actGroupCommand:
          return actGroupCommand.Commands.FirstOrDefault<IActCommand>((Func<IActCommand, bool>) (p => p.GetType() == typeof (T))) as T;
        case T _:
          return command as T;
        default:
          return default (T);
      }
    }

    private void _internalCleanSelection()
    {
      if (this._actEditor != null && this._actEditor.LayerEditor != null)
      {
        foreach (int selectedItem in this.SelectedItems)
          this._actEditor.LayerEditor.Provider.Get(selectedItem).IsSelected = false;
      }
      this._refreshSelection();
    }

    private void _internalFullClearSelection()
    {
      if (GrfEditorConfiguration.KeepPreviewSelectionFromActionChange)
      {
        this._refreshSelection();
      }
      else
      {
        if (this._actEditor != null && this._actEditor.LayerEditor != null)
        {
          foreach (int selectedItem in this.SelectedItems)
            this._actEditor.LayerEditor.Provider.Get(selectedItem).IsSelected = false;
        }
        this.SelectedItems.Clear();
        this._refreshSelection();
      }
    }

    private void _refreshSelection()
    {
      ActDraw main = this.Main;
      if (main == null)
        return;
      for (int index = 0; index < main.Components.Count; ++index)
        main.Components[index].IsSelected = this.SelectedItems.Contains(index);
    }
  }
}
