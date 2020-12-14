using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace Microsoft.Deployment.Compression.Cab
{
	internal class CabPacker : CabWorker
	{
		private const string TempStreamName = "%%TEMP%%";

		private NativeMethods.FCI.Handle fciHandle;

		private NativeMethods.FCI.PFNALLOC fciAllocMemHandler;

		private NativeMethods.FCI.PFNFREE fciFreeMemHandler;

		private NativeMethods.FCI.PFNOPEN fciOpenStreamHandler;

		private NativeMethods.FCI.PFNREAD fciReadStreamHandler;

		private NativeMethods.FCI.PFNWRITE fciWriteStreamHandler;

		private NativeMethods.FCI.PFNCLOSE fciCloseStreamHandler;

		private NativeMethods.FCI.PFNSEEK fciSeekStreamHandler;

		private NativeMethods.FCI.PFNFILEPLACED fciFilePlacedHandler;

		private NativeMethods.FCI.PFNDELETE fciDeleteFileHandler;

		private NativeMethods.FCI.PFNGETTEMPFILE fciGetTempFileHandler;

		private NativeMethods.FCI.PFNGETNEXTCABINET fciGetNextCabinet;

		private NativeMethods.FCI.PFNSTATUS fciCreateStatus;

		private NativeMethods.FCI.PFNGETOPENINFO fciGetOpenInfo;

		private IPackStreamContext context;

		private FileAttributes fileAttributes;

		private DateTime fileLastWriteTime;

		private int maxCabBytes;

		private long totalFolderBytesProcessedInCurrentCab;

		private CompressionLevel compressionLevel;

		private bool dontUseTempFiles;

		private IList<Stream> tempStreams;

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

		public CabPacker(CabEngine cabEngine)
			: base(cabEngine)
		{
			fciAllocMemHandler = base.CabAllocMem;
			fciFreeMemHandler = base.CabFreeMem;
			fciOpenStreamHandler = CabOpenStreamEx;
			fciReadStreamHandler = CabReadStreamEx;
			fciWriteStreamHandler = CabWriteStreamEx;
			fciCloseStreamHandler = CabCloseStreamEx;
			fciSeekStreamHandler = CabSeekStreamEx;
			fciFilePlacedHandler = CabFilePlaced;
			fciDeleteFileHandler = CabDeleteFile;
			fciGetTempFileHandler = CabGetTempFile;
			fciGetNextCabinet = CabGetNextCabinet;
			fciCreateStatus = CabCreateStatus;
			fciGetOpenInfo = CabGetOpenInfo;
			tempStreams = new List<Stream>();
			compressionLevel = CompressionLevel.Normal;
		}

		private void CreateFci(long maxArchiveSize)
		{
			NativeMethods.FCI.CCAB cCAB = new NativeMethods.FCI.CCAB();
			checked
			{
				if (maxArchiveSize > 0 && maxArchiveSize < cCAB.cb)
				{
					cCAB.cb = Math.Max(32768, (int)maxArchiveSize);
				}
				object option = context.GetOption("maxFolderSize", null);
				if (option != null)
				{
					long num = Convert.ToInt64(option, CultureInfo.InvariantCulture);
					if (num > 0 && num < cCAB.cbFolderThresh)
					{
						cCAB.cbFolderThresh = (int)num;
					}
				}
				maxCabBytes = cCAB.cb;
				cCAB.szCab = context.GetArchiveName(0);
				if (cCAB.szCab == null)
				{
					throw new FileNotFoundException("Cabinet name not provided by stream context.");
				}
				cCAB.setID = (short)new Random().Next(-32768, 32768);
				base.CabNumbers[cCAB.szCab] = 0;
				currentArchiveName = cCAB.szCab;
				totalArchives = 1;
				base.CabStream = null;
				base.Erf.Clear();
				fciHandle = NativeMethods.FCI.Create(base.ErfHandle.AddrOfPinnedObject(), fciFilePlacedHandler, fciAllocMemHandler, fciFreeMemHandler, fciOpenStreamHandler, fciReadStreamHandler, fciWriteStreamHandler, fciCloseStreamHandler, fciSeekStreamHandler, fciDeleteFileHandler, fciGetTempFileHandler, cCAB, IntPtr.Zero);
				CheckError(extracting: false);
			}
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		public void Pack(IPackStreamContext streamContext, IEnumerable<string> files, long maxArchiveSize)
		{
			if (streamContext == null)
			{
				throw new ArgumentNullException("streamContext");
			}
			if (files == null)
			{
				throw new ArgumentNullException("files");
			}
			checked
			{
				lock (this)
				{
					try
					{
						context = streamContext;
						ResetProgressData();
						CreateFci(maxArchiveSize);
						foreach (string file in files)
						{
							FileAttributes attributes;
							DateTime lastWriteTime;
							Stream stream = context.OpenFileReadStream(file, out attributes, out lastWriteTime);
							if (stream != null)
							{
								totalFileBytes += stream.Length;
								totalFiles++;
								context.CloseFileReadStream(file, stream);
							}
						}
						long num = 0L;
						currentFileNumber = -1;
						foreach (string file2 in files)
						{
							FileAttributes attributes2;
							DateTime lastWriteTime2;
							Stream stream2 = context.OpenFileReadStream(file2, out attributes2, out lastWriteTime2);
							if (stream2 == null)
							{
								continue;
							}
							if (stream2.Length >= 2147450880)
							{
								throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "File {0} exceeds maximum file size for cabinet format.", file2));
							}
							if (num > 0)
							{
								bool flag = num + stream2.Length >= 2147450880;
								if (!flag)
								{
									flag = Convert.ToBoolean(streamContext.GetOption("nextFolder", new object[2]
									{
										file2,
										currentFolderNumber
									}), CultureInfo.InvariantCulture);
								}
								if (flag)
								{
									FlushFolder();
									num = 0L;
								}
							}
							if (currentFolderTotalBytes > 0)
							{
								currentFolderTotalBytes = 0L;
								currentFolderNumber++;
								num = 0L;
							}
							currentFileName = file2;
							currentFileNumber++;
							currentFileTotalBytes = stream2.Length;
							currentFileBytesProcessed = 0L;
							OnProgress(ArchiveProgressType.StartFile);
							num += stream2.Length;
							AddFile(file2, stream2, attributes2, lastWriteTime2, execute: false, CompressionLevel);
						}
						FlushFolder();
						FlushCabinet();
					}
					finally
					{
						if (base.CabStream != null)
						{
							context.CloseArchiveWriteStream(currentArchiveNumber, currentArchiveName, base.CabStream);
							base.CabStream = null;
						}
						if (base.FileStream != null)
						{
							context.CloseFileReadStream(currentFileName, base.FileStream);
							base.FileStream = null;
						}
						context = null;
						if (fciHandle != null)
						{
							fciHandle.Dispose();
							fciHandle = null;
						}
					}
				}
			}
		}

		internal override int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
		{
			if (base.CabNumbers.ContainsKey(path))
			{
				Stream cabStream = base.CabStream;
				if (cabStream == null)
				{
					short num = base.CabNumbers[path];
					currentFolderTotalBytes = 0L;
					cabStream = context.OpenArchiveWriteStream(num, path, truncate: true, base.CabEngine);
					if (cabStream == null)
					{
						throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", num));
					}
					currentArchiveName = path;
					currentArchiveTotalBytes = Math.Min(totalFolderBytesProcessedInCurrentCab, maxCabBytes);
					currentArchiveBytesProcessed = 0L;
					OnProgress(ArchiveProgressType.StartArchive);
					base.CabStream = cabStream;
				}
				path = "%%CAB%%";
			}
			else
			{
				if (path == "%%TEMP%%")
				{
					Stream stream = new MemoryStream();
					tempStreams.Add(stream);
					int result = base.StreamHandles.AllocHandle(stream);
					err = 0;
					return result;
				}
				if (path != "%%CAB%%")
				{
					path = Path.Combine(Path.GetTempPath(), path);
					Stream stream2 = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
					tempStreams.Add(stream2);
					stream2 = new DuplicateStream(stream2);
					int result2 = base.StreamHandles.AllocHandle(stream2);
					err = 0;
					return result2;
				}
			}
			return base.CabOpenStreamEx(path, openFlags, shareMode, out err, pv);
		}

		internal override int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
		{
			int num = base.CabWriteStreamEx(streamHandle, memory, cb, out err, pv);
			checked
			{
				if (num > 0 && err == 0 && DuplicateStream.OriginalStream(base.StreamHandles[streamHandle]) == DuplicateStream.OriginalStream(base.CabStream))
				{
					currentArchiveBytesProcessed += cb;
					if (currentArchiveBytesProcessed > currentArchiveTotalBytes)
					{
						currentArchiveBytesProcessed = currentArchiveTotalBytes;
					}
				}
				return num;
			}
		}

		internal override int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
		{
			Stream stream = DuplicateStream.OriginalStream(base.StreamHandles[streamHandle]);
			checked
			{
				if (stream == DuplicateStream.OriginalStream(base.FileStream))
				{
					context.CloseFileReadStream(currentFileName, stream);
					base.FileStream = null;
					long num = currentFileTotalBytes - currentFileBytesProcessed;
					currentFileBytesProcessed += num;
					fileBytesProcessed += num;
					OnProgress(ArchiveProgressType.FinishFile);
					currentFileTotalBytes = 0L;
					currentFileBytesProcessed = 0L;
					currentFileName = null;
				}
				else if (stream == DuplicateStream.OriginalStream(base.CabStream))
				{
					if (stream.CanWrite)
					{
						stream.Flush();
					}
					currentArchiveBytesProcessed = currentArchiveTotalBytes;
					OnProgress(ArchiveProgressType.FinishArchive);
					currentArchiveNumber++;
					totalArchives++;
					context.CloseArchiveWriteStream(currentArchiveNumber, currentArchiveName, stream);
					currentArchiveName = base.NextCabinetName;
					currentArchiveBytesProcessed = (currentArchiveTotalBytes = 0L);
					totalFolderBytesProcessedInCurrentCab = 0L;
					base.CabStream = null;
				}
				else
				{
					stream.Close();
					tempStreams.Remove(stream);
				}
				return base.CabCloseStreamEx(streamHandle, out err, pv);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && fciHandle != null)
				{
					fciHandle.Dispose();
					fciHandle = null;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private static NativeMethods.FCI.TCOMP GetCompressionType(CompressionLevel compLevel)
		{
			if (compLevel < CompressionLevel.Min)
			{
				return NativeMethods.FCI.TCOMP.TYPE_NONE;
			}
			if (compLevel > CompressionLevel.Max)
			{
				compLevel = CompressionLevel.Max;
			}
			int num = checked(6 * unchecked((int)checked(compLevel - 1))) / 9;
			return (NativeMethods.FCI.TCOMP)checked((ushort)(3 | (3840 + (num << 8))));
		}

		private void AddFile(string name, Stream stream, FileAttributes attributes, DateTime lastWriteTime, bool execute, CompressionLevel compLevel)
		{
			base.FileStream = stream;
			fileAttributes = attributes & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive);
			fileLastWriteTime = lastWriteTime;
			currentFileName = name;
			NativeMethods.FCI.TCOMP compressionType = GetCompressionType(compLevel);
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				Encoding encoding = Encoding.ASCII;
				if (Encoding.UTF8.GetByteCount(name) > name.Length)
				{
					encoding = Encoding.UTF8;
					fileAttributes |= FileAttributes.Normal;
				}
				byte[] bytes = encoding.GetBytes(name);
				intPtr = Marshal.AllocHGlobal(checked(bytes.Length + 1));
				Marshal.Copy(bytes, 0, intPtr, bytes.Length);
				Marshal.WriteByte(intPtr, bytes.Length, 0);
				base.Erf.Clear();
				NativeMethods.FCI.AddFile(fciHandle, string.Empty, intPtr, execute, fciGetNextCabinet, fciCreateStatus, fciGetOpenInfo, compressionType);
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			CheckError(extracting: false);
			base.FileStream = null;
			currentFileName = null;
		}

		private void FlushFolder()
		{
			base.Erf.Clear();
			NativeMethods.FCI.FlushFolder(fciHandle, fciGetNextCabinet, fciCreateStatus);
			CheckError(extracting: false);
		}

		private void FlushCabinet()
		{
			base.Erf.Clear();
			NativeMethods.FCI.FlushCabinet(fciHandle, fGetNextCab: false, fciGetNextCabinet, fciCreateStatus);
			CheckError(extracting: false);
		}

		private int CabGetOpenInfo(string path, out short date, out short time, out short attribs, out int err, IntPtr pv)
		{
			CompressionEngine.DateTimeToDosDateAndTime(fileLastWriteTime, out date, out time);
			attribs = checked((short)fileAttributes);
			Stream fileStream = base.FileStream;
			base.FileStream = new DuplicateStream(fileStream);
			int result = base.StreamHandles.AllocHandle(fileStream);
			err = 0;
			return result;
		}

		private int CabFilePlaced(IntPtr pccab, string filePath, long fileSize, int continuation, IntPtr pv)
		{
			return 0;
		}

		private int CabGetNextCabinet(IntPtr pccab, uint prevCabSize, IntPtr pv)
		{
			NativeMethods.FCI.CCAB cCAB = new NativeMethods.FCI.CCAB();
			Marshal.PtrToStructure(pccab, cCAB);
			cCAB.szDisk = string.Empty;
			cCAB.szCab = context.GetArchiveName(cCAB.iCab);
			base.CabNumbers[cCAB.szCab] = checked((short)cCAB.iCab);
			base.NextCabinetName = cCAB.szCab;
			Marshal.StructureToPtr(cCAB, pccab, fDeleteOld: false);
			return 1;
		}

		private int CabCreateStatus(NativeMethods.FCI.STATUS typeStatus, uint cb1, uint cb2, IntPtr pv)
		{
			checked
			{
				switch (typeStatus)
				{
				case NativeMethods.FCI.STATUS.FILE:
					if (cb2 != 0 && currentFileBytesProcessed < currentFileTotalBytes)
					{
						if (currentFileBytesProcessed + unchecked((long)cb2) > currentFileTotalBytes)
						{
							cb2 = (uint)currentFileTotalBytes - (uint)currentFileBytesProcessed;
						}
						currentFileBytesProcessed += cb2;
						fileBytesProcessed += cb2;
						OnProgress(ArchiveProgressType.PartialFile);
					}
					break;
				case NativeMethods.FCI.STATUS.FOLDER:
					if (cb1 == 0)
					{
						currentFolderTotalBytes = unchecked((long)cb2) - totalFolderBytesProcessedInCurrentCab;
						totalFolderBytesProcessedInCurrentCab = cb2;
					}
					else if (currentFolderTotalBytes == 0L)
					{
						OnProgress(ArchiveProgressType.PartialArchive);
					}
					break;
				}
				return 0;
			}
		}

		private int CabGetTempFile(IntPtr tempNamePtr, int tempNameSize, IntPtr pv)
		{
			string s = ((!UseTempFiles) ? "%%TEMP%%" : Path.GetFileName(Path.GetTempFileName()));
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			if (bytes.Length >= tempNameSize)
			{
				return -1;
			}
			Marshal.Copy(bytes, 0, tempNamePtr, bytes.Length);
			Marshal.WriteByte(tempNamePtr, bytes.Length, 0);
			return 1;
		}

		private int CabDeleteFile(string path, out int err, IntPtr pv)
		{
			try
			{
				if (path != "%%TEMP%%")
				{
					path = Path.Combine(Path.GetTempPath(), path);
					File.Delete(path);
				}
			}
			catch (IOException)
			{
			}
			err = 0;
			return 1;
		}
	}
}
