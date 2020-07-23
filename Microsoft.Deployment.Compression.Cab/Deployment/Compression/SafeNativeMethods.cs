// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.SafeNativeMethods
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Deployment.Compression
{
  [SuppressUnmanagedCodeSecurity]
  internal static class SafeNativeMethods
  {
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DosDateTimeToFileTime(
      short wFatDate,
      short wFatTime,
      out long fileTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool FileTimeToDosDateTime(
      ref long fileTime,
      out short wFatDate,
      out short wFatTime);
  }
}
