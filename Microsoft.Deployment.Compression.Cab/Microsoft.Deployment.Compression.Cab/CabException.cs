using System;
using System.Globalization;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Deployment.Compression.Cab
{
	[Serializable]
	public class CabException : ArchiveException
	{
		private static ResourceManager errorResources;

		private int error;

		private int errorCode;

		public int Error => error;

		public int ErrorCode => errorCode;

		internal static ResourceManager ErrorResources
		{
			get
			{
				if (errorResources == null)
				{
					errorResources = new ResourceManager(typeof(CabException).Namespace + ".Errors", typeof(CabException).Assembly);
				}
				return errorResources;
			}
		}

		public CabException(string message, Exception innerException)
			: this(0, 0, message, innerException)
		{
		}

		public CabException(string message)
			: this(0, 0, message, null)
		{
		}

		public CabException()
			: this(0, 0, null, null)
		{
		}

		internal CabException(int error, int errorCode, string message, Exception innerException)
			: base(message, innerException)
		{
			this.error = error;
			this.errorCode = errorCode;
		}

		internal CabException(int error, int errorCode, string message)
			: this(error, errorCode, message, null)
		{
		}

		protected CabException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			error = info.GetInt32("cabError");
			errorCode = info.GetInt32("cabErrorCode");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("cabError", error);
			info.AddValue("cabErrorCode", errorCode);
			base.GetObjectData(info, context);
		}

		internal static string GetErrorMessage(int error, int errorCode, bool extracting)
		{
			int num = (extracting ? 2000 : 1000);
			string text = ErrorResources.GetString(checked(num + error).ToString(CultureInfo.InvariantCulture.NumberFormat), CultureInfo.CurrentCulture);
			if (text == null)
			{
				text = ErrorResources.GetString(num.ToString(CultureInfo.InvariantCulture.NumberFormat), CultureInfo.CurrentCulture);
			}
			if (errorCode != 0)
			{
				string @string = ErrorResources.GetString("1", CultureInfo.CurrentCulture);
				text = string.Format(CultureInfo.InvariantCulture, "{0} " + @string, text, errorCode);
			}
			return text;
		}
	}
}
