// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.DuplicateStream
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

using System;
using System.IO;

namespace Microsoft.Deployment.Compression
{
  public class DuplicateStream : Stream
  {
    private Stream source;
    private long position;

    public DuplicateStream(Stream source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      this.source = DuplicateStream.OriginalStream(source);
    }

    public Stream Source
    {
      get
      {
        return this.source;
      }
    }

    public override bool CanRead
    {
      get
      {
        return this.source.CanRead;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return this.source.CanWrite;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return this.source.CanSeek;
      }
    }

    public override long Length
    {
      get
      {
        return this.source.Length;
      }
    }

    public override long Position
    {
      get
      {
        return this.position;
      }
      set
      {
        this.position = value;
      }
    }

    public static Stream OriginalStream(Stream stream)
    {
      DuplicateStream duplicateStream = stream as DuplicateStream;
      if (duplicateStream == null)
        return stream;
      return duplicateStream.Source;
    }

    public override void Flush()
    {
      this.source.Flush();
    }

    public override void SetLength(long value)
    {
      this.source.SetLength(value);
    }

    public override void Close()
    {
      this.source.Close();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      long position = this.source.Position;
      this.source.Position = this.position;
      int num = this.source.Read(buffer, offset, count);
      this.position = this.source.Position;
      this.source.Position = position;
      return num;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      long position = this.source.Position;
      this.source.Position = this.position;
      this.source.Write(buffer, offset, count);
      this.position = this.source.Position;
      this.source.Position = position;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      long num = 0;
      switch (origin)
      {
        case SeekOrigin.Current:
          num = this.position;
          break;
        case SeekOrigin.End:
          num = this.Length;
          break;
      }
      this.position = checked (num + offset);
      return this.position;
    }
  }
}
