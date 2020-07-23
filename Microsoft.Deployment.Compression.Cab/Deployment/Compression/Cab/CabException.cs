// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.Cab.CabException
// Assembly: Microsoft.Deployment.Compression.Cab, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: D94CEDF8-4B4A-4AC8-B27E-50F0AAABF518
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.Cab.dll

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

    public CabException(string message, Exception innerException)
      : this(0, 0, message, innerException)
    {
    }

    public CabException(string message)
      : this(0, 0, message, (Exception) null)
    {
    }

    public CabException()
      : this(0, 0, (string) null, (Exception) null)
    {
    }

    internal CabException(int error, int errorCode, string message, Exception innerException)
      : base(message, innerException)
    {
      this.error = error;
      this.errorCode = errorCode;
    }

    internal CabException(int error, int errorCode, string message)
      : this(error, errorCode, message, (Exception) null)
    {
    }

    protected CabException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      if (info == null)
        throw new ArgumentNullException(nameof (info));
      this.error = info.GetInt32("cabError");
      this.errorCode = info.GetInt32("cabErrorCode");
    }

    public int Error
    {
      get
      {
        return this.error;
      }
    }

    public int ErrorCode
    {
      get
      {
        return this.errorCode;
      }
    }

    internal static ResourceManager ErrorResources
    {
      get
      {
        if (CabException.errorResources == null)
          CabException.errorResources = new ResourceManager(typeof (CabException).Namespace + ".Errors", typeof (CabException).Assembly);
        return CabException.errorResources;
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException(nameof (info));
      info.AddValue("cabError", this.error);
      info.AddValue("cabErrorCode", this.errorCode);
      base.GetObjectData(info, context);
    }

    internal static string GetErrorMessage(int error, int errorCode, bool extracting)
    {
      int num = extracting ? 2000 : 1000;
      string str = CabException.ErrorResources.GetString(checked (num + error).ToString((IFormatProvider) CultureInfo.InvariantCulture.NumberFormat), CultureInfo.CurrentCulture) ?? CabException.ErrorResources.GetString(num.ToString((IFormatProvider) CultureInfo.InvariantCulture.NumberFormat), CultureInfo.CurrentCulture);
      if (errorCode != 0)
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} " + CabException.ErrorResources.GetString("1", CultureInfo.CurrentCulture), (object) str, (object) errorCode);
      return str;
    }
  }
}
