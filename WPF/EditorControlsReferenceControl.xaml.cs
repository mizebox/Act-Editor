// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.EditorControls.ReferenceControl
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.GenericControls;
using ActEditor.Tools.GrfShellExplorer;
using ErrorManager;
using GRF;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.SprFormat;
using GRF.Image;
using GRF.IO;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WPF.Styles.ListView;
using Utilities;
using Utilities.Extension;

namespace ActEditor.Core.WPF.EditorControls
{
  public partial class ReferenceControl : UserControl, IComponentConnector
  {
    private readonly ActEditorWindow _actEditor;
    private readonly string _defaultFemale;
    private readonly string _defaultMale;
    private readonly List<FancyButton> _fancyButtons;
    private readonly LayerControl _layerControl;
    private readonly string _name;
    private string _filePath;
    private ZMode _mode;
    private bool _sex;
    private bool _directional;
    internal CheckBox _cbRef;
    internal Grid _grid;
    internal FancyButton _fancyButton3;
    internal FancyButton _fancyButton4;
    internal FancyButton _fancyButton5;
    internal FancyButton _fancyButton2;
    internal FancyButton _fancyButton6;
    internal FancyButton _fancyButton1;
    internal FancyButton _fancyButton0;
    internal FancyButton _fancyButton7;
    internal FancyButton _buttonAnchor;
    internal ComboBox _cbAnchor;
    internal FancyButton _refZState;
    internal LinkControl _buttonSprite;
    internal FancyButton _reset;
    internal FancyButton _gender;
    internal StackPanel _sp;
    internal LayerControlHeader _header;
    internal Rectangle _rectangleVisibility;
    private bool _contentLoaded;

    public ReferenceControl() => this.InitializeComponent();

