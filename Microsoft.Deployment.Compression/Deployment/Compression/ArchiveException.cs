// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.ArchiveException
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

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
      : this(message, (Exception) null)
    {
    }

    public ArchiveException()
      : this((string) null, (Exception) null)
    {
    }

    protected ArchiveException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
