// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.DrawingComponents.ImageDraw
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.Dialogs;
using ErrorManager;
using GRF;
using GRF.FileFormats;
using GRF.FileFormats.ActFormat;
using GRF.Image;
using GRF.IO;
using GRF.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TokeiLibrary;
using Utilities.Extension;

namespace ActEditor.Core.DrawingComponents
{
  public class ImageDraw : DrawingComponent
  {
    private readonly IPreviewEditor _actEditor;
    private readonly int _imageIndex;
    private System.Windows.Controls.Image _image;
    private string _toolTip = "";

    public ImageDraw(int imageIndex, IPreviewEditor actEditor)
    {
      this._imageIndex = imageIndex;
      this._actEditor = actEditor;
      this.Selected += new DrawingComponent.DrawingComponentDelegate(this._imageDraw_Selected);
    }

    public bool IsPreviewing { get; set; }

    public Rectangle Overlay { get; private set; }

    public Border Border { get; private set; }

    private void _imageDraw_Selected(object sender, int index, bool selected)
    {
      this._initBorder();
      this.Border.BorderBrush = (Brush) new SolidColorBrush((selected ? GrfEditorConfiguration.ActEditorSpriteSelectionBorder : GrfColor.FromArgb((byte) 0, (byte) 0, (byte) 0, (byte) 0)).ToColor());
    }

    public override void Render(IPreview frameEditor)
    {
      this._initBorder();
      GrfImage grfImage = frameEditor.Act.Sprite.Images[this._imageIndex];
      int count = this._actEditor.Act.FindUsageOf(this._imageIndex).Count;
      string str = "Image #" + (object) this._imageIndex + "\r\nWidth = " + (object) grfImage.Width + "\r\nHeight = " + (object) grfImage.Height + "\r\nFormat = " + (grfImage.GrfImageType == GrfImageType.Indexed8 ? (object) "Indexed8" : (object) "Bgra32") + "\r\nUsed in " + (object) count + " layer(s)";
      if (str != this._toolTip)
      {
        this._toolTip = str;
        if (this.Border.ToolTip == null)
          this.Border.ToolTip = (object) new TextBlock();
        ((TextBlock) this.Border.ToolTip).Text = this._toolTip;
      }
      if (grfImage.GrfImageType == GrfImageType.Indexed8)
      {
        grfImage = grfImage.Copy();
        grfImage.Palette[3] = (byte) 0;
      }
      this._image.Source = (ImageSource) grfImage.Cast<BitmapSource>();
    }

    public override void QuickRender(IPreview frameEditor)
    {
      GrfImage image = frameEditor.Act.Sprite.Images[this._imageIndex];
      if (image.GrfImageType != GrfImageType.Indexed8)
        return;
      GrfImage grfImage = image.Copy();
      grfImage.Palette[3] = (byte) 0;
      this._image.Source = (ImageSource) grfImage.Cast<BitmapSource>();
    }

    public override void Remove(IPreview frameEditor)
    {
      if (this.Border == null)
        return;
      frameEditor.Canva.Children.Remove((UIElement) this.Border);
    }

    private void _initBorder()
    {
      if (this.Border == null)
      {
        this.Border = new Border();
        this.Border.Background = (Brush) Brushes.Transparent;
        this.Border.BorderThickness = new Thickness(1.0);
        this.Border.Margin = new Thickness(0.0);
        this.Border.BorderBrush = (Brush) new SolidColorBrush(Colors.Transparent);
        this._initImage();
        Grid grid = new Grid();
        grid.Margin = new Thickness(1.0);
        this.Border.Child = (UIElement) grid;
        grid.RowDefinitions.Add(new RowDefinition()
        {
          Height = new GridLength(-1.0, GridUnitType.Auto)
        });
        grid.RowDefinitions.Add(new RowDefinition());
        grid.Children.Add((UIElement) this._image);
        this._image.VerticalAlignment = VerticalAlignment.Top;
        this._image.HorizontalAlignment = HorizontalAlignment.Left;
        this._image.SetValue(Grid.RowProperty, (object) 1);
        this._image.Stretch = Stretch.None;
        this._image.SnapsToDevicePixels = true;
        grid.SizeChanged += (SizeChangedEventHandler) delegate
        {
          this._image.Margin = new Thickness((double) (int) ((grid.ActualWidth - this._image.ActualWidth) / 2.0), (double) (int) ((grid.ActualHeight - this._image.ActualHeight) / 2.0), 0.0, 0.0);
        };
        this.Border.SetBinding(FrameworkElement.HeightProperty, (BindingBase) new Binding("ViewportHeight")
        {
          Source = (object) this._actEditor.SpriteSelector._sv
        });
        this.Border.SnapsToDevicePixels = true;
        Label element1 = new Label();
        element1.Content = (object) this._imageIndex;
        element1.HorizontalAlignment = HorizontalAlignment.Center;
        element1.VerticalAlignment = VerticalAlignment.Bottom;
        element1.FontWeight = FontWeights.Bold;
        element1.FontSize = 13.0;
        Label element2 = new Label();
        element2.Content = (object) this._imageIndex;
        element2.HorizontalAlignment = HorizontalAlignment.Center;
        element2.VerticalAlignment = VerticalAlignment.Bottom;
        element2.Effect = (Effect) new BlurEffect();
        element2.Foreground = (Brush) Brushes.White;
        element2.FontWeight = FontWeights.Bold;
        element2.FontSize = 13.0;
        grid.Children.Add((UIElement) element2);
        grid.Children.Add((UIElement) element1);
        element1.SetValue(Grid.RowProperty, (object) 1);
        element2.SetValue(Grid.RowProperty, (object) 1);
        this.Overlay = new Rectangle();
        this.Overlay.Fill = (Brush) Brushes.Transparent;
        this.Overlay.SetValue(UIElement.IsHitTestVisibleProperty, (object) false);
        this.Overlay.SetValue(Grid.RowSpanProperty, (object) 2);
        grid.Children.Add((UIElement) this.Overlay);
      }
      if (this.IsSelectable)
        return;
      this.IsSelected = false;
      this.IsHitTestVisible = false;
      if (this._image == null)
        return;
      this._image.IsHitTestVisible = false;
    }

