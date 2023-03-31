// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.PaletteEditorTool.SpriteModifiedCommand
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.PalFormat;
using GRF.FileFormats.SprFormat;

namespace ActEditor.Tools.PaletteEditorTool
{
  public class SpriteModifiedCommand : IPaletteCommand
  {
    private readonly Spr _currentSprite;
    private readonly Spr _oldSprite;
    private readonly Spr _newSprite;

    public SpriteModifiedCommand(Spr currentSprite, Spr oldSprite, Spr newSprite)
    {
      this._currentSprite = currentSprite;
      this._oldSprite = oldSprite;
      this._newSprite = newSprite;
    }

    public void Execute(Pal palette)
    {
      for (int index = 0; index < this._currentSprite.Images.Count; ++index)
        this._currentSprite.Images[index] = this._newSprite.Images[index];
      this._currentSprite.Palette.OnPaletteChanged();
    }

    public void Undo(Pal palette)
    {
      for (int index = 0; index < this._currentSprite.Images.Count; ++index)
        this._currentSprite.Images[index] = this._oldSprite.Images[index];
      this._currentSprite.Palette.OnPaletteChanged();
    }

    public string CommandDescription => "Source sprite changed";
  }
}
