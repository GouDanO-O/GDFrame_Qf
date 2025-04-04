﻿using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MoonSharp.Interpreter.IO
{
    /// <summary>
    /// "Optimized" BinaryReader which shares strings and use a dumb compression for integers
    /// </summary>
    public class BinDumpBinaryReader : BinaryReader
    {
        public BinDumpBinaryReader(Stream s) : base(s)
        {
        }

        public BinDumpBinaryReader(Stream s, Encoding e) : base(s, e)
        {
        }

        private List<string> m_Strings = new();

        public override int ReadInt32()
        {
            var b = base.ReadSByte();

            if (b == 0x7F)
                return (int)base.ReadInt16();
            else if (b == 0x7E)
                return (int)base.ReadInt32();
            else
                return (int)b;
        }

        public override uint ReadUInt32()
        {
            var b = base.ReadByte();

            if (b == 0x7F)
                return (uint)base.ReadUInt16();
            else if (b == 0x7E)
                return (uint)base.ReadUInt32();
            else
                return (uint)b;
        }

        public override string ReadString()
        {
            var pos = ReadInt32();

            if (pos < m_Strings.Count)
            {
                return m_Strings[pos];
            }
            else if (pos == m_Strings.Count)
            {
                var str = base.ReadString();
                m_Strings.Add(str);
                return str;
            }
            else
            {
                throw new IOException("string map failure");
            }
        }
    }
}