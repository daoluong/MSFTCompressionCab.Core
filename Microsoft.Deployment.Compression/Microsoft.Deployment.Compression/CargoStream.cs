using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Deployment.Compression
{
	public class CargoStream : Stream
	{
		private Stream source;

		private List<IDisposable> cargo;

		public Stream Source => source;

		public IList<IDisposable> Cargo => cargo;

		public override bool CanRead => source.CanRead;

		public override bool CanWrite => source.CanWrite;

		public override bool CanSeek => source.CanSeek;

		public override long Length => source.Length;

		public override long Position
		{
			get
			{
				return source.Position;
			}
			set
			{
				source.Position = value;
			}
		}

		public CargoStream(Stream source, params IDisposable[] cargo)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			this.source = source;
			this.cargo = new List<IDisposable>(cargo);
		}

		public override void Flush()
		{
			source.Flush();
		}

		public override void SetLength(long value)
		{
			source.SetLength(value);
		}

		public override void Close()
		{
			source.Close();
			foreach (IDisposable item in cargo)
			{
				item.Dispose();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return source.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			source.Write(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return source.Seek(offset, origin);
		}
	}
}
