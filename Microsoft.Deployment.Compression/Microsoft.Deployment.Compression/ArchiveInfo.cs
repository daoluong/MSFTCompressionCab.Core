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
		public DirectoryInfo Directory => new DirectoryInfo(Path.GetDirectoryName(FullName));

		public string DirectoryName => Path.GetDirectoryName(FullName);

		public long Length => new FileInfo(FullName).Length;

		public override string Name => Path.GetFileName(FullName);

		public override bool Exists => File.Exists(FullName);

		protected ArchiveInfo(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			OriginalPath = path;
			FullPath = Path.GetFullPath(path);
		}

		protected ArchiveInfo(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override string ToString()
		{
			return FullName;
		}

		public override void Delete()
		{
			File.Delete(FullName);
		}

		public void CopyTo(string destFileName)
		{
			File.Copy(FullName, destFileName);
		}

		public void CopyTo(string destFileName, bool overwrite)
		{
			File.Copy(FullName, destFileName, overwrite);
		}

		public void MoveTo(string destFileName)
		{
			File.Move(FullName, destFileName);
			FullPath = Path.GetFullPath(destFileName);
		}

		public bool IsValid()
		{
			using Stream stream = File.OpenRead(FullName);
			using CompressionEngine compressionEngine = CreateCompressionEngine();
			return compressionEngine.FindArchiveOffset(stream) >= 0;
		}

		public IList<ArchiveFileInfo> GetFiles()
		{
			return InternalGetFiles(null);
		}

		public IList<ArchiveFileInfo> GetFiles(string searchPattern)
		{
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			string pattern = string.Format(CultureInfo.InvariantCulture, "^{0}$", Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", "."));
			Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			return InternalGetFiles((string match) => regex.IsMatch(match));
		}

		public void Unpack(string destDirectory)
		{
			Unpack(destDirectory, null);
		}

		public void Unpack(string destDirectory, EventHandler<ArchiveProgressEventArgs> progressHandler)
		{
			using CompressionEngine compressionEngine = CreateCompressionEngine();
			compressionEngine.Progress += progressHandler;
			ArchiveFileStreamContext archiveFileStreamContext = new ArchiveFileStreamContext(FullName, destDirectory, null);
			archiveFileStreamContext.EnableOffsetOpen = true;
			compressionEngine.Unpack(archiveFileStreamContext, null);
		}

		public void UnpackFile(string fileName, string destFileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (destFileName == null)
			{
				throw new ArgumentNullException("destFileName");
			}
			UnpackFiles(new string[1]
			{
				fileName
			}, null, new string[1]
			{
				destFileName
			});
		}

		public void UnpackFiles(IList<string> fileNames, string destDirectory, IList<string> destFileNames)
		{
			UnpackFiles(fileNames, destDirectory, destFileNames, null);
		}

		public void UnpackFiles(IList<string> fileNames, string destDirectory, IList<string> destFileNames, EventHandler<ArchiveProgressEventArgs> progressHandler)
		{
			if (fileNames == null)
			{
				throw new ArgumentNullException("fileNames");
			}
			if (destFileNames == null)
			{
				if (destDirectory == null)
				{
					throw new ArgumentNullException("destFileNames");
				}
				destFileNames = fileNames;
			}
			if (destFileNames.Count != fileNames.Count)
			{
				throw new ArgumentOutOfRangeException("destFileNames");
			}
			IDictionary<string, string> fileNames2 = CreateStringDictionary(fileNames, destFileNames);
			UnpackFileSet(fileNames2, destDirectory, progressHandler);
		}

		public void UnpackFileSet(IDictionary<string, string> fileNames, string destDirectory)
		{
			UnpackFileSet(fileNames, destDirectory, null);
		}

		public void UnpackFileSet(IDictionary<string, string> fileNames, string destDirectory, EventHandler<ArchiveProgressEventArgs> progressHandler)
		{
			if (fileNames == null)
			{
				throw new ArgumentNullException("fileNames");
			}
			using CompressionEngine compressionEngine = CreateCompressionEngine();
			compressionEngine.Progress += progressHandler;
			ArchiveFileStreamContext archiveFileStreamContext = new ArchiveFileStreamContext(FullName, destDirectory, fileNames);
			archiveFileStreamContext.EnableOffsetOpen = true;
			compressionEngine.Unpack(archiveFileStreamContext, (string match) => fileNames.ContainsKey(match));
		}

		public Stream OpenRead(string fileName)
		{
			Stream stream = File.OpenRead(FullName);
			CompressionEngine compressionEngine = CreateCompressionEngine();
			return new CargoStream(compressionEngine.Unpack(stream, fileName), stream, compressionEngine);
		}

		public StreamReader OpenText(string fileName)
		{
			return new StreamReader(OpenRead(fileName));
		}

		public void Pack(string sourceDirectory)
		{
			Pack(sourceDirectory, includeSubdirectories: false, CompressionLevel.Max, null);
		}

		public void Pack(string sourceDirectory, bool includeSubdirectories, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler)
		{
			IList<string> relativeFilePathsInDirectoryTree = GetRelativeFilePathsInDirectoryTree(sourceDirectory, includeSubdirectories);
			PackFiles(sourceDirectory, relativeFilePathsInDirectoryTree, relativeFilePathsInDirectoryTree, compLevel, progressHandler);
		}

		public void PackFiles(string sourceDirectory, IList<string> sourceFileNames, IList<string> fileNames)
		{
			PackFiles(sourceDirectory, sourceFileNames, fileNames, CompressionLevel.Max, null);
		}

		public void PackFiles(string sourceDirectory, IList<string> sourceFileNames, IList<string> fileNames, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler)
		{
			if (sourceFileNames == null)
			{
				throw new ArgumentNullException("sourceFileNames");
			}
			if (fileNames == null)
			{
				string[] array = new string[sourceFileNames.Count];
				for (int i = 0; i < sourceFileNames.Count; i = checked(i + 1))
				{
					array[i] = Path.GetFileName(sourceFileNames[i]);
				}
				fileNames = array;
			}
			else if (fileNames.Count != sourceFileNames.Count)
			{
				throw new ArgumentOutOfRangeException("fileNames");
			}
			using CompressionEngine compressionEngine = CreateCompressionEngine();
			compressionEngine.Progress += progressHandler;
			IDictionary<string, string> files = CreateStringDictionary(fileNames, sourceFileNames);
			ArchiveFileStreamContext archiveFileStreamContext = new ArchiveFileStreamContext(FullName, sourceDirectory, files);
			archiveFileStreamContext.EnableOffsetOpen = true;
			compressionEngine.CompressionLevel = compLevel;
			compressionEngine.Pack(archiveFileStreamContext, fileNames);
		}

		public void PackFileSet(string sourceDirectory, IDictionary<string, string> fileNames)
		{
			PackFileSet(sourceDirectory, fileNames, CompressionLevel.Max, null);
		}

		public void PackFileSet(string sourceDirectory, IDictionary<string, string> fileNames, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler)
		{
			if (fileNames == null)
			{
				throw new ArgumentNullException("fileNames");
			}
			string[] array = new string[fileNames.Count];
			fileNames.Keys.CopyTo(array, 0);
			using CompressionEngine compressionEngine = CreateCompressionEngine();
			compressionEngine.Progress += progressHandler;
			ArchiveFileStreamContext archiveFileStreamContext = new ArchiveFileStreamContext(FullName, sourceDirectory, fileNames);
			archiveFileStreamContext.EnableOffsetOpen = true;
			compressionEngine.CompressionLevel = compLevel;
			compressionEngine.Pack(archiveFileStreamContext, array);
		}

		internal IList<string> GetRelativeFilePathsInDirectoryTree(string dir, bool includeSubdirectories)
		{
			IList<string> list = new List<string>();
			RecursiveGetRelativeFilePathsInDirectoryTree(dir, string.Empty, includeSubdirectories, list);
			return list;
		}

		internal ArchiveFileInfo GetFile(string path)
		{
			IList<ArchiveFileInfo> list = InternalGetFiles((string match) => string.Compare(match, path, ignoreCase: true, CultureInfo.InvariantCulture) == 0);
			if (list == null || list.Count <= 0)
			{
				return null;
			}
			return list[0];
		}

		protected abstract CompressionEngine CreateCompressionEngine();

		private static IDictionary<string, string> CreateStringDictionary(IList<string> keys, IList<string> values)
		{
			IDictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < keys.Count; i = checked(i + 1))
			{
				dictionary.Add(keys[i], values[i]);
			}
			return dictionary;
		}

		private void RecursiveGetRelativeFilePathsInDirectoryTree(string dir, string relativeDir, bool includeSubdirectories, IList<string> fileList)
		{
			string[] files = System.IO.Directory.GetFiles(dir);
			for (int i = 0; i < files.Length; i++)
			{
				string fileName = Path.GetFileName(files[i]);
				fileList.Add(Path.Combine(relativeDir, fileName));
			}
			if (includeSubdirectories)
			{
				files = System.IO.Directory.GetDirectories(dir);
				for (int i = 0; i < files.Length; i++)
				{
					string fileName2 = Path.GetFileName(files[i]);
					RecursiveGetRelativeFilePathsInDirectoryTree(Path.Combine(dir, fileName2), Path.Combine(relativeDir, fileName2), includeSubdirectories, fileList);
				}
			}
		}

		private IList<ArchiveFileInfo> InternalGetFiles(Predicate<string> fileFilter)
		{
			using CompressionEngine compressionEngine = CreateCompressionEngine();
			ArchiveFileStreamContext archiveFileStreamContext = new ArchiveFileStreamContext(FullName, null, null);
			archiveFileStreamContext.EnableOffsetOpen = true;
			IList<ArchiveFileInfo> fileInfo = compressionEngine.GetFileInfo(archiveFileStreamContext, fileFilter);
			for (int i = 0; i < fileInfo.Count; i = checked(i + 1))
			{
				fileInfo[i].Archive = this;
			}
			return fileInfo;
		}
	}
}
