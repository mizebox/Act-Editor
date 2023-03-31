// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.EditPalette
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using GRF.FileFormats.ActFormat;
using GRF.FileFormats.PalFormat;
using GRF.Threading;
using PaletteEditor;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using TokeiLibrary;
using Utilities;

namespace ActEditor.Core.Scripts
{
  public class EditPalette : IActScript
  {
    public static bool CanOpen = true;
    private readonly object _palLock = new object();
    private readonly object _quickLock = new object();
    private int _count;
    private int _currentEvent;

    public void Execute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      if (act.Sprite.Palette == null)
        return;
      this._count = 0;
      EditPalette.CanOpen = false;
      SingleColorEditDialog dialog = new SingleColorEditDialog();
      dialog.PreviewKeyDown += (KeyEventHandler) delegate
      {
        if (!Keyboard.IsKeyDown(Key.Escape))
          return;
        dialog.Close();
      };
      byte[] paletteBefore = Methods.Copy(act.Sprite.Palette.BytePalette);
      Pal pal = new Pal(paletteBefore);
      dialog.SingleColorEditControl.SetPalette(pal);
      pal.PaletteChanged += (Pal.PalEventHandler) delegate
      {
        int currentId;
        lock (this._quickLock)
        {
          this._currentEvent = this._count;
          currentId = this._count;
          ++this._count;
        }
        GrfThread.Start((System.Action) (() =>
        {
          lock (this._palLock)
          {
            try
            {
              if (currentId != this._currentEvent)
                return;
              act.Sprite.Palette.SetPalette(pal.BytePalette);
              if (currentId != this._currentEvent)
                return;
              dialog.Dispatch<SingleColorEditDialog>((Action<SingleColorEditDialog>) (p => act.InvalidateVisual()));
              if (currentId != this._currentEvent)
                return;
              dialog.Dispatch<SingleColorEditDialog>((Action<SingleColorEditDialog>) (p => act.InvalidatePaletteVisual()));
            }
            finally
            {
              lock (this._quickLock)
                --this._count;
            }
            Thread.Sleep(2000);
          }
        }));
      };
      dialog.Owner = WpfUtilities.TopWindow;
      dialog.Closing += (CancelEventHandler) delegate
      {
        EditPalette.CanOpen = true;
        act.Sprite.Palette.SetPalette(paletteBefore);
        if (Methods.ByteArrayCompare(paletteBefore, 4, 1020, dialog.SingleColorEditControl.PaletteSelector.Palette.BytePalette, 4))
          return;
        act.Commands.SpriteSetPalette(dialog.SingleColorEditControl.PaletteSelector.Palette.BytePalette);
        act.InvalidateVisual();
      };
      dialog.Show();
    }

    public bool CanExecute(
      Act act,
      int selectedActionIndex,
      int selectedFrameIndex,
      int[] selectedLayerIndexes)
    {
      return act != null && EditPalette.CanOpen && act.Sprite.NumberOfIndexed8Images > 0;
    }

    public object DisplayName => (object) "Quick palette edit...";

    public string Group => "Edit";

    public string InputGesture => (string) null;

    public string Image => "pal.png";
  }
}
