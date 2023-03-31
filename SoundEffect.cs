// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.SoundEffect
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using System.IO;
using System.Media;
using System.Threading;

namespace ActEditor.Core
{
  public class SoundEffect
  {
    private bool _isStopped = true;
    private byte[] _soundFile;
    private Thread _soundThread;

    public bool IsFinished => this._isStopped;

    public void Play(byte[] file)
    {
      this._soundFile = file;
      if (!this._isStopped)
        return;
      this._soundThread = new Thread(new ThreadStart(this._playThread));
      this._soundThread.Start();
    }

    private void _playThread()
    {
      if (this._soundFile == null)
        return;
      this._isStopped = false;
      new SoundPlayer()
      {
        Stream = ((Stream) new MemoryStream(this._soundFile))
      }.PlaySync();
      this._isStopped = true;
    }
  }
}
