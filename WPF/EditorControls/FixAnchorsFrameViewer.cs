// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.FixAnchorsFrameViewer
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.DrawingComponents;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;

namespace ActEditor.Core.WPF.EditorControls
{
  public class FixAnchorsFrameViewer : EditableFrameViewer
  {
    private bool _anchorEdit;
    private Point _oldAnchorPoint;

    public FixAnchorsFrameViewer()
    {
      this._gridBackground.LostMouseCapture += new MouseEventHandler(this._gridBackground_LostMouseCapture);
      this.Loaded += (RoutedEventHandler) delegate
      {
        this._parentWnd = WpfUtilities.FindDirectParentControl<Window>((FrameworkElement) this);
        if (this._parentWnd == null)
          return;
        this._parentWnd.PreviewKeyDown += new KeyEventHandler(this._fixAnchorsFrameViewer_PreviewKeyDown);
      };
    }

    private void _fixAnchorsFrameViewer_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (!ApplicationShortcut.Is((KeyGesture) ApplicationShortcut.FromString("Ctrl-K", "FixAnchors")))
        return;
      this.EditAnchors();
    }

    public void EditAnchors()
    {
      if (this._settings.IsPlaying() || this._gridBackground.IsMouseCaptured || this._anchorEdit)
        return;
      if (!this._settings.ShowAnchors())
      {
        ErrorHandler.HandleException("You must turn on the anchors by going in Anchors > Show anchors before editing them.");
      }
      else
      {
        this._anchorEdit = true;
        if (0 >= this.Act[this.SelectedAction, this.SelectedFrame].Anchors.Count)
        {
          this.Act.Commands.SetAnchorPosition(this.SelectedAction, this.SelectedFrame, 0, 0, 0);
          this.Update();
        }
        Anchor anchor = this.Act[this.SelectedAction, this.SelectedFrame].Anchors[0];
        this._oldAnchorPoint = new Point((double) anchor.OffsetX, (double) anchor.OffsetY);
        this._gridBackground.CaptureMouse();
        this._parentWnd.PreviewKeyDown += new KeyEventHandler(this._window_PreviewKeyDown);
        this._gridBackground.PreviewMouseMove += new MouseEventHandler(this._gridBackground_MouseMove);
        this._gridBackground.PreviewMouseUp += new MouseButtonEventHandler(this._gridBackground_MouseUp);
        this._gridBackground.PreviewMouseDown += new MouseButtonEventHandler(this._gridBackground_MouseDown);
        this._setAnchorPosition(Mouse.GetPosition((IInputElement) this._gridBackground));
      }
    }

    private void _gridBackground_MouseMove(object sender, MouseEventArgs e)
    {
      if (!this._anchorEdit)
        return;
      this._setAnchorPosition(e.GetPosition((IInputElement) this._gridBackground));
      e.Handled = true;
    }

    private void _gridBackground_MouseDown(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void _gridBackground_MouseUp(object sender, MouseButtonEventArgs e)
    {
      this._setAnchorPosition(e.GetPosition((IInputElement) this._gridBackground));
      this._gridBackground.ReleaseMouseCapture();
      e.Handled = true;
    }

    private void _gridBackground_LostMouseCapture(object sender, MouseEventArgs e)
    {
      if (!this._anchorEdit)
        return;
      try
      {
        this._parentWnd.PreviewKeyDown -= new KeyEventHandler(this._window_PreviewKeyDown);
        this._gridBackground.PreviewMouseMove -= new MouseEventHandler(this._gridBackground_MouseMove);
        this._gridBackground.PreviewMouseUp -= new MouseButtonEventHandler(this._gridBackground_MouseUp);
        this._gridBackground.PreviewMouseDown -= new MouseButtonEventHandler(this._gridBackground_MouseDown);
        Anchor anchor = this.Act[this.SelectedAction, this.SelectedFrame].Anchors[0];
        int offsetX = anchor.OffsetX;
        int offsetY = anchor.OffsetY;
        anchor.OffsetX = (int) this._oldAnchorPoint.X;
        anchor.OffsetY = (int) this._oldAnchorPoint.Y;
        this.Act.Commands.SetAnchorPosition(this.SelectedAction, this.SelectedFrame, offsetX, offsetY, 0);
        this.SizeUpdate();
      }
      catch
      {
      }
      finally
      {
        this._anchorEdit = false;
      }
    }

    private void _setAnchorPosition(Point absolutePosition)
    {
      if (!this._anchorEdit)
        return;
      int x1 = (int) ((absolutePosition.X - (double) this.CenterX) / this.ZoomEngine.Scale);
      int y1 = (int) ((absolutePosition.Y - (double) this.CenterY) / this.ZoomEngine.Scale);
      Point point = new Point((double) x1, (double) y1);
      Frame frame = this.Act.AnchoredTo.TryGetFrame(this.SelectedAction, this.SelectedFrame);
      Anchor anchor = this.Act[this.SelectedAction, this.SelectedFrame].Anchors[0];
      anchor.OffsetX = x1;
      anchor.OffsetY = y1;
      if (frame != null)
      {
        int x2 = (int) this._oldAnchorPoint.X;
        int num1 = x1 - x2;
        int y2 = (int) this._oldAnchorPoint.Y;
        int num2 = y1 - y2;
        anchor.OffsetX = x2 - num1;
        anchor.OffsetY = y2 - num2;
      }
      this._components.OfType<AnchorDraw>().ToList<AnchorDraw>()[0]?.RenderOffsets((IPreview) this, point);
      foreach (DrawingComponent drawingComponent in this._components.OfType<ActDraw>())
        drawingComponent.Render((IPreview) this);
    }

    private void _window_PreviewKeyDown(object sender, KeyEventArgs e) => this._gridBackground.ReleaseMouseCapture();
  }
}
