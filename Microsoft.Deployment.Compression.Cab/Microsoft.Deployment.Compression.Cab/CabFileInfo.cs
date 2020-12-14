using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Deployment.Compression.Cab
{
	[Serializable]
	public class CabFileInfo : ArchiveFileInfo
	{
		private int cabFolder;

		public CabInfo Cabinet => (CabInfo)base.Archive;

		public string CabinetName => base.ArchiveName;

		public int CabinetFolderNumber
		{
			get
			{
				if (cabFolder < 0)
				{
					Refresh();
				}
				return cabFolder;
			}
		}

		public CabFileInfo(CabInfo cabinetInfo, string filePath)
			: base(cabinetInfo, filePath)
		{
			if (cabinetInfo == null)
			{
				throw new ArgumentNullException("cabinetInfo");
			}
			cabFolder = -1;
		}

		internal CabFileInfo(string filePath, int cabFolder, int cabNumber, FileAttributes attributes, DateTime lastWriteTime, long length)
			: base(filePath, cabNumber, attributes, lastWriteTime, length)
		{
			this.cabFolder = cabFolder;
		}

		protected CabFileInfo(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			cabFolder = info.GetInt32("cabFolder");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("cabFolder", cabFolder);
		}

		protected override void Refresh(ArchiveFileInfo newFileInfo)
		{
			base.Refresh(newFileInfo);
			cabFolder = ((CabFileInfo)newFileInfo).cabFolder;
		}
	}
}
