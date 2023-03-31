// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.DrawingComponents.ActDraw
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ActEditor.Core.DrawingComponents
{
  public class ActDraw : DrawingComponent
  {
    private readonly Act _act;
    private readonly IPreviewEditor _actEditor;
    private readonly List<DrawingComponent> _components = new List<DrawingComponent>();
    private bool _componentsInitiated;
    private readonly IPreview _preview;

    public ActDraw(Act act) => this._act = act;

    public ActDraw(Act act, IPreviewEditor actEditor)
    {
      this._act = act;
      this._actEditor = actEditor;
    }

    public ActDraw(Act act, IPreview preview)
    {
      this._act = act;
      this._preview = preview;
    }

    public bool Primary => this._act.Name == null;

    public ReadOnlyCollection<DrawingComponent> Components => this._components.AsReadOnly();

    public override void Render(IPreview frameEditor)
    {
      if (frameEditor == null)
        throw new ArgumentNullException(nameof (frameEditor));
      if (!this._componentsInitiated)
      {
        int selectedAction = frameEditor.SelectedAction;
        int selectedFrame = frameEditor.SelectedFrame;
        if (selectedAction >= this._act.NumberOfActions)
          return;
        if (selectedFrame >= this._act[selectedAction].NumberOfFrames)
        {
          if (this.Primary)
            return;
          selectedFrame %= this._act[selectedAction].NumberOfFrames;
        }
        Frame frame = this._act[selectedAction, selectedFrame];
        for (int layerIndex = 0; layerIndex < frame.NumberOfLayers; ++layerIndex)
        {
          LayerDraw layerDraw = this._actEditor != null ? new LayerDraw(this._actEditor, this._act, layerIndex, this) : new LayerDraw(this._preview, this._act, layerIndex, this);
          if (this.Primary)
            layerDraw.Selected += (DrawingComponent.DrawingComponentDelegate) ((s, e, a) => this.OnSelected(e, a));
          this._components.Add((DrawingComponent) layerDraw);
        }
        this._componentsInitiated = true;
      }
      foreach (DrawingComponent component in this.Components)
      {
        if (this.Primary)
          component.IsSelectable = true;
        component.Render(frameEditor);
      }
    }

    public override void QuickRender(IPreview frameEditor)
    {
      foreach (DrawingComponent component in this.Components)
        component.QuickRender(frameEditor);
    }

    public override void Remove(IPreview frameEditor)
    {
      foreach (DrawingComponent component in this.Components)
        component.Remove(frameEditor);
    }

    public void Render(IPreview frameEditor, int layerIndex) => this.Components[layerIndex].Render(frameEditor);

    public override void Select()
    {
      foreach (DrawingComponent component in this.Components)
        component.Select();
    }

    public void Select(int layer)
    {
      if (layer <= -1 || layer >= this.Components.Count)
        return;
      this.Components[layer].Select();
    }

    public void Deselect(int layer)
    {
      if (layer <= -1 || layer >= this.Components.Count)
        return;
      this.Components[layer].IsSelected = false;
    }

    public void Deselect()
    {
      foreach (DrawingComponent component in this.Components)
        component.IsSelected = false;
    }

    public LayerDraw Get(int layerIndex) => layerIndex < 0 || layerIndex >= this.Components.Count ? (LayerDraw) null : this.Components[layerIndex] as LayerDraw;
  }
}
