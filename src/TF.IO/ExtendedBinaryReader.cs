using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TF.IO
{
    public class ExtendedBinaryReader : BinaryReader
    {
        private const int MAX_STRING_LENGTH = ushort.MaxValue;

        public Endianness Endianness { get; set; }
        public Encoding Encoding { get; }

        #region Constructors

        public ExtendedBinaryReader(Stream input, Endianness endianness = Endianness.LittleEndian) : base(input)
        {
            Endianness = endianness;
            Encoding = Encoding.UTF8;
        }

        public ExtendedBinaryReader(Stream input, Encoding encoding, Endianness endianness = Endianness.LittleEndian) : base(input, encoding)
        {
            Endianness = endianness;
            Encoding = encoding;
        }

        public ExtendedBinaryReader(Stream input, Encoding encoding, bool leaveOpen, Endianness endianness = Endianness.LittleEndian) : base(input, encoding, leaveOpen)
        {
            Endianness = endianness;
            Encoding = encoding;
        }

        #endregion

        #region Stream Functions

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public long Length => BaseStream.Length;

        #endregion

        #region Read Methods

        public override short ReadInt16()
        {
            var buffer = ReadBytes(2);

            if (Endianness == Endianness.LittleEndian)
            {
                return (short)(buffer[0] | buffer[1] << 8);
            }

            return (short)(buffer[0] << 8 | buffer[1]);
        }

        public override int ReadInt32()
        {
            var buffer = ReadBytes(4);

            if (Endianness == Endianness.LittleEndian)
            {
                return buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
            }

            return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        }

        public override long ReadInt64()
        {
            var buffer = ReadBytes(8);

            if (Endianness == Endianness.LittleEndian)
            {
                return buffer[0] | (long)buffer[1] << 8 | (long)buffer[2] << 16 | (long)buffer[3] << 24 | (long)buffer[4] << 32 | (long)buffer[5] << 40 | (long)buffer[6] << 48 | (long)buffer[7] << 56;
            }

            return (long)buffer[0] << 56 | (long)buffer[1] << 48 | (long)buffer[2] << 40 | (long)buffer[3] << 32 | (long)buffer[4] << 24 | (long)buffer[5] << 16 | (long)buffer[6] << 8 | buffer[7];
        }

        public override ushort ReadUInt16()
        {
            var buffer = ReadBytes(2);

            if (Endianness == Endianness.LittleEndian)
            {
                return (ushort)(buffer[0] | buffer[1] << 8);
            }

            return (ushort)(buffer[0] << 8 | buffer[1]);

        }

        public override uint ReadUInt32()
        {
            var buffer = ReadBytes(4);

            if (Endianness == Endianness.LittleEndian)
            {
                return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
            }

            return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);

        }

        public override ulong ReadUInt64()
        {
            var buffer = ReadBytes(8);

            if (Endianness == Endianness.LittleEndian)
            {
                return buffer[0] | (ulong)buffer[1] << 8 | (ulong)buffer[2] << 16 | (ulong)buffer[3] << 24 | (ulong)buffer[4] << 32 | (ulong)buffer[5] << 40 | (ulong)buffer[6] << 48 | (ulong)buffer[7] << 56;
            }

            return (ulong)buffer[0] << 56 | (ulong)buffer[1] << 48 | (ulong)buffer[2] << 40 | (ulong)buffer[3] << 32 | (ulong)buffer[4] << 24 | (ulong)buffer[5] << 16 | (ulong)buffer[6] << 8 | buffer[7];
        }

        public override float ReadSingle()
        {
            if (BitConverter.IsLittleEndian)
            {
                return Endianness == Endianness.LittleEndian ? base.ReadSingle() : BitConverter.ToSingle(ReadBytes(4).Reverse().ToArray(), 0);
            }

            return BitConverter.ToSingle(Endianness == Endianness.LittleEndian ? ReadBytes(4).Reverse().ToArray() : ReadBytes(4).ToArray(), 0);
        }

        public string ReadString(Encoding encoding, char endChar = '\0')
        {
            var pos = Position;
            var endPos = pos;

            var found = false;
            var sb = new StringBuilder();
            while (!found)
            {
                var str = ReadString(256, encoding);
                var position = str.IndexOf(endChar);

                if (position >= 0)
                {
                    str = str.Substring(0, position);
                    sb.Append(str);

                    //endPos += position;
                    endPos += encoding.GetByteCount(str);
                    found = true;
                }
                else
                {
                    sb.Append(str);
                    endPos += 256;
                }
            }

            Seek(endPos + encoding.GetByteCount(endChar.ToString()), SeekOrigin.Begin);

            return sb.ToString();
        }

        public string ReadString(int length, bool trimEnd = false, char endChar = '\0')
        {
            return ReadString(length, Encoding, trimEnd, endChar);
        }

        public string ReadString(int length, Encoding encoding, bool trimEnd = false, char endChar = '\0')
        {
            var pos = Position;

            var buffer = ReadBytes(length);

            var str = encoding.GetString(buffer, 0, buffer.Length);

            if (trimEnd)
            {
                var position = str.IndexOf(endChar);
                if (position >= 0)
                {
                    str = str.Substring(0, position);
                    Seek(pos + encoding.GetByteCount(str) + encoding.GetByteCount(endChar.ToString()), SeekOrigin.Begin);
                }
            }

            return str;
        }

        public override string ReadString()
        {
            return ReadString(Encoding);
        }

        #endregion

        #region Peek Methods

        public short PeekInt16()
        {
            var value = ReadInt16();
            Seek(-2, SeekOrigin.Current);
            return value;
        }

        public int PeekInt32()
        {
            var value = ReadInt32();
            Seek(-4, SeekOrigin.Current);
            return value;
        }

        public long PeekInt64()
        {
            var value = ReadInt64();
            Seek(-8, SeekOrigin.Current);
            return value;
        }

        public ushort PeekUInt16()
        {
            var value = ReadUInt16();
            Seek(-2, SeekOrigin.Current);
            return value;
        }

        public uint PeekUInt32()
        {
            var value = ReadUInt32();
            Seek(-4, SeekOrigin.Current);
            return value;
        }

        public ulong PeekUInt64()
        {
            var value = ReadUInt64();
            Seek(-8, SeekOrigin.Current);
            return value;
        }

        public float PeekSingle()
        {
            var value = ReadSingle();
            Seek(-4, SeekOrigin.Current);
            return value;
        }

        #endregion

        public int FindPattern(byte[] pattern)
        {
            if (pattern.Length > Length)
                return -1;

            var buffer = ReadBytes(pattern.Length);

            while (buffer.Length == pattern.Length)
            {
                if (pattern.SequenceEqual(buffer))
                {
                    return (int)(Position - pattern.Length);
                }

                Position -= pattern.Length - PadLeftSequence(buffer, pattern);

                buffer = ReadBytes(pattern.Length);
            }

            return -1;
        }

        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            var i = 1;
            while (i < bytes.Length)
            {
                var n = bytes.Length - i;
                var aux1 = new byte[n];
                var aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                {
                    return i;
                }
                i++;
            }
            return i;
        }

        private int InternalReadOneChar(byte[] charBytes, char[] singleChar, bool is2BytesPerChar, Decoder decoder)
        {
            var numChars = 0;
            var pos = Position;

            while (numChars == 0)
            {
                var byteCount = is2BytesPerChar ? 2 : 1;
                
                var byte1 = BaseStream.ReadByte();
                charBytes[0] = (byte) byte1;
                
                if (byte1 == -1)
                {
                    byteCount = 0;
                }
                
                if (byteCount == 2)
                {
                    var byte2 = BaseStream.ReadByte();
                    charBytes[1] = (byte) byte2;
                    if (byte2 == -1)
                    {
                        byteCount = 1;
                    }
                }

                if (byteCount == 0)
                {
                    return -1;
                }
                
                try
                {
                    numChars = decoder.GetChars(charBytes, 0, byteCount, singleChar, 0);
                }
                catch
                {
                    Seek(pos - Position, SeekOrigin.Current);
                    throw;
                }
            }

            return (int) singleChar[0];
        }
    }
}