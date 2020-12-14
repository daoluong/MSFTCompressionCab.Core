using System;
using System.IO;

namespace Microsoft.Deployment.Compression
{
	public class BasicUnpackStreamContext : IUnpackStreamContext
	{
		private Stream archiveStream;

		private Stream fileStream;

		public Stream FileStream => fileStream;

		public BasicUnpackStreamContext(Stream archiveStream)
		{
			this.archiveStream = archiveStream;
		}

		public Stream OpenArchiveReadStream(int archiveNumber, string archiveName, CompressionEngine compressionEngine)
		{
			return new DuplicateStream(archiveStream);
		}

		public void CloseArchiveReadStream(int archiveNumber, string archiveName, Stream stream)
		{
		}

		public Stream OpenFileWriteStream(string path, long fileSize, DateTime lastWriteTime)
		{
			fileStream = new MemoryStream(new byte[fileSize], 0, checked((int)fileSize), writable: true, publiclyVisible: true);
			return fileStream;
		}

		public void CloseFileWriteStream(string path, Stream stream, FileAttributes attributes, DateTime lastWriteTime)
		{
		}
	}
}
