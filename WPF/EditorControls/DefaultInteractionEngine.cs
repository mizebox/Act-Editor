// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.DefaultInteractionEngine
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ActEditor.Core.WPF.EditorControls
{
  public class DefaultInteractionEngine : IEditorInteractionEngine
  {
    private readonly FramePreview _framePreview;
    private readonly IPreviewEditor _editor;

    public DefaultInteractionEngine(FramePreview framePreview, IPreviewEditor editor)
    {
      this._framePreview = framePreview;
      this._editor = editor;
    }

    public void Copy()
    {
      if (this._framePreview.GetSelectedLayers().Length <= 0)
        return;
      Clipboard.SetDataObject((object) new DataObject("Layers", (object) ((IEnumerable<Layer>) this._framePreview.GetSelectedLayers()).ToList<Layer>().Select<Layer, Layer>((Func<Layer, Layer>) (p => new Layer(p))).ToArray<Layer>()));
    }

    public void Paste()
    {
      if (this._editor.Act == null)
        return;
      IDataObject dataObject = Clipboard.GetDataObject();
      if (dataObject == null || !(dataObject.GetData("Layers") is Layer[] data) || data.Length == 0)
        return;
      int numberOfLayers = this._editor.Act[this._editor.SelectedAction, this._editor.SelectedFrame].NumberOfLayers;
      try
      {
        this._editor.Act.Commands.Begin();
        this._editor.Act.Commands.LayerAdd(this._editor.SelectedAction, this._editor.SelectedFrame, data);
      }
      catch (Exception ex)
      {
        this._editor.Act.Commands.CancelEdit();
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._editor.Act.Commands.End();
        this._editor.FrameSelector.OnFrameChanged(this._editor.SelectedFrame);
        this._editor.SelectionEngine.SetSelection(numberOfLayers, this._editor.Act[this._editor.SelectedAction, this._editor.SelectedFrame].NumberOfLayers - numberOfLayers);
      }
    }

    public void Cut()
    {
      Layer[] selectedLayers = this._editor.SelectionEngine.SelectedLayers;
      if (selectedLayers.Length <= 0)
        return;
      Clipboard.SetDataObject((object) new DataObject("Layers", (object) ((IEnumerable<Layer>) selectedLayers).ToList<Layer>().Select<Layer, Layer>((Func<Layer, Layer>) (p => new Layer(p))).ToArray<Layer>()));
      try
      {
        this._editor.Act.Commands.Begin();
        foreach (int layerIndex in (IEnumerable<int>) this._editor.SelectionEngine.CurrentlySelected.OrderByDescending<int, int>((Func<int, int>) (p => p)))
          this._editor.Act.Commands.LayerDelete(this._editor.SelectedAction, this._editor.SelectedFrame, layerIndex);
        this._editor.SelectionEngine.ClearSelection();
      }
      catch (Exception ex)
      {
        this._editor.Act.Commands.CancelEdit();
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._editor.Act.Commands.End();
        this._editor.Act.InvalidateVisual();
      }
    }

    public void Delete()
    {
      try
      {
        this._editor.Act.Commands.Begin();
        foreach (int layerIndex in (IEnumerable<int>) this._editor.SelectionEngine.CurrentlySelected.OrderByDescending<int, int>((Func<int, int>) (p => p)))
          this._editor.Act.Commands.LayerDelete(this._editor.SelectedAction, this._editor.SelectedFrame, layerIndex);
        this._editor.SelectionEngine.ClearSelection();
      }
      catch (Exception ex)
      {
        this._editor.Act.Commands.CancelEdit();
        ErrorHandler.HandleException(ex);
      }
      finally
      {
        this._editor.Act.Commands.End();
        this._editor.Act.InvalidateVisual();
      }
    }
  }
}
