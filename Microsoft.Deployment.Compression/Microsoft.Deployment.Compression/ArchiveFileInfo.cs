using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Deployment.Compression
{
	[Serializable]
	public abstract class ArchiveFileInfo : FileSystemInfo
	{
		private ArchiveInfo archiveInfo;

		private string name;

		private string path;

		private bool initialized;

		private bool exists;

		private int archiveNumber;

		private FileAttributes attributes;

		private DateTime lastWriteTime;

		private long length;

		public override string Name => name;

		public string Path => path;

		public override string FullName
		{
			get
			{
				string text = System.IO.Path.Combine(Path, Name);
				if (Archive != null)
				{
					text = System.IO.Path.Combine(ArchiveName, text);
				}
				return text;
			}
		}

		public ArchiveInfo Archive
		{
			get
			{
				return archiveInfo;
			}
			internal set
			{
				archiveInfo = value;
				OriginalPath = value?.FullName;
				FullPath = OriginalPath;
			}
		}

		public string ArchiveName
		{
			get
			{
				if (Archive == null)
				{
					return null;
				}
				return Archive.FullName;
			}
		}

		public int ArchiveNumber => archiveNumber;

		public override bool Exists
		{
			get
			{
				if (!initialized)
				{
					Refresh();
				}
				return exists;
			}
		}

		public long Length
		{
			get
			{
				if (!initialized)
				{
					Refresh();
				}
				return length;
			}
		}

		public new FileAttributes Attributes
		{
			get
			{
				if (!initialized)
				{
					Refresh();
				}
				return attributes;
			}
		}

		public new DateTime LastWriteTime
		{
			get
			{
				if (!initialized)
				{
					Refresh();
				}
				return lastWriteTime;
			}
		}

		protected ArchiveFileInfo(ArchiveInfo archiveInfo, string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			Archive = archiveInfo;
			name = System.IO.Path.GetFileName(filePath);
			path = System.IO.Path.GetDirectoryName(filePath);
			attributes = FileAttributes.Normal;
			lastWriteTime = DateTime.MinValue;
		}

		protected ArchiveFileInfo(string filePath, int archiveNumber, FileAttributes attributes, DateTime lastWriteTime, long length)
			: this(null, filePath)
		{
			exists = true;
			this.archiveNumber = archiveNumber;
			this.attributes = attributes;
			this.lastWriteTime = lastWriteTime;
			this.length = length;
			initialized = true;
		}

		protected ArchiveFileInfo(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			archiveInfo = (ArchiveInfo)info.GetValue("archiveInfo", typeof(ArchiveInfo));
			name = info.GetString("name");
			path = info.GetString("path");
			initialized = info.GetBoolean("initialized");
			exists = info.GetBoolean("exists");
			archiveNumber = info.GetInt32("archiveNumber");
			attributes = (FileAttributes)info.GetValue("attributes", typeof(FileAttributes));
			lastWriteTime = info.GetDateTime("lastWriteTime");
			length = info.GetInt64("length");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("archiveInfo", archiveInfo);
			info.AddValue("name", name);
			info.AddValue("path", path);
			info.AddValue("initialized", initialized);
			info.AddValue("exists", exists);
			info.AddValue("archiveNumber", archiveNumber);
			info.AddValue("attributes", attributes);
			info.AddValue("lastWriteTime", lastWriteTime);
			info.AddValue("length", length);
		}

		public override string ToString()
		{
			return FullName;
		}

		public override void Delete()
		{
			throw new NotSupportedException();
		}

		public new void Refresh()
		{
			base.Refresh();
			if (Archive != null)
			{
				string fileName = System.IO.Path.Combine(Path, Name);
				ArchiveFileInfo file = Archive.GetFile(fileName);
				if (file == null)
				{
					throw new FileNotFoundException("File not found in archive.", fileName);
				}
				Refresh(file);
			}
		}

		public void CopyTo(string destFileName)
		{
			CopyTo(destFileName, overwrite: false);
		}

		public void CopyTo(string destFileName, bool overwrite)
		{
			if (destFileName == null)
			{
				throw new ArgumentNullException("destFileName");
			}
			if (!overwrite && File.Exists(destFileName))
			{
				throw new IOException();
			}
			if (Archive == null)
			{
				throw new InvalidOperationException();
			}
			Archive.UnpackFile(System.IO.Path.Combine(Path, Name), destFileName);
		}

		public Stream OpenRead()
		{
			return Archive.OpenRead(System.IO.Path.Combine(Path, Name));
		}

		public StreamReader OpenText()
		{
			return Archive.OpenText(System.IO.Path.Combine(Path, Name));
		}

		protected virtual void Refresh(ArchiveFileInfo newFileInfo)
		{
			exists = newFileInfo.exists;
			length = newFileInfo.length;
			attributes = newFileInfo.attributes;
			lastWriteTime = newFileInfo.lastWriteTime;
		}
	}
}
