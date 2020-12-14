using System;
using System.IO;

namespace Microsoft.Deployment.Compression
{
	public class OffsetStream : Stream
	{
		private Stream source;

		private long sourceOffset;

		public Stream Source => source;

		public long Offset => sourceOffset;

		public override bool CanRead => source.CanRead;

		public override bool CanWrite => source.CanWrite;

		public override bool CanSeek => source.CanSeek;

		public override long Length => checked(source.Length - sourceOffset);

		public override long Position
		{
			get
			{
				return checked(source.Position - sourceOffset);
			}
			set
			{
				source.Position = checked(value + sourceOffset);
			}
		}

		public OffsetStream(Stream source, long offset)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			this.source = source;
			sourceOffset = offset;
			this.source.Seek(sourceOffset, SeekOrigin.Current);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return source.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			source.Write(buffer, offset, count);
		}

		public override int ReadByte()
		{
			return source.ReadByte();
		}

		public override void WriteByte(byte value)
		{
			source.WriteByte(value);
		}

		public override void Flush()
		{
			source.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return checked(source.Seek(offset + ((origin == SeekOrigin.Begin) ? sourceOffset : 0), origin) - sourceOffset);
		}

		public override void SetLength(long value)
		{
			source.SetLength(checked(value + sourceOffset));
		}

		public override void Close()
		{
			source.Close();
		}
	}
}
