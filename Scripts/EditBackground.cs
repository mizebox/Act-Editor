// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.EditBackground
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using GRF;
using GRF.FileFormats;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.Paths;
using Utilities;
using Utilities.Extension;

namespace ActEditor.Core.Scripts
{
  public class EditBackground : IActScript
  {
    private readonly ActEditorWindow _actEditor;
    private readonly Grid _gridBackground;

    public EditBackground(ActEditorWindow actEditor)
    {
      this._actEditor = actEditor;
      this._gridBackground = this._actEditor._framePreview._gridBackground;
      this._gridBackground.Loaded += (RoutedEventHandler) delegate
      {
        this._loadBackground(this.BackgroundPath);
      };
    }

    public object DisplayName
    {
      get
      {
        Grid displayName = new Grid();
        displayName.ColumnDefinitions.Add(new ColumnDefinition());
        displayName.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = new GridLength(-1.0, GridUnitType.Auto)
        });
        displayName.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = new GridLength(-1.0, GridUnitType.Auto)
        });
        Label label = new Label();
        label.Content = (object) "Select background";
        label.Padding = new Thickness(0.0);
        label.Margin = new Thickness(0.0);
        label.VerticalAlignment = VerticalAlignment.Center;
        Label element1 = label;
        displayName.Children.Add((UIElement) element1);
        element1.SetValue(Grid.ColumnProperty, (object) 0);
        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
        image.Source = (ImageSource) ApplicationManager.GetResourceImage("reset.png");
        image.Width = 16.0;
        image.Height = 16.0;
        image.Stretch = Stretch.None;
        image.ToolTip = (object) "Resets the background to its original value.";
        System.Windows.Controls.Image element2 = image;
        displayName.Children.Add((UIElement) element2);
        element2.SetValue(Grid.ColumnProperty, (object) 2);
        element2.MouseEnter += (MouseEventHandler) ((s, e) => Mouse.OverrideCursor = Cursors.Hand);
        element2.MouseLeave += (MouseEventHandler) ((s, e) => Mouse.OverrideCursor = (Cursor) null);
        element2.PreviewMouseDown += (MouseButtonEventHandler) ((s, e) =>
        {
          e.Handled = true;
          this._resetBackground();
        });
        element2.PreviewMouseUp += (MouseButtonEventHandler) ((s, e) =>
        {
          e.Handled = true;
          this._resetBackground();
        });
        return (object) displayName;
      }
    }

    public string Group => "Edit";

    public string InputGesture => "Ctrl-Shift-B";

    public string Image => "background.png";

    public string BackgroundPath
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Script - Background path]", Configuration.ApplicationPath];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Script - Background path]"] = value;
    }

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      this._loadBackground(TkPathRequest.OpenFile(new Setting((object) this, this.GetType().GetProperty("BackgroundPath")), "filter", FileFormat.MergeFilters(Format.Image)));
    }

    private void _loadBackground(string path)
    {
      if (path == null || this._gridBackground == null)
        return;
      this._gridBackground.Dispatch<Grid>((Action<Grid>) delegate
      {
        try
        {
          if (!File.Exists(path))
            return;
          GrfImage grfImage = new GrfImage((MultiType) path);
          this._gridBackground.Background = (Brush) new ImageBrush()
          {
            ImageSource = (ImageSource) grfImage.Cast<BitmapSource>(),
            TileMode = TileMode.Tile,
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = new Rect(0.0, 0.0, (double) grfImage.Width, (double) grfImage.Height)
          };
          ((Panel) this._gridBackground.Children[0]).Background = (Brush) Brushes.Transparent;
          this._actEditor._framePreview.SizeUpdate();
        }
        catch
        {
          this._resetBackground();
        }
      });
    }

    private void _resetBackground()
    {
      if (this._gridBackground == null)
        return;
      VisualBrush visualBrush1 = new VisualBrush();
      visualBrush1.TileMode = TileMode.Tile;
      visualBrush1.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
      visualBrush1.Viewport = new Rect(0.0, 0.0, 0.5, 0.5);
      VisualBrush visualBrush2 = visualBrush1;
      System.Windows.Controls.Image image1 = new System.Windows.Controls.Image();
      image1.Source = (ImageSource) ApplicationManager.GetResourceImage("background.png");
      image1.Width = 256.0;
      image1.Height = 256.0;
      image1.SnapsToDevicePixels = true;
      System.Windows.Controls.Image image2 = image1;
      image2.SetValue(RenderOptions.BitmapScalingModeProperty, (object) BitmapScalingMode.NearestNeighbor);
      visualBrush2.Visual = (Visual) image2;
      this._gridBackground.Background = (Brush) visualBrush2;
      if (File.Exists(this.BackgroundPath))
        this.BackgroundPath = this.BackgroundPath.Replace(this.BackgroundPath.GetExtension() ?? "", "");
      GrfColor grfColor = new GrfColor(Configuration.ConfigAsker["[ActEditor - Background preview color]", GrfColor.ToHex((byte) 150, (byte) 0, (byte) 0, (byte) 0)]);
      ((Panel) this._gridBackground.Children[0]).Background = (Brush) new SolidColorBrush(Color.FromArgb(grfColor.A, grfColor.R, grfColor.G, grfColor.B));
      this._actEditor._framePreview.SizeUpdate();
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return true;
    }
  }
}
