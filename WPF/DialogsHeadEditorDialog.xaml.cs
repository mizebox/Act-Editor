// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.HeadEditorDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.DrawingComponents;
using ActEditor.Core.WPF.EditorControls;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.FileFormats.SprFormat;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;
using Utilities.Commands;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class HeadEditorDialog : TkWindow, IPreviewEditor, IComponentConnector
  {
    private ActEditorWindow _editor;
    private Act _actSource;
    private Act _actHeadReference;
    private Act _actBodyReference;
    private Spr _sprReference;
    private readonly SelectionEngine _selectionEngine;
    private readonly List<ReferenceControl> _references = new List<ReferenceControl>();
    public HeadEditorDialog.HeadEditorActIndexSelector _actIndexSelector;
    public SpriteManager _spriteManager;
    private Act _actOriginal;
    private Act _actReferenceOriginal;
    internal Grid _gridPrimary;
    internal System.Windows.Controls.ListView _listViewHeads;
    internal FramePreview _framePreview;
    internal SpriteSelector _spriteSelector;
    internal Grid _gridActionPresenter;
    internal Button _buttonOk;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public HeadEditorDialog()
      : base("Setup Headgear", "advanced.png", SizeToContent.Manual, ResizeMode.CanResize)
    {
      this.InitializeComponent();
      this.ShowInTaskbar = true;
      this._selectionEngine = new SelectionEngine();
      this._actIndexSelector = new HeadEditorDialog.HeadEditorActIndexSelector(this);
      this._selectionEngine.Init((IPreviewEditor) this);
      this._spriteManager = new SpriteManager();
      this._spriteManager.AddDisabledMode(SpriteEditMode.Add);
      this._spriteManager.AddDisabledMode(SpriteEditMode.After);
      this._spriteManager.AddDisabledMode(SpriteEditMode.Before);
      this._spriteManager.AddDisabledMode(SpriteEditMode.Convert);
      this._spriteManager.AddDisabledMode(SpriteEditMode.Remove);
      this._spriteManager.AddDisabledMode(SpriteEditMode.Replace);
      this._spriteManager.AddDisabledMode(SpriteEditMode.ReplaceFlipHorizontal);
      this._spriteManager.AddDisabledMode(SpriteEditMode.ReplaceFlipVertical);
      this._spriteManager.AddDisabledMode(SpriteEditMode.Usage);
      this._listViewHeads.PreviewKeyDown += new KeyEventHandler(this._listViewHeads_PreviewKeyDown);
      this._framePreview.PreviewDrop += new DragEventHandler(this._framePreview_PreviewDrop);
      ListViewDataTemplateHelper.GenerateListViewTemplateNew(this._listViewHeads, new ListViewDataTemplateHelper.GeneralColumnInfo[1]
      {
        new ListViewDataTemplateHelper.GeneralColumnInfo()
        {
          Header = "File name",
          DisplayExpression = "DisplayName",
          SearchGetAccessor = "DisplayName",
          IsFill = true,
          TextAlignment = TextAlignment.Left,
          ToolTipBinding = "DisplayName"
        }
      }, (ListViewCustomComparer) new DefaultListViewComparer<HeadEditorDialog.ActReferenceView>(), (IList<string>) new string[2]
      {
        "Default",
        "Black"
      }, "generateHeader", "true", "overrideSizeRedraw", "true");
      ApplicationShortcut.Link(ApplicationShortcut.Undo, (System.Action) (() =>
      {
        if (this.Act == null)
          return;
        this.Act.Commands.Undo();
        this._selectionEngine.Select(0);
      }), (FrameworkElement) this);
      ApplicationShortcut.Link(ApplicationShortcut.Redo, (System.Action) (() =>
      {
        if (this.Act == null)
          return;
        this.Act.Commands.Redo();
        this._selectionEngine.Select(0);
      }), (FrameworkElement) this);
    }

    private void _listViewHeads_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      this._framePreview.FramePreview_KeyDown(sender, e);
      e.Handled = true;
    }

    private void _framePreview_PreviewDrop(object sender, DragEventArgs e)
    {
      object data = e.Data.GetData("ImageIndex");
      if (data == null)
        return;
      int absoluteId = (int) data;
      Point position = e.GetPosition((IInputElement) this._framePreview);
      this.Act.Commands.BeginNoDelay();
      this.Act.Commands.SetAbsoluteSpriteId(this.SelectedAction, this.SelectedFrame, 0, absoluteId);
      this.Act.Commands.SetOffsets(this.SelectedAction, this.SelectedFrame, 0, (int) ((position.X - this._framePreview.RelativeCenter.X * this._framePreview.ActualWidth) / this._framePreview.ZoomEngine.Scale), (int) ((position.Y - this._framePreview.RelativeCenter.Y * this._framePreview.ActualHeight) / this._framePreview.ZoomEngine.Scale));
      this.Act.Commands.End();
      this.FrameSelector.OnFrameChanged(this.SelectedFrame);
      this.SelectionEngine.SetSelection(0);
      Keyboard.Focus((IInputElement) this.GridPrimary);
      e.Handled = true;
    }

    public void Init(ActEditorWindow editor, Act actOriginal)
    {
      this._actOriginal = actOriginal;
      this._actSource = new Act(actOriginal);
      this._editor = editor;
      ReferenceControl referenceControl1 = this._editor.References.First<ReferenceControl>((Func<ReferenceControl, bool>) (p => p.ReferenceName == "Head"));
      ReferenceControl referenceControl2 = this._editor.References.First<ReferenceControl>((Func<ReferenceControl, bool>) (p => p.ReferenceName == "Body"));
      referenceControl1.MakeAct();
      referenceControl2.MakeAct();
      this._actHeadReference = new Act(referenceControl1.Act);
      this._sprReference = new Spr(referenceControl1.Spr);
      this._actReferenceOriginal = new Act(new Act(referenceControl1.Act));
      this._actBodyReference = new Act(referenceControl2.Act);
      Act act1 = new Act(actOriginal.Sprite);
      Act act2 = new Act(this._sprReference);
      Act act3 = new Act(referenceControl2.Spr);
      for (int absoluteSpriteIndex = 0; absoluteSpriteIndex < this._sprReference.NumberOfIndexed8Images; ++absoluteSpriteIndex)
      {
        List<ActIndex> list = this._actHeadReference.FindUsageOf(absoluteSpriteIndex).Where<ActIndex>((Func<ActIndex, bool>) (p => !this._actHeadReference[p].Mirror)).ToList<ActIndex>();
        if (list.Count > 0)
        {
          GRF.FileFormats.ActFormat.Action action1 = new GRF.FileFormats.ActFormat.Action();
          GRF.FileFormats.ActFormat.Frame frame1 = new GRF.FileFormats.ActFormat.Frame();
          frame1.Anchors.AddRange((IEnumerable<Anchor>) this._actHeadReference[list[0].ActionIndex, list[0].FrameIndex].Anchors);
          action1.Frames.Add(frame1);
          frame1.Layers.Add(this._actHeadReference[list[0]]);
          act2.AddAction(action1);
          GRF.FileFormats.ActFormat.Action action2 = new GRF.FileFormats.ActFormat.Action();
          GRF.FileFormats.ActFormat.Frame frame2 = new GRF.FileFormats.ActFormat.Frame();
          action2.Frames.Add(frame2);
          bool flag1 = false;
          foreach (ActIndex actIndex in list)
          {
            GRF.FileFormats.ActFormat.Frame frame3 = this._actBodyReference.TryGetFrame(actIndex.ActionIndex, actIndex.FrameIndex);
            if (frame3 != null && frame3.NumberOfLayers > 0)
            {
              Layer layer1 = frame3.Layers.FirstOrDefault<Layer>((Func<Layer, bool>) (p => p.SpriteIndex > -1));
              if (layer1 != null)
              {
                frame2.Anchors.AddRange((IEnumerable<Anchor>) frame3.Anchors);
                Layer layer2 = new Layer(layer1);
                frame2.Layers.Add(layer2);
                flag1 = true;
                break;
              }
            }
          }
          if (!flag1)
          {
            frame2.Anchors.AddRange((IEnumerable<Anchor>) this._actHeadReference[list[0].ActionIndex, list[0].FrameIndex].Anchors);
            frame2.Layers.Add(new Layer(0, this._sprReference)
            {
              SpriteIndex = -1
            });
          }
          act3.AddAction(action2);
          GRF.FileFormats.ActFormat.Action action3 = new GRF.FileFormats.ActFormat.Action();
          GRF.FileFormats.ActFormat.Frame frame4 = new GRF.FileFormats.ActFormat.Frame();
          action3.Frames.Add(frame4);
          bool flag2 = false;
          foreach (ActIndex actIndex in list)
          {
            GRF.FileFormats.ActFormat.Frame frame5 = actOriginal.TryGetFrame(actIndex.ActionIndex, actIndex.FrameIndex);
            if (frame5 != null && frame5.NumberOfLayers > 0)
            {
              Layer layer3 = frame5.Layers.FirstOrDefault<Layer>((Func<Layer, bool>) (p => p.SpriteIndex > -1));
              if (layer3 != null)
              {
                frame4.Anchors.AddRange((IEnumerable<Anchor>) this._actHeadReference[list[0].ActionIndex, list[0].FrameIndex].Anchors);
                Layer layer4 = new Layer(layer3);
                frame4.Layers.Add(layer4);
                flag2 = true;
                break;
              }
            }
          }
          if (!flag2)
          {
            frame4.Anchors.AddRange((IEnumerable<Anchor>) this._actHeadReference[list[0].ActionIndex, list[0].FrameIndex].Anchors);
            frame4.Layers.Add(new Layer(0, this._sprReference)
            {
              SpriteIndex = -1
            });
          }
          act1.AddAction(action3);
        }
      }
      this._actSource = act1;
      this._actSource.Commands.CommandIndexChanged += (AbstractCommand<IActCommand>.AbstractCommandsEventHandler) delegate
      {
        this._buttonOk.IsEnabled = this._actSource.Commands.IsModified;
      };
      this._actHeadReference = act2;
      this._actHeadReference.Name = "Head";
      this._actHeadReference.AnchoredTo = this._actSource;
      this._actBodyReference = act3;
      this._actBodyReference.Name = "Body";
      this._actBodyReference.AnchoredTo = this._actHeadReference;
      List<HeadEditorDialog.ActReferenceView> actReferenceViewList = new List<HeadEditorDialog.ActReferenceView>(this._sprReference.NumberOfIndexed8Images);
      for (int index = 0; index < this._sprReference.NumberOfIndexed8Images; ++index)
        actReferenceViewList.Add(new HeadEditorDialog.ActReferenceView(index.ToString() + " - Head", index));
      this._listViewHeads.SelectionChanged += new SelectionChangedEventHandler(this._listViewHeads_SelectionChanged);
      this._spriteSelector.Init((IPreviewEditor) this);
      this._framePreview.Init((IPreviewEditor) this);
      this._framePreview.InteractionEngine = (IEditorInteractionEngine) new HeadInteractionEngine(this._framePreview, (IPreviewEditor) this);
      this._framePreview.PreviewRender += new FramePreview.FramePreviewEventDelegate(this._framePreview_PreviewRender);
      this._framePreview.CustomUpdate = (Action<List<DrawingComponent>>) (components =>
      {
        components.Add((DrawingComponent) new ActDraw(this._actBodyReference, (IPreviewEditor) this));
        components.Add((DrawingComponent) new ActDraw(this._actHeadReference, (IPreviewEditor) this));
        components.Add((DrawingComponent) new ActDraw(this._actSource, (IPreviewEditor) this));
        components.Last<DrawingComponent>().Selected += new DrawingComponent.DrawingComponentDelegate(this._headEditorDialog_Selected);
      });
      this._listViewHeads.ItemsSource = (IEnumerable) actReferenceViewList;
      this._listViewHeads.SelectedIndex = 0;
      this.OnActLoaded();
    }

    private void _headEditorDialog_Selected(object sender, int index, bool selected)
    {
      if (selected)
        this.SelectionEngine.AddSelection(index);
      else
        this.SelectionEngine.RemoveSelection(index);
    }

    private void _framePreview_PreviewRender(object sender)
    {
    }

    private void _listViewHeads_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this._actIndexSelector.OnActionChanged(this.SelectedAction);
      this._actIndexSelector.OnFrameChanged(this.SelectedFrame);
      this._selectionEngine.Select(0);
    }

    protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) => base.GRFEditorWindowKeyDown(sender, e);

    private void _buttonOk_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!this._createHeadSprite())
          return;
        this._buttonOk.IsEnabled = false;
        this.Act.Commands.SaveCommandIndex();
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private int _getReferenceFrameIndex(int actionIndex, int frameIndex, Act act)
    {
      if (actionIndex >= act.NumberOfActions)
        return -1;
      if ((act.Name == "Head" || act.Name == "Body") && (act[actionIndex].NumberOfFrames == 3 && 0 <= actionIndex && actionIndex < 8 || 16 <= actionIndex && actionIndex < 24) && this._actOriginal != null)
      {
        int num = this._actOriginal[actionIndex].NumberOfFrames / 3;
        if (num != 0)
        {
          if (frameIndex < num)
            return 0;
          if (frameIndex < 2 * num)
            return 1;
          return frameIndex < 3 * num ? 2 : 2;
        }
      }
      if (frameIndex < act[actionIndex].NumberOfFrames)
        return frameIndex;
      return act[actionIndex].NumberOfFrames > 0 ? frameIndex % act[actionIndex].NumberOfFrames : 0;
    }

    private int _getRerenceSpriteIndex(int aid, int fid, Act act)
    {
      Layer layer = act[aid, fid].Layers.FirstOrDefault<Layer>((Func<Layer, bool>) (p => p.SpriteIndex > -1));
      return layer == null ? -1 : layer.SpriteIndex;
    }

    private int _getSourceSPriteIndex(int referenceSpriteIndex)
    {
      GRF.FileFormats.ActFormat.Frame frame = this._actSource[referenceSpriteIndex, 0];
      return frame.Layers.Count <= 0 ? -1 : frame.Layers[0].SpriteIndex;
    }

    private bool _createHeadSprite() => this._editor.Element.Dispatch<UIElement, bool>((Func<UIElement, bool>) delegate
    {
      try
      {
        this._actOriginal.Commands.Begin();
        this._actOriginal.Commands.Backup((Action<Act>) (act =>
        {
          for (int index3 = 0; index3 < this._actOriginal.Actions.Count && index3 < this._actReferenceOriginal.Actions.Count; ++index3)
          {
            GRF.FileFormats.ActFormat.Action action = this._actOriginal.Actions[index3];
            for (int index4 = 0; index4 < action.Frames.Count; ++index4)
            {
              this._cleanUpFrame(index3, index4);
              int referenceFrameIndex = this._getReferenceFrameIndex(index3, index4, this._actReferenceOriginal);
              int rerenceSpriteIndex = this._getRerenceSpriteIndex(index3, referenceFrameIndex, this._actReferenceOriginal);
              if (rerenceSpriteIndex >= 0)
              {
                int sourceSpriteIndex = this._getSourceSPriteIndex(rerenceSpriteIndex);
                if (sourceSpriteIndex >= 0)
                {
                  Layer layerSource = this._actOriginal[index3, index4, 0];
                  layerSource.SetAbsoluteSpriteId(sourceSpriteIndex, this._actOriginal.Sprite);
                  this._adjustLayerCoordinates(index3, index4, referenceFrameIndex, layerSource, rerenceSpriteIndex);
                }
              }
            }
          }
        }), "Head sprite generation", true);
      }
      catch (Exception ex)
      {
        this._actOriginal.Commands.CancelEdit();
        ErrorHandler.HandleException(ex);
        return false;
      }
      finally
      {
        this._actOriginal.Commands.End();
        this._actOriginal.InvalidateVisual();
        this._actOriginal.InvalidateSpriteVisual();
      }
      return true;
    });

    private void _adjustLayerCoordinates(
      int aid,
      int fid,
      int referenceFrameIndex,
      Layer layerSource,
      int referenceSpriteIndex)
    {
      GRF.FileFormats.ActFormat.Frame frame1 = this._actOriginal[aid, fid];
      GRF.FileFormats.ActFormat.Frame frame2 = this._actReferenceOriginal[aid, referenceFrameIndex];
      Layer layer1 = frame2.Layers.FirstOrDefault<Layer>((Func<Layer, bool>) (p => p.SpriteIndex > -1));
      if (layer1 == null)
        return;
      GRF.FileFormats.ActFormat.Frame frame3 = this._actSource[referenceSpriteIndex, 0];
      GRF.FileFormats.ActFormat.Frame frame4 = this._actHeadReference[referenceSpriteIndex, 0];
      Layer layer2 = frame3.Layers[0];
      Layer layer3 = frame4.Layers[0];
      layerSource.Mirror = layer1.Mirror;
      int offsetX1 = layer2.OffsetX;
      int offsetY1 = layer2.OffsetY;
      int offsetX2 = layer3.OffsetX;
      int offsetY2 = layer3.OffsetY;
      if (frame1.Anchors.Count > 0 && frame2.Anchors.Count > 0)
      {
        frame1.Anchors[0].OffsetX = frame2.Anchors[0].OffsetX;
        frame1.Anchors[0].OffsetY = frame2.Anchors[0].OffsetY;
      }
      layerSource.OffsetX = layer1.OffsetX;
      layerSource.OffsetY = layer1.OffsetY;
      layerSource.ScaleX = layer2.ScaleX;
      layerSource.ScaleY = layer2.ScaleY;
      layerSource.Rotation = layer2.Rotation;
      int x = offsetX1 - offsetX2;
      int y = offsetY1 - offsetY2;
      if (layerSource.Mirror)
      {
        x *= -1;
        if (layerSource.Rotation > 0)
        {
          int num = 360 - layerSource.Rotation;
          layerSource.Rotation = num < 0 ? num + 360 : num;
        }
      }
      layerSource.Translate(x, y);
    }

    private void _cleanUpFrame(int aid, int fid)
    {
      GRF.FileFormats.ActFormat.Frame frame = this._actOriginal[aid].Frames[fid];
      while (frame.Layers.Count > 1)
        frame.Layers.RemoveAt(1);
      if (frame.Layers.Count != 0)
        return;
      Layer layer = new Layer(0, this._actOriginal.Sprite);
      frame.Layers.Add(layer);
    }

    private void _buttonCancel_Click(object sender, RoutedEventArgs e) => this.Close();

    public UIElement Element => (UIElement) this;

    public Act Act => this._actSource;

    public int SelectedAction => this._listViewHeads.SelectedItem == null ? 0 : ((HeadEditorDialog.ActReferenceView) this._listViewHeads.SelectedItem).Index;

    public int SelectedFrame => 0;

    public SelectionEngine SelectionEngine => this._selectionEngine;

    public List<ReferenceControl> References => this._references;

    public IActIndexSelector FrameSelector => (IActIndexSelector) this._actIndexSelector;

    public event ActEditorWindow.ActEditorEventDelegate ReferencesChanged;

    public event ActEditorWindow.ActEditorEventDelegate ActLoaded;

    public void OnActLoaded()
    {
      ActEditorWindow.ActEditorEventDelegate actLoaded = this.ActLoaded;
      if (actLoaded == null)
        return;
      actLoaded((object) this);
    }

    public Grid GridPrimary => this._gridPrimary;

    public LayerEditor LayerEditor => (LayerEditor) null;

    public SpriteSelector SpriteSelector => this._spriteSelector;

    public FramePreview FramePreview => this._framePreview;

    public SpriteManager SpriteManager => this._spriteManager;

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/headeditordialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._gridPrimary = (Grid) target;
          break;
        case 2:
          this._listViewHeads = (System.Windows.Controls.ListView) target;
          break;
        case 3:
          this._framePreview = (FramePreview) target;
          break;
        case 4:
          this._spriteSelector = (SpriteSelector) target;
          break;
        case 5:
          this._gridActionPresenter = (Grid) target;
          break;
        case 6:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        case 7:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public class ActReferenceView
    {
      private readonly string _name;

      public int Index { get; set; }

      public bool Default => true;

      public ActReferenceView(string name, int index)
      {
        this.Index = index;
        this._name = name;
      }

      public string DisplayName => this._name;

      public override string ToString() => this._name;
    }

    public class HeadEditorActIndexSelector : IActIndexSelector
    {
      private readonly HeadEditorDialog _editor;

      public HeadEditorActIndexSelector(HeadEditorDialog editor) => this._editor = editor;

      public void OnFrameChanged(int actionindex)
      {
        ActIndexSelector.FrameIndexChangedDelegate frameChanged = this.FrameChanged;
        if (frameChanged == null)
          return;
        frameChanged((object) this, actionindex);
      }

      public bool IsPlaying => false;

      public event ActIndexSelector.FrameIndexChangedDelegate ActionChanged;

      public void OnActionChanged(int actionindex)
      {
        ActIndexSelector.FrameIndexChangedDelegate actionChanged = this.ActionChanged;
        if (actionChanged == null)
          return;
        actionChanged((object) this, actionindex);
      }

      public event ActIndexSelector.FrameIndexChangedDelegate FrameChanged;

      public event ActIndexSelector.FrameIndexChangedDelegate SpecialFrameChanged;

      public void OnSpecialFrameChanged(int actionindex)
      {
        ActIndexSelector.FrameIndexChangedDelegate specialFrameChanged = this.SpecialFrameChanged;
        if (specialFrameChanged == null)
          return;
        specialFrameChanged((object) this, actionindex);
      }

      public void OnAnimationPlaying(int actionindex)
      {
      }

      public void SetAction(int index) => this._editor._listViewHeads.SelectedIndex = index;

      public void SetFrame(int index)
      {
      }
    }
  }
}
