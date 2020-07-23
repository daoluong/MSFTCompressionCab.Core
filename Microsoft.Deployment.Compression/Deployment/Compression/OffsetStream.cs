// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.OffsetStream
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

using System;
using System.IO;

namespace Microsoft.Deployment.Compression
{
  public class OffsetStream : Stream
  {
    private Stream source;
    private long sourceOffset;

    public OffsetStream(Stream source, long offset)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      this.source = source;
      this.sourceOffset = offset;
      this.source.Seek(this.sourceOffset, SeekOrigin.Current);
    }

    public Stream Source
    {
      get
      {
        return this.source;
      }
    }

    public long Offset
    {
      get
      {
        return this.sourceOffset;
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
        return checked (this.source.Length - this.sourceOffset);
      }
    }

    public override long Position
    {
      get
      {
        return checked (this.source.Position - this.sourceOffset);
      }
      set
      {
        this.source.Position = checked (value + this.sourceOffset);
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return this.source.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      this.source.Write(buffer, offset, count);
    }

    public override int ReadByte()
    {
      return this.source.ReadByte();
    }

    public override void WriteByte(byte value)
    {
      this.source.WriteByte(value);
    }

    public override void Flush()
    {
      this.source.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return checked (this.source.Seek(offset + (unchecked (origin == SeekOrigin.Begin) ? this.sourceOffset : 0L), origin) - this.sourceOffset);
    }

    public override void SetLength(long value)
    {
      this.source.SetLength(checked (value + this.sourceOffset));
    }

    public override void Close()
    {
      this.source.Close();
    }
  }
}
