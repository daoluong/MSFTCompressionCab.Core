using System;
using System.IO;

namespace Microsoft.Deployment.Compression
{
	public class DuplicateStream : Stream
	{
		private Stream source;

		private long position;

		public Stream Source => source;

		public override bool CanRead => source.CanRead;

		public override bool CanWrite => source.CanWrite;

		public override bool CanSeek => source.CanSeek;

		public override long Length => source.Length;

		public override long Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
			}
		}

		public DuplicateStream(Stream source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			this.source = OriginalStream(source);
		}

		public static Stream OriginalStream(Stream stream)
		{
			DuplicateStream duplicateStream = stream as DuplicateStream;
			if (duplicateStream == null)
			{
				return stream;
			}
			return duplicateStream.Source;
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
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			long num = source.Position;
			source.Position = position;
			int result = source.Read(buffer, offset, count);
			position = source.Position;
			source.Position = num;
			return result;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			long num = source.Position;
			source.Position = position;
			source.Write(buffer, offset, count);
			position = source.Position;
			source.Position = num;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long num = 0L;
			switch (origin)
			{
			case SeekOrigin.Current:
				num = position;
				break;
			case SeekOrigin.End:
				num = Length;
				break;
			}
			position = checked(num + offset);
			return position;
		}
	}
}
