// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.GrfShellExplorer.PreviewTabs.PreviewImage
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using GRF;
using GRF.Core;
using GRF.Image;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace ActEditor.Tools.GrfShellExplorer.PreviewTabs
{
  public partial class PreviewImage : FilePreviewTab, IComponentConnector
  {
    private readonly GrfImageWrapper _wrapper = new GrfImageWrapper();
    private readonly GrfImageWrapper _wrapper2 = new GrfImageWrapper();
    private TransformGroup _regularTransformGroup = new TransformGroup();
    internal Label _labelHeader;
    internal FancyButton _buttonExportAt;
    internal ComboBox _comboBoxAnimationIndex;
    internal ScrollViewer _scrollViewer;
    internal StackPanel _dockPanelImages;
    internal System.Windows.Controls.Image _imagePreview;
    internal MenuItem _menuItemImageExport;
    internal System.Windows.Controls.Image _imagePreviewSprite;
    internal MenuItem _menuItemImageExport2;
    private bool _contentLoaded;

    public PreviewImage()
    {
      this.InitializeComponent();
      this._loadOtherTransformGroup();
      this._imagePreview.RenderTransform = (Transform) this._regularTransformGroup;
      int num;
      this._isInvisibleResult = (Action) (() => num = (int) this._imagePreview.Dispatch<System.Windows.Controls.Image, Visibility>((Func<System.Windows.Controls.Image, Visibility>) (p => p.Visibility = Visibility.Hidden)));
    }

    public ScrollViewer ScrollViewer => this._scrollViewer;

    private void _loadOtherTransformGroup()
    {
      this._regularTransformGroup = new TransformGroup();
      ScaleTransform scaleTransform = new ScaleTransform();
      TranslateTransform translateTransform = new TranslateTransform();
      RotateTransform rotateTransform = new RotateTransform();
      translateTransform.X = 0.0;
      translateTransform.Y = 0.0;
      rotateTransform.Angle = 0.0;
      scaleTransform.ScaleX = 1.0;
      scaleTransform.ScaleY = 1.0;
      this._regularTransformGroup.Children.Add((Transform) rotateTransform);
      this._regularTransformGroup.Children.Add((Transform) scaleTransform);
      this._regularTransformGroup.Children.Add((Transform) translateTransform);
    }

    private void _menuItemImageExport_Click(object sender, RoutedEventArgs e)
    {
      if (this._wrapper.Image == null)
        return;
      this._wrapper.Image.SaveTo(this._entry.RelativePath, PathRequest.ExtractSetting);
    }

    protected override void _load(FileEntry entry)
    {
      try
      {
        string fileName = entry.RelativePath;
        this._imagePreview.Dispatch<System.Windows.Controls.Image, object>((Func<System.Windows.Controls.Image, object>) (p => p.Tag = (object) Path.GetFileNameWithoutExtension(fileName)));
        this._labelHeader.Dispatch<Label, object>((Func<Label, object>) (p => p.Content = (object) ("Image preview : " + Path.GetFileName(fileName))));
        this._wrapper.Image = ImageProvider.GetImage((MultiType) this._grfData.FileTable[fileName].GetDecompressedData(), Path.GetExtension(fileName).ToLower());
        this._imagePreview.Dispatch<System.Windows.Controls.Image, ImageSource>((Func<System.Windows.Controls.Image, ImageSource>) (p => p.Source = (ImageSource) this._wrapper.Image.Cast<BitmapSource>()));
        int num1 = (int) this._imagePreview.Dispatch<System.Windows.Controls.Image, Visibility>((Func<System.Windows.Controls.Image, Visibility>) (p => p.Visibility = Visibility.Visible));
        int num2 = (int) this._scrollViewer.Dispatch<ScrollViewer, Visibility>((Func<ScrollViewer, Visibility>) (p => p.Visibility = Visibility.Visible));
      }
      catch
      {
      }
    }

    private void _buttonExportAt_Click(object sender, RoutedEventArgs e)
    {
      if (this._wrapper.Image == null)
        return;
      this._wrapper.Image.SaveTo(this._entry.RelativePath, PathRequest.ExtractSetting);
    }

    private void _menuItemImageExport2_Click(object sender, RoutedEventArgs e)
    {
      if (this._wrapper2.Image == null)
        return;
      this._wrapper2.Image.SaveTo(this._imagePreviewSprite.Tag.ToString(), PathRequest.ExtractSetting);
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/tools/grfshellexplorer/previewtabs/previewimage.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._labelHeader = (Label) target;
          break;
        case 2:
          this._buttonExportAt = (FancyButton) target;
          this._buttonExportAt.Click += new RoutedEventHandler(this._buttonExportAt_Click);
          break;
        case 3:
          this._comboBoxAnimationIndex = (ComboBox) target;
          break;
        case 4:
          this._scrollViewer = (ScrollViewer) target;
          break;
        case 5:
          this._dockPanelImages = (StackPanel) target;
          break;
        case 6:
          this._imagePreview = (System.Windows.Controls.Image) target;
          break;
        case 7:
          this._menuItemImageExport = (MenuItem) target;
          this._menuItemImageExport.Click += new RoutedEventHandler(this._menuItemImageExport_Click);
          break;
        case 8:
          this._imagePreviewSprite = (System.Windows.Controls.Image) target;
          break;
        case 9:
          this._menuItemImageExport2 = (MenuItem) target;
          this._menuItemImageExport2.Click += new RoutedEventHandler(this._menuItemImageExport2_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
