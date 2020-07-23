// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.ArchiveProgressEventArgs
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

using System;

namespace Microsoft.Deployment.Compression
{
  public class ArchiveProgressEventArgs : EventArgs
  {
    private ArchiveProgressType progressType;
    private string currentFileName;
    private int currentFileNumber;
    private int totalFiles;
    private long currentFileBytesProcessed;
    private long currentFileTotalBytes;
    private string currentArchiveName;
    private short currentArchiveNumber;
    private short totalArchives;
    private long currentArchiveBytesProcessed;
    private long currentArchiveTotalBytes;
    private long fileBytesProcessed;
    private long totalFileBytes;

    public ArchiveProgressEventArgs(
      ArchiveProgressType progressType,
      string currentFileName,
      int currentFileNumber,
      int totalFiles,
      long currentFileBytesProcessed,
      long currentFileTotalBytes,
      string currentArchiveName,
      int currentArchiveNumber,
      int totalArchives,
      long currentArchiveBytesProcessed,
      long currentArchiveTotalBytes,
      long fileBytesProcessed,
      long totalFileBytes)
    {
      this.progressType = progressType;
      this.currentFileName = currentFileName;
      this.currentFileNumber = currentFileNumber;
      this.totalFiles = totalFiles;
      this.currentFileBytesProcessed = currentFileBytesProcessed;
      this.currentFileTotalBytes = currentFileTotalBytes;
      this.currentArchiveName = currentArchiveName;
      this.currentArchiveNumber = checked ((short) currentArchiveNumber);
      this.totalArchives = checked ((short) totalArchives);
      this.currentArchiveBytesProcessed = currentArchiveBytesProcessed;
      this.currentArchiveTotalBytes = currentArchiveTotalBytes;
      this.fileBytesProcessed = fileBytesProcessed;
      this.totalFileBytes = totalFileBytes;
    }

    public ArchiveProgressType ProgressType
    {
      get
      {
        return this.progressType;
      }
    }

    public string CurrentFileName
    {
      get
      {
        return this.currentFileName;
      }
    }

    public int CurrentFileNumber
    {
      get
      {
        return this.currentFileNumber;
      }
    }

    public int TotalFiles
    {
      get
      {
        return this.totalFiles;
      }
    }

    public long CurrentFileBytesProcessed
    {
      get
      {
        return this.currentFileBytesProcessed;
      }
    }

    public long CurrentFileTotalBytes
    {
      get
      {
        return this.currentFileTotalBytes;
      }
    }

    public string CurrentArchiveName
    {
      get
      {
        return this.currentArchiveName;
      }
    }

    public int CurrentArchiveNumber
    {
      get
      {
        return (int) this.currentArchiveNumber;
      }
    }

    public int TotalArchives
    {
      get
      {
        return (int) this.totalArchives;
      }
    }

    public long CurrentArchiveBytesProcessed
    {
      get
      {
        return this.currentArchiveBytesProcessed;
      }
    }

    public long CurrentArchiveTotalBytes
    {
      get
      {
        return this.currentArchiveTotalBytes;
      }
    }

    public long FileBytesProcessed
    {
      get
      {
        return this.fileBytesProcessed;
      }
    }

    public long TotalFileBytes
    {
      get
      {
        return this.totalFileBytes;
      }
    }
  }
}
