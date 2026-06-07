using System;
using System.IO;
using System.IO.Compression;


namespace XeXtractor
{

    public class XSRC
    {
        protected EndianIo io;
        private int magic;
        private int version;
        private int size;

        public XSRC(EndianIo io) => this.io = io;

        public void Open() => this.io.Open();

        public void Close() => this.io.Close();

        public virtual void Read() => this.Read(this.io.In);

        public XSRC(byte[] data) => this.io = new EndianIo(data, EndianType.BigEndian);

        public XSRC(string filePath) => this.io = new EndianIo(filePath, EndianType.BigEndian);

        public virtual void Read(EndianReader er)
        {
            this.magic = er.ReadInt32();
            if (this.magic != 1481855555)
                throw new Exception("Invalid magic!");
            this.version = er.ReadInt32();
            this.size = er.ReadInt32();
            int length = er.ReadInt32();
            string str = er.ReadAsciiString(length);
            int count1 = er.ReadInt32();
            int count2 = er.ReadInt32();
            GZipStream gzipStream = new GZipStream((Stream)new MemoryStream(er.ReadBytes(count2)), CompressionMode.Decompress, true);
            byte[] buffer = new byte[count1];
            gzipStream.Read(buffer, 0, count1);
            gzipStream.Close();
            InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
            {
                fileName = str,
                Data = buffer,
                folder = "",
                type = nameof(XSRC)
            });
        }
    }
}