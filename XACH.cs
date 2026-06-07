using System;


namespace XeXtractor
{

    public class XACH
    {
        protected EndianIo io;
        private int magic;
        private int version;
        private int size;

        public XACH(EndianIo io) => this.io = io;

        public void Open() => this.io.Open();

        public void Close() => this.io.Close();

        public virtual void Read(XSTR xstr, string lang) => this.Read(this.io.In, xstr, lang);

        public XACH(byte[] data) => this.io = new EndianIo(data, EndianType.BigEndian);

        public XACH(string filePath) => this.io = new EndianIo(filePath, EndianType.BigEndian);

        public virtual void Read(EndianReader er, XSTR xstr, string lang)
        {
            this.magic = er.ReadInt32();
            if (this.magic != 1480672072)
                throw new Exception("Invalid magic!");
            this.version = er.ReadInt32();
            this.size = er.ReadInt32();
            short num1 = er.ReadInt16();
            for (int index = 0; index < (int)num1; ++index)
            {
                short num2 = er.ReadInt16();
                ushort stringId1 = er.ReadUInt16();
                ushort stringId2 = er.ReadUInt16();
                ushort stringId3 = er.ReadUInt16();
                int id = er.ReadInt32();
                short num3 = er.ReadInt16();
                int num4 = (int)er.ReadInt16();
                er.ReadInt32();
                er.ReadInt32();
                er.ReadInt32();
                er.ReadInt32();
                er.ReadInt32();
                string stringId4 = xstr.GetStringId(stringId1);
                string stringId5 = xstr.GetStringId(stringId2);
                string stringId6 = xstr.GetStringId(stringId3);
                InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
                {
                    type = nameof(XACH),
                    folder = $"{lang}\\{num2.ToString("X")} - {stringId4}",
                    fileName = "Description : " + stringId5,
                    Data = new byte[0]
                });
                InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
                {
                    type = nameof(XACH),
                    folder = $"{lang}\\{num2.ToString("X")} - {stringId4}",
                    fileName = "Unachieved : " + stringId6,
                    Data = new byte[0]
                });
                InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
                {
                    type = nameof(XACH),
                    folder = $"{lang}\\{num2.ToString("X")} - {stringId4}",
                    fileName = "Gamercred : " + (object)num3,
                    Data = new byte[0]
                });
                foreach (FileEntry file in InnerFileStructure.getInstance().getFiles("XDBF", "Png\\Achievement", (long)id, true))
                    InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
                    {
                        type = nameof(XACH),
                        folder = $"{lang}\\{num2.ToString("X")} - {stringId4}",
                        fileName = file.fileName,
                        Data = file.Data
                    });
            }
        }
    }
}