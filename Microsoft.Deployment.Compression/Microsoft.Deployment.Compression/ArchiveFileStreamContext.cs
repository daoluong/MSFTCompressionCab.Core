using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Deployment.Compression
{
	public class ArchiveFileStreamContext : IPackStreamContext, IUnpackStreamContext
	{
		private IList<string> archiveFiles;

		private string directory;

		private IDictionary<string, string> files;

		private bool extractOnlyNewerFiles;

		private bool enableOffsetOpen;

		public IList<string> ArchiveFiles => archiveFiles;

		public string Directory => directory;

		public IDictionary<string, string> Files => files;

		public bool ExtractOnlyNewerFiles
		{
			get
			{
				return extractOnlyNewerFiles;
			}
			set
			{
				extractOnlyNewerFiles = value;
			}
		}

		public bool EnableOffsetOpen
		{
			get
			{
				return enableOffsetOpen;
			}
			set
			{
				enableOffsetOpen = value;
			}
		}

		public ArchiveFileStreamContext(string archiveFile)
			: this(archiveFile, null, null)
		{
		}

		public ArchiveFileStreamContext(string archiveFile, string directory, IDictionary<string, string> files)
			: this(new string[1]
			{
				archiveFile
			}, directory, files)
		{
			if (archiveFile == null)
			{
				throw new ArgumentNullException("archiveFile");
			}
		}

		public ArchiveFileStreamContext(IList<string> archiveFiles, string directory, IDictionary<string, string> files)
		{
			if (archiveFiles == null || archiveFiles.Count == 0)
			{
				throw new ArgumentNullException("archiveFiles");
			}
			this.archiveFiles = archiveFiles;
			this.directory = directory;
			this.files = files;
		}

		public virtual string GetArchiveName(int archiveNumber)
		{
			if (archiveNumber < archiveFiles.Count)
			{
				return Path.GetFileName(archiveFiles[archiveNumber]);
			}
			return string.Empty;
		}

		public virtual Stream OpenArchiveWriteStream(int archiveNumber, string archiveName, bool truncate, CompressionEngine compressionEngine)
		{
			if (archiveNumber >= archiveFiles.Count)
			{
				return null;
			}
			if (string.IsNullOrEmpty(archiveName))
			{
				throw new ArgumentNullException("archiveName");
			}
			Stream stream = File.Open(Path.Combine(Path.GetDirectoryName(archiveFiles[0]), archiveName), truncate ? FileMode.OpenOrCreate : FileMode.Open, FileAccess.ReadWrite);
			if (enableOffsetOpen)
			{
				long num = compressionEngine.FindArchiveOffset(new DuplicateStream(stream));
				if (num < 0)
				{
					num = stream.Length;
				}
				if (num > 0)
				{
					stream = new OffsetStream(stream, num);
				}
				stream.Seek(0L, SeekOrigin.Begin);
			}
			if (truncate)
			{
				stream.SetLength(0L);
			}
			return stream;
		}

		public virtual void CloseArchiveWriteStream(int archiveNumber, string archiveName, Stream stream)
		{
			if (stream == null)
			{
				return;
			}
			stream.Close();
			FileStream fileStream = stream as FileStream;
			if (fileStream == null)
			{
				return;
			}
			string name = fileStream.Name;
			if (!string.IsNullOrEmpty(archiveName) && archiveName != Path.GetFileName(name))
			{
				string text = Path.Combine(Path.GetDirectoryName(archiveFiles[0]), archiveName);
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				File.Move(name, text);
			}
		}

		public virtual Stream OpenFileReadStream(string path, out FileAttributes attributes, out DateTime lastWriteTime)
		{
			string text = TranslateFilePath(path);
			if (text == null)
			{
				attributes = FileAttributes.Normal;
				lastWriteTime = DateTime.Now;
				return null;
			}
			attributes = File.GetAttributes(text);
			lastWriteTime = File.GetLastWriteTime(text);
			return File.Open(text, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public virtual void CloseFileReadStream(string path, Stream stream)
		{
			stream?.Close();
		}

		public virtual object GetOption(string optionName, object[] parameters)
		{
			return null;
		}

		public virtual Stream OpenArchiveReadStream(int archiveNumber, string archiveName, CompressionEngine compressionEngine)
		{
			if (archiveNumber >= archiveFiles.Count)
			{
				return null;
			}
			Stream stream = File.Open(archiveFiles[archiveNumber], FileMode.Open, FileAccess.Read, FileShare.Read);
			if (enableOffsetOpen)
			{
				long num = compressionEngine.FindArchiveOffset(new DuplicateStream(stream));
				if (num > 0)
				{
					stream = new OffsetStream(stream, num);
				}
				else
				{
					stream.Seek(0L, SeekOrigin.Begin);
				}
			}
			return stream;
		}

		public virtual void CloseArchiveReadStream(int archiveNumber, string archiveName, Stream stream)
		{
			stream?.Close();
		}

		public virtual Stream OpenFileWriteStream(string path, long fileSize, DateTime lastWriteTime)
		{
			string text = TranslateFilePath(path);
			if (text == null)
			{
				return null;
			}
			FileInfo fileInfo = new FileInfo(text);
			if (fileInfo.Exists)
			{
				if (extractOnlyNewerFiles && lastWriteTime != DateTime.MinValue && fileInfo.LastWriteTime >= lastWriteTime)
				{
					return null;
				}
				FileAttributes fileAttributes = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System;
				if ((fileInfo.Attributes & fileAttributes) != 0)
				{
					fileInfo.Attributes &= ~fileAttributes;
				}
			}
			if (!fileInfo.Directory.Exists)
			{
				fileInfo.Directory.Create();
			}
			return File.Open(text, FileMode.Create, FileAccess.Write, FileShare.None);
		}

		public virtual void CloseFileWriteStream(string path, Stream stream, FileAttributes attributes, DateTime lastWriteTime)
		{
			stream?.Close();
			string text = TranslateFilePath(path);
			if (text == null)
			{
				return;
			}
			FileInfo fileInfo = new FileInfo(text);
			if (lastWriteTime != DateTime.MinValue)
			{
				try
				{
					fileInfo.LastWriteTime = lastWriteTime;
				}
				catch (ArgumentException)
				{
				}
				catch (IOException)
				{
				}
			}
			try
			{
				fileInfo.Attributes = attributes;
			}
			catch (IOException)
			{
			}
		}

		private string TranslateFilePath(string path)
		{
			string text = ((files == null) ? path : files[path]);
			if (text != null && directory != null)
			{
				text = Path.Combine(directory, text);
			}
			return text;
		}
	}
}
