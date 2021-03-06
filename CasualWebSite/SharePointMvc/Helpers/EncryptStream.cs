﻿using System;
using System.IO;

namespace SharePointMvc.Helpers
{
    public class EncryptStream : Stream
    {
        private Stream stream;
        bool disposed;

        public EncryptStream(Func<Stream> streamer)
        {
            stream = streamer();
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
            Encryption.XorInplaceCrypt(buffer);
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
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }

                stream.Dispose();
                stream.Close();
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