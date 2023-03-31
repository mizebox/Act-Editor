// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.GrfShellExplorer.PreviewTabs.FilePreviewTab
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.Core;
using System;
using System.Threading;
using System.Windows.Controls;

namespace ActEditor.Tools.GrfShellExplorer.PreviewTabs
{
  public class FilePreviewTab : UserControl
  {
    protected readonly object _lock = new object();
    protected FileEntry _entry;
    protected GrfHolder _grfData;
    protected Action _isInvisibleResult;
    protected FileEntry _oldEntry;
    protected bool _requiresSTA;

    public FilePreviewTab()
    {
    }

    protected FilePreviewTab(bool requiresSTA = false) => this._requiresSTA = requiresSTA;

    protected Func<bool> _isCancelRequired { get; private set; }

    public void Update(bool forceUpdate)
    {
      if (forceUpdate)
        this._oldEntry = (FileEntry) null;
      this.Update();
    }

    public void Update()
    {
      if (this._oldEntry == this._entry)
        return;
      if (this._isInvisibleResult != null)
        this.Dispatcher.Invoke((Delegate) (() =>
        {
          if (this.IsVisible || this._isInvisibleResult == null)
            return;
          this._isInvisibleResult();
        }));
      Thread thread = new Thread((ThreadStart) (() => this._baseLoad(this._entry)))
      {
        Name = "GrfEditor - IPreview base loading thread"
      };
      if (this._requiresSTA)
        thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    public void Load(GrfHolder grfData, FileEntry entry)
    {
      this._entry = entry;
      this._grfData = grfData;
      this.Update();
    }

    protected void _baseLoad(FileEntry entry)
    {
      try
      {
        lock (this._lock)
        {
          if (this._entry == null)
            return;
          this._isCancelRequired = (Func<bool>) (() => entry != this._entry);
          if (this._isCancelRequired())
            return;
          this._load(entry);
          this._oldEntry = entry;
        }
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
      }
    }

    protected virtual void _load(FileEntry entry)
    {
    }

    public void InvalidateOnReload(GrfHolder grf)
    {
      lock (this._lock)
      {
        this._oldEntry = (FileEntry) null;
        if (!grf.FileTable.ContainsFile(this._entry.RelativePath))
          return;
        this.Load(grf, grf.FileTable[this._entry.RelativePath]);
      }
    }
  }
}