    public ReferenceControl(
      ActEditorWindow actEditor,
      string defaultMale,
      string defaultFemale,
      string name,
      bool directional)
    {
      ReferenceControl referenceControl = this;
      this.InitializeComponent();
      try
      {
        if (directional)
        {
          this._fancyButtons = ((IEnumerable<FancyButton>) new FancyButton[8]
          {
            this._fancyButton0,
            this._fancyButton1,
            this._fancyButton2,
            this._fancyButton3,
            this._fancyButton4,
            this._fancyButton5,
            this._fancyButton6,
            this._fancyButton7
          }).ToList<FancyButton>();
          byte[] resource1 = ApplicationManager.GetResource("arrow.png");
          BitmapSource bitmapSource1 = new GrfImage(ref resource1).Cast<BitmapSource>();
          byte[] resource2 = ApplicationManager.GetResource("arrowoblique.png");
          BitmapSource bitmapSource2 = new GrfImage(ref resource2).Cast<BitmapSource>();
          this._fancyButtons.ForEach((Action<FancyButton>) (p => p.ImageIcon.Stretch = Stretch.Uniform));
          this._fancyButton0.ImageIcon.Source = (ImageSource) bitmapSource1;
          this._fancyButton0.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton0.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 90.0
          };
          this._fancyButton1.ImageIcon.Source = (ImageSource) bitmapSource2;
          this._fancyButton1.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton1.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 90.0
          };
          this._fancyButton2.ImageIcon.Source = (ImageSource) bitmapSource1;
          this._fancyButton2.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton2.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 180.0
          };
          this._fancyButton3.ImageIcon.Source = (ImageSource) bitmapSource2;
          this._fancyButton3.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton3.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 180.0
          };
          this._fancyButton4.ImageIcon.Source = (ImageSource) bitmapSource1;
          this._fancyButton4.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton4.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 270.0
          };
          this._fancyButton5.ImageIcon.Source = (ImageSource) bitmapSource2;
          this._fancyButton5.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton5.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 270.0
          };
          this._fancyButton6.ImageIcon.Source = (ImageSource) bitmapSource1;
          this._fancyButton6.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton6.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 360.0
          };
          this._fancyButton7.ImageIcon.Source = (ImageSource) bitmapSource2;
          this._fancyButton7.ImageIcon.RenderTransformOrigin = new Point(0.5, 0.5);
          this._fancyButton7.ImageIcon.RenderTransform = (Transform) new RotateTransform()
          {
            Angle = 360.0
          };
          this._grid.Visibility = Visibility.Visible;
        }
      }
      catch
      {
      }
      this._directional = directional;
      this._layerControl = new LayerControl(actEditor, this._header, name);
      this._layerControl.IsEnabled = false;
      this._sp.Children.Add((UIElement) this._layerControl);
      TextBlock name1 = (TextBlock) this._refZState.FindName("_tbIdentifier");
      name1.Margin = new Thickness(2.0);
      name1.FontSize = 12.0;
      name1.Foreground = (Brush) Brushes.Black;
      Grid child1 = (Grid) ((Panel) ((Decorator) this._refZState.FindName("_border")).Child).Children[2];
      child1.HorizontalAlignment = HorizontalAlignment.Stretch;
      child1.Margin = new Thickness(0.0, 0.0, 2.0, 0.0);
      child1.ColumnDefinitions[0] = new ColumnDefinition();
      child1.ColumnDefinitions[1] = new ColumnDefinition()
      {
        Width = new GridLength(-1.0, GridUnitType.Auto)
      };
      UIElement child2 = child1.Children[0];
      UIElement child3 = child1.Children[1];
      child2.SetValue(Grid.ColumnProperty, (object) 1);
      child3.SetValue(Grid.ColumnProperty, (object) 0);
      this._header.HideStart();
      this._actEditor = actEditor;
      this._defaultMale = defaultMale;
      this._defaultFemale = defaultFemale;
      this._name = name;
      this._layerControl._tbSpriteId.Visibility = Visibility.Collapsed;
      this._layerControl._tbSpriteNumber.Visibility = Visibility.Collapsed;
      this._layerControl.Grid.ColumnDefinitions[0].MinWidth = 0.0;
      this._layerControl.Grid.ColumnDefinitions[0].Width = new GridLength(0.0);
      this._layerControl.Grid.ColumnDefinitions[1].MinWidth = 0.0;
      this._layerControl.Grid.ColumnDefinitions[1].Width = new GridLength(0.0);
      this._layerControl.Grid.ColumnDefinitions[2].MinWidth = 0.0;
      this._layerControl.Grid.ColumnDefinitions[2].Width = new GridLength(0.0);
      this._cbRef.Checked += (RoutedEventHandler) delegate
      {
        this._layerControl.IsEnabled = true;
        this._rectangleVisibility.Visibility = Visibility.Collapsed;
        this._rectangleVisibility.IsHitTestVisible = false;
        GrfEditorConfiguration.ConfigAsker["[ActEditor - IsEnabled - " + this._name + "]"] = true.ToString();
        this.Update(true);
      };
      this._cbRef.Unchecked += (RoutedEventHandler) delegate
      {
        this._layerControl.IsEnabled = false;
        this._rectangleVisibility.Visibility = Visibility.Visible;
        this._rectangleVisibility.IsHitTestVisible = true;
        GrfEditorConfiguration.ConfigAsker["[ActEditor - IsEnabled - " + this._name + "]"] = false.ToString();
        this.OnUpdated();
        this._actEditor.OnReferencesChanged();
      };
      this._filePath = GrfEditorConfiguration.ConfigAsker["[ActEditor - Path - " + name + "]", ""];
      this._actEditor.Loaded += (RoutedEventHandler) delegate
      {
        this._cbRef.IsChecked = new bool?(bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - IsEnabled - " + this._name + "]", false.ToString()]));
      };
      this._mode = int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Mode - " + name + "]", "0"]) == 0 ? ZMode.Front : ZMode.Back;
      this._sex = bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Gender - " + name + "]", "true"]);
      this._updateGenderButton();
      this.FilePathChanged += new ReferenceControl.ReferenceFrameEventHandler(this._referenceFrame_FilePathChanged);
      System.Action action = (System.Action) (() =>
      {
        bool flag = this.Mode == ZMode.Front;
        this._refZState.ImagePath = flag ? "front.png" : "back.png";
        this._refZState.TextHeader = flag ? "Front" : "Back";
      });
      this._refZState.Click += (RoutedEventHandler) delegate
      {
        closure_0.Mode = closure_0.Mode == ZMode.Front ? ZMode.Back : ZMode.Front;
        action();
      };
      action();
      this._referenceFrame_FilePathChanged((object) null);
      if (name == "Head" || name == "Other" || name == "Body")
      {
        this._buttonAnchor.Visibility = Visibility.Visible;
        this._buttonAnchor.IsEnabled = true;
        this._cbAnchor.Visibility = Visibility.Visible;
        this._cbAnchor.IsEnabled = true;
      }
      this._cbRef.Content = (object) name;
      WpfUtils.AddMouseInOutEffectsBox(this._cbRef);
      this._cbAnchor.DropDownOpened += (EventHandler) delegate
      {
        this._buttonAnchor.IsPressed = true;
      };
      this._cbAnchor.DropDownClosed += (EventHandler) delegate
      {
        referenceControl._buttonAnchor.IsPressed = false;
        Keyboard.Focus((IInputElement) actEditor._gridPrimary);
      };
      this._cbAnchor.SelectionChanged += new SelectionChangedEventHandler(this._cbAnchor_SelectionChanged);
      this._cbAnchor_SelectionChanged((object) null, (SelectionChangedEventArgs) null);
      if (!(name == "Neighbor"))
        return;
      this._buttonAnchor.Visibility = Visibility.Collapsed;
      this._cbAnchor.Visibility = Visibility.Collapsed;
    }

    private void _updateGenderButton()
    {
      if (this._directional)
        this._gender.Visibility = Visibility.Collapsed;
      else if (!string.IsNullOrEmpty(this._filePath))
      {
        this._gender.Visibility = Visibility.Collapsed;
      }
      else
      {
        this._gender.IsPressed = this._sex;
        this._gender.ImagePath = this._sex ? "female.png" : "male.png";
      }
    }

    public bool ShowReference => this._cbRef.Dispatch<bool>((Func<bool>) (() =>
    {
      bool? isChecked = this._cbRef.IsChecked;
      return isChecked.GetValueOrDefault() && isChecked.HasValue;
    }));

    public ZMode Mode
    {
      get => this._mode;
      set
      {
        if (this._mode == value)
          return;
        this._mode = value;
        GrfEditorConfiguration.ConfigAsker["[ActEditor - Mode - " + this._name + "]"] = value == ZMode.Front ? "0" : "1";
        if (this._mode == ZMode.Back)
        {
          this._actEditor.References.Remove(this);
          this._actEditor.References.Insert(0, this);
        }
        else
        {
          this._actEditor.References.Remove(this);
          this._actEditor.References.Add(this);
        }
        this.Update(false);
      }
    }

    public Act Act { get; set; }

    public Spr Spr { get; set; }

    public string FilePath
    {
      get => this._filePath;
      set
      {
        this._filePath = value;
        GrfEditorConfiguration.ConfigAsker["[ActEditor - Path - " + this._name + "]"] = value;
        this.OnFilePathChanged();
        this.Update(true);
      }
    }

    public event ReferenceControl.ReferenceFrameEventHandler Updated;

    public void OnUpdated()
    {
      ReferenceControl.ReferenceFrameEventHandler updated = this.Updated;
      if (updated == null)
        return;
      updated((object) this);
    }

    public event ReferenceControl.ReferenceFrameEventHandler FilePathChanged;

    public void OnFilePathChanged()
    {
      ReferenceControl.ReferenceFrameEventHandler filePathChanged = this.FilePathChanged;
      if (filePathChanged == null)
        return;
      filePathChanged((object) this);
    }

    public void Init()
    {
      if (!(this._name == "Head") && !(this._name == "Other") && !(this._name == "Body"))
        return;
      if (this._name == "Body")
        this._cbAnchor.SelectedIndex = 3;
      if (this._name == "Head")
      {
        int previousIndex = 3;
        this._actEditor.References.First<ReferenceControl>((Func<ReferenceControl, bool>) (p => p._name == "Body"))._cbRef.Checked += (RoutedEventHandler) delegate
        {
          previousIndex = this._cbAnchor.SelectedIndex;
          this._cbAnchor.SelectedIndex = 0;
        };
        this._actEditor.References.First<ReferenceControl>((Func<ReferenceControl, bool>) (p => p._name == "Body"))._cbRef.Unchecked += (RoutedEventHandler) delegate
        {
          this._cbAnchor.SelectedIndex = previousIndex;
        };
      }
      this._cbAnchor.SelectionChanged += (SelectionChangedEventHandler) ((e, a) =>
      {
        this._frame_Updated(e);
        this._actEditor.OnReferencesChanged();
      });
      this.Updated += new ReferenceControl.ReferenceFrameEventHandler(this._frame_Updated);
      this._actEditor.References.First<ReferenceControl>((Func<ReferenceControl, bool>) (p => p._name == "Neighbor")).Updated += new ReferenceControl.ReferenceFrameEventHandler(this._frame_Updated);
      this._actEditor.ActLoaded += (ActEditorWindow.ActEditorEventDelegate) (e =>
      {
        this._frame_Updated((object) null);
        if (this._actEditor.Act != null)
        {
          foreach (ReferenceControl reference in this._actEditor.References)
          {
            if (reference.Act != null && reference.Act.Name == "Body")
            {
              if (GrfEditorConfiguration.ReverseAnchor)
              {
                this._actEditor.Act.AnchoredTo = reference.Act;
                reference.Act.AnchoredTo = (Act) null;
                break;
              }
              reference.RefreshSelection();
              break;
            }
          }
        }
        this._actEditor._framePreview.Update();
      });
    }

    public void RefreshSelection() => this._frame_Updated((object) null);

    private void _frame_Updated(object sender)
    {
      int index = this._cbAnchor.SelectedIndex;
      if (this.Act != null)
        this.Act.AnchoredTo = (Act) null;
      switch (this._cbAnchor.SelectedIndex)
      {
        case 0:
        case 1:
        case 2:
          ReferenceControl referenceControl = this._actEditor.References.First<ReferenceControl>((Func<ReferenceControl, bool>) (p => p._name == (index == 0 ? "Body" : (index == 1 ? "Other" : "Neighbor"))));
          if (this.Act == null || !this.ShowReference || !referenceControl.ShowReference)
            break;
          this.Act.AnchoredTo = referenceControl.Act;
          break;
        case 3:
          if (this.Act == null || !this.ShowReference || this._actEditor.Act == null)
            break;
          this.Act.AnchoredTo = this._actEditor.Act;
          break;
      }
    }

    public void Update(bool updateSprite)
    {
      if (updateSprite)
      {
        this.MakeAct(true);
        this.Act.Name = this._name;
      }
      this._layerControl.ReferenceSetAndUpdate(this.Act, this._actEditor._frameSelector.SelectedAction, this._actEditor._frameSelector.SelectedFrame, 0, false);
      this.OnUpdated();
      this._actEditor.OnReferencesChanged();
    }

    public void Reset()
    {
      if (this._cbAnchor.SelectedIndex != 3 || this.Act == null)
        return;
      this.Act.AnchoredTo = (Act) null;
    }

    private void _cbAnchor_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this._cbAnchor.Items.Cast<ComboBoxItem>().ToList<ComboBoxItem>().ForEach((Action<ComboBoxItem>) (p => p.SetValue(Control.FontWeightProperty, (object) FontWeights.Normal)));
      if (this._cbAnchor.SelectedItem == null)
        return;
      ((DependencyObject) this._cbAnchor.SelectedItem).SetValue(Control.FontWeightProperty, (object) FontWeights.Bold);
    }

    private void _referenceFrame_FilePathChanged(object sender)
    {
      this._reset.Visibility = string.IsNullOrEmpty(this.FilePath) ? Visibility.Hidden : Visibility.Visible;
      if (this._directional)
        return;
      this._gender.Visibility = string.IsNullOrEmpty(this.FilePath) ? Visibility.Visible : Visibility.Hidden;
    }

    private void _buttonSprite_Click(object sender)
    {
      try
      {
        string str1 = GrfEditorConfiguration.ExtractingServiceLastPath;
        if (this.FilePath != null && File.Exists(this.FilePath))
          str1 = this.FilePath;
        string str2 = PathRequest.OpenFileExtract("fileName", str1, "filter", "Act or Container Files|*.act;*.grf;*.rgz;*.gpf;*.thor|Act Files|*.act|Container Files|*.grf;*.rgz;*.gpf;*.thor");
        if (str2 == null)
          return;
        if (str2.IsExtension(".grf", ".rgz", ".gpf", ".thor"))
        {
          GrfExplorer grfExplorer = new GrfExplorer(str2, SelectMode.Act);
          bool? nullable = grfExplorer.ShowDialog();
          if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
            return;
          str2 = str2 + "?" + grfExplorer.SelectedItem;
        }
        this.FilePath = str2;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _reset_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        this.FilePath = (string) null;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    private void _fancyButton_Click(object sender, RoutedEventArgs e)
    {
      this._fancyButtons.ForEach((Action<FancyButton>) (p => p.IsPressed = false));
      FancyButton fancyButton = (FancyButton) sender;
      fancyButton.IsPressed = true;
      int num1 = 0;
      int num2 = 0;
      switch ((string) fancyButton.Tag)
      {
        case "0":
          num2 += 30;
          break;
        case "1":
          num1 -= 25;
          num2 += 30;
          break;
        case "2":
          num1 -= 25;
          break;
        case "3":
          num1 -= 25;
          num2 -= 30;
          break;
        case "4":
          num2 -= 30;
          break;
        case "5":
          num1 += 25;
          num2 -= 30;
          break;
        case "6":
          num1 += 25;
          break;
        case "7":
          num1 += 25;
          num2 += 30;
          break;
      }
      this._layerControl._tbOffsetX.Text = num1.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      this._layerControl._tbOffsetY.Text = num2.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    private void _buttonAnchor_Click(object sender, RoutedEventArgs e) => this._cbAnchor.IsDropDownOpen = true;

    private void _gender_Click(object sender, RoutedEventArgs e)
    {
      this._sex = !this._sex;
      this.Sex = this._sex;
    }

    public string ReferenceName => this._name;

    public bool Sex
    {
      get => this._sex;
      set
      {
        this._sex = value;
        GrfEditorConfiguration.ConfigAsker["[ActEditor - Gender - " + this._name + "]"] = value.ToString();
        this._updateGenderButton();
        this.Update(true);
      }
    }

    public void MakeAct(bool force = false)
    {
      if (this.Act != null && this.Spr != null && !force)
        return;
      byte[] actData = ApplicationManager.GetResource((this._sex ? this._defaultFemale : this._defaultMale) + ".act");
      byte[] sprData = ApplicationManager.GetResource((this._sex ? this._defaultFemale : this._defaultMale) + ".spr");
      if (this.FilePath != null)
      {
        if (this.FilePath.IsExtension(".spr", ".act"))
        {
          TkPath filePath = (TkPath) this.FilePath;
          if (File.Exists(filePath.FilePath))
          {
            try
            {
              actData = GrfPath.GetData(filePath);
              if (actData == null)
                throw new FileNotFoundException("File not found : " + (string) filePath);
            }
            catch (Exception ex)
            {
              ErrorHandler.HandleException(ex);
            }
            try
            {
              TkPath tkPath = (TkPath) filePath.GetFullPath().ReplaceExtension(".spr");
              sprData = GrfPath.GetData(tkPath);
              if (sprData == null)
                throw new FileNotFoundException("File not found : " + (string) tkPath);
            }
            catch (Exception ex)
            {
              ErrorHandler.HandleException(ex);
            }
          }
        }
      }
      this.Spr = new Spr((MultiType) sprData);
      this.Act = new Act((MultiType) actData, this.Spr);
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/editorcontrols/referencecontrol.xaml", UriKind.Relative));
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
          this._cbRef = (CheckBox) target;
          break;
        case 2:
          this._grid = (Grid) target;
          break;
        case 3:
          this._fancyButton3 = (FancyButton) target;
          this._fancyButton3.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 4:
          this._fancyButton4 = (FancyButton) target;
          this._fancyButton4.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 5:
          this._fancyButton5 = (FancyButton) target;
          this._fancyButton5.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 6:
          this._fancyButton2 = (FancyButton) target;
          this._fancyButton2.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 7:
          this._fancyButton6 = (FancyButton) target;
          this._fancyButton6.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 8:
          this._fancyButton1 = (FancyButton) target;
          this._fancyButton1.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 9:
          this._fancyButton0 = (FancyButton) target;
          this._fancyButton0.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 10:
          this._fancyButton7 = (FancyButton) target;
          this._fancyButton7.Click += new RoutedEventHandler(this._fancyButton_Click);
          break;
        case 11:
          this._buttonAnchor = (FancyButton) target;
          this._buttonAnchor.Click += new RoutedEventHandler(this._buttonAnchor_Click);
          break;
        case 12:
          this._cbAnchor = (ComboBox) target;
          break;
        case 13:
          this._refZState = (FancyButton) target;
          break;
        case 14:
          this._buttonSprite = (LinkControl) target;
          break;
        case 15:
          this._reset = (FancyButton) target;
          this._reset.Click += new RoutedEventHandler(this._reset_Click);
          break;
        case 16:
          this._gender = (FancyButton) target;
          this._gender.Click += new RoutedEventHandler(this._gender_Click);
          break;
        case 17:
          this._sp = (StackPanel) target;
          break;
        case 18:
          this._header = (LayerControlHeader) target;
          break;
        case 19:
          this._rectangleVisibility = (Rectangle) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void ReferenceFrameEventHandler(object sender);
  }
}
