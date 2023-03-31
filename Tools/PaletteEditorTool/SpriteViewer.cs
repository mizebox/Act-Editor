// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.PaletteEditorTool.SpriteViewer
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.SprFormat;
using GRF.Image;
using System.Windows.Media.Imaging;

namespace ActEditor.Tools.PaletteEditorTool
{
  public class SpriteViewer : ImageViewer
  {
    private Spr _spr;

    public void SetSpr(Spr spr) => this._spr = spr;

    public void Load(int index) => this.Load(this._spr.Images[index].Cast<BitmapSource>());

    public void LoadIndexed8(int index) => this.Load(this._spr.Images[index].Cast<BitmapSource>());

    public void LoadImage(GrfImage image) => this.Load(image.Cast<BitmapSource>());

    public void LoadBgra32(int index) => this.Load(this._spr.Images[index + this._spr.NumberOfIndexed8Images].Cast<BitmapSource>());
  }
}
