using System;


namespace XeXtractor
{

    public class XITB
    {
        protected EndianIo io;

        public XITB(EndianIo io) => this.io = io;

        public void Open() => this.io.Open();

        public void Close() => this.io.Close();

        public virtual void Read() => this.Read(this.io.In);

        public XITB(byte[] data) => this.io = new EndianIo(data, EndianType.BigEndian);

        public XITB(string filePath) => this.io = new EndianIo(filePath, EndianType.BigEndian);

        public virtual void Read(EndianReader er)
        {
            if (er.ReadInt32() != 1481200706)
                throw new Exception("Invalid magic!");
            er.ReadInt32();
            er.ReadInt32();
            int num = er.ReadInt32();
            for (int index = 0; index < num; ++index)
            {
                int id = er.ReadInt32();
                int length = er.ReadInt32();
                string str = er.ReadAsciiString(length);
                foreach (FileEntry file in InnerFileStructure.getInstance().getFiles("XDBF", "Png", (long)id, false))
                    file.fileName = str;
            }
        }
    }
}