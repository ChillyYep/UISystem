using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigData
{
    public interface IBinaryDeserializer
    {
        void Deserialize(BinaryParser reader);
    }
    public class BinaryParser
    {
        public BinaryParser(Stream stream)
        {
            m_br = new BinaryReader(stream, System.Text.Encoding.Default);
        }

        public ushort GetLength()
        {
            return m_br.ReadUInt16();
        }

        public byte[] GetDataBytes(ushort length)
        {
            return m_br.ReadBytes(length);
        }

        public bool ReadBoolean()
        {
            return m_br.ReadBoolean();
        }

        public List<bool> ReadBooleanList()
        {
            List<bool> list = new List<bool>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadBoolean());
            }
            return list;
        }

        public byte ReadInt8()
        {
            return m_br.ReadByte();
        }

        public List<byte> ReadInt8List()
        {
            List<byte> list = new List<byte>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadInt8());
            }
            return list;
        }

        public char ReadChar()
        {
            return m_br.ReadChar();
        }

        public List<char> ReadCharList()
        {
            List<char> list = new List<char>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadChar());
            }
            return list;
        }

        public Int16 ReadInt16()
        {
            return m_br.ReadInt16();
        }

        public List<Int16> ReadInt16List()
        {
            List<Int16> list = new List<Int16>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadInt16());
            }
            return list;
        }

        public int ReadInt32()
        {
            return m_br.ReadInt32();
        }

        public List<Int32> ReadInt32List()
        {
            List<Int32> list = new List<Int32>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadInt32());
            }
            return list;
        }

        public int ReadEnum()
        {
            return ReadInt32();
        }
        
        public Int64 ReadInt64()
        {
            return m_br.ReadInt64();
        }

        public List<Int64> ReadInt64List()
        {
            List<Int64> list = new List<Int64>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadInt64());
            }
            return list;
        }

        public float ReadSingle()
        {
            return m_br.ReadSingle();
        }

        public List<float> ReadSingleList()
        {
            List<float> list = new List<float>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadSingle());
            }
            return list;
        }

        public double ReadDouble()
        {
            return m_br.ReadDouble();
        }

        public List<double> ReadDoubleList()
        {
            List<double> list = new List<double>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadDouble());
            }
            return list;
        }

        public string ReadString()
        {
            GetLength();
            return m_br.ReadString();
        }

        public List<string> ReadStringList()
        {
            List<string> list = new List<string>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadString());
            }
            Console.WriteLine(string.Format("ReadStringList:{0}", list.Count));
            return list;
        }

        public T ReadObject<T>() where T : IBinaryDeserializer, new()
        {
            GetLength();
            var deserializer = new T();
            deserializer.Deserialize(this);
            return deserializer;
        }
        
        public List<T> ReadObjectList<T>() where T : IBinaryDeserializer, new()
        {
            List<T> list = new List<T>();
            var size = GetLength();
            for (int i = 0; i < size; ++i)
            {
                list.Add(ReadObject<T>());
            }
            return list;
        }


        public void Close()
        {
            m_br.Close();
        }

        private BinaryReader m_br;
        public byte[] m_dataBytes;
    }
}
