using System;
using System.IO;
using System.Runtime.Serialization;

namespace Microsoft.Deployment.Compression
{
	[Serializable]
	public class ArchiveException : IOException
	{
		public ArchiveException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public ArchiveException(string message)
			: this(message, null)
		{
		}

		public ArchiveException()
			: this(null, null)
		{
		}

		protected ArchiveException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