    private void _initImage()
    {
      if (this._image != null)
        return;
      this._image = new System.Windows.Controls.Image();
      if (!this.IsSelectable)
      {
        this._image.IsHitTestVisible = false;
      }
      else
      {
        this.Border.MouseDown += (MouseButtonEventHandler) ((sender, e) =>
        {
          if (e.LeftButton == MouseButtonState.Pressed)
            this.IsSelected = true;
          this._actEditor.SpriteSelector.DeselectAllExcept(this);
        });
        this.Border.MouseMove += new MouseEventHandler(this._image_MouseMove);
        this.Border.MouseEnter += (MouseEventHandler) delegate
        {
          this.IsSelected = true;
          this._actEditor.SpriteSelector.DeselectAllExcept(this);
        };
        this.Border.MouseLeave += (MouseEventHandler) ((sender, e) =>
        {
          if (e.LeftButton == MouseButtonState.Released)
            this.IsSelected = false;
          if (this._image.ContextMenu == null || !this._image.ContextMenu.IsOpen)
            return;
          this.IsSelected = true;
        });
        this.Border.MouseUp += (MouseButtonEventHandler) ((sender, e) =>
        {
          if (e.ChangedButton != MouseButton.Right)
            return;
          if (this._image.ContextMenu == null)
          {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem newItem1 = new MenuItem();
            newItem1.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("forward.png")
            };
            newItem1.Header = (object) "Add after...";
            newItem1.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.After));
            contextMenu.Items.Add((object) newItem1);
            MenuItem newItem2 = new MenuItem();
            newItem2.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("backward.png")
            };
            newItem2.Header = (object) "Add before...";
            newItem2.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.Before));
            contextMenu.Items.Add((object) newItem2);
            MenuItem newItem3 = new MenuItem();
            newItem3.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("delete.png")
            };
            newItem3.Header = (object) "Remove";
            newItem3.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.Remove));
            contextMenu.Items.Add((object) newItem3);
            MenuItem newItem4 = new MenuItem();
            newItem4.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("refresh.png")
            };
            newItem4.Header = (object) "Replace...";
            newItem4.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.Replace));
            contextMenu.Items.Add((object) newItem4);
            contextMenu.Items.Add((object) new Separator());
            MenuItem newItem5 = new MenuItem();
            newItem5.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("flip2.png")
            };
            newItem5.Header = (object) "Flip horizontal";
            newItem5.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.ReplaceFlipHorizontal));
            contextMenu.Items.Add((object) newItem5);
            MenuItem newItem6 = new MenuItem();
            newItem6.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("flip.png")
            };
            newItem6.Header = (object) "Flip vertical";
            newItem6.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.ReplaceFlipVertical));
            contextMenu.Items.Add((object) newItem6);
            contextMenu.Items.Add((object) new Separator());
            MenuItem menuItem2 = new MenuItem();
            menuItem2.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("convert.png")
            };
            menuItem2.Header = (object) "Convert to ";
            menuItem2.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.Convert));
            contextMenu.Opened += (RoutedEventHandler) delegate
            {
              menuItem2.Header = (object) ("Convert to " + (this._actEditor.Act.Sprite.Images[this._imageIndex].GrfImageType == GrfImageType.Indexed8 ? "Bgra32" : "Indexed8"));
            };
            contextMenu.Items.Add((object) menuItem2);
            MenuItem newItem7 = new MenuItem();
            newItem7.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("export.png")
            };
            newItem7.Header = (object) "Export...";
            newItem7.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.Export));
            contextMenu.Items.Add((object) newItem7);
            contextMenu.Items.Add((object) new Separator());
            MenuItem newItem8 = new MenuItem();
            newItem8.Icon = (object) new System.Windows.Controls.Image()
            {
              Source = (ImageSource) ApplicationManager.GetResourceImage("help.png")
            };
            newItem8.Header = (object) "Find usage...";
            newItem8.Click += (RoutedEventHandler) ((s, args) => this._insert(SpriteEditMode.Usage));
            contextMenu.Items.Add((object) newItem8);
            this._image.ContextMenu = contextMenu;
          }
          this._image.ContextMenu.IsOpen = true;
          e.Handled = true;
        });
      }
    }

    private void _insert(SpriteEditMode mode)
    {
      if (this._actEditor.SpriteManager.IsModeDisabled(mode))
      {
        ErrorHandler.HandleException("This feature is disabled.");
      }
      else
      {
        switch (mode)
        {
          case SpriteEditMode.Replace:
          case SpriteEditMode.Before:
          case SpriteEditMode.After:
            string[] source = PathRequest.OpenFilesExtract("filter", FileFormat.MergeFilters(Format.Image));
            if (source == null)
              break;
            if (source.Length <= 0)
              break;
            try
            {
              try
              {
                this._actEditor.Act.Commands.BeginNoDelay();
                SpriteManager.SpriteConverterOption = -1;
                try
                {
                  List<GrfImage> list = ((IEnumerable<string>) source).Where<string>((Func<string, bool>) (p => p.IsExtension(".bmp", ".jpg", ".png", ".tga"))).Select<string, GrfImage>((Func<string, GrfImage>) (file1 => new GrfImage((MultiType) file1))).ToList<GrfImage>();
                  int imageIndex = this._imageIndex;
                  using (List<GrfImage>.Enumerator enumerator = list.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      GrfImage current = enumerator.Current;
                      this._actEditor.SpriteManager.Execute(imageIndex, current, mode);
                      ++imageIndex;
                    }
                    break;
                  }
                }
                catch (OperationCanceledException ex)
                {
                  break;
                }
              }
              catch (Exception ex)
              {
                this._actEditor.Act.Commands.CancelEdit();
                ErrorHandler.HandleException(ex);
                break;
              }
              finally
              {
                this._actEditor.Act.Commands.End();
                SpriteManager.SpriteConverterOption = -1;
              }
            }
            catch (Exception ex)
            {
              ErrorHandler.HandleException(ex);
              break;
            }
          case SpriteEditMode.Remove:
          case SpriteEditMode.ReplaceFlipHorizontal:
          case SpriteEditMode.ReplaceFlipVertical:
          case SpriteEditMode.Export:
          case SpriteEditMode.Convert:
            this._actEditor.SpriteManager.Execute(this._imageIndex, (GrfImage) null, mode);
            break;
          case SpriteEditMode.Usage:
            new UsageDialog(this._actEditor, (IEnumerable<ActIndex>) this._actEditor.Act.FindUsageOf(this._imageIndex)).Show();
            break;
        }
      }
    }

    private void _image_MouseMove(object sender, MouseEventArgs e)
    {
      if (!this._valideMouseOperation() || e.LeftButton != MouseButtonState.Pressed)
        return;
      this.IsSelected = true;
      this._actEditor.SpriteSelector.DeselectAllExcept(this);
      DataObject data = new DataObject();
      try
      {
        if (this._imageIndex > -1)
        {
          if (this._imageIndex < this._actEditor.Act.Sprite.NumberOfImagesLoaded)
          {
            GrfImage image = this._actEditor.Act.Sprite.Images[this._imageIndex];
            string pathDestination = GrfPath.Combine(Settings.TempPath, string.Format("image_{0:000}", (object) this._imageIndex) + (image.GrfImageType == GrfImageType.Indexed8 ? ".bmp" : ".png"));
            image.Save(pathDestination);
            data.SetData(DataFormats.FileDrop, (object) new string[1]
            {
              pathDestination
            });
          }
        }
      }
      catch
      {
      }
      data.SetData("ImageIndex", (object) this._imageIndex);
      int num = (int) DragDrop.DoDragDrop((DependencyObject) this, (object) data, DragDropEffects.Copy);
      this.IsSelected = false;
      if (this._actEditor.LayerEditor != null)
        this._actEditor.LayerEditor._lineMoveLayer.Visibility = Visibility.Hidden;
      e.Handled = true;
    }

    private bool _valideMouseOperation()
    {
      if (this.IsSelectable)
        return true;
      this.IsSelected = false;
      return false;
    }
  }
}
