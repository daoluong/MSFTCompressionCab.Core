using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Deployment.Compression.Cab
{
	public class CabEngine : CompressionEngine
	{
		private CabPacker packer;

		private CabUnpacker unpacker;

		private CabPacker Packer
		{
			get
			{
				if (packer == null)
				{
					packer = new CabPacker(this);
				}
				return packer;
			}
		}

		private CabUnpacker Unpacker
		{
			get
			{
				if (unpacker == null)
				{
					unpacker = new CabUnpacker(this);
				}
				return unpacker;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (packer != null)
				{
					packer.Dispose();
					packer = null;
				}
				if (unpacker != null)
				{
					unpacker.Dispose();
					unpacker = null;
				}
			}
			base.Dispose(disposing);
		}

		public override void Pack(IPackStreamContext streamContext, IEnumerable<string> files, long maxArchiveSize)
		{
			Packer.CompressionLevel = base.CompressionLevel;
			Packer.UseTempFiles = base.UseTempFiles;
			Packer.Pack(streamContext, files, maxArchiveSize);
		}

		public override bool IsArchive(Stream stream)
		{
			return Unpacker.IsArchive(stream);
		}

		public override IList<ArchiveFileInfo> GetFileInfo(IUnpackStreamContext streamContext, Predicate<string> fileFilter)
		{
			return Unpacker.GetFileInfo(streamContext, fileFilter);
		}

		public override void Unpack(IUnpackStreamContext streamContext, Predicate<string> fileFilter)
		{
			Unpacker.Unpack(streamContext, fileFilter);
		}

		internal void ReportProgress(ArchiveProgressEventArgs e)
		{
			OnProgress(e);
		}
	}
}
