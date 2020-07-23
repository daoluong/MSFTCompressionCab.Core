﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.ArchiveInfo
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Microsoft.Deployment.Compression
{
  [Serializable]
  public abstract class ArchiveInfo : FileSystemInfo
  {
    protected ArchiveInfo(string path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      this.OriginalPath = path;
      this.FullPath = Path.GetFullPath(path);
    }

    protected ArchiveInfo(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public DirectoryInfo Directory
    {
      get
      {
        return new DirectoryInfo(Path.GetDirectoryName(this.FullName));
      }
    }

    public string DirectoryName
    {
      get
      {
        return Path.GetDirectoryName(this.FullName);
      }
    }

    public long Length
    {
      get
      {
        return new FileInfo(this.FullName).Length;
      }
    }

    public override string Name
    {
      get
      {
        return Path.GetFileName(this.FullName);
      }
    }

    public override bool Exists
    {
      get
      {
        return File.Exists(this.FullName);
      }
    }

    public override string ToString()
    {
      return this.FullName;
    }

    public override void Delete()
    {
      File.Delete(this.FullName);
    }

    public void CopyTo(string destFileName)
    {
      File.Copy(this.FullName, destFileName);
    }

    public void CopyTo(string destFileName, bool overwrite)
    {
      File.Copy(this.FullName, destFileName, overwrite);
    }

    public void MoveTo(string destFileName)
    {
      File.Move(this.FullName, destFileName);
      this.FullPath = Path.GetFullPath(destFileName);
    }

    public bool IsValid()
    {
      using (Stream stream = (Stream) File.OpenRead(this.FullName))
      {
        using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
          return compressionEngine.FindArchiveOffset(stream) >= 0L;
      }
    }

    public IList<ArchiveFileInfo> GetFiles()
    {
      return this.InternalGetFiles((Predicate<string>) null);
    }

    public IList<ArchiveFileInfo> GetFiles(string searchPattern)
    {
      if (searchPattern == null)
        throw new ArgumentNullException(nameof (searchPattern));
      Regex regex = new Regex(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "^{0}$", (object) Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", ".")), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
      return this.InternalGetFiles((Predicate<string>) (match => regex.IsMatch(match)));
    }

    public void Unpack(string destDirectory)
    {
      this.Unpack(destDirectory, (EventHandler<ArchiveProgressEventArgs>) null);
    }

    public void Unpack(
      string destDirectory,
      EventHandler<ArchiveProgressEventArgs> progressHandler)
    {
      using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
      {
        compressionEngine.Progress += progressHandler;
        compressionEngine.Unpack((IUnpackStreamContext) new ArchiveFileStreamContext(this.FullName, destDirectory, (IDictionary<string, string>) null)
        {
          EnableOffsetOpen = true
        }, (Predicate<string>) null);
      }
    }

    public void UnpackFile(string fileName, string destFileName)
    {
      if (fileName == null)
        throw new ArgumentNullException(nameof (fileName));
      if (destFileName == null)
        throw new ArgumentNullException(nameof (destFileName));
      this.UnpackFiles((IList<string>) new string[1]
      {
        fileName
      }, (string) null, (IList<string>) new string[1]
      {
        destFileName
      });
    }

    public void UnpackFiles(
      IList<string> fileNames,
      string destDirectory,
      IList<string> destFileNames)
    {
      this.UnpackFiles(fileNames, destDirectory, destFileNames, (EventHandler<ArchiveProgressEventArgs>) null);
    }

    public void UnpackFiles(
      IList<string> fileNames,
      string destDirectory,
      IList<string> destFileNames,
      EventHandler<ArchiveProgressEventArgs> progressHandler)
    {
      if (fileNames == null)
        throw new ArgumentNullException(nameof (fileNames));
      if (destFileNames == null)
      {
        if (destDirectory == null)
          throw new ArgumentNullException(nameof (destFileNames));
        destFileNames = fileNames;
      }
      if (destFileNames.Count != fileNames.Count)
        throw new ArgumentOutOfRangeException(nameof (destFileNames));
      this.UnpackFileSet(ArchiveInfo.CreateStringDictionary(fileNames, destFileNames), destDirectory, progressHandler);
    }

    public void UnpackFileSet(IDictionary<string, string> fileNames, string destDirectory)
    {
      this.UnpackFileSet(fileNames, destDirectory, (EventHandler<ArchiveProgressEventArgs>) null);
    }

    public void UnpackFileSet(
      IDictionary<string, string> fileNames,
      string destDirectory,
      EventHandler<ArchiveProgressEventArgs> progressHandler)
    {
      if (fileNames == null)
        throw new ArgumentNullException(nameof (fileNames));
      using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
      {
        compressionEngine.Progress += progressHandler;
        compressionEngine.Unpack((IUnpackStreamContext) new ArchiveFileStreamContext(this.FullName, destDirectory, fileNames)
        {
          EnableOffsetOpen = true
        }, (Predicate<string>) (match => fileNames.ContainsKey(match)));
      }
    }

    public Stream OpenRead(string fileName)
    {
      Stream stream = (Stream) File.OpenRead(this.FullName);
      CompressionEngine compressionEngine = this.CreateCompressionEngine();
      return (Stream) new CargoStream(compressionEngine.Unpack(stream, fileName), new IDisposable[2]
      {
        (IDisposable) stream,
        (IDisposable) compressionEngine
      });
    }

    public StreamReader OpenText(string fileName)
    {
      return new StreamReader(this.OpenRead(fileName));
    }

    public void Pack(string sourceDirectory)
    {
      this.Pack(sourceDirectory, false, CompressionLevel.Max, (EventHandler<ArchiveProgressEventArgs>) null);
    }

    public void Pack(
      string sourceDirectory,
      bool includeSubdirectories,
      CompressionLevel compLevel,
      EventHandler<ArchiveProgressEventArgs> progressHandler)
    {
      IList<string> pathsInDirectoryTree = this.GetRelativeFilePathsInDirectoryTree(sourceDirectory, includeSubdirectories);
      string sourceDirectory1 = sourceDirectory;
      IList<string> stringList = pathsInDirectoryTree;
      int num = (int) compLevel;
      EventHandler<ArchiveProgressEventArgs> progressHandler1 = progressHandler;
      this.PackFiles(sourceDirectory1, stringList, stringList, (CompressionLevel) num, progressHandler1);
    }

    public void PackFiles(
      string sourceDirectory,
      IList<string> sourceFileNames,
      IList<string> fileNames)
    {
      this.PackFiles(sourceDirectory, sourceFileNames, fileNames, CompressionLevel.Max, (EventHandler<ArchiveProgressEventArgs>) null);
    }

    public void PackFiles(
      string sourceDirectory,
      IList<string> sourceFileNames,
      IList<string> fileNames,
      CompressionLevel compLevel,
      EventHandler<ArchiveProgressEventArgs> progressHandler)
    {
      if (sourceFileNames == null)
        throw new ArgumentNullException(nameof (sourceFileNames));
      if (fileNames == null)
      {
        string[] strArray = new string[sourceFileNames.Count];
        int index = 0;
        while (index < sourceFileNames.Count)
        {
          strArray[index] = Path.GetFileName(sourceFileNames[index]);
          checked { ++index; }
        }
        fileNames = (IList<string>) strArray;
      }
      else if (fileNames.Count != sourceFileNames.Count)
        throw new ArgumentOutOfRangeException(nameof (fileNames));
      using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
      {
        compressionEngine.Progress += progressHandler;
        IDictionary<string, string> stringDictionary = ArchiveInfo.CreateStringDictionary(fileNames, sourceFileNames);
        ArchiveFileStreamContext fileStreamContext = new ArchiveFileStreamContext(this.FullName, sourceDirectory, stringDictionary);
        fileStreamContext.EnableOffsetOpen = true;
        compressionEngine.CompressionLevel = compLevel;
        compressionEngine.Pack((IPackStreamContext) fileStreamContext, (IEnumerable<string>) fileNames);
      }
    }

    public void PackFileSet(string sourceDirectory, IDictionary<string, string> fileNames)
    {
      this.PackFileSet(sourceDirectory, fileNames, CompressionLevel.Max, (EventHandler<ArchiveProgressEventArgs>) null);
    }

    public void PackFileSet(
      string sourceDirectory,
      IDictionary<string, string> fileNames,
      CompressionLevel compLevel,
      EventHandler<ArchiveProgressEventArgs> progressHandler)
    {
      if (fileNames == null)
        throw new ArgumentNullException(nameof (fileNames));
      string[] array = new string[fileNames.Count];
      fileNames.Keys.CopyTo(array, 0);
      using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
      {
        compressionEngine.Progress += progressHandler;
        ArchiveFileStreamContext fileStreamContext = new ArchiveFileStreamContext(this.FullName, sourceDirectory, fileNames);
        fileStreamContext.EnableOffsetOpen = true;
        compressionEngine.CompressionLevel = compLevel;
        compressionEngine.Pack((IPackStreamContext) fileStreamContext, (IEnumerable<string>) array);
      }
    }

    internal IList<string> GetRelativeFilePathsInDirectoryTree(
      string dir,
      bool includeSubdirectories)
    {
      IList<string> fileList = (IList<string>) new List<string>();
      this.RecursiveGetRelativeFilePathsInDirectoryTree(dir, string.Empty, includeSubdirectories, fileList);
      return fileList;
    }

    internal ArchiveFileInfo GetFile(string path)
    {
      IList<ArchiveFileInfo> files = this.InternalGetFiles((Predicate<string>) (match => string.Compare(match, path, true, CultureInfo.InvariantCulture) == 0));
      if (files == null || files.Count <= 0)
        return (ArchiveFileInfo) null;
      return files[0];
    }

    protected abstract CompressionEngine CreateCompressionEngine();

    private static IDictionary<string, string> CreateStringDictionary(
      IList<string> keys,
      IList<string> values)
    {
      IDictionary<string, string> dictionary = (IDictionary<string, string>) new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      int index = 0;
      while (index < keys.Count)
      {
        dictionary.Add(keys[index], values[index]);
        checked { ++index; }
      }
      return dictionary;
    }

    private void RecursiveGetRelativeFilePathsInDirectoryTree(
      string dir,
      string relativeDir,
      bool includeSubdirectories,
      IList<string> fileList)
    {
      foreach (string file in System.IO.Directory.GetFiles(dir))
      {
        string fileName = Path.GetFileName(file);
        fileList.Add(Path.Combine(relativeDir, fileName));
      }
      if (!includeSubdirectories)
        return;
      foreach (string directory in System.IO.Directory.GetDirectories(dir))
      {
        string fileName = Path.GetFileName(directory);
        this.RecursiveGetRelativeFilePathsInDirectoryTree(Path.Combine(dir, fileName), Path.Combine(relativeDir, fileName), includeSubdirectories, fileList);
      }
    }

    private IList<ArchiveFileInfo> InternalGetFiles(Predicate<string> fileFilter)
    {
      using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
      {
        IList<ArchiveFileInfo> fileInfo = compressionEngine.GetFileInfo((IUnpackStreamContext) new ArchiveFileStreamContext(this.FullName, (string) null, (IDictionary<string, string>) null)
        {
          EnableOffsetOpen = true
        }, fileFilter);
        int index = 0;
        while (index < fileInfo.Count)
        {
          fileInfo[index].Archive = this;
          checked { ++index; }
        }
        return fileInfo;
      }
    }
  }
}