// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.CompressionEngine
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.Deployment.Compression
{
  public abstract class CompressionEngine : IDisposable
  {
    private CompressionLevel compressionLevel;
    private bool dontUseTempFiles;

    protected CompressionEngine()
    {
      this.compressionLevel = CompressionLevel.Normal;
    }

    ~CompressionEngine()
    {
      this.Dispose(false);
    }

    public event EventHandler<ArchiveProgressEventArgs> Progress;

    public bool UseTempFiles
    {
      get
      {
        return !this.dontUseTempFiles;
      }
      set
      {
        this.dontUseTempFiles = !value;
      }
    }

    public CompressionLevel CompressionLevel
    {
      get
      {
        return this.compressionLevel;
      }
      set
      {
        this.compressionLevel = value;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Pack(IPackStreamContext streamContext, IEnumerable<string> files)
    {
      if (files == null)
        throw new ArgumentNullException(nameof (files));
      this.Pack(streamContext, files, 0L);
    }

    public abstract void Pack(
      IPackStreamContext streamContext,
      IEnumerable<string> files,
      long maxArchiveSize);

    public abstract bool IsArchive(Stream stream);

    public virtual long FindArchiveOffset(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException(nameof (stream));
      long num = 4;
      long length = stream.Length;
      long offset = 0;
      while (offset <= checked (length - num))
      {
        stream.Seek(offset, SeekOrigin.Begin);
        if (this.IsArchive(stream))
          return offset;
        checked { offset += num; }
      }
      return -1;
    }

    public IList<ArchiveFileInfo> GetFileInfo(Stream stream)
    {
      return this.GetFileInfo((IUnpackStreamContext) new BasicUnpackStreamContext(stream), (Predicate<string>) null);
    }

    public abstract IList<ArchiveFileInfo> GetFileInfo(
      IUnpackStreamContext streamContext,
      Predicate<string> fileFilter);

    public IList<string> GetFiles(Stream stream)
    {
      return this.GetFiles((IUnpackStreamContext) new BasicUnpackStreamContext(stream), (Predicate<string>) null);
    }

    public IList<string> GetFiles(
      IUnpackStreamContext streamContext,
      Predicate<string> fileFilter)
    {
      if (streamContext == null)
        throw new ArgumentNullException(nameof (streamContext));
      IList<ArchiveFileInfo> fileInfo = this.GetFileInfo(streamContext, fileFilter);
      IList<string> stringList = (IList<string>) new List<string>(fileInfo.Count);
      int index = 0;
      while (index < fileInfo.Count)
      {
        stringList.Add(fileInfo[index].Name);
        checked { ++index; }
      }
      return stringList;
    }

    public Stream Unpack(Stream stream, string path)
    {
      if (stream == null)
        throw new ArgumentNullException(nameof (stream));
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      BasicUnpackStreamContext unpackStreamContext = new BasicUnpackStreamContext(stream);
      this.Unpack((IUnpackStreamContext) unpackStreamContext, (Predicate<string>) (match => string.Compare(match, path, true, CultureInfo.InvariantCulture) == 0));
      Stream fileStream = unpackStreamContext.FileStream;
      if (fileStream != null)
        fileStream.Position = 0L;
      return fileStream;
    }

    public abstract void Unpack(IUnpackStreamContext streamContext, Predicate<string> fileFilter);

    protected void OnProgress(ArchiveProgressEventArgs e)
    {
      if (this.Progress == null)
        return;
      this.Progress((object) this, e);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public static void DosDateAndTimeToDateTime(
      short dosDate,
      short dosTime,
      out DateTime dateTime)
    {
      if (dosDate == (short) 0 && dosTime == (short) 0)
      {
        dateTime = DateTime.MinValue;
      }
      else
      {
        long fileTime;
        SafeNativeMethods.DosDateTimeToFileTime(dosDate, dosTime, out fileTime);
        dateTime = DateTime.FromFileTimeUtc(fileTime);
        dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Local);
      }
    }

    public static void DateTimeToDosDateAndTime(
      DateTime dateTime,
      out short dosDate,
      out short dosTime)
    {
      dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
      long fileTimeUtc = dateTime.ToFileTimeUtc();
      SafeNativeMethods.FileTimeToDosDateTime(ref fileTimeUtc, out dosDate, out dosTime);
    }
  }
}
