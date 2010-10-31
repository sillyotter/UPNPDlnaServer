using System;
using System.IO;
using System.Text;

namespace MediaServer.Web
{
    class UnbufferedStreamReader : TextReader
    {
        private readonly Stream _stream;

        public Stream BaseStream { get { return _stream; }}

        public UnbufferedStreamReader(Stream stream)
        {
            _stream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }

            base.Dispose(disposing);
        }

        public override void Close()
        {
            _stream.Close();
        }

        public override int Peek()
        {
            var res = _stream.ReadByte();
            _stream.Position--;
            return res;
        }

        public override int Read()
        {
            return _stream.ReadByte();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            for (var i = 0; i < count; ++i)
            {
                var data = Read();
                if (data == -1) return i;
                buffer[index + i] = (char) data;
            }

            return count;
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return Read(buffer, index, count);
        }

        public override string ReadLine()
        {
            var sb = new StringBuilder();

            var c = Read();
            while(c != -1)
            {
                sb.Append((char) c);
                if (c == '\n') break;
                c = Read();
            }
            var line = sb.ToString().Trim();
			return String.IsNullOrEmpty(line) ? null : line;
        }

        public override string ReadToEnd()
        {
            var sb = new StringBuilder();

            var c = Read();
            while (c != -1)
            {
                sb.Append((char)c);
                c = Read();
            }
            var line =  sb.ToString().Trim();
			return String.IsNullOrEmpty(line) ? null : line;
        }
    }
}
