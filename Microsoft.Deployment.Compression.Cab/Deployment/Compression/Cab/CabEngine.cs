// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.Cab.CabEngine
// Assembly: Microsoft.Deployment.Compression.Cab, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: D94CEDF8-4B4A-4AC8-B27E-50F0AAABF518
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.Cab.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Deployment.Compression.Cab
{
  public class CabEngine : CompressionEngine
  {
    private CabPacker packer;
    private CabUnpacker unpacker;

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (this.packer != null)
        {
          this.packer.Dispose();
          this.packer = (CabPacker) null;
        }
        if (this.unpacker != null)
        {
          this.unpacker.Dispose();
          this.unpacker = (CabUnpacker) null;
        }
      }
      base.Dispose(disposing);
    }

    private CabPacker Packer
    {
      get
      {
        if (this.packer == null)
          this.packer = new CabPacker(this);
        return this.packer;
      }
    }

    private CabUnpacker Unpacker
    {
      get
      {
        if (this.unpacker == null)
          this.unpacker = new CabUnpacker(this);
        return this.unpacker;
      }
    }

    public override void Pack(
      IPackStreamContext streamContext,
      IEnumerable<string> files,
      long maxArchiveSize)
    {
      this.Packer.CompressionLevel = this.CompressionLevel;
      this.Packer.UseTempFiles = this.UseTempFiles;
      this.Packer.Pack(streamContext, files, maxArchiveSize);
    }

    public override bool IsArchive(Stream stream)
    {
      return this.Unpacker.IsArchive(stream);
    }

    public override IList<ArchiveFileInfo> GetFileInfo(
      IUnpackStreamContext streamContext,
      Predicate<string> fileFilter)
    {
      return this.Unpacker.GetFileInfo(streamContext, fileFilter);
    }

    public override void Unpack(IUnpackStreamContext streamContext, Predicate<string> fileFilter)
    {
      this.Unpacker.Unpack(streamContext, fileFilter);
    }

    internal void ReportProgress(ArchiveProgressEventArgs e)
    {
      this.OnProgress(e);
    }
  }
}
