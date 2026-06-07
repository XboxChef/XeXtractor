using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace XeXtractor
{

    public class XDBF
    {
        protected int entryCurrent;
        protected int entryMax;
        protected List<XDBF.EntryData> entryTable = new List<XDBF.EntryData>();
        protected int freeCurrent = 1;
        protected int freeMax;
        protected List<XDBF.FileLoc> freeTable = new List<XDBF.FileLoc>();
        protected EndianIo io;
        protected int magic = 1480868422;
        protected int version = 65536 /*0x010000*/;
        protected long startpos;

        public XDBF(EndianIo io) => this.io = io;

        public XDBF(byte[] data) => this.io = new EndianIo(data, EndianType.BigEndian);

        public XDBF(string filePath) => this.io = new EndianIo(filePath, EndianType.BigEndian);

        public int Magic
        {
            get => this.magic;
            set => this.magic = value;
        }

        public int Version
        {
            get => this.version;
            set => this.version = value;
        }

        public int EntryMax
        {
            get => this.entryMax;
            set => this.entryMax = value;
        }

        public int EntryCurrent
        {
            get => this.entryCurrent;
            set => this.entryCurrent = value;
        }

        public int FreeMax
        {
            get => this.freeMax;
            set => this.freeMax = value;
        }

        public int FreeCurrent
        {
            get => this.freeCurrent;
            set => this.freeCurrent = value;
        }

        public List<XDBF.EntryData> EntryTable
        {
            get => this.entryTable;
            set => this.entryTable = value;
        }

        public List<XDBF.FileLoc> FreeTable
        {
            get => this.freeTable;
            set => this.freeTable = value;
        }

        public int HeaderSize => 24 + this.entryMax * 18 + this.freeMax * 8;

        public EndianIo Io => this.io;

        public void Open() => this.io.Open();

        public void Close() => this.io.Close();

        public virtual void Read() => this.Read(this.io.In);

        public virtual void Read(EndianReader er)
        {
            this.version = (this.magic = er.ReadInt32()) == 1480868422 ? er.ReadInt32() : throw new Exception("Invalid magic!");
            this.entryMax = er.ReadInt32();
            this.entryCurrent = er.ReadInt32();
            this.freeMax = er.ReadInt32();
            this.freeCurrent = er.ReadInt32();
            for (int index = 0; index < this.entryMax; ++index)
                this.entryTable.Add(new XDBF.EntryData(er));
            for (int index = 0; index < this.freeCurrent; ++index)
                this.freeTable.Add(new XDBF.FileLoc(er));
            for (int index = 0; index < this.freeMax - this.freeCurrent; ++index)
            {
                XDBF.FileLoc fileLoc = new XDBF.FileLoc(er);
            }
            this.startpos = er.BaseStream.Position;
        }

        public string ExtractFileLoc(string folder)
        {
            string fileLoc1 = "";
            string str = Path.Combine(folder, "FileLoc");
            EndianReader endianReader = this.io.In;
            for (int index = 0; index < this.freeTable.Count; ++index)
            {
                XDBF.FileLoc fileLoc2 = this.freeTable[index];
                if (fileLoc2.Size != 0U)
                {
                    endianReader.SeekTo((long)fileLoc2.Offset + this.startpos);
                    byte[] bytes = endianReader.ReadBytes((int)fileLoc2.Size);
                    if (!Directory.Exists(str))
                        Directory.CreateDirectory(str);
                    fileLoc1 = $"{fileLoc1}Extracted FileLoc\\{index.ToString()}.bin";
                    File.WriteAllBytes(Path.Combine(str, index.ToString() + ".bin"), bytes);
                }
            }
            return fileLoc1;
        }

        private string StringIdtoString(long id)
        {
            string str = id.ToString("X");
            switch (id)
            {
                case 1:
                    str = "en-US";
                    break;
                case 2:
                    str = "ja-JP";
                    break;
                case 3:
                    str = "de-DE";
                    break;
                case 4:
                    str = "fr-FR";
                    break;
                case 5:
                    str = "es-ES";
                    break;
                case 6:
                    str = "it-IT";
                    break;
                case 7:
                    str = "ko-KR";
                    break;
                case 8:
                    str = "zh-TW";
                    break;
                case 9:
                    str = "pt-BR";
                    break;
                case 10:
                    str = "zh-CN";
                    break;
                case 11:
                    str = "pl-PL";
                    break;
                case 12:
                    str = "ru-RU";
                    break;
            }
            return str;
        }

        public void ExtractEntryData()
        {
            Log.getInstance().AddEntry("Extract XDBF");
            EndianReader endianReader = this.io.In;
            for (int index1 = 0; index1 < this.entryTable.Count; ++index1)
            {
                XDBF.EntryData entryData = this.entryTable[index1];
                if (entryData.Namespace != (short)2)
                {
                    if (entryData.Namespace == (short)3)
                    {
                        endianReader.SeekTo((long)entryData.Offset + this.startpos);
                        byte[] numArray = endianReader.ReadBytes(entryData.Size);
                        string str = this.StringIdtoString(entryData.ID) + ".xstr";
                        Log.getInstance().AddEntry("Extract Strings\\" + str);
                        InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
                        {
                            fileName = str,
                            folder = "Strings",
                            type = nameof(XDBF),
                            Data = numArray,
                            id = entryData.ID
                        });
                    }
                    else
                    {
                        endianReader.SeekTo((long)entryData.Offset + this.startpos);
                        byte[] bytes = endianReader.ReadBytes(entryData.Size);
                        this.io.Out.SeekTo((long)entryData.Offset + this.startpos);
                        string str1 = entryData.ID.ToString("X") + ".";
                        string str2 = Encoding.UTF8.GetString(bytes, 0, 4);
                        string str3 = "";
                        for (int index2 = 0; index2 < str2.Length; ++index2)
                        {
                            if (char.IsLetter(str2[index2]))
                                str3 += (string)(object)str2[index2];
                        }
                        string str4 = str1 + str3;
                        Log.getInstance().AddEntry("Extract Bins\\" + str4);
                        InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
                        {
                            fileName = str4,
                            folder = "Bins",
                            type = nameof(XDBF),
                            Data = bytes,
                            id = entryData.ID
                        });
                    }
                }
                else
                {
                    endianReader.SeekTo((long)entryData.Offset + this.startpos);
                    byte[] numArray = endianReader.ReadBytes(entryData.Size);
                    string str5 = this.pngIdToName(entryData.ID) + ".png";
                    Log.getInstance().AddEntry("Extract Png\\" + str5);
                    string str6 = "";
                    int length = str5.LastIndexOf("\\");
                    string str7;
                    if (length != -1)
                    {
                        str7 = str5.Substring(length + 1);
                        str6 = str5.Substring(0, length);
                    }
                    else
                        str7 = str5;
                    InnerFileStructure.getInstance().AddFileEntry(new FileEntry()
                    {
                        fileName = str7,
                        folder = "Png\\" + str6,
                        type = nameof(XDBF),
                        Data = numArray,
                        id = entryData.ID
                    });
                }
            }
            Log.getInstance().AddSeperator();
        }

        public string pngIdToName(long id)
        {
            return id != 32768L /*0x8000*/ ? (id >= 32768L /*0x8000*/ ? (id <= 131072L /*0x020000*/ ? (id <= 65536L /*0x010000*/ ? "Unknow\\" + id.ToString("X") : "GamerPic\\Small-" + id.ToString("X")) : "GamerPic\\Large-" + id.ToString("X")) : "Achievement\\" + id.ToString("X")) : "GameIcon\\" + id.ToString("X");
        }

        public virtual void Write() => this.Write(this.io.Out);

        public virtual void Write(EndianWriter ew)
        {
            ew.Write(this.magic);
            ew.Write(this.version);
            ew.Write(this.entryMax);
            ew.Write(this.entryCurrent);
            ew.Write(this.freeMax);
            ew.Write(this.freeCurrent);
            for (int index = 0; index < this.entryMax; ++index)
                this.entryTable[index].Write(ew);
            for (int index = 0; index < this.freeCurrent; ++index)
                this.freeTable[index].Write(ew);
        }

        public class EntryData
        {
            private long id;
            private short ns;
            private int offset;
            private int size;

            public EntryData()
            {
            }

            public EntryData(EndianReader er) => this.Read(er);

            public short NamespaceShort
            {
                get => this.ns;
                set => this.ns = value;
            }

            public bool IsEmpty => this.ns == (short)0 || this.offset == 0 || this.size == 0;

            public long ID
            {
                get => this.id;
                set => this.id = value;
            }

            public int Offset
            {
                get => this.offset;
                set => this.offset = value;
            }

            public int Size
            {
                get => this.size;
                set => this.size = value;
            }

            public void Null()
            {
                this.ns = (short)0;
                this.offset = 0;
                this.size = 0;
            }

            public short Namespace
            {
                get => this.ns;
                set => this.ns = value;
            }

            public void Read(EndianReader er)
            {
                this.ns = er.ReadInt16();
                this.id = er.ReadInt64();
                this.offset = er.ReadInt32();
                this.size = er.ReadInt32();
            }

            public void Write(EndianWriter ew)
            {
                ew.Write(this.ns);
                ew.Write(this.id);
                ew.Write(this.offset);
                ew.Write(this.size);
            }
        }

        public class FileLoc
        {
            private int offset;
            private uint size;

            public FileLoc()
            {
            }

            public FileLoc(EndianReader er) => this.Read(er);

            public int Offset
            {
                get => this.offset;
                set => this.offset = value;
            }

            public uint Size
            {
                get => this.size;
                set => this.size = value;
            }

            public void Read(EndianReader er)
            {
                this.offset = er.ReadInt32();
                this.size = er.ReadUInt32();
            }

            public void Write(EndianWriter ew)
            {
                ew.Write(this.offset);
                ew.Write(this.size);
            }

            public override string ToString() => $"0x{this.offset:X} - 0x{this.size:X}";
        }
    }
}