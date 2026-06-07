using System;
using System.Collections;
using System.IO;
using System.Text;


namespace XeXtractor
{

    public class XSTR
    {
        protected EndianIo io;
        private int magic;
        private int version;
        private int unknow;
        private short nbString;
        private string originalFile = "";
        private ArrayList stringList = new ArrayList();

        public XSTR(byte[] data) => this.io = new EndianIo(data, EndianType.BigEndian);

        public XSTR(EndianIo io) => this.io = io;

        public void Open() => this.io.Open();

        public void Close() => this.io.Close();

        public virtual void Read(bool DoNotAddOutput, string outputName)
        {
            this.Read(this.io.In, DoNotAddOutput, outputName);
        }

        public XSTR(string filePath)
        {
            this.originalFile = filePath;
            this.io = new EndianIo(filePath, EndianType.BigEndian);
        }

        public string GetStringId(ushort stringId)
        {
            string stringId1 = "";
            foreach (XSTR.StringEntry stringEntry in this.stringList)
            {
                if ((int)stringEntry.id == (int)stringId)
                {
                    stringId1 = stringEntry.theString;
                    break;
                }
            }
            return stringId1;
        }

        public virtual void Read(EndianReader er, bool DoNotAddOutput, string outputName)
        {
            this.magic = er.ReadInt32();
            if (this.magic != 1481856082)
                throw new Exception("File is not an XSTR file");
            this.version = er.ReadInt32();
            this.unknow = er.ReadInt32();
            this.nbString = er.ReadInt16();
            for (int index = 0; index < (int)this.nbString; ++index)
                this.stringList.Add((object)new XSTR.StringEntry(er));
            MemoryStream memoryStream = new MemoryStream();
            TextWriter textWriter = (TextWriter)new StreamWriter((Stream)memoryStream, Encoding.UTF8);
            for (int index = 0; index < (int)this.nbString; ++index)
            {
                XSTR.StringEntry stringEntry = (XSTR.StringEntry)this.stringList[index];
                textWriter.WriteLine($"{stringEntry.id.ToString("X")} - {stringEntry.theString}");
            }
            textWriter.Close();
            if (DoNotAddOutput)
                return;
            InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
            {
                Data = memoryStream.ToArray(),
                fileName = outputName + ".txt",
                type = nameof(XSTR),
                folder = ""
            });
        }

        public class StringEntry
        {
            public ushort id;
            public ushort length;
            public string theString;

            public StringEntry()
            {
            }

            public StringEntry(EndianReader er) => this.Read(er);

            public void Read(EndianReader er)
            {
                this.id = er.ReadUInt16();
                this.length = er.ReadUInt16();
                this.theString = er.ReadAsciiString((int)this.length);
            }
        }
    }
}