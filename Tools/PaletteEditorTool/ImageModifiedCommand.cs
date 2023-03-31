// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.PaletteEditorTool.ImageModifiedCommand
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.PalFormat;
using GRF.FileFormats.SprFormat;
using GRF.Image;

namespace ActEditor.Tools.PaletteEditorTool
{
  public class ImageModifiedCommand : IPaletteCommand
  {
    private readonly Spr _sprite;
    private readonly int _index;
    private GrfImage _oldSprite;
    private readonly GrfImage _newSprite;

    public ImageModifiedCommand(Spr sprite, int index, GrfImage newSprite)
    {
      this._sprite = sprite;
      this._index = index;
      this._newSprite = newSprite;
    }

    public void Execute(Pal palette)
    {
      if (this._oldSprite == null)
        this._oldSprite = this._sprite.Images[this._index].Copy();
      this._sprite.Images[this._index] = this._newSprite;
      this._sprite.Palette.OnPaletteChanged();
    }

    public void Undo(Pal palette)
    {
      this._sprite.Images[this._index] = this._oldSprite;
      this._sprite.Palette.OnPaletteChanged();
    }

    public string CommandDescription => "Sprite changed";
  }
}
