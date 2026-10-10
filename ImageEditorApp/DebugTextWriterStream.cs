using System.Diagnostics;
using System.IO;
using System.Text;

namespace ImageEditorApp
{
	internal class DebugTextWriterStream : Stream
	{
		public override void Write(byte[] buffer, int offset, int count)
		{
			Debug.Write(Encoding.UTF8.GetString(buffer, offset, count));
		}

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => 0;
		public override long Position {	get => 0; set {}}
		public override void Flush(){}
		public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
		public override void SetLength(long value) => throw new NotImplementedException();
	}
}