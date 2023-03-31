// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.DrawingComponents.DrawingComponent
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System.Windows.Controls;

namespace ActEditor.Core.DrawingComponents
{
  public abstract class DrawingComponent : Control
  {
    private bool _isSelected;

    public virtual bool IsSelectable { get; set; }

    public virtual bool IsSelected
    {
      get => this._isSelected;
      set
      {
        bool flag = this._isSelected != value;
        this._isSelected = value;
        if (!flag)
          return;
        this.OnSelected(-1, this._isSelected);
      }
    }

    public event DrawingComponent.DrawingComponentDelegate Selected;

    public virtual void OnSelected(int index, bool isSelected)
    {
      DrawingComponent.DrawingComponentDelegate selected = this.Selected;
      if (selected == null)
        return;
      selected((object) this, index, isSelected);
    }

    public abstract void Render(IPreview frameEditor);

    public abstract void QuickRender(IPreview frameEditor);

    public abstract void Remove(IPreview frameEditor);

    public virtual void Select()
    {
    }

    public delegate void DrawingComponentDelegate(object sender, int index, bool selected);
  }
}
