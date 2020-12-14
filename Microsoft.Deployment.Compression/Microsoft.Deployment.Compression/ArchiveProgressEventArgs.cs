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

		public ArchiveProgressType ProgressType => progressType;

		public string CurrentFileName => currentFileName;

		public int CurrentFileNumber => currentFileNumber;

		public int TotalFiles => totalFiles;

		public long CurrentFileBytesProcessed => currentFileBytesProcessed;

		public long CurrentFileTotalBytes => currentFileTotalBytes;

		public string CurrentArchiveName => currentArchiveName;

		public int CurrentArchiveNumber => currentArchiveNumber;

		public int TotalArchives => totalArchives;

		public long CurrentArchiveBytesProcessed => currentArchiveBytesProcessed;

		public long CurrentArchiveTotalBytes => currentArchiveTotalBytes;

		public long FileBytesProcessed => fileBytesProcessed;

		public long TotalFileBytes => totalFileBytes;

		public ArchiveProgressEventArgs(ArchiveProgressType progressType, string currentFileName, int currentFileNumber, int totalFiles, long currentFileBytesProcessed, long currentFileTotalBytes, string currentArchiveName, int currentArchiveNumber, int totalArchives, long currentArchiveBytesProcessed, long currentArchiveTotalBytes, long fileBytesProcessed, long totalFileBytes)
		{
			this.progressType = progressType;
			this.currentFileName = currentFileName;
			this.currentFileNumber = currentFileNumber;
			this.totalFiles = totalFiles;
			this.currentFileBytesProcessed = currentFileBytesProcessed;
			this.currentFileTotalBytes = currentFileTotalBytes;
			this.currentArchiveName = currentArchiveName;
			checked
			{
				this.currentArchiveNumber = (short)currentArchiveNumber;
				this.totalArchives = (short)totalArchives;
				this.currentArchiveBytesProcessed = currentArchiveBytesProcessed;
				this.currentArchiveTotalBytes = currentArchiveTotalBytes;
				this.fileBytesProcessed = fileBytesProcessed;
				this.totalFileBytes = totalFileBytes;
			}
		}
	}
}
