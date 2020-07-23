// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.Cab.CabFileInfo
// Assembly: Microsoft.Deployment.Compression.Cab, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: D94CEDF8-4B4A-4AC8-B27E-50F0AAABF518
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.Cab.dll

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Deployment.Compression.Cab
{
  [Serializable]
  public class CabFileInfo : ArchiveFileInfo
  {
    private int cabFolder;

    public CabFileInfo(CabInfo cabinetInfo, string filePath)
      : base((ArchiveInfo) cabinetInfo, filePath)
    {
      if (cabinetInfo == null)
        throw new ArgumentNullException(nameof (cabinetInfo));
      this.cabFolder = -1;
    }

    internal CabFileInfo(
      string filePath,
      int cabFolder,
      int cabNumber,
      FileAttributes attributes,
      DateTime lastWriteTime,
      long length)
      : base(filePath, cabNumber, attributes, lastWriteTime, length)
    {
      this.cabFolder = cabFolder;
    }

    protected CabFileInfo(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.cabFolder = info.GetInt32(nameof (cabFolder));
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("cabFolder", this.cabFolder);
    }

    public CabInfo Cabinet
    {
      get
      {
        return (CabInfo) this.Archive;
      }
    }

    public string CabinetName
    {
      get
      {
        return this.ArchiveName;
      }
    }

    public int CabinetFolderNumber
    {
      get
      {
        if (this.cabFolder < 0)
          this.Refresh();
        return this.cabFolder;
      }
    }

    protected override void Refresh(ArchiveFileInfo newFileInfo)
    {
      base.Refresh(newFileInfo);
      this.cabFolder = ((CabFileInfo) newFileInfo).cabFolder;
    }
  }
}
