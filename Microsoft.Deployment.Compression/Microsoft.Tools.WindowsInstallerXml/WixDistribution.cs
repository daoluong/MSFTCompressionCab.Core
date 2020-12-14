using System;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Tools.WindowsInstallerXml
{
	internal static class WixDistribution
	{
		public static string NewsUrl = "http://wixtoolset.org/news/";

		public static string ShortProduct = "WiX Toolset";

		public static string SupportUrl = "http://wixtoolset.org/";

		public static string TelemetryUrlFormat = "http://wixtoolset.org/telemetry/v{0}/?r={1}";

		public static string ReplacePlaceholders(string original, Assembly assembly)
		{
			if (assembly != null)
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
				original = original.Replace("[FileComments]", versionInfo.Comments);
				original = original.Replace("[FileCopyright]", versionInfo.LegalCopyright);
				original = original.Replace("[FileProductName]", versionInfo.ProductName);
				original = original.Replace("[FileVersion]", versionInfo.FileVersion);
				if (original.Contains("[FileVersionMajorMinor]"))
				{
					Version version = new Version(versionInfo.FileVersion);
					original = original.Replace("[FileVersionMajorMinor]", version.Major + "." + version.Minor);
				}
				if (TryGetAttribute<AssemblyCompanyAttribute>(assembly, out var attribute))
				{
					original = original.Replace("[AssemblyCompany]", attribute.Company);
				}
				if (TryGetAttribute<AssemblyCopyrightAttribute>(assembly, out var attribute2))
				{
					original = original.Replace("[AssemblyCopyright]", attribute2.Copyright);
				}
				if (TryGetAttribute<AssemblyDescriptionAttribute>(assembly, out var attribute3))
				{
					original = original.Replace("[AssemblyDescription]", attribute3.Description);
				}
				if (TryGetAttribute<AssemblyProductAttribute>(assembly, out var attribute4))
				{
					original = original.Replace("[AssemblyProduct]", attribute4.Product);
				}
				if (TryGetAttribute<AssemblyTitleAttribute>(assembly, out var attribute5))
				{
					original = original.Replace("[AssemblyTitle]", attribute5.Title);
				}
			}
			original = original.Replace("[NewsUrl]", NewsUrl);
			original = original.Replace("[ShortProduct]", ShortProduct);
			original = original.Replace("[SupportUrl]", SupportUrl);
			return original;
		}

		private static bool TryGetAttribute<T>(Assembly assembly, out T attribute) where T : Attribute
		{
			attribute = null;
			object[] customAttributes = assembly.GetCustomAttributes(typeof(T), inherit: false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				attribute = customAttributes[0] as T;
			}
			return attribute != null;
		}
	}
}
