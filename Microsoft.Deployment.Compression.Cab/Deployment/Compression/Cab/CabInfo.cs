// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.Cab.CabInfo
// Assembly: Microsoft.Deployment.Compression.Cab, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: D94CEDF8-4B4A-4AC8-B27E-50F0AAABF518
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.Cab.dll

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Deployment.Compression.Cab
{
  [Serializable]
  public class CabInfo : ArchiveInfo
  {
    public CabInfo(string path)
      : base(path)
    {
    }

    protected CabInfo(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    protected override CompressionEngine CreateCompressionEngine()
    {
      return (CompressionEngine) new CabEngine();
    }

    public IList<CabFileInfo> GetFiles()
    {
      IList<ArchiveFileInfo> files = base.GetFiles();
      List<CabFileInfo> cabFileInfoList = new List<CabFileInfo>(files.Count);
      foreach (CabFileInfo cabFileInfo in (IEnumerable<ArchiveFileInfo>) files)
        cabFileInfoList.Add(cabFileInfo);
      return (IList<CabFileInfo>) cabFileInfoList.AsReadOnly();
    }

    public IList<CabFileInfo> GetFiles(string searchPattern)
    {
      IList<ArchiveFileInfo> files = base.GetFiles(searchPattern);
      List<CabFileInfo> cabFileInfoList = new List<CabFileInfo>(files.Count);
      foreach (CabFileInfo cabFileInfo in (IEnumerable<ArchiveFileInfo>) files)
        cabFileInfoList.Add(cabFileInfo);
      return (IList<CabFileInfo>) cabFileInfoList.AsReadOnly();
    }
  }
}
