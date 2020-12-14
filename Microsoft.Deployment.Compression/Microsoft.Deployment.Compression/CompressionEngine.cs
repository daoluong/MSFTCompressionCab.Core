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

		public bool UseTempFiles
		{
			get
			{
				return !dontUseTempFiles;
			}
			set
			{
				dontUseTempFiles = !value;
			}
		}

		public CompressionLevel CompressionLevel
		{
			get
			{
				return compressionLevel;
			}
			set
			{
				compressionLevel = value;
			}
		}

		public event EventHandler<ArchiveProgressEventArgs> Progress;

		protected CompressionEngine()
		{
			compressionLevel = CompressionLevel.Normal;
		}

		~CompressionEngine()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void Pack(IPackStreamContext streamContext, IEnumerable<string> files)
		{
			if (files == null)
			{
				throw new ArgumentNullException("files");
			}
			Pack(streamContext, files, 0L);
		}

		public abstract void Pack(IPackStreamContext streamContext, IEnumerable<string> files, long maxArchiveSize);

		public abstract bool IsArchive(Stream stream);

		public virtual long FindArchiveOffset(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			long num = 4L;
			long length = stream.Length;
			checked
			{
				for (long num2 = 0L; num2 <= length - num; num2 += num)
				{
					stream.Seek(num2, SeekOrigin.Begin);
					if (IsArchive(stream))
					{
						return num2;
					}
				}
				return -1L;
			}
		}

		public IList<ArchiveFileInfo> GetFileInfo(Stream stream)
		{
			return GetFileInfo(new BasicUnpackStreamContext(stream), null);
		}

		public abstract IList<ArchiveFileInfo> GetFileInfo(IUnpackStreamContext streamContext, Predicate<string> fileFilter);

		public IList<string> GetFiles(Stream stream)
		{
			return GetFiles(new BasicUnpackStreamContext(stream), null);
		}

		public IList<string> GetFiles(IUnpackStreamContext streamContext, Predicate<string> fileFilter)
		{
			if (streamContext == null)
			{
				throw new ArgumentNullException("streamContext");
			}
			IList<ArchiveFileInfo> fileInfo = GetFileInfo(streamContext, fileFilter);
			IList<string> list = new List<string>(fileInfo.Count);
			for (int i = 0; i < fileInfo.Count; i = checked(i + 1))
			{
				list.Add(fileInfo[i].Name);
			}
			return list;
		}

		public Stream Unpack(Stream stream, string path)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			BasicUnpackStreamContext basicUnpackStreamContext = new BasicUnpackStreamContext(stream);
			Unpack(basicUnpackStreamContext, (string match) => string.Compare(match, path, ignoreCase: true, CultureInfo.InvariantCulture) == 0);
			Stream fileStream = basicUnpackStreamContext.FileStream;
			if (fileStream != null)
			{
				fileStream.Position = 0L;
			}
			return fileStream;
		}

		public abstract void Unpack(IUnpackStreamContext streamContext, Predicate<string> fileFilter);

		protected void OnProgress(ArchiveProgressEventArgs e)
		{
			if (this.Progress != null)
			{
				this.Progress(this, e);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public static void DosDateAndTimeToDateTime(short dosDate, short dosTime, out DateTime dateTime)
		{
			if (dosDate == 0 && dosTime == 0)
			{
				dateTime = DateTime.MinValue;
				return;
			}
			SafeNativeMethods.DosDateTimeToFileTime(dosDate, dosTime, out var fileTime);
			dateTime = DateTime.FromFileTimeUtc(fileTime);
			dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Local);
		}

		public static void DateTimeToDosDateAndTime(DateTime dateTime, out short dosDate, out short dosTime)
		{
			dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
			long fileTime = dateTime.ToFileTimeUtc();
			SafeNativeMethods.FileTimeToDosDateTime(ref fileTime, out dosDate, out dosTime);
		}
	}
}
