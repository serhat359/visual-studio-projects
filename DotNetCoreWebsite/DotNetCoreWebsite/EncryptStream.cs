using System;
using System.IO;

namespace DotNetCoreWebsite
{
    public class EncryptStream : Stream
    {
        private readonly Stream stream;
        private readonly CoreEncryption coreEncryption;
        private readonly long misalignment;
        private readonly long rangeStart;
        Action? onClose = null;
        bool isOnCloseExecuted = false;

        public EncryptStream(Stream stream, CoreEncryption coreEncryption, long rangeStart, Action? onClose = null)
        {
            this.stream = stream;
            this.coreEncryption = coreEncryption;
            this.rangeStart = rangeStart;
            this.misalignment = rangeStart % 512;
            this.onClose = onClose;
        }

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length - rangeStart;

        public override long Position { get { return stream.Position; } set { stream.Position = value; } }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readCount = stream.Read(buffer, offset, count);
            coreEncryption.EncryptInPlace(buffer, this.misalignment);
            return readCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
            stream.Close();

            if (onClose != null && !isOnCloseExecuted)
            {
                isOnCloseExecuted = true;
                onClose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            stream.Dispose();
        }
    }
}
