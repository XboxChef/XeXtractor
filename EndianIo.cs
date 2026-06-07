using System;
using System.IO;


namespace XeXtractor
{

    public class EndianIo : IDisposable
    {
        private readonly bool isFile;
        private bool isOpen;
        private Stream stream;
        private readonly string filepath = string.Empty;
        private readonly EndianType endiantype = EndianType.LittleEndian;
        private EndianReader @in;
        private EndianWriter @out;

        public bool Opened => this.isOpen;

        public bool Closed => !this.isOpen;

        public EndianReader In => this.@in;

        public EndianWriter Out => this.@out;

        public Stream Stream => this.stream;

        public long Position => this.stream.Position;

        public EndianIo(string filePath, EndianType endianStyle)
        {
            this.endiantype = endianStyle;
            this.filepath = filePath;
            this.isFile = true;
        }

        public EndianIo(Stream stream, EndianType endianStyle)
        {
            this.endiantype = endianStyle;
            this.stream = stream;
            this.isFile = false;
        }

        public EndianIo(byte[] buffer, EndianType endianStyle)
        {
            this.endiantype = endianStyle;
            this.stream = (Stream)new MemoryStream(buffer);
            this.isFile = false;
        }

        public void SeekTo(int offset) => this.SeekTo((long)offset, SeekOrigin.Begin);

        public void SeekTo(long offset) => this.SeekTo(offset, SeekOrigin.Begin);

        public void SeekTo(long offset, SeekOrigin seekOrigin) => this.stream.Seek(offset, seekOrigin);

        public void Open() => this.Open(FileMode.OpenOrCreate);

        public void Open(FileMode fileMode)
        {
            if (this.isOpen)
                return;
            if (this.isFile)
                this.stream = (Stream)new FileStream(this.filepath, fileMode, FileAccess.ReadWrite);
            this.@in = new EndianReader(this.stream, this.endiantype);
            this.@out = new EndianWriter(this.stream, this.endiantype);
            this.isOpen = true;
        }

        public void Close()
        {
            if (!this.isOpen)
                return;
            this.stream.Close();
            this.@in.Close();
            this.@out.Close();
            this.isOpen = false;
        }

        public byte[] ToArray() => ((MemoryStream)this.stream).ToArray();

        public void Dispose()
        {
            Close();
        }
    }
}