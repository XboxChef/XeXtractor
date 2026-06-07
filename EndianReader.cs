using System;
using System.IO;
using System.Text;


namespace XeXtractor
{

    public class EndianReader : BinaryReader
    {
        private readonly EndianType endianStyle;

        public EndianReader(Stream stream, EndianType endianStyle)
          : base(stream)
        {
            this.endianStyle = endianStyle;
        }

        public void SeekTo(int offset) => this.SeekTo((long)offset, SeekOrigin.Begin);

        public void SeekTo(long offset) => this.SeekTo(offset, SeekOrigin.Begin);

        public void SeekTo(long offset, SeekOrigin seekOrigin)
        {
            this.BaseStream.Seek(offset, seekOrigin);
        }

        public override short ReadInt16() => this.ReadInt16(this.endianStyle);

        public short ReadInt16(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(2);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToInt16(numArray, 0);
        }

        public override ushort ReadUInt16() => this.ReadUInt16(this.endianStyle);

        public ushort ReadUInt16(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(2);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToUInt16(numArray, 0);
        }

        public override int ReadInt32() => this.ReadInt32(this.endianStyle);

        public int ReadInt32(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(4);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToInt32(numArray, 0);
        }

        public override uint ReadUInt32() => this.ReadUInt32(this.endianStyle);

        public uint ReadUInt32(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(4);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToUInt32(numArray, 0);
        }

        public override long ReadInt64() => this.ReadInt64(this.endianStyle);

        public long ReadInt64(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(8);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToInt64(numArray, 0);
        }

        public override ulong ReadUInt64() => this.ReadUInt64(this.endianStyle);

        public ulong ReadUInt64(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(8);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToUInt64(numArray, 0);
        }

        public override float ReadSingle() => this.ReadSingle(this.endianStyle);

        public float ReadSingle(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(4);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToSingle(numArray, 0);
        }

        public override double ReadDouble() => this.ReadDouble(this.endianStyle);

        public double ReadDouble(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(8);
            if (endianType == EndianType.BigEndian)
                Array.Reverse((Array)numArray);
            return BitConverter.ToDouble(numArray, 0);
        }

        public string ReadNullTerminatedString()
        {
            string result = string.Empty;
            byte value;
            while ((value = ReadByte()) != 0)
                result += (char)value;
            return result;
        }

        public string ReadUTF16String(int length) => this.ReadUTF16String(length, this.endianStyle);

        public string ReadUTF16String(int length, EndianType endianType)
        {
            length *= 2;
            byte[] bytes = this.ReadBytes(length);
            if (endianType != EndianType.LittleEndian)
            {
                for (int index = 0; index < length / 2; ++index)
                {
                    byte num = bytes[2 * index];
                    bytes[2 * index] = bytes[2 * index + 1];
                    bytes[2 * index + 1] = num;
                }
            }
            return Encoding.Unicode.GetString(bytes, 0, length);
        }

        public string ReadAsciiString(int length) => this.ReadAsciiString(length, this.endianStyle);

        public string ReadAsciiString(int length, EndianType endianType)
        {
            byte[] bytes = this.ReadBytes(length);
            int count = 0;
            while (count < length && bytes[count] != (byte)0)
                ++count;
            return Encoding.UTF8.GetString(bytes, 0, count);
        }

        public string ReadUnicodeString(int length) => this.ReadUnicodeString(length, this.endianStyle);

        public string ReadUnicodeString(int length, EndianType endianType)
        {
            string empty = string.Empty;
            int num1 = 0;
            for (int index = 0; index < length; ++index)
            {
                ushort num2 = this.ReadUInt16(endianType);
                ++num1;
                if (num2 != (ushort)0)
                    empty += (string)(object)(char)num2;
                else
                    break;
            }
            this.BaseStream.Seek((long)((length - num1) * 2), SeekOrigin.Current);
            return empty;
        }

        public string ReadUnicodeNullTermString() => this.ReadUnicodeNullTermString(this.endianStyle);

        public string ReadUnicodeNullTermString(EndianType endianType)
        {
            string empty = string.Empty;
            while (true)
            {
                ushort num = this.ReadUInt16(endianType);
                if (num != (ushort)0)
                    empty += (string)(object)(char)num;
                else
                    break;
            }
            return empty;
        }

        public int ReadInt24() => this.ReadInt24(this.endianStyle);

        public int ReadInt24(EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(3);
            return endianType == EndianType.BigEndian ? (int)numArray[0] << 16 /*0x10*/ | (int)numArray[1] << 8 | (int)numArray[2] : (int)numArray[2] << 16 /*0x10*/ | (int)numArray[1] << 8 | (int)numArray[0];
        }
    }
}