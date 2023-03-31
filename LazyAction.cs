// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.LazyAction
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ErrorManager;
using GRF.Threading;
using System;
using System.Collections.Generic;

namespace ActEditor.Core
{
  public class LazyAction
  {
    private static readonly Dictionary<int, object> _locks = new Dictionary<int, object>();
    private static readonly Dictionary<int, object> _locks2 = new Dictionary<int, object>();
    private static readonly Dictionary<int, int> _counts = new Dictionary<int, int>();

    public static void Execute(Action action, int instance)
    {
      if (!LazyAction._locks.ContainsKey(instance))
      {
        LazyAction._locks[instance] = new object();
        LazyAction._locks2[instance] = new object();
        LazyAction._counts[instance] = -1;
      }
      object oLock = LazyAction._locks[instance];
      object qLock = LazyAction._locks2[instance];
      lock (qLock)
      {
        Dictionary<int, int> counts;
        int key;
        (counts = LazyAction._counts)[key = instance] = counts[key] + 1;
      }
      GrfThread.Start((Action) (() =>
      {
        lock (oLock)
        {
          try
          {
            if (LazyAction._counts[instance] > 0)
              return;
            action();
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex);
          }
          finally
          {
            lock (qLock)
            {
              Dictionary<int, int> counts;
              int key;
              (counts = LazyAction._counts)[key = instance] = counts[key] - 1;
            }
          }
        }
      }));
    }
  }
}
