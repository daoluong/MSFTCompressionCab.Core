using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace Microsoft.Deployment.Compression.Cab
{
	internal class CabUnpacker : CabWorker
	{
		private NativeMethods.FDI.Handle fdiHandle;

		private NativeMethods.FDI.PFNALLOC fdiAllocMemHandler;

		private NativeMethods.FDI.PFNFREE fdiFreeMemHandler;

		private NativeMethods.FDI.PFNOPEN fdiOpenStreamHandler;

		private NativeMethods.FDI.PFNREAD fdiReadStreamHandler;

		private NativeMethods.FDI.PFNWRITE fdiWriteStreamHandler;

		private NativeMethods.FDI.PFNCLOSE fdiCloseStreamHandler;

		private NativeMethods.FDI.PFNSEEK fdiSeekStreamHandler;

		private IUnpackStreamContext context;

		private List<ArchiveFileInfo> fileList;

		private int folderId;

		private Predicate<string> filter;

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		public CabUnpacker(CabEngine cabEngine)
			: base(cabEngine)
		{
			fdiAllocMemHandler = base.CabAllocMem;
			fdiFreeMemHandler = base.CabFreeMem;
			fdiOpenStreamHandler = base.CabOpenStream;
			fdiReadStreamHandler = base.CabReadStream;
			fdiWriteStreamHandler = base.CabWriteStream;
			fdiCloseStreamHandler = base.CabCloseStream;
			fdiSeekStreamHandler = base.CabSeekStream;
			fdiHandle = NativeMethods.FDI.Create(fdiAllocMemHandler, fdiFreeMemHandler, fdiOpenStreamHandler, fdiReadStreamHandler, fdiWriteStreamHandler, fdiCloseStreamHandler, fdiSeekStreamHandler, 1, base.ErfHandle.AddrOfPinnedObject());
			if (base.Erf.Error)
			{
				int oper = base.Erf.Oper;
				int type = base.Erf.Type;
				base.ErfHandle.Free();
				throw new CabException(oper, type, CabException.GetErrorMessage(oper, type, extracting: true));
			}
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		public bool IsArchive(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			lock (this)
			{
				short id;
				int cabFolderCount;
				int fileCount;
				return IsCabinet(stream, out id, out cabFolderCount, out fileCount);
			}
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		public IList<ArchiveFileInfo> GetFileInfo(IUnpackStreamContext streamContext, Predicate<string> fileFilter)
		{
			if (streamContext == null)
			{
				throw new ArgumentNullException("streamContext");
			}
			lock (this)
			{
				context = streamContext;
				filter = fileFilter;
				base.NextCabinetName = string.Empty;
				fileList = new List<ArchiveFileInfo>();
				bool suppressProgressEvents = base.SuppressProgressEvents;
				base.SuppressProgressEvents = true;
				try
				{
					short num = 0;
					while (base.NextCabinetName != null)
					{
						base.Erf.Clear();
						base.CabNumbers[base.NextCabinetName] = num;
						NativeMethods.FDI.Copy(fdiHandle, base.NextCabinetName, string.Empty, 0, CabListNotify, IntPtr.Zero, IntPtr.Zero);
						CheckError(extracting: true);
						num = checked((short)(num + 1));
					}
					List<ArchiveFileInfo> list = fileList;
					fileList = null;
					return list.AsReadOnly();
				}
				finally
				{
					base.SuppressProgressEvents = suppressProgressEvents;
					if (base.CabStream != null)
					{
						context.CloseArchiveReadStream(currentArchiveNumber, currentArchiveName, base.CabStream);
						base.CabStream = null;
					}
					context = null;
				}
			}
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		public void Unpack(IUnpackStreamContext streamContext, Predicate<string> fileFilter)
		{
			checked
			{
				lock (this)
				{
					IList<ArchiveFileInfo> fileInfo = GetFileInfo(streamContext, fileFilter);
					ResetProgressData();
					if (fileInfo != null)
					{
						totalFiles = fileInfo.Count;
						for (int i = 0; i < fileInfo.Count; i++)
						{
							totalFileBytes += fileInfo[i].Length;
							if (fileInfo[i].ArchiveNumber >= totalArchives)
							{
								int num = fileInfo[i].ArchiveNumber + 1;
								totalArchives = (short)num;
							}
						}
					}
					context = streamContext;
					fileList = null;
					base.NextCabinetName = string.Empty;
					folderId = -1;
					currentFileNumber = -1;
					try
					{
						short num2 = 0;
						while (base.NextCabinetName != null)
						{
							base.Erf.Clear();
							base.CabNumbers[base.NextCabinetName] = num2;
							NativeMethods.FDI.Copy(fdiHandle, base.NextCabinetName, string.Empty, 0, CabExtractNotify, IntPtr.Zero, IntPtr.Zero);
							CheckError(extracting: true);
							num2 = (short)(num2 + 1);
						}
					}
					finally
					{
						if (base.CabStream != null)
						{
							context.CloseArchiveReadStream(currentArchiveNumber, currentArchiveName, base.CabStream);
							base.CabStream = null;
						}
						if (base.FileStream != null)
						{
							context.CloseFileWriteStream(currentFileName, base.FileStream, FileAttributes.Normal, DateTime.Now);
							base.FileStream = null;
						}
						context = null;
					}
				}
			}
		}

		internal override int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
		{
			checked
			{
				if (base.CabNumbers.ContainsKey(path))
				{
					Stream cabStream = base.CabStream;
					if (cabStream == null)
					{
						short num = base.CabNumbers[path];
						cabStream = context.OpenArchiveReadStream(num, path, base.CabEngine);
						if (cabStream == null)
						{
							throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", num));
						}
						currentArchiveName = path;
						currentArchiveNumber = num;
						if (totalArchives <= currentArchiveNumber)
						{
							int num2 = currentArchiveNumber + 1;
							totalArchives = (short)num2;
						}
						currentArchiveTotalBytes = cabStream.Length;
						currentArchiveBytesProcessed = 0L;
						if (folderId != -3)
						{
							OnProgress(ArchiveProgressType.StartArchive);
						}
						base.CabStream = cabStream;
					}
					path = "%%CAB%%";
				}
				return base.CabOpenStreamEx(path, openFlags, shareMode, out err, pv);
			}
		}

		internal override int CabReadStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
		{
			int result = base.CabReadStreamEx(streamHandle, memory, cb, out err, pv);
			checked
			{
				if (err == 0 && base.CabStream != null && fileList == null && DuplicateStream.OriginalStream(base.StreamHandles[streamHandle]) == DuplicateStream.OriginalStream(base.CabStream))
				{
					currentArchiveBytesProcessed += cb;
					if (currentArchiveBytesProcessed > currentArchiveTotalBytes)
					{
						currentArchiveBytesProcessed = currentArchiveTotalBytes;
					}
				}
				return result;
			}
		}

		internal override int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
		{
			int num = base.CabWriteStreamEx(streamHandle, memory, cb, out err, pv);
			checked
			{
				if (num > 0 && err == 0)
				{
					currentFileBytesProcessed += cb;
					fileBytesProcessed += cb;
					OnProgress(ArchiveProgressType.PartialFile);
				}
				return num;
			}
		}

		internal override int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
		{
			Stream stream = DuplicateStream.OriginalStream(base.StreamHandles[streamHandle]);
			if (stream == DuplicateStream.OriginalStream(base.CabStream))
			{
				if (folderId != -3)
				{
					OnProgress(ArchiveProgressType.FinishArchive);
				}
				context.CloseArchiveReadStream(currentArchiveNumber, currentArchiveName, stream);
				currentArchiveName = base.NextCabinetName;
				currentArchiveBytesProcessed = (currentArchiveTotalBytes = 0L);
				base.CabStream = null;
			}
			return base.CabCloseStreamEx(streamHandle, out err, pv);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && fdiHandle != null)
				{
					fdiHandle.Dispose();
					fdiHandle = null;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private static string GetFileName(NativeMethods.FDI.NOTIFICATION notification)
		{
			Encoding encoding = ((((uint)notification.attribs & 0x80u) != 0) ? Encoding.UTF8 : Encoding.Default);
			int i;
			for (i = 0; Marshal.ReadByte(notification.psz1, i) != 0; i = checked(i + 1))
			{
			}
			byte[] array = new byte[i];
			Marshal.Copy(notification.psz1, array, 0, i);
			string text = encoding.GetString(array);
			if (Path.IsPathRooted(text))
			{
				string text2 = text;
				char volumeSeparatorChar = Path.VolumeSeparatorChar;
				text = text2.Replace(volumeSeparatorChar.ToString() ?? "", "");
			}
			return text;
		}

		private bool IsCabinet(Stream cabStream, out short id, out int cabFolderCount, out int fileCount)
		{
			int num = base.StreamHandles.AllocHandle(cabStream);
			try
			{
				base.Erf.Clear();
				NativeMethods.FDI.CABINFO pfdici;
				bool result = NativeMethods.FDI.IsCabinet(fdiHandle, num, out pfdici) != 0;
				if (base.Erf.Error)
				{
					if (base.Erf.Oper != 3)
					{
						throw new CabException(base.Erf.Oper, base.Erf.Type, CabException.GetErrorMessage(base.Erf.Oper, base.Erf.Type, extracting: true));
					}
					result = false;
				}
				id = pfdici.setID;
				cabFolderCount = pfdici.cFolders;
				fileCount = pfdici.cFiles;
				return result;
			}
			finally
			{
				base.StreamHandles.FreeHandle(num);
			}
		}

		private int CabListNotify(NativeMethods.FDI.NOTIFICATIONTYPE notificationType, NativeMethods.FDI.NOTIFICATION notification)
		{
			checked
			{
				switch (notificationType)
				{
				case NativeMethods.FDI.NOTIFICATIONTYPE.CABINET_INFO:
				{
					string text = Marshal.PtrToStringAnsi(notification.psz1);
					base.NextCabinetName = ((text.Length != 0) ? text : null);
					return 0;
				}
				case NativeMethods.FDI.NOTIFICATIONTYPE.PARTIAL_FILE:
					return 0;
				case NativeMethods.FDI.NOTIFICATIONTYPE.COPY_FILE:
				{
					string fileName = GetFileName(notification);
					if ((filter == null || filter(fileName)) && fileList != null)
					{
						FileAttributes fileAttributes = unchecked((FileAttributes)(notification.attribs & 0x27));
						if (fileAttributes == (FileAttributes)0)
						{
							fileAttributes = FileAttributes.Normal;
						}
						CompressionEngine.DosDateAndTimeToDateTime(notification.date, notification.time, out var dateTime);
						long length = notification.cb;
						CabFileInfo item = new CabFileInfo(fileName, notification.iFolder, notification.iCabinet, fileAttributes, dateTime, length);
						fileList.Add(item);
						currentFileNumber = fileList.Count - 1;
						fileBytesProcessed += notification.cb;
					}
					totalFiles++;
					totalFileBytes += notification.cb;
					return 0;
				}
				default:
					return 0;
				}
			}
		}

		private int CabExtractNotify(NativeMethods.FDI.NOTIFICATIONTYPE notificationType, NativeMethods.FDI.NOTIFICATION notification)
		{
			switch (notificationType)
			{
			case NativeMethods.FDI.NOTIFICATIONTYPE.CABINET_INFO:
				if (base.NextCabinetName != null && base.NextCabinetName.StartsWith("?", StringComparison.Ordinal))
				{
					base.NextCabinetName = base.NextCabinetName.Substring(1);
				}
				else
				{
					string text = Marshal.PtrToStringAnsi(notification.psz1);
					base.NextCabinetName = ((text.Length != 0) ? text : null);
				}
				return 0;
			case NativeMethods.FDI.NOTIFICATIONTYPE.NEXT_CABINET:
			{
				string key = Marshal.PtrToStringAnsi(notification.psz1);
				base.CabNumbers[key] = notification.iCabinet;
				base.NextCabinetName = "?" + base.NextCabinetName;
				return 0;
			}
			case NativeMethods.FDI.NOTIFICATIONTYPE.COPY_FILE:
				return CabExtractCopyFile(notification);
			case NativeMethods.FDI.NOTIFICATIONTYPE.CLOSE_FILE_INFO:
				return CabExtractCloseFile(notification);
			default:
				return 0;
			}
		}

		private int CabExtractCopyFile(NativeMethods.FDI.NOTIFICATION notification)
		{
			checked
			{
				if (notification.iFolder != folderId)
				{
					if (notification.iFolder != -3 && folderId != -1)
					{
						currentFolderNumber++;
					}
					folderId = notification.iFolder;
				}
				string fileName = GetFileName(notification);
				if (filter == null || filter(fileName))
				{
					currentFileNumber++;
					currentFileName = fileName;
					currentFileBytesProcessed = 0L;
					currentFileTotalBytes = notification.cb;
					OnProgress(ArchiveProgressType.StartFile);
					CompressionEngine.DosDateAndTimeToDateTime(notification.date, notification.time, out var dateTime);
					Stream stream = context.OpenFileWriteStream(fileName, notification.cb, dateTime);
					if (stream != null)
					{
						base.FileStream = stream;
						return base.StreamHandles.AllocHandle(stream);
					}
					fileBytesProcessed += notification.cb;
					OnProgress(ArchiveProgressType.FinishFile);
					currentFileName = null;
				}
				return 0;
			}
		}

		private int CabExtractCloseFile(NativeMethods.FDI.NOTIFICATION notification)
		{
			Stream stream = base.StreamHandles[notification.hf];
			base.StreamHandles.FreeHandle(notification.hf);
			string fileName = GetFileName(notification);
			FileAttributes fileAttributes = (FileAttributes)(notification.attribs & 0x27);
			if (fileAttributes == (FileAttributes)0)
			{
				fileAttributes = FileAttributes.Normal;
			}
			CompressionEngine.DosDateAndTimeToDateTime(notification.date, notification.time, out var dateTime);
			stream.Flush();
			context.CloseFileWriteStream(fileName, stream, fileAttributes, dateTime);
			base.FileStream = null;
			checked
			{
				long num = currentFileTotalBytes - currentFileBytesProcessed;
				currentFileBytesProcessed += num;
				fileBytesProcessed += num;
				OnProgress(ArchiveProgressType.FinishFile);
				currentFileName = null;
				return 1;
			}
		}
	}
}
