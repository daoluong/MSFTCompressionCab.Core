// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.Cab.HandleManager`1
// Assembly: Microsoft.Deployment.Compression.Cab, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: D94CEDF8-4B4A-4AC8-B27E-50F0AAABF518
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.Cab.dll

using System.Collections.Generic;

namespace Microsoft.Deployment.Compression.Cab
{
  internal sealed class HandleManager<T> where T : class
  {
    private List<T> handles;

    public HandleManager()
    {
      this.handles = new List<T>();
    }

    public T this[int handle]
    {
      get
      {
        if (handle > 0 && handle <= this.handles.Count)
          return this.handles[checked (handle - 1)];
        return default (T);
      }
    }

    public int AllocHandle(T obj)
    {
      this.handles.Add(obj);
      return this.handles.Count;
    }

    public void FreeHandle(int handle)
    {
      if (handle <= 0 || handle > this.handles.Count)
        return;
      this.handles[checked (handle - 1)] = default (T);
    }
  }
}
