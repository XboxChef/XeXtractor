using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;


namespace XeXtractor
{

    public class XEX2
    {
        private readonly byte[] devkitKey = new byte[16 /*0x10*/]
        {
    (byte) 168,
    (byte) 176 /*0xB0*/,
    (byte) 5,
    (byte) 18,
    (byte) 237,
    (byte) 227,
    (byte) 99,
    (byte) 141,
    (byte) 198,
    (byte) 88,
    (byte) 179,
    (byte) 16 /*0x10*/,
    (byte) 31 /*0x1F*/,
    (byte) 159,
    (byte) 80 /*0x50*/,
    (byte) 209
        };
        private readonly byte[] devkitKey2 = new byte[16 /*0x10*/];
        private readonly byte[] retailKey = new byte[16 /*0x10*/]
        {
    (byte) 162,
    (byte) 108,
    (byte) 16 /*0x10*/,
    (byte) 247,
    (byte) 31 /*0x1F*/,
    (byte) 217,
    (byte) 53,
    (byte) 233,
    (byte) 139,
    (byte) 153,
    (byte) 146,
    (byte) 44,
    (byte) 233,
    (byte) 50,
    (byte) 21,
    (byte) 114
        };
        private readonly byte[] retailKey2 = new byte[16 /*0x10*/]
        {
    (byte) 32 /*0x20*/,
    (byte) 177,
    (byte) 133,
    (byte) 165,
    (byte) 157,
    (byte) 40,
    (byte) 253,
    (byte) 195,
    (byte) 64 /*0x40*/,
    (byte) 88,
    (byte) 63 /*0x3F*/,
    (byte) 187,
    (byte) 8,
    (byte) 150,
    (byte) 191,
    (byte) 145
        };
        private static readonly Dictionary<XEX2.ImageKeys, Type> HeaderInfo = new Dictionary<XEX2.ImageKeys, Type>();
        public XEX2.XexHeader Header;
        public Dictionary<XEX2.ImageKeys, XEX2.OptionalHeader> OptionalHeaders;
        public XEX2.XexSecurityInfo SecurityInfo;
        public XEX2.XexSectionTable SectionTable;
        private MemoryStream ms = new MemoryStream();

        public EndianIo Io { get; private set; }

        public bool OptionalHeaderExists(XEX2.ImageKeys key) => this.OptionalHeaders.ContainsKey(key);

        public void RemoveOptionalHeader(XEX2.ImageKeys key)
        {
            if (!this.OptionalHeaderExists(key))
                return;
            this.OptionalHeaders.Remove(key);
        }

        public void CreateOptionalHeader(XEX2.ImageKeys key)
        {
            if (!this.OptionalHeaderExists(key))
                return;
            XEX2.OptionalHeader optionalHeader = this.GetOptionalHeader(key, 0);
            this.OptionalHeaders.Add(key, optionalHeader);
        }

        public XEX2.OriginalPEName PEName
        {
            get
            {
                return !this.OptionalHeaderExists(XEX2.ImageKeys.OriginalPEImageName) ? (XEX2.OriginalPEName)null : (XEX2.OriginalPEName)this.OptionalHeaders[XEX2.ImageKeys.OriginalPEImageName];
            }
        }

        static XEX2()
        {
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.ResourceInfo, typeof(XEX2.XexResources));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.BaseFileFormat, typeof(XEX2.BaseFileFormat));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.DeltaPatchDescriptor, typeof(XEX2.DeltaPatchDescriptor));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.BaseReference, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.OriginalBaseAddress, typeof(XEX2.BaseFileAddress));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.EntryPoint, typeof(XEX2.BaseFileEntryPoint));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.ImageBaseAddress, typeof(XEX2.BaseFileAddress));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.ImportLibraries, typeof(XEX2.ImportLibraries));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.ImageChecksumTimestamp, typeof(XEX2.BaseFileChecksumAndTimeStamp));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.EnabledForCallcap, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.EnabledForFastcap, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.OriginalPEImageName, typeof(XEX2.OriginalPEName));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.StaticLibraries, typeof(XEX2.StaticLibraries));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.TLSInfo, typeof(XEX2.TLSInfo));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.DefaultStackSize, typeof(XEX2.BaseFileDefaultStackSize));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.DefaultFilesystemCacheSize, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.DefaultHeapSize, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.PageHeapSizeAndflags, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.SystemFlags, typeof(XEX2.SystemFlags));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.ExecutionID, typeof(XEX2.ExecutionId));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.TitleWorkspaceSize, typeof(XEX2.WorkspaceSize));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.GameRatingsSpecified, typeof(XEX2.GameRatings));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.LANKey, typeof(XEX2.LANKey));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.IncludesXbox360Logo, typeof(XEX2.Xbox360Logo));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.MultidiscMediaIDs, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.AlternateTitleIDs, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.AdditionalTitleMemory, (Type)null);
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.BoundingPath, typeof(XEX2.BoundingPath));
            XEX2.HeaderInfo.Add(XEX2.ImageKeys.IncludesExportsByName, (Type)null);
        }

        public XEX2(byte[] data) => this.Io = new EndianIo(data, EndianType.BigEndian);

        public XEX2(EndianIo io) => this.Io = io;

        public XEX2(string filePath) => this.Io = new EndianIo(filePath, EndianType.BigEndian);

        public void Open() => this.Io.Open(FileMode.Open);

        public void Close() => this.Io.Close();

        public void Read()
        {
            this.Io.SeekTo(0);
            this.Header.Read(this.Io.In);
            this.OptionalHeaders = new Dictionary<XEX2.ImageKeys, XEX2.OptionalHeader>();
            for (int index = 0; index < this.Header.OptionalHeaderEntries; ++index)
            {
                XEX2.ImageKeys key = (XEX2.ImageKeys)this.Io.In.ReadInt32();
                XEX2.OptionalHeader optionalHeader = this.GetOptionalHeader(key, this.Io.In.ReadInt32());
                this.OptionalHeaders.Add(key, optionalHeader);
            }
            this.Io.SeekTo(this.Header.SecurityInfoOffset);
            this.SecurityInfo.Read(this.Io.In);
            this.SectionTable.Read(this.Io.In);
            foreach (KeyValuePair<XEX2.ImageKeys, XEX2.OptionalHeader> optionalHeader in this.OptionalHeaders)
                optionalHeader.Value.Read(this.Io.In);
            this.DecryptRsa();
        }

        public void Write(Stream outStream)
        {
            EndianWriter ew = new EndianWriter(outStream, EndianType.BigEndian);
            int num1 = this.Header.SizeOf + this.OptionalHeaders.Count * 8;
            if (this.Header.SecurityInfoOffset < num1)
                this.Header.SecurityInfoOffset = num1;
            ew.SeekTo(this.Header.SecurityInfoOffset);
            this.SecurityInfo.Write(ew);
            this.SectionTable.Write(ew);
            foreach (KeyValuePair<XEX2.ImageKeys, XEX2.OptionalHeader> optionalHeader in this.OptionalHeaders)
            {
                if (optionalHeader.Key != XEX2.ImageKeys.ImportLibraries)
                    optionalHeader.Value.Write(ew);
            }
            if (this.OptionalHeaders.ContainsKey(XEX2.ImageKeys.ImportLibraries))
            {
                int num2 = this.OptionalHeaders[XEX2.ImageKeys.ImportLibraries].SizeOf();
                int length = ((int)outStream.Position + num2) % 4096 /*0x1000*/;
                if (length != 0)
                    length = 4096 /*0x1000*/ - length;
                ew.Write(new byte[length]);
                this.OptionalHeaders[XEX2.ImageKeys.ImportLibraries].Write(ew);
            }
            this.Header.DataOffset = (int)outStream.Position;
            ew.SeekTo(0);
            this.Header.Write(ew);
            foreach (KeyValuePair<XEX2.ImageKeys, XEX2.OptionalHeader> optionalHeader in this.OptionalHeaders)
            {
                ew.Write((int)optionalHeader.Value.ImageKey);
                ew.Write(optionalHeader.Value.Data);
            }
            byte[] numArray = new byte[this.Header.DataOffset];
            outStream.Seek(0L, SeekOrigin.Begin);
            outStream.Read(numArray, 0, this.Header.DataOffset);
            SHA1 shA1 = SHA1.Create();
            int inputOffset = this.Header.SecurityInfoOffset + 380;
            shA1.TransformBlock(numArray, inputOffset, this.Header.DataOffset - inputOffset, (byte[])null, 0);
            byte[] hash = shA1.ComputeHash(numArray, 0, this.Header.SecurityInfoOffset + 8);
            ew.SeekTo(this.Header.SecurityInfoOffset + 356);
            ew.Write(hash);
        }

        public void DecryptRsa()
        {
        }

        public void ExtractAllRessource()
        {
            XEX2.XexResources optionalHeader = (XEX2.XexResources)this.OptionalHeaders[XEX2.ImageKeys.ResourceInfo];
            for (int resourceIndex = 0; resourceIndex < optionalHeader.Resources.Length; ++resourceIndex)
                this.ExtractResource(resourceIndex, optionalHeader.Resources[resourceIndex].Name);
        }

        public bool ExtractSpa()
        {
            return this.OptionalHeaders.ContainsKey(XEX2.ImageKeys.ExecutionID) && this.ExtractResource(((XEX2.ExecutionId)this.OptionalHeaders[XEX2.ImageKeys.ExecutionID]).TitleId.ToString("X8"));
        }

        public bool ExtractSpa(Stream spaStream)
        {
            return this.OptionalHeaders.ContainsKey(XEX2.ImageKeys.ExecutionID) && this.ExtractResource(((XEX2.ExecutionId)this.OptionalHeaders[XEX2.ImageKeys.ExecutionID]).TitleId.ToString("X8"), spaStream);
        }

        public bool ExtractResource(string resourceName)
        {
            if (!this.OptionalHeaders.ContainsKey(XEX2.ImageKeys.ResourceInfo))
                return false;
            XEX2.XexResources optionalHeader = (XEX2.XexResources)this.OptionalHeaders[XEX2.ImageKeys.ResourceInfo];
            for (int resourceIndex = 0; resourceIndex < optionalHeader.Resources.Length; ++resourceIndex)
            {
                if (optionalHeader.Resources[resourceIndex].Name == resourceName)
                    return this.ExtractResource(resourceIndex, resourceName);
            }
            return false;
        }

        public bool ExtractResource(string resourceName, Stream resourceStream)
        {
            if (!this.OptionalHeaders.ContainsKey(XEX2.ImageKeys.ResourceInfo))
                return false;
            XEX2.XexResources optionalHeader = (XEX2.XexResources)this.OptionalHeaders[XEX2.ImageKeys.ResourceInfo];
            for (int resourceIndex = 0; resourceIndex < optionalHeader.Resources.Length; ++resourceIndex)
            {
                if (optionalHeader.Resources[resourceIndex].Name == resourceName)
                    return this.ExtractResource(resourceIndex, resourceStream);
            }
            return false;
        }

        public bool ExtractResource(int resourceIndex, string name)
        {
            MemoryStream resourceStream = new MemoryStream();
            if (!this.ExtractResource(resourceIndex, (Stream)resourceStream))
                return false;
            FileEntry entr = new FileEntry();
            entr.Data = resourceStream.ToArray();
            entr.fileName = name + ".";
            FileHandler.GetFileType(entr.Data);
            string fileType = FileHandler.GetFileType(entr.Data);
            string str = "";
            if (entr.Data[0] == (byte)59 && entr.Data[1] == (byte)32 /*0x20*/ || entr.Data[0] == (byte)13 && entr.Data[1] == (byte)10 && entr.Data[2] == (byte)59 && entr.Data[3] == (byte)32 /*0x20*/)
                str = "ini";
            else if (entr.Data[0] == (byte)91 || entr.Data[0] == (byte)35)
                str = "ini";
            else if (fileType.StartsWith("<"))
            {
                str = "xml";
            }
            else
            {
                for (int index = 0; index < fileType.Length; ++index)
                {
                    if (char.IsLetter(fileType[index]))
                        str += (string)(object)fileType[index];
                }
            }
            if (str == "")
                str = "bin";
            entr.fileName += str;
            entr.type = "Resources";
            InnerFileStructure.getInstance().AddFileEntry(entr);
            resourceStream.Close();
            return true;
        }

        public void DecryptBaseFile() => this.DecryptBaseFile((Stream)this.ms);

        public byte[] getBaseFile() => this.ms.ToArray();

        public bool ExtractResource(int resourceIndex, Stream resourceStream)
        {
            if (!this.OptionalHeaders.ContainsKey(XEX2.ImageKeys.ResourceInfo))
                return false;
            XEX2.XexResources optionalHeader = (XEX2.XexResources)this.OptionalHeaders[XEX2.ImageKeys.ResourceInfo];
            if (resourceIndex > optionalHeader.Resources.Length)
                return false;
            XEX2.XexResources.Resource resource = optionalHeader.Resources[resourceIndex];
            this.ms.Seek((long)(resource.Address - this.SecurityInfo.LoadAddress), SeekOrigin.Begin);
            byte[] buffer = new byte[resource.Size];
            this.ms.Read(buffer, 0, resource.Size);
            resourceStream.Write(buffer, 0, resource.Size);
            resourceStream.Seek(0L, SeekOrigin.Begin);
            return true;
        }

        public void GetXexHeader(Stream outStream)
        {
            this.Io.SeekTo(0);
            outStream.Write(this.Io.In.ReadBytes(this.Header.DataOffset), 0, this.Header.DataOffset);
            outStream.Seek(0L, SeekOrigin.Begin);
        }

        public bool DecrpytBaseFile(string filePath)
        {
            FileStream outBaseFile = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            if (!this.DecryptBaseFile((Stream)outBaseFile))
                return false;
            outBaseFile.Close();
            return true;
        }

        public bool DecryptBaseFile(Stream outBaseFile)
        {
            Stream stream = (Stream)new MemoryStream();
            XEX2.BaseFileFormat optionalHeader = (XEX2.BaseFileFormat)this.OptionalHeaders[XEX2.ImageKeys.BaseFileFormat];
            this.Io.SeekTo(this.Header.DataOffset);
            if (optionalHeader.EncryptionType == XEX2.BaseFileFormat.EncryptionTypes.Encrypted)
            {
                Rijndael rijndael1 = Rijndael.Create();
                rijndael1.Padding = PaddingMode.None;
                MemoryStream memoryStream = new MemoryStream(this.SecurityInfo.AesKey);
                byte[] numArray = new byte[16 /*0x10*/];
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, rijndael1.CreateDecryptor(this.retailKey2, new byte[16 /*0x10*/]), CryptoStreamMode.Read))
                    cryptoStream.Read(numArray, 0, 16 /*0x10*/);
                Rijndael rijndael2 = Rijndael.Create();
                rijndael2.Padding = PaddingMode.None;
                using (CryptoStream cryptoStream = new CryptoStream(this.Io.Stream, rijndael2.CreateDecryptor(numArray, new byte[16 /*0x10*/]), CryptoStreamMode.Read))
                {
                    if (optionalHeader.CompressionType == XEX2.BaseFileFormat.CompressionTypes.NotCompressed)
                    {
                        XEX2.RawBaseFile rawFormat = optionalHeader.RawFormat;
                        for (int index = 0; index < rawFormat.Blocks.Length; ++index)
                        {
                            byte[] buffer = new byte[rawFormat.Blocks[index].DataSize];
                            cryptoStream.Read(buffer, 0, buffer.Length);
                            outBaseFile.Write(buffer, 0, buffer.Length);
                            outBaseFile.Write(new byte[rawFormat.Blocks[index].ZeroSize], 0, rawFormat.Blocks[index].ZeroSize);
                        }
                        outBaseFile.Seek(0L, SeekOrigin.Begin);
                        outBaseFile.SetLength((long)this.SecurityInfo.ImageSize);
                        return true;
                    }
                    XEX2.CompressedBaseFile.CompBaseFileBlock block = optionalHeader.CompressedFormat.Block;
                    int num = 0;
                    while (block.DataSize != 0)
                    {
                        this.Io.Stream.Read(new byte[block.DataSize], 0, block.DataSize);
                        this.Io.Stream.Seek((long)(block.DataSize * -1), SeekOrigin.Current);
                        byte[] buffer = new byte[block.DataSize];
                        cryptoStream.Read(buffer, 0, buffer.Length);
                        byte[] hash = SHA1.Create().ComputeHash(buffer);
                        for (int index = 0; index < 20; ++index)
                        {
                            if ((int)hash[index] != (int)block.Hash[index])
                                throw new Exception("Bad hash");
                        }
                        stream.Write(buffer, 0, block.DataSize);
                        EndianReader er = new EndianReader((Stream)new MemoryStream(buffer), EndianType.BigEndian);
                        block.Read(er);
                        ++num;
                    }
                    stream.Seek(0L, SeekOrigin.Begin);
                }
            }
            else if (optionalHeader.CompressionType == XEX2.BaseFileFormat.CompressionTypes.NotCompressed)
            {
                XEX2.RawBaseFile rawFormat = optionalHeader.RawFormat;
                for (int index = 0; index < rawFormat.Blocks.Length; ++index)
                {
                    byte[] buffer = this.Io.In.ReadBytes(rawFormat.Blocks[index].DataSize);
                    outBaseFile.Write(buffer, 0, buffer.Length);
                    outBaseFile.Write(new byte[rawFormat.Blocks[index].ZeroSize], 0, rawFormat.Blocks[index].ZeroSize);
                }
                outBaseFile.Seek(0L, SeekOrigin.Begin);
                outBaseFile.SetLength((long)this.SecurityInfo.ImageSize);
                return true;
            }
            int imageSize = this.SecurityInfo.ImageSize;
            XEX2.CompressedBaseFile.CompBaseFileBlock block1 = optionalHeader.CompressedFormat.Block;
            if (optionalHeader.CompressionType == XEX2.BaseFileFormat.CompressionTypes.DeltaCompressed)
            {
                while (block1.DataSize != 0)
                {
                    byte[] buffer = new byte[block1.DataSize];
                    stream.Read(buffer, 0, buffer.Length);
                    outBaseFile.Write(buffer, 0, block1.DataSize);
                    EndianReader er = new EndianReader((Stream)new MemoryStream(buffer), EndianType.BigEndian);
                    block1.Read(er);
                }
                outBaseFile.Seek(0L, SeekOrigin.Begin);
                return true;
            }
            int pcbDataBlockMax = 32768 /*0x8000*/;
            int ldiContext = -1;
            int unknown = 0;
            XCompress.LzxDecompress pvConfiguration;
            pvConfiguration.CpuType = 1L;
            pvConfiguration.WindowSize = (long)optionalHeader.CompressedFormat.CompressionWindow;
            IntPtr num1 = Marshal.AllocHGlobal(2097152 /*0x200000*/);
            if (XCompress.LDICreateDecompression(ref pcbDataBlockMax, ref pvConfiguration, 0, 0, num1, ref unknown, ref ldiContext) != 0)
                throw new Exception("Failed to create decompression");
            int num2 = 0;
            while (block1.DataSize != 0)
            {
                byte[] buffer = new byte[block1.DataSize];
                stream.Read(buffer, 0, buffer.Length);
                EndianReader er = new EndianReader((Stream)new MemoryStream(buffer), EndianType.BigEndian);
                block1.Read(er);
                int num3 = 0;
                while (true)
                {
                    uint num4 = (uint)er.ReadUInt16();
                    if (num4 != 0U)
                    {
                        byte[] pbSrc = er.ReadBytes((int)num4);
                        int pcbDecompressed = imageSize < pcbDataBlockMax ? imageSize : pcbDataBlockMax;
                        byte[] numArray = new byte[pcbDecompressed];
                        if (XCompress.LDIDecompress(ldiContext, pbSrc, (int)num4, numArray, ref pcbDecompressed) == 0)
                        {
                            if (pcbDecompressed != 32768 /*0x8000*/)
                                Console.Write("pause");
                            ++num3;
                            outBaseFile.Write(numArray, 0, pcbDecompressed);
                            imageSize -= pcbDecompressed;
                        }
                        else
                            break;
                    }
                    else
                        goto label_43;
                }
                throw new Exception("Failed to decompress");
            label_43:
                ++num2;
            }
            if (XCompress.LDIDestroyDecompression(ldiContext) != 0)
                throw new Exception("Failed to destroy decompression");
            Marshal.FreeHGlobal(num1);
            outBaseFile.Seek(0L, SeekOrigin.Begin);
            outBaseFile.SetLength((long)this.SecurityInfo.ImageSize);
            return true;
        }

        public bool EncryptBaseFile(Stream outBaseFile, Stream inBaseFile) => false;

        public bool ApplyPatch(Stream outStream, XEX2 patch)
        {
            XEX2.DeltaPatchDescriptor optionalHeader = (XEX2.DeltaPatchDescriptor)patch.OptionalHeaders[XEX2.ImageKeys.DeltaPatchDescriptor];
            MemoryStream outBaseFile = new MemoryStream();
            if (!patch.DecryptBaseFile((Stream)outBaseFile))
                return false;
            MemoryStream memoryStream1 = new MemoryStream();
            this.GetXexHeader((Stream)memoryStream1);
            MemoryStream memoryStream2 = new MemoryStream();
            if (!this.DecryptBaseFile((Stream)memoryStream2))
                return false;
            int pcbDataBlockMax = 32768 /*0x8000*/;
            int ldiContext = -1;
            int unknown = 0;
            XCompress.LzxDecompress pvConfiguration;
            pvConfiguration.CpuType = 1L;
            pvConfiguration.WindowSize = (long)pcbDataBlockMax;
            IntPtr num = Marshal.AllocHGlobal(176640);
            if (XCompress.LDICreateDecompression(ref pcbDataBlockMax, ref pvConfiguration, 0, 0, num, ref unknown, ref ldiContext) != 0)
                throw new Exception("Failed to create decompression");
            this.DeltaDecompress(ldiContext, (Stream)memoryStream1, optionalHeader.DeltaHeaderPatchData);
            XCompress.LDIResetDecompression(ldiContext);
            XEX2.CompressedBaseFile.CompBaseFileBlock block = ((XEX2.BaseFileFormat)patch.OptionalHeaders[XEX2.ImageKeys.BaseFileFormat]).CompressedFormat.Block;
            while (block.DataSize > 0)
            {
                EndianReader er = new EndianReader((Stream)outBaseFile, EndianType.BigEndian);
                int count = block.DataSize - 24;
                block.Read(er);
                this.DeltaDecompress(ldiContext, (Stream)memoryStream2, er.ReadBytes(count));
            }
            if (XCompress.LDIDestroyDecompression(ldiContext) != 0)
                throw new Exception("Failed to destroy decompression");
            Marshal.FreeHGlobal(num);
            memoryStream2.Seek(0L, SeekOrigin.Begin);
            outStream.Seek(0L, SeekOrigin.Begin);
            byte[] buffer = new byte[memoryStream2.Length];
            memoryStream2.Read(buffer, 0, (int)memoryStream2.Length);
            outStream.Write(buffer, 0, (int)memoryStream2.Length);
            outStream.Seek(0L, SeekOrigin.Begin);
            return true;
        }

        public void DeltaDecompress(int ldiContext, Stream baseFile, byte[] patchData)
        {
            EndianReader er = new EndianReader((Stream)new MemoryStream(patchData), EndianType.BigEndian);
            XEX2.DeltaPatch deltaPatch = new XEX2.DeltaPatch();
            for (int length = patchData.Length; length > 12; length -= deltaPatch.Size)
            {
                deltaPatch.Read(er);
                if (deltaPatch.UncompressedLen == (short)0)
                    break;
                if (deltaPatch.CompressedLen == (short)0)
                {
                    baseFile.Seek((long)deltaPatch.YPos, SeekOrigin.Begin);
                    baseFile.Write(new byte[(int)deltaPatch.UncompressedLen], 0, (int)deltaPatch.UncompressedLen);
                }
                else if (deltaPatch.CompressedLen == (short)1)
                {
                    byte[] buffer = new byte[(int)deltaPatch.UncompressedLen];
                    baseFile.Seek((long)deltaPatch.XPos, SeekOrigin.Begin);
                    baseFile.Read(buffer, 0, (int)deltaPatch.UncompressedLen);
                    baseFile.Seek((long)deltaPatch.YPos, SeekOrigin.Begin);
                    baseFile.Write(buffer, 0, (int)deltaPatch.UncompressedLen);
                }
                else
                {
                    int uncompressedLen = (int)deltaPatch.UncompressedLen;
                    byte[] numArray = new byte[uncompressedLen];
                    baseFile.Seek((long)deltaPatch.XPos, SeekOrigin.Begin);
                    baseFile.Read(numArray, 0, uncompressedLen);
                    if (XCompress.LDISetWindowData(ldiContext, numArray, uncompressedLen) != 0)
                        throw new Exception("Failed to set window data");
                    if (deltaPatch.Decompress(ldiContext) != 0)
                        throw new Exception("Failed to decompress");
                    baseFile.Seek((long)deltaPatch.YPos, SeekOrigin.Begin);
                    baseFile.Write(deltaPatch.DecompressedData, 0, (int)deltaPatch.UncompressedLen);
                    if (XCompress.LDIResetDecompression(ldiContext) != 0)
                        throw new Exception("Failed reset decompression");
                }
            }
        }

        public string OutputInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Xex Info");
            XEX2.BaseFileFormat optionalHeader1 = (XEX2.BaseFileFormat)this.OptionalHeaders[XEX2.ImageKeys.BaseFileFormat];
            stringBuilder.AppendFormat("  {0}\r\n", (object)optionalHeader1.CompressionType);
            stringBuilder.AppendFormat("  {0}\r\n", (object)optionalHeader1.EncryptionType);
            foreach (int num in (int[])Enum.GetValues(typeof(XEX2.ModuleFlag)))
            {
                if ((this.Header.ModuleFlags & (XEX2.ModuleFlag)num) == (XEX2.ModuleFlag)num)
                    stringBuilder.AppendFormat("  {0}\r\n", (object)Enum.GetName(typeof(XEX2.ModuleFlag), (object)num));
            }
            foreach (KeyValuePair<XEX2.ImageKeys, XEX2.OptionalHeader> optionalHeader2 in this.OptionalHeaders)
                stringBuilder.AppendFormat("{0}\r\n", (object)optionalHeader2.Value);
            stringBuilder.AppendFormat("Header Length: {0:X}\r\n", (object)this.SecurityInfo.HeaderLength);
            stringBuilder.AppendFormat("Image Size: {0:X}\r\n", (object)this.SecurityInfo.ImageSize);
            stringBuilder.AppendFormat("Length2: {0:X}\r\n", (object)this.SecurityInfo.Length2);
            stringBuilder.AppendFormat("Image Flags: {0}\r\n", (object)this.SecurityInfo.ImageFlags);
            stringBuilder.AppendFormat("Load Address: {0:X}\r\n", (object)this.SecurityInfo.LoadAddress);
            stringBuilder.AppendFormat("Section Table Digest: {0}\r\n", (object)XEX2.BytesToString(this.SecurityInfo.SectionDigest));
            stringBuilder.AppendFormat("Import Table Count: {0:X}\r\n", (object)this.SecurityInfo.ImportTableCount);
            stringBuilder.AppendFormat("Import Table Digest: {0}\r\n", (object)XEX2.BytesToString(this.SecurityInfo.ImportTableDigest));
            stringBuilder.AppendFormat("Media Id: {0}\r\n", (object)XEX2.BytesToString(this.SecurityInfo.Xgd2MediaId));
            stringBuilder.AppendFormat("AES Seed: {0}\r\n", (object)XEX2.BytesToString(this.SecurityInfo.AesKey));
            stringBuilder.AppendFormat("Export Table: {0:X}\r\n", (object)this.SecurityInfo.ExportTable);
            stringBuilder.AppendFormat("Header Hash: {0}\r\n", (object)XEX2.BytesToString(this.SecurityInfo.HeaderDigest));
            stringBuilder.AppendFormat("Game Region: {0}\r\n", (object)this.SecurityInfo.GameRegions);
            stringBuilder.AppendFormat("Media Types: {0}\r\n", (object)this.SecurityInfo.AllowedMediaTypes);
            if (this.OptionalHeaders.ContainsKey(XEX2.ImageKeys.ResourceInfo))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("XEX Resources");
                foreach (XEX2.XexResources.Resource resource in ((XEX2.XexResources)this.OptionalHeaders[XEX2.ImageKeys.ResourceInfo]).Resources)
                    stringBuilder.AppendLine(resource.ToString());
            }
            return stringBuilder.ToString();
        }

        public static string BytesToString(byte[] data)
        {
            string str = "";
            for (int index = 0; index < data.Length; ++index)
                str += data[index].ToString("X2");
            return str;
        }

        public XEX2.OptionalHeader GetOptionalHeader(XEX2.ImageKeys key, int data)
        {
            if (!XEX2.HeaderInfo.ContainsKey(key) || XEX2.HeaderInfo[key] == null)
                return new XEX2.OptionalHeader()
                {
                    ImageKey = key,
                    Data = data
                };
            XEX2.OptionalHeader instance = (XEX2.OptionalHeader)Activator.CreateInstance(XEX2.HeaderInfo[key], new object[0]);
            instance.ImageKey = key;
            instance.Data = data;
            return instance;
        }

        private static byte[] XeCryptRotSumSha(byte[] data, int size)
        {
            byte[] numArray = new byte[32 /*0x20*/];
            XEX2.XeCryptRotSum(data, 0, size / 8, numArray);
            SHA1 shA1 = SHA1.Create();
            shA1.TransformBlock(numArray, 0, 32 /*0x20*/, (byte[])null, 0);
            shA1.TransformBlock(numArray, 0, 32 /*0x20*/, (byte[])null, 0);
            shA1.TransformBlock(data, 0, size, (byte[])null, 0);
            for (int index = 0; index < 32 /*0x20*/; ++index)
                numArray[index] = numArray[index];
            shA1.TransformBlock(numArray, 0, 32 /*0x20*/, (byte[])null, 0);
            shA1.TransformFinalBlock(numArray, 0, 32 /*0x20*/);
            return shA1.Hash;
        }

        private static void XeCryptRotSum(byte[] data, int index, int size, byte[] output)
        {
            if (size == 0)
                return;
            for (int index1 = 0; index1 < 4; ++index1)
                Array.Reverse((Array)output, index1 * 8, 8);
            for (int index2 = 0; index2 < size; ++index2)
                Array.Reverse((Array)data, index2 * 8, 8);
            ulong num1 = BitConverter.ToUInt64(output, 0);
            ulong num2 = BitConverter.ToUInt64(output, 8);
            ulong uint64_1 = BitConverter.ToUInt64(output, 16 /*0x10*/);
            ulong num3 = BitConverter.ToUInt64(output, 24);
            int num4 = size;
            while (num4 > 0)
            {
                ulong uint64_2 = BitConverter.ToUInt64(data, index);
                ulong num5 = uint64_2 + num2;
                ulong num6 = num5 < uint64_2 ? 1UL : 0UL;
                ulong num7 = num3 - uint64_2;
                num1 = num6 + num1;
                num2 = num5 << 29 | num5 >> 35;
                ulong num8 = num7 > uint64_2 ? 1UL : 0UL;
                uint64_1 -= num8;
                num3 = num7 << 31 /*0x1F*/ | num7 >> 33;
                --num4;
                index += 8;
            }
            Array.Copy((Array)BitConverter.GetBytes(num1), 0, (Array)output, 0, 8);
            Array.Copy((Array)BitConverter.GetBytes(num2), 0, (Array)output, 8, 8);
            Array.Copy((Array)BitConverter.GetBytes(uint64_1), 0, (Array)output, 16 /*0x10*/, 8);
            Array.Copy((Array)BitConverter.GetBytes(num3), 0, (Array)output, 24, 8);
            for (int index3 = 0; index3 < 4; ++index3)
                Array.Reverse((Array)output, index3 * 8, 8);
            for (int index4 = 0; index4 < size; ++index4)
                Array.Reverse((Array)data, index4 * 8, 8);
        }

        public enum ImageKeys
        {
            ResourceInfo = 767, // 0x000002FF
            BaseFileFormat = 1023, // 0x000003FF
            BaseReference = 1029, // 0x00000405
            DeltaPatchDescriptor = 1535, // 0x000005FF
            BoundingPath = 33023, // 0x000080FF
            DeviceId = 33029, // 0x00008105
            OriginalBaseAddress = 65537, // 0x00010001
            EntryPoint = 65792, // 0x00010100
            ImageBaseAddress = 66049, // 0x00010201
            ImportLibraries = 66559, // 0x000103FF
            ImageChecksumTimestamp = 98306, // 0x00018002
            EnabledForCallcap = 98562, // 0x00018102
            EnabledForFastcap = 98816, // 0x00018200
            OriginalPEImageName = 99327, // 0x000183FF
            StaticLibraries = 131327, // 0x000200FF
            TLSInfo = 131332, // 0x00020104
            DefaultStackSize = 131584, // 0x00020200
            DefaultFilesystemCacheSize = 131841, // 0x00020301
            DefaultHeapSize = 132097, // 0x00020401
            PageHeapSizeAndflags = 163842, // 0x00028002
            SystemFlags = 196608, // 0x00030000
            ExecutionID = 262150, // 0x00040006
            TitleWorkspaceSize = 262657, // 0x00040201
            GameRatingsSpecified = 262928, // 0x00040310
            LANKey = 263172, // 0x00040404
            IncludesXbox360Logo = 263679, // 0x000405FF
            MultidiscMediaIDs = 263935, // 0x000406FF
            AlternateTitleIDs = 264191, // 0x000407FF
            AdditionalTitleMemory = 264193, // 0x00040801
            IncludesExportsByName = 14746626, // 0x00E10402
        }

        [Flags]
        public enum ModuleFlag
        {
            TitleModule = 1,
            ExportsToTitle = 2,
            SystemDebugger = 4,
            DllModule = 8,
            ModulePatch = 16, // 0x00000010
            PatchFull = 32, // 0x00000020
            PatchDelta = 64, // 0x00000040
            UserMode = 128, // 0x00000080
        }

        [Flags]
        public enum AllowedMediaType
        {
            HardDisk = 1,
            DvdX2 = 2,
            DvdCd = 4,
            Dvd5 = 8,
            Dvd9 = 16, // 0x00000010
            SystemFlash = 32, // 0x00000020
            MemoryUnit = 128, // 0x00000080
            MassStorageDevice = 256, // 0x00000100
            SmbFilesystem = 512, // 0x00000200
            DirectFromRam = 1024, // 0x00000400
            SecureVirtualOpticalDevice = 4096, // 0x00001000
            WirelessNStorageDevice = 8192, // 0x00002000
            InsecurePackage = 16777216, // 0x01000000
            SaveGamePackage = 33554432, // 0x02000000
            LocallySignedPackage = 67108864, // 0x04000000
            LiveSignedPackage = 134217728, // 0x08000000
            XboxPlatformPackage = 268435456, // 0x10000000
        }

        [Flags]
        public enum ImageFlag
        {
            RevocationCheckRequired = 1,
            ManufacturingUtility = 2,
            ManufacturingSupportTool = 4,
            ManufacturingAwareModule = ManufacturingSupportTool | ManufacturingUtility, // 0x00000006
            Xgd2MediaOnly = 8,
            CardeaKey = 256, // 0x00000100
            XeikaKey = 512, // 0x00000200
            TitleUserMode = 1024, // 0x00000400
            SystemUserMode = 2048, // 0x00000800
            Orange0 = 4096, // 0x00001000
            Orange1 = 8192, // 0x00002000
            Orange2 = 16384, // 0x00004000
            IptvSignupApplication = 65536, // 0x00010000
            IptvTitleApplication = 131072, // 0x00020000
            KeyVaultPrivilegesRequired = 67108864, // 0x04000000
            OnlineActivationRequired = 134217728, // 0x08000000
            PageSize4Kb = 268435456, // 0x10000000
            NoGameRegion = 536870912, // 0x20000000
            RevocationCheckOptional = 1073741824, // 0x40000000
        }

        [Flags]
        public enum GameRegion : uint
        {
            NorthAmerica = 255, // 0x000000FF
            Japan = 256, // 0x00000100
            China = 512, // 0x00000200
            RestOfAsia = 64512, // 0x0000FC00
            AustraliaNewZealand = 65536, // 0x00010000
            RestOfEurope = 16646144, // 0x00FE0000
            RestOfWord = 4278190080, // 0xFF000000
            AllRegions = RestOfWord | RestOfEurope | AustraliaNewZealand | RestOfAsia | China | Japan | NorthAmerica, // 0xFFFFFFFF
        }

        public enum SectionInfo
        {
            Code = 1,
            Data = 2,
            ReadOnly = 3,
        }

        public struct XexVersion
        {
            public int Major;
            public int Minor;
            public int Build;
            public int Qfe;

            public XexVersion(int version)
            {
                Major = version >> 28;
                Minor = version >> 24 & 15;
                Build = version >> 8 & (int)ushort.MaxValue;
                Qfe = version & (int)byte.MaxValue;
            }

            public static implicit operator XEX2.XexVersion(int version) => new XEX2.XexVersion(version);

            public static implicit operator int(XEX2.XexVersion version)
            {
                return (version.Major & 15) << 28 | (version.Minor & 15) << 24 | (version.Build & (int)ushort.MaxValue) << 8 | version.Qfe & (int)byte.MaxValue;
            }

            public override string ToString() => $"{this.Major}.{this.Minor}.{this.Build}.{this.Qfe}";
        }

        public struct XexHeader
        {
            public int Magic;
            public XEX2.ModuleFlag ModuleFlags;
            public int DataOffset;
            public int Reserved;
            public int SecurityInfoOffset;
            public int OptionalHeaderEntries;

            public int SizeOf => 24;

            public void Read(EndianReader er)
            {
                this.ModuleFlags = (this.Magic = er.ReadInt32()) == 1480939570 ? (XEX2.ModuleFlag)er.ReadInt32() : throw new Exception("Invalid XEX Header magic");
                this.DataOffset = er.ReadInt32();
                this.Reserved = er.ReadInt32();
                this.SecurityInfoOffset = er.ReadInt32();
                this.OptionalHeaderEntries = er.ReadInt32();
            }

            public void Write(EndianWriter ew)
            {
                ew.Write(this.Magic);
                ew.Write((int)this.ModuleFlags);
                ew.Write(this.DataOffset);
                ew.Write(this.Reserved);
                ew.Write(this.SecurityInfoOffset);
                ew.Write(this.OptionalHeaderEntries);
            }
        }

        public struct XexSection
        {
            public XEX2.SectionInfo Info;
            public int Size;
            public byte[] Digest;

            public static int SizeOf => 24;

            public void Read(EndianReader er)
            {
                int num = er.ReadInt32();
                this.Info = (XEX2.SectionInfo)(num & 15);
                this.Size = num >> 4;
                this.Digest = er.ReadBytes(20);
            }

            public void Write(EndianWriter ew)
            {
                int num = (int)((XEX2.SectionInfo)(this.Size << 4) | this.Info);
                ew.Write(num);
                ew.Write(this.Digest);
            }

            public override string ToString() => $"0x{this.Size:X} : {this.Info}";
        }

        public struct XexSectionTable
        {
            public int SectionCount;
            public XEX2.XexSection[] Sections;

            public int SizeOf => 4 + this.SectionCount * XEX2.XexSection.SizeOf;

            public void Read(EndianReader er)
            {
                this.SectionCount = er.ReadInt32();
                this.Sections = new XEX2.XexSection[this.SectionCount];
                for (int index = 0; index < this.SectionCount; ++index)
                    this.Sections[index].Read(er);
            }

            public void Write(EndianWriter ew)
            {
                ew.Write(this.SectionCount);
                for (int index = 0; index < this.SectionCount; ++index)
                    this.Sections[index].Write(ew);
            }
        }

        public struct XexSecurityInfo
        {
            public int HeaderLength;
            public int ImageSize;
            public byte[] RsaSignature;
            public int Length2;
            public XEX2.ImageFlag ImageFlags;
            public int LoadAddress;
            public byte[] SectionDigest;
            public int ImportTableCount;
            public byte[] ImportTableDigest;
            public byte[] Xgd2MediaId;
            public byte[] AesKey;
            public int ExportTable;
            public byte[] HeaderDigest;
            public XEX2.GameRegion GameRegions;
            public XEX2.AllowedMediaType AllowedMediaTypes;

            public int SizeOf => 384;

            public void Read(EndianReader er)
            {
                this.HeaderLength = er.ReadInt32();
                this.ImageSize = er.ReadInt32();
                this.RsaSignature = er.ReadBytes(256 /*0x0100*/);
                this.Length2 = er.ReadInt32();
                this.ImageFlags = (XEX2.ImageFlag)er.ReadInt32();
                this.LoadAddress = er.ReadInt32();
                this.SectionDigest = er.ReadBytes(20);
                this.ImportTableCount = er.ReadInt32();
                this.ImportTableDigest = er.ReadBytes(20);
                this.Xgd2MediaId = er.ReadBytes(16 /*0x10*/);
                this.AesKey = er.ReadBytes(16 /*0x10*/);
                this.ExportTable = er.ReadInt32();
                this.HeaderDigest = er.ReadBytes(20);
                int num = er.ReadInt32();
                long position = er.BaseStream.Position;
                this.GameRegions = (XEX2.GameRegion)num;
                this.AllowedMediaTypes = (XEX2.AllowedMediaType)er.ReadInt32();
            }

            public void Write(EndianWriter ew)
            {
                ew.Write(this.HeaderLength);
                ew.Write(this.ImageSize);
                ew.Write(this.RsaSignature);
                ew.Write(this.Length2);
                ew.Write((int)this.ImageFlags);
                ew.Write(this.LoadAddress);
                ew.Write(this.SectionDigest);
                ew.Write(this.ImportTableCount);
                ew.Write(this.ImportTableDigest);
                ew.Write(this.Xgd2MediaId);
                ew.Write(this.AesKey);
                ew.Write(this.ExportTable);
                ew.Write(this.HeaderDigest);
                ew.Write((int)this.GameRegions);
                ew.Write((int)this.AllowedMediaTypes);
            }
        }

        public class OptionalHeader
        {
            public XEX2.ImageKeys ImageKey;
            public int Data;

            public virtual int SizeOf() => 8;

            public virtual void Read(EndianReader er)
            {
            }

            public virtual void Write(EndianWriter ew)
            {
            }

            public override string ToString() => $"{this.ImageKey} - 0x{this.Data:X8}";
        }

        public class OriginalPEName : XEX2.OptionalHeader
        {
            public string Name;

            public override int SizeOf()
            {
                return this.Name.Length % 4 == 0 ? 4 + this.Name.Length : 4 + this.Name.Length + (4 - this.Name.Length % 4);
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                int num = er.ReadInt32();
                this.Name = er.ReadAsciiString(num - 4);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.SizeOf());
                ew.WriteAsciiString(this.Name, this.Name.Length);
                ew.Write(new byte[this.SizeOf() - (4 + this.Name.Length)]);
            }
        }

        public class BaseFileAddress : XEX2.OptionalHeader
        {
            public int BaseAddress;

            public override int SizeOf() => 4;

            public override void Read(EndianReader er) => this.BaseAddress = this.Data;

            public override void Write(EndianWriter ew) => this.Data = this.BaseAddress;
        }

        public class BaseFileEntryPoint : XEX2.OptionalHeader
        {
            public int EntryPoint;

            public override int SizeOf() => 4;

            public override void Read(EndianReader er) => this.EntryPoint = this.Data;

            public override void Write(EndianWriter ew) => this.Data = this.EntryPoint;
        }

        public class BaseFileChecksumAndTimeStamp : XEX2.OptionalHeader
        {
            public int Checksum;
            public DateTime Timestamp;

            public override int SizeOf() => 8;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.Checksum = er.ReadInt32();
                this.Timestamp = new DateTime(1970, 1, 1).AddSeconds((double)er.ReadInt32());
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.Checksum);
                TimeSpan timeSpan = this.Timestamp - new DateTime(1970, 1, 1);
                ew.Write((int)timeSpan.TotalSeconds);
            }
        }

        public class BaseFileDefaultStackSize : XEX2.OptionalHeader
        {
            public int DefaultStackSize;

            public override int SizeOf() => 4;

            public override void Read(EndianReader er) => this.DefaultStackSize = this.Data;

            public override void Write(EndianWriter ew) => this.Data = this.DefaultStackSize;
        }

        public class BaseFileFormat : XEX2.OptionalHeader
        {
            public int InfoSize;
            public XEX2.BaseFileFormat.EncryptionTypes EncryptionType;
            public XEX2.BaseFileFormat.CompressionTypes CompressionType;
            public XEX2.RawBaseFile RawFormat;
            public XEX2.CompressedBaseFile CompressedFormat;

            public override int SizeOf()
            {
                return this.CompressionType == XEX2.BaseFileFormat.CompressionTypes.NotCompressed ? 8 + this.RawFormat.SizeOf : 8 + this.CompressedFormat.SizeOf;
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.InfoSize = er.ReadInt32();
                this.EncryptionType = (XEX2.BaseFileFormat.EncryptionTypes)er.ReadInt16();
                this.CompressionType = (XEX2.BaseFileFormat.CompressionTypes)er.ReadInt16();
                if (this.CompressionType == XEX2.BaseFileFormat.CompressionTypes.NotCompressed)
                {
                    this.RawFormat = new XEX2.RawBaseFile();
                    this.RawFormat.Read(er, this.InfoSize - 8);
                }
                else
                {
                    this.CompressedFormat = new XEX2.CompressedBaseFile();
                    this.CompressedFormat.Read(er);
                }
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                this.InfoSize = this.SizeOf();
                ew.Write(this.InfoSize);
                ew.Write((short)this.EncryptionType);
                ew.Write((short)this.CompressionType);
                if (this.CompressionType == XEX2.BaseFileFormat.CompressionTypes.NotCompressed)
                    this.RawFormat.Write(ew);
                else
                    this.CompressedFormat.Write(ew);
            }

            public enum EncryptionTypes
            {
                NotEncrypted,
                Encrypted,
            }

            public enum CompressionTypes
            {
                NotCompressed = 1,
                Compressed = 2,
                DeltaCompressed = 3,
            }
        }

        public class RawBaseFile
        {
            public XEX2.RawBaseFile.RawBaseFileBlock[] Blocks;

            public int SizeOf => this.Blocks.Length * 8;

            public void Read(EndianReader er, int size)
            {
                this.Blocks = new XEX2.RawBaseFile.RawBaseFileBlock[size / 8];
                for (int index = 0; index < this.Blocks.Length; ++index)
                    this.Blocks[index].Read(er);
            }

            public void Write(EndianWriter ew)
            {
                for (int index = 0; index < this.Blocks.Length; ++index)
                    this.Blocks[index].Write(ew);
            }

            public struct RawBaseFileBlock
            {
                public int DataSize;
                public int ZeroSize;

                public void Read(EndianReader er)
                {
                    this.DataSize = er.ReadInt32();
                    this.ZeroSize = er.ReadInt32();
                }

                public void Write(EndianWriter ew)
                {
                    ew.Write(this.DataSize);
                    ew.Write(this.ZeroSize);
                }
            }
        }

        public class CompressedBaseFile
        {
            public int CompressionWindow = 32768 /*0x8000*/;
            public XEX2.CompressedBaseFile.CompBaseFileBlock Block;

            public int SizeOf => 28;

            public void Read(EndianReader er)
            {
                this.CompressionWindow = er.ReadInt32();
                this.Block.Read(er);
            }

            public void Write(EndianWriter ew)
            {
                ew.Write(this.CompressionWindow);
                this.Block.Write(ew);
            }

            public struct CompBaseFileBlock
            {
                public int DataSize;
                public byte[] Hash;

                public void Read(EndianReader er)
                {
                    this.DataSize = er.ReadInt32();
                    this.Hash = er.ReadBytes(20);
                }

                public void Read(byte[] data)
                {
                    this.DataSize = (int)data[0] << 24 | (int)data[1] << 16 /*0x10*/ | (int)data[2] << 8 | (int)data[3];
                    for (int index = 4; index < 24; ++index)
                        this.Hash[index - 4] = data[index];
                }

                public void Write(EndianWriter ew)
                {
                    ew.Write(this.DataSize);
                    ew.Write(this.Hash);
                }
            }
        }

        public class ImportLibraries : XEX2.OptionalHeader
        {
            public int SectionSize;
            public int HeaderSize;
            public int LibraryCount;
            public string[] LibNames;
            public XEX2.ImportLibraries.ImportLib[] Libs;

            public override int SizeOf()
            {
                int num = 12 + this.SizeOfStrings();
                for (int index = 0; index < this.LibraryCount; ++index)
                    num += this.Libs[index].SizeOf();
                return num;
            }

            public int SizeOfStrings()
            {
                int num = 0;
                for (int index = 0; index < this.LibraryCount; ++index)
                {
                    num += this.LibNames[index].Length + 1;
                    if (num % 4 != 0)
                        num += 4 - num % 4;
                }
                return num;
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.SectionSize = er.ReadInt32();
                this.HeaderSize = er.ReadInt32();
                this.LibraryCount = er.ReadInt32();
                this.LibNames = new string[this.LibraryCount];
                EndianReader endianReader = new EndianReader((Stream)new MemoryStream(er.ReadBytes(this.HeaderSize)), EndianType.BigEndian);
                for (int index = 0; index < this.LibraryCount; ++index)
                {
                    this.LibNames[index] = endianReader.ReadNullTerminatedString();
                    int num = (int)endianReader.BaseStream.Position % 4;
                    if (num != 0)
                        endianReader.BaseStream.Seek((long)(4 - num), SeekOrigin.Current);
                }
                this.Libs = new XEX2.ImportLibraries.ImportLib[this.LibraryCount];
                for (int index = 0; index < this.LibraryCount; ++index)
                    this.Libs[index].Read(er);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                this.SectionSize = this.SizeOf();
                this.HeaderSize = this.SizeOfStrings();
                ew.Write(this.SectionSize);
                ew.Write(this.HeaderSize);
                ew.Write(this.LibraryCount);
                for (int index = 0; index < this.LibraryCount; ++index)
                {
                    int num = this.LibNames[index].Length + 1;
                    ew.WriteNullTermString(this.LibNames[index]);
                    if (num % 4 != 0)
                        ew.Write(new byte[4 - num % 4]);
                }
                for (int index = 0; index < this.LibraryCount; ++index)
                    this.Libs[index].Write(ew);
            }

            public struct ImportLib
            {
                public int Size;
                public byte[] NextImportDigest;
                public int ID;
                public XEX2.XexVersion Version;
                public XEX2.XexVersion VersionMin;
                public short NameIndex;
                public ushort Count;
                public int[] ImportTable;

                public int SizeOf() => 40 + 4 * (int)this.Count;

                public void Read(EndianReader er)
                {
                    this.Size = er.ReadInt32();
                    this.NextImportDigest = er.ReadBytes(20);
                    this.ID = er.ReadInt32();
                    this.Version = (XEX2.XexVersion)er.ReadInt32();
                    this.VersionMin = (XEX2.XexVersion)er.ReadInt32();
                    this.NameIndex = er.ReadInt16();
                    this.Count = er.ReadUInt16();
                    this.ImportTable = new int[(int)this.Count];
                    for (int index = 0; index < (int)this.Count; ++index)
                        this.ImportTable[index] = er.ReadInt32();
                }

                public void Write(EndianWriter ew)
                {
                    this.Size = this.SizeOf();
                    ew.Write(this.Size);
                    ew.Write(this.NextImportDigest);
                    ew.Write(this.ID);
                    ew.Write((int)this.Version);
                    ew.Write((int)this.VersionMin);
                    ew.Write(this.NameIndex);
                    ew.Write(this.Count);
                    for (int index = 0; index < (int)this.Count; ++index)
                        ew.Write(this.ImportTable[index]);
                }
            }
        }

        public class StaticLibraries : XEX2.OptionalHeader
        {
            private int sectionSize;
            public XEX2.StaticLibraries.Library[] Libraries;

            public override int SizeOf() => 4 + this.Libraries.Length * 16 /*0x10*/;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.sectionSize = er.ReadInt32();
                this.Libraries = new XEX2.StaticLibraries.Library[(this.sectionSize - 4) / 16 /*0x10*/];
                for (int index = 0; index < this.Libraries.Length; ++index)
                    this.Libraries[index].Read(er);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.SizeOf());
                for (int index = 0; index < this.Libraries.Length; ++index)
                    this.Libraries[index].Write(ew);
            }

            public struct Library
            {
                public string Name;
                public short VersionMajor;
                public short VersionMinor;
                public short VersionBuild;
                public XEX2.StaticLibraries.Library.ApprovalTypes ApprovalType;
                public byte VersionQfe;

                public void Read(EndianReader er)
                {
                    this.Name = er.ReadAsciiString(8);
                    this.VersionMajor = er.ReadInt16();
                    this.VersionMinor = er.ReadInt16();
                    this.VersionBuild = er.ReadInt16();
                    this.ApprovalType = (XEX2.StaticLibraries.Library.ApprovalTypes)((int)er.ReadByte() >> 5);
                    this.VersionQfe = er.ReadByte();
                }

                public void Write(EndianWriter ew)
                {
                    ew.WriteAsciiString(this.Name, 8);
                    ew.Write(this.VersionMajor);
                    ew.Write(this.VersionMinor);
                    ew.Write(this.VersionBuild);
                    ew.Write((byte)((uint)this.ApprovalType << 5));
                    ew.Write(this.VersionQfe);
                }

                public override string ToString()
                {
                    return $"{this.Name} {this.VersionMajor}.{this.VersionMinor}.{this.VersionBuild}.{this.VersionQfe} [{this.ApprovalType}]";
                }

                public enum ApprovalTypes
                {
                    Unapproved,
                    PossibleApproved,
                    Approved,
                    Expired,
                }
            }
        }

        public class ExecutionId : XEX2.OptionalHeader
        {
            public int MediaId;
            public XEX2.XexVersion Version;
            public XEX2.XexVersion BaseVersion;
            public int TitleId;
            public byte Platform;
            public byte ExecutableType;
            public byte DiscNumber;
            public byte NumberOfDiscs;
            public int SavegameId;

            public override int SizeOf() => 24;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.MediaId = er.ReadInt32();
                this.Version = (XEX2.XexVersion)er.ReadInt32();
                this.BaseVersion = (XEX2.XexVersion)er.ReadInt32();
                this.TitleId = er.ReadInt32();
                this.Platform = er.ReadByte();
                this.ExecutableType = er.ReadByte();
                this.DiscNumber = er.ReadByte();
                this.NumberOfDiscs = er.ReadByte();
                this.SavegameId = er.ReadInt32();
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.MediaId);
                ew.Write((int)this.Version);
                ew.Write((int)this.BaseVersion);
                ew.Write(this.TitleId);
                ew.Write(this.Platform);
                ew.Write(this.ExecutableType);
                ew.Write(this.DiscNumber);
                ew.Write(this.NumberOfDiscs);
                ew.Write(this.SavegameId);
            }
        }

        public class TLSInfo : XEX2.OptionalHeader
        {
            public int NumberOfSlots;
            public int DataSize;
            public int RawDataAddress;
            public int RawDataSize;

            public override int SizeOf() => 16 /*0x10*/;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.NumberOfSlots = er.ReadInt32();
                this.DataSize = er.ReadInt32();
                this.RawDataAddress = er.ReadInt32();
                this.RawDataSize = er.ReadInt32();
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.NumberOfSlots);
                ew.Write(this.DataSize);
                ew.Write(this.RawDataAddress);
                ew.Write(this.RawDataSize);
            }
        }

        public class LANKey : XEX2.OptionalHeader
        {
            public byte[] Key;

            public override int SizeOf() => 16 /*0x10*/;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.Key = er.ReadBytes(16 /*0x10*/);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.Key);
            }
        }

        public class XexResources : XEX2.OptionalHeader
        {
            public XEX2.XexResources.Resource[] Resources;

            public override int SizeOf() => 4 + this.Resources.Length * 16 /*0x10*/;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.Resources = new XEX2.XexResources.Resource[(er.ReadInt32() - 4) / 16 /*0x10*/];
                for (int index = 0; index < this.Resources.Length; ++index)
                    this.Resources[index].Read(er);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.SizeOf());
                for (int index = 0; index < this.Resources.Length; ++index)
                    this.Resources[index].Write(ew);
            }

            public struct Resource
            {
                public string Name { get; set; }

                public int Address { get; set; }

                public int Size { get; set; }

                public void Read(EndianReader er)
                {
                    this.Name = er.ReadAsciiString(8);
                    this.Address = er.ReadInt32();
                    this.Size = er.ReadInt32();
                }

                public void Write(EndianWriter ew)
                {
                    ew.WriteAsciiString(this.Name, 8);
                    ew.Write(this.Address);
                    ew.Write(this.Size);
                }

                public override string ToString() => $"{this.Name} : 0x{this.Address:X} - 0x{this.Size:X}";
            }
        }

        public class SystemFlags : XEX2.OptionalHeader
        {
            public XEX2.SystemFlags.Privilege Privileges;

            public override int SizeOf() => 4;

            public override void Read(EndianReader er)
            {
                this.Privileges = (XEX2.SystemFlags.Privilege)this.Data;
            }

            public override void Write(EndianWriter ew) => this.Data = (int)this.Privileges;

            [Flags]
            public enum Privilege : uint
            {
                NoForceReboot = 1,
                ForegroundTasks = 2,
                NoOddMapping = 4,
                HandleMceInput = 8,
                RestrictHudFeatures = 16, // 0x00000010
                HandleGamepadDisconnect = 32, // 0x00000020
                InsecureSockets = 64, // 0x00000040
                Xbox1XspInterop = 128, // 0x00000080
                SetDashContext = 256, // 0x00000100
                TitleUsesGameVoiceChannel = 512, // 0x00000200
                TitlePal50Incompatible = 1024, // 0x00000400
                TitleInsecureUtilitydrive = 2048, // 0x00000800
                TitleXamHooks = 4096, // 0x00001000
                TitlePii = 8192, // 0x00002000
                CrossplatformSystemLink = 16384, // 0x00004000
                MultidiscSwap = 32768, // 0x00008000
                MultidiscInsecureMedia = 65536, // 0x00010000
                Ap25Media = 131072, // 0x00020000
                NoConfirmExit = 262144, // 0x00040000
                AllowBackgroundDownload = 524288, // 0x00080000
                CreatePersistableRamdrive = 1048576, // 0x00100000
                InheritPersistedRamdrive = 2097152, // 0x00200000
                AllowHudVibration = 4194304, // 0x00400000
                TitleBothUtilityPartitions = 8388608, // 0x00800000
                HandleIPTVInput = 16777216, // 0x01000000
                PreferBigbuttonInput = 33554432, // 0x02000000
                Reserved26 = 67108864, // 0x04000000
                MultidiscCrossTitle = 134217728, // 0x08000000
                TitleInstallIncompatible = 268435456, // 0x10000000
                AllowAvatarGetMetadataByXUID = 536870912, // 0x20000000
                AllowControllerSwapping = 1073741824, // 0x40000000
                DashExtensibilityModule = 2147483648, // 0x80000000
            }
        }

        public class Xbox360Logo : XEX2.OptionalHeader
        {
            public int SectionSize;
            public int ImageLength;
            public byte[] ImageData;

            public override int SizeOf() => 8 + this.ImageData.Length;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.SectionSize = er.ReadInt32();
                this.ImageLength = er.ReadInt32();
                this.ImageData = er.ReadBytes(this.ImageLength);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.SectionSize);
                ew.Write(this.ImageLength);
                ew.Write(this.ImageData);
            }
        }

        public class GameRatings : XEX2.OptionalHeader
        {
            public byte[] Ratings;

            public override int SizeOf() => 64 /*0x40*/;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.Ratings = er.ReadBytes(64 /*0x40*/);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.Ratings);
            }

            public enum ESRB
            {
                Everyone = 0,
                Everyone10AndOlder = 2,
                Teen = 4,
                Mature = 6,
                RatingPending = 8,
                Unrated = 255, // 0x000000FF
            }

            public enum PEGI
            {
                Ages3AndUp = 0,
                Ages4AndUp = 1,
                Ages5AndUp = 2,
                Ages6AndUp = 3,
                Ages7AndUp = 4,
                Ages8AndUp = 5,
                Ages9AndUp = 6,
                Ages10AndUp = 7,
                Ages11AndUp = 8,
                Ages12AndUp = 9,
                Ages13AndUp = 10, // 0x0000000A
                Ages14AndUp = 11, // 0x0000000B
                Ages15AndUp = 12, // 0x0000000C
                Ages16AndUp = 13, // 0x0000000D
                Ages18AndUp = 14, // 0x0000000E
                Unrated = 255, // 0x000000FF
            }

            public enum CERO
            {
                AllAges = 0,
                Ages12AndUp = 2,
                Ages15AndUp = 4,
                Ages17AndUp = 6,
                Ages18AndUp = 8,
            }

            public enum USK
            {
                AllAges = 0,
                Ages6AndUp = 2,
                Ages12AndUp = 4,
                Ages16AndUp = 6,
                Ages18AndUp = 8,
                Unrated = 255, // 0x000000FF
            }

            public enum OFLC
            {
                AllAges = 0,
                Ages8AndUp = 2,
                Mature = 4,
                MatureAccompanied = 6,
                Unrated = 255, // 0x000000FF
            }

            public enum KMRB
            {
                Unrated = 255, // 0x000000FF
            }

            public enum DJCTQ
            {
                Unrated = 255, // 0x000000FF
            }

            public enum FPB
            {
                AllAges = 0,
                ParentalGuidance = 6,
                Ages10AndUp = 7,
                Ages13AndUp = 10, // 0x0000000A
                Ages16AndUp = 13, // 0x0000000D
                Ages18AndUp = 14, // 0x0000000E
                Unrated = 255, // 0x000000FF
            }
        }

        public class BoundingPath : XEX2.OptionalHeader
        {
            public string Path;

            public override int SizeOf()
            {
                return this.Path.Length % 4 == 0 ? 4 + this.Path.Length : 4 + this.Path.Length + (4 - this.Path.Length % 4);
            }

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                int num = er.ReadInt32();
                this.Path = er.ReadAsciiString(num - 4);
            }

            public override void Write(EndianWriter ew)
            {
                this.Data = (int)ew.BaseStream.Position;
                ew.Write(this.SizeOf());
                ew.WriteAsciiString(this.Path, this.Path.Length);
                ew.Write(new byte[this.SizeOf() - (4 + this.Path.Length)]);
            }
        }

        public struct DeltaPatch
        {
            public int XPos;
            public int YPos;
            public short UncompressedLen;
            public short CompressedLen;
            public byte[] CompressedData;
            public byte[] DecompressedData;

            public int Size => 12 + (int)this.CompressedLen;

            public void Read(EndianReader er)
            {
                this.XPos = er.ReadInt32();
                this.YPos = er.ReadInt32();
                this.UncompressedLen = er.ReadInt16();
                this.CompressedLen = er.ReadInt16();
                this.CompressedData = er.ReadBytes((int)this.CompressedLen);
            }

            public int Decompress()
            {
                int pcbDataBlockMax = 32768 /*0x8000*/;
                int ldiContext = -1;
                int unknown = 38912;
                XCompress.LzxDecompress pvConfiguration;
                pvConfiguration.CpuType = 1L;
                pvConfiguration.WindowSize = (long)pcbDataBlockMax;
                IntPtr num = Marshal.AllocHGlobal(143872);
                bool flag = XCompress.LDICreateDecompression(ref pcbDataBlockMax, ref pvConfiguration, 0, 0, num, ref unknown, ref ldiContext) == 0;
                if (flag)
                    flag = this.Decompress(ldiContext) == 0;
                if (XCompress.LDIDestroyDecompression(ldiContext) != 0)
                    flag = false;
                Marshal.FreeHGlobal(num);
                return !flag ? 1 : 0;
            }

            public int Decompress(int ctx)
            {
                int compressedLen = (int)this.CompressedLen;
                byte[] compressedData = this.CompressedData;
                int uncompressedLen = (int)this.UncompressedLen;
                this.DecompressedData = new byte[uncompressedLen];
                return XCompress.LDIDecompress(ctx, compressedData, compressedLen, this.DecompressedData, ref uncompressedLen);
            }
        }

        public class DeltaPatchDescriptor : XEX2.OptionalHeader
        {
            public int DescriptorSize;
            public XEX2.XexVersion TargetVersion;
            public XEX2.XexVersion SourceVersion;
            public byte[] SourceDigest;
            public byte[] EncryptionSeed;
            public int SizeOfTargetHeaders;
            public int DeltaHeadersSourceOffset;
            public int DeltaHeadersSourceSize;
            public int DeltaHeadersTargetOffset;
            public int DeltaImageSourceOffset;
            public int DeltaImageSourceSize;
            public int DeltaImageTargetOffset;
            public byte[] DeltaHeaderPatchData;

            public override int SizeOf() => 76 + this.DeltaHeaderPatchData.Length;

            public override void Read(EndianReader er)
            {
                er.SeekTo(this.Data);
                this.DescriptorSize = er.ReadInt32();
                this.TargetVersion = (XEX2.XexVersion)er.ReadInt32();
                this.SourceVersion = (XEX2.XexVersion)er.ReadInt32();
                this.SourceDigest = er.ReadBytes(20);
                this.EncryptionSeed = er.ReadBytes(16 /*0x10*/);
                this.SizeOfTargetHeaders = er.ReadInt32();
                this.DeltaHeadersSourceOffset = er.ReadInt32();
                this.DeltaHeadersSourceSize = er.ReadInt32();
                this.DeltaHeadersTargetOffset = er.ReadInt32();
                this.DeltaImageSourceOffset = er.ReadInt32();
                this.DeltaImageSourceSize = er.ReadInt32();
                this.DeltaImageTargetOffset = er.ReadInt32();
                this.DeltaHeaderPatchData = er.ReadBytes(this.Data - 76);
            }

            public override void Write(EndianWriter ew) => this.Data = (int)ew.BaseStream.Position;
        }

        public class WorkspaceSize : XEX2.OptionalHeader
        {
            public int TitleWorkspaceSize;

            public override int SizeOf() => 4;

            public override void Read(EndianReader er) => this.TitleWorkspaceSize = this.Data;

            public override void Write(EndianWriter ew) => this.Data = this.TitleWorkspaceSize;
        }
    }
}
