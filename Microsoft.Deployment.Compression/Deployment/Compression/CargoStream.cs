// Decompiled with JetBrains decompiler
// Type: Microsoft.Deployment.Compression.CargoStream
// Assembly: Microsoft.Deployment.Compression, Version=3.0.0.0, Culture=neutral, PublicKeyToken=ce35f76fcda82bad
// MVID: 6A7FAA37-3E7D-4F08-9CEC-6FFE0F4F3B2D
// Assembly location: E:\Gits\sample code\msftcompressioncab.1.0.0\lib\Microsoft.Deployment.Compression.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Deployment.Compression
{
  public class CargoStream : Stream
  {
    private Stream source;
    private List<IDisposable> cargo;

    public CargoStream(Stream source, params IDisposable[] cargo)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      this.source = source;
      this.cargo = new List<IDisposable>((IEnumerable<IDisposable>) cargo);
    }

    public Stream Source
    {
      get
      {
        return this.source;
      }
    }

    public IList<IDisposable> Cargo
    {
      get
      {
        return (IList<IDisposable>) this.cargo;
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
        return this.source.Position;
      }
      set
      {
        this.source.Position = value;
      }
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
      foreach (IDisposable disposable in this.cargo)
        disposable.Dispose();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return this.source.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      this.source.Write(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return this.source.Seek(offset, origin);
    }
  }
}
