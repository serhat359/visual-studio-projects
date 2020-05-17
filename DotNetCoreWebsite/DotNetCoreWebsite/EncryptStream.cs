using System;
using System.IO;

namespace DotNetCoreWebsite
{
    public class EncryptStream : Stream
    {
        private Stream stream;
        private CoreEncryption coreEncryption;
        private readonly long misalignment;
        bool disposed;
        Action onClose = null;
        bool isOnCloseExecuted = false;

        public EncryptStream(Func<Stream> streamer, CoreEncryption coreEncryption, long misalignment, Action onClose = null)
        {
            stream = streamer();
            this.coreEncryption = coreEncryption;
            this.misalignment = misalignment;
            this.onClose = onClose;
        }

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }
            }
            //dispose unmanaged resources
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
