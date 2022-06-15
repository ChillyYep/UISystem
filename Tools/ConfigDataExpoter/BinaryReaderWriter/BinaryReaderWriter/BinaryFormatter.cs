using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ConfigData
{
    /// <summary>
    /// 每个配置类数据都需继承实现的序列化接口
    /// </summary>
    public interface IBinarySerializer
    {
        void Serialize(BinaryFormatter formatter);
    }

    /// <summary>
    /// 翻译sourceInfo-id转换器
    /// </summary>
    public interface ITextWriter
    {
        int Encode(string sourceText);
    }

    /// <summary>
    /// 二进制序列化器
    /// </summary>
    public class BinaryFormatter
    {
        public BinaryFormatter(Stream stream)
        {
            m_bw = new BinaryWriter(stream);
        }

        public BinaryFormatter(Stream stream, ITextWriter textWriter)
        {
            m_bw = new BinaryWriter(stream);
            m_textWriter = textWriter;
        }

        public void WriteObjectNoGeneric(object data)
        {
            Type type = data.GetType();
            if (typeof(IList).IsAssignableFrom(type) && typeof(ICollection).IsAssignableFrom(type))
            {
                if (type.Equals(typeof(List<byte>)))
                {
                    WriteByteList(data as List<byte>);
                }
                else if (type.Equals(typeof(List<char>)))
                {
                    WriteCharList(data as List<char>);
                }
                else if (type.Equals(typeof(List<bool>)))
                {
                    WriteBooleanList(data as List<bool>);
                }
                else if (type.Equals(typeof(List<Int16>)))
                {
                    WriteInt16List(data as List<Int16>);
                }
                else if (type.Equals(typeof(List<Int32>)))
                {
                    WriteInt32List(data as List<Int32>);
                }
                else if (type.Equals(typeof(List<Int64>)))
                {
                    WriteInt64List(data as List<Int64>);
                }
                else if (type.Equals(typeof(List<float>)))
                {
                    WriteSingleList(data as List<float>);
                }
                else if (type.Equals(typeof(List<double>)))
                {
                    WriteDoubleList(data as List<double>);
                }
                else if (type.Equals(typeof(List<string>)))
                {
                    WriteStringList(data as List<string>);
                }
                else if (type.IsGenericType && type.GenericTypeArguments.Length > 0)
                {
                    WriteObjectList(data, type);
                }
                else
                {
                    throw new Exception("不支持该类型的数据写入");
                }
            }
            else
            {
                if (type.Equals(typeof(byte)))
                {
                    WriteByte((byte)data);
                }
                else if (type.Equals(typeof(char)))
                {
                    WriteChar((char)data);
                }
                else if (type.Equals(typeof(bool)))
                {
                    WriteBoolean((bool)data);
                }
                else if (type.Equals(typeof(Int16)))
                {
                    WriteInt16((Int16)data);
                }
                else if (type.Equals(typeof(Int32)))
                {
                    WriteInt32((Int32)data);
                }
                else if (type.Equals(typeof(Int64)))
                {
                    WriteInt64((Int64)data);
                }
                else if (type.Equals(typeof(float)))
                {
                    WriteSingle((float)data);
                }
                else if (type.Equals(typeof(double)))
                {
                    WriteDouble((double)data);
                }
                else if (type.Equals(typeof(string)))
                {
                    WriteString((string)data);
                }
                else if (typeof(IBinarySerializer).IsAssignableFrom(type))
                {
                    WriteObject(data as IBinarySerializer);
                }
                else
                {
                    throw new Exception("不支持该类型的数据写入");
                }
            }

        }

        public void WriteSize(int size)
        {
            m_bw.Write((ushort)size);
        }

        public void WriteByte(byte value)
        {
            m_bw.Write(value);
        }

        public void WriteByteList(List<byte> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteByte(value[i]);
            }
        }

        public void WriteBoolean(Boolean value)
        {
            m_bw.Write(value);
        }

        public void WriteBooleanList(List<bool> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteBoolean(value[i]);
            }
        }

        public void WriteChar(char value)
        {
            m_bw.Write(value);
        }

        public void WriteCharList(List<char> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteChar(value[i]);
            }
        }

        public void WriteInt16(Int16 value)
        {
            m_bw.Write(value);
        }

        public void WriteInt16List(List<Int16> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteInt16(value[i]);
            }
        }

        public void WriteInt32(Int32 value)
        {
            m_bw.Write(value);
        }

        public void WriteInt32List(List<int> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteInt32(value[i]);
            }
        }

        public void WriteEnum(Int32 value)
        {
            WriteInt32(value);
        }

        public void WriteEnumList<T>(List<T> value) where T : struct
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteEnum((Int32)Enum.Parse(typeof(T), value[i].ToString(), true));
            }
        }

        public void WriteInt64(Int64 value)
        {
            m_bw.Write(value);
        }

        public void WriteInt64List(List<Int64> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteInt64(value[i]);
            }
        }

        public void WriteSingle(float value)
        {
            m_bw.Write(value);
        }

        public void WriteSingleList(List<float> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteSingle(value[i]);
            }
        }

        public void WriteDouble(double value)
        {
            m_bw.Write(value);
        }

        public void WriteDoubleList(List<double> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteDouble(value[i]);
            }
        }

        public void WriteString(string value)
        {
            m_bw.Write(value);
        }

        public void WriteStringList(List<string> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteString(value[i]);
            }
        }

        public void WriteText(string value)
        {
            var intValue = m_textWriter?.Encode(value) ?? default(int);
            m_bw.Write(intValue);
        }

        public void WriteTextList(List<string> value)
        {
            WriteSize(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                WriteText(value[i]);
            }
        }

        public void WriteObject<T>(T value) where T : IBinarySerializer
        {
            value.Serialize(this);
        }

        public void WriteObjectList<T>(List<T> value) where T : IBinarySerializer
        {
            WriteSize(value.Count);
            foreach (var element in value)
            {
                WriteObject(element);
            }
        }

        public void WriteObjectList(object value, Type type)
        {
            var indexer = type.GetProperty("Item").GetMethod;
            var size = (int)type.GetProperty("Count").GetGetMethod().Invoke(value, new object[] { });
            WriteSize(size);
            for (int i = 0; i < size; ++i)
            {
                WriteObjectNoGeneric(indexer.Invoke(value, new object[] { i }));
            }

        }

        public void Flush()
        {
            m_bw.Flush();
        }

        public void Close()
        {
            m_bw.Close();
        }

        private BinaryWriter m_bw;

        private ITextWriter m_textWriter;
    }
}
