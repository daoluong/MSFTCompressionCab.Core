using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Deployment.Compression.Cab
{
	internal abstract class CabWorker : IDisposable
	{
		internal const string CabStreamName = "%%CAB%%";

		private CabEngine cabEngine;

		private HandleManager<Stream> streamHandles;

		private Stream cabStream;

		private Stream fileStream;

		private NativeMethods.ERF erf;

		private GCHandle erfHandle;

		private IDictionary<string, short> cabNumbers;

		private string nextCabinetName;

		private bool suppressProgressEvents;

		private byte[] buf;

		protected string currentFileName;

		protected int currentFileNumber;

		protected int totalFiles;

		protected long currentFileBytesProcessed;

		protected long currentFileTotalBytes;

		protected short currentFolderNumber;

		protected long currentFolderTotalBytes;

		protected string currentArchiveName;

		protected short currentArchiveNumber;

		protected short totalArchives;

		protected long currentArchiveBytesProcessed;

		protected long currentArchiveTotalBytes;

		protected long fileBytesProcessed;

		protected long totalFileBytes;

		public CabEngine CabEngine => cabEngine;

		internal NativeMethods.ERF Erf => erf;

		internal GCHandle ErfHandle => erfHandle;

		internal HandleManager<Stream> StreamHandles => streamHandles;

		internal bool SuppressProgressEvents
		{
			get
			{
				return suppressProgressEvents;
			}
			set
			{
				suppressProgressEvents = value;
			}
		}

		internal IDictionary<string, short> CabNumbers => cabNumbers;

		internal string NextCabinetName
		{
			get
			{
				return nextCabinetName;
			}
			set
			{
				nextCabinetName = value;
			}
		}

		internal Stream CabStream
		{
			get
			{
				return cabStream;
			}
			set
			{
				cabStream = value;
			}
		}

		internal Stream FileStream
		{
			get
			{
				return fileStream;
			}
			set
			{
				fileStream = value;
			}
		}

		protected CabWorker(CabEngine cabEngine)
		{
			this.cabEngine = cabEngine;
			streamHandles = new HandleManager<Stream>();
			erf = new NativeMethods.ERF();
			erfHandle = GCHandle.Alloc(erf, GCHandleType.Pinned);
			cabNumbers = new Dictionary<string, short>(1);
			buf = new byte[32768];
		}

		~CabWorker()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected void ResetProgressData()
		{
			currentFileName = null;
			currentFileNumber = 0;
			totalFiles = 0;
			currentFileBytesProcessed = 0L;
			currentFileTotalBytes = 0L;
			currentFolderNumber = 0;
			currentFolderTotalBytes = 0L;
			currentArchiveName = null;
			currentArchiveNumber = 0;
			totalArchives = 0;
			currentArchiveBytesProcessed = 0L;
			currentArchiveTotalBytes = 0L;
			fileBytesProcessed = 0L;
			totalFileBytes = 0L;
		}

		protected void OnProgress(ArchiveProgressType progressType)
		{
			if (!suppressProgressEvents)
			{
				ArchiveProgressEventArgs e = new ArchiveProgressEventArgs(progressType, currentFileName, (currentFileNumber >= 0) ? currentFileNumber : 0, totalFiles, currentFileBytesProcessed, currentFileTotalBytes, currentArchiveName, currentArchiveNumber, totalArchives, currentArchiveBytesProcessed, currentArchiveTotalBytes, fileBytesProcessed, totalFileBytes);
				CabEngine.ReportProgress(e);
			}
		}

		internal IntPtr CabAllocMem(int byteCount)
		{
			return Marshal.AllocHGlobal((IntPtr)byteCount);
		}

		internal void CabFreeMem(IntPtr memPointer)
		{
			Marshal.FreeHGlobal(memPointer);
		}

		internal int CabOpenStream(string path, int openFlags, int shareMode)
		{
			int err;
			return CabOpenStreamEx(path, openFlags, shareMode, out err, IntPtr.Zero);
		}

		internal virtual int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
		{
			path = path.Trim();
			Stream stream = cabStream;
			cabStream = new DuplicateStream(stream);
			int result = streamHandles.AllocHandle(stream);
			err = 0;
			return result;
		}

		internal int CabReadStream(int streamHandle, IntPtr memory, int cb)
		{
			int err;
			return CabReadStreamEx(streamHandle, memory, cb, out err, IntPtr.Zero);
		}

		internal virtual int CabReadStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
		{
			Stream stream = streamHandles[streamHandle];
			int num = cb;
			if (num > buf.Length)
			{
				buf = new byte[num];
			}
			num = stream.Read(buf, 0, num);
			Marshal.Copy(buf, 0, memory, num);
			err = 0;
			return num;
		}

		internal int CabWriteStream(int streamHandle, IntPtr memory, int cb)
		{
			int err;
			return CabWriteStreamEx(streamHandle, memory, cb, out err, IntPtr.Zero);
		}

		internal virtual int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
		{
			Stream stream = streamHandles[streamHandle];
			if (cb > buf.Length)
			{
				buf = new byte[cb];
			}
			Marshal.Copy(memory, buf, 0, cb);
			stream.Write(buf, 0, cb);
			err = 0;
			return cb;
		}

		internal int CabCloseStream(int streamHandle)
		{
			int err;
			return CabCloseStreamEx(streamHandle, out err, IntPtr.Zero);
		}

		internal virtual int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
		{
			streamHandles.FreeHandle(streamHandle);
			err = 0;
			return 0;
		}

		internal int CabSeekStream(int streamHandle, int offset, int seekOrigin)
		{
			int err;
			return CabSeekStreamEx(streamHandle, offset, seekOrigin, out err, IntPtr.Zero);
		}

		internal virtual int CabSeekStreamEx(int streamHandle, int offset, int seekOrigin, out int err, IntPtr pv)
		{
			checked
			{
				offset = (int)streamHandles[streamHandle].Seek(offset, unchecked((SeekOrigin)seekOrigin));
				err = 0;
				return offset;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (cabStream != null)
				{
					cabStream.Close();
					cabStream = null;
				}
				if (fileStream != null)
				{
					fileStream.Close();
					fileStream = null;
				}
			}
			if (erfHandle.IsAllocated)
			{
				erfHandle.Free();
			}
		}

		protected void CheckError(bool extracting)
		{
			if (Erf.Error)
			{
				throw new CabException(Erf.Oper, Erf.Type, CabException.GetErrorMessage(Erf.Oper, Erf.Type, extracting));
			}
		}
	}
}
