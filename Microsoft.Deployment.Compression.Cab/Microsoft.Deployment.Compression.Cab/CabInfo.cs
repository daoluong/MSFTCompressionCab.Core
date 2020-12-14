using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Deployment.Compression.Cab
{
	[Serializable]
	public class CabInfo : ArchiveInfo
	{
		public CabInfo(string path)
			: base(path)
		{
		}

		protected CabInfo(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		protected override CompressionEngine CreateCompressionEngine()
		{
			return new CabEngine();
		}

		public new IList<CabFileInfo> GetFiles()
		{
			IList<ArchiveFileInfo> files = base.GetFiles();
			List<CabFileInfo> list = new List<CabFileInfo>(files.Count);
			foreach (CabFileInfo item in files)
			{
				list.Add(item);
			}
			return list.AsReadOnly();
		}

		public new IList<CabFileInfo> GetFiles(string searchPattern)
		{
			IList<ArchiveFileInfo> files = base.GetFiles(searchPattern);
			List<CabFileInfo> list = new List<CabFileInfo>(files.Count);
			foreach (CabFileInfo item in files)
			{
				list.Add(item);
			}
			return list.AsReadOnly();
		}
	}
}
