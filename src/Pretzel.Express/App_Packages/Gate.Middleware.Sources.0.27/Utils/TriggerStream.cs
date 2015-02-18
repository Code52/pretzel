using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Gate.Middleware.Utils
{
    // Used by several middleware to be notified when the first write takes place.
    // This gives them one last chance to examine and modify the headers.
    internal class TriggerStream : Stream
    {
        public TriggerStream(Stream innerStream)
        {
            InnerStream = innerStream;
        }

        public Stream InnerStream { get; set; }
        public Action OnFirstWrite { get; set; }
        private bool IsStarted { get; set; }

        public override bool CanRead
        {
            get { return InnerStream.CanRead; }
        }

        public override bool CanWrite
        {
            get { return InnerStream.CanWrite; }
        }

        public override bool CanSeek
        {
            get { return InnerStream.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return InnerStream.CanTimeout; }
        }

        public override int WriteTimeout
        {
            get { return InnerStream.WriteTimeout; }
            set { InnerStream.WriteTimeout = value; }
        }

        public override int ReadTimeout
        {
            get { return InnerStream.ReadTimeout; }
            set { InnerStream.ReadTimeout = value; }
        }

        public override long Position
        {
            get { return InnerStream.Position; }
            set { InnerStream.Position = value; }
        }

        public override long Length
        {
            get { return InnerStream.Length; }
        }

        public override void Close()
        {
            InnerStream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InnerStream.Dispose();
            }
        }

        public override string ToString()
        {
            return InnerStream.ToString();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override int ReadByte()
        {
            return InnerStream.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return InnerStream.Read(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return InnerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return InnerStream.EndRead(asyncResult);
        }

        public override void WriteByte(byte value)
        {
            Start();
            InnerStream.WriteByte(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Start();
            InnerStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Start();
            return InnerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            InnerStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            Start();
            InnerStream.Flush();
        }

        private void Start()
        {
            if (!IsStarted)
            {
                IsStarted = true;
                if (OnFirstWrite != null)
                {
                    OnFirstWrite();
                }                     
            }
        }
    }
}
