using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    public enum FormatterType
    {
        //Json,
        Binary,
        //CSV,
        //XML
    }

    public interface IFormatter
    {
        /// <summary>
        /// 序列化成字节流，切记不要自行编码字节流，会使部分类型如浮点数类型丢失精度
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        byte[] SerializeDataTable(List<object> dataTable, Type classType);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        List<T> DeSerializeDataTable<T>(Stream stream) where T : ConfigData.IBinaryDeserializer, new();
    }
    /// <summary>
    /// 序列化反序列化中间装置
    /// </summary>
    public abstract class FormatterImpBase : IFormatter
    {
        public abstract List<T> DeSerializeDataTable<T>(Stream stream) where T : ConfigData.IBinaryDeserializer, new();

        public abstract byte[] SerializeDataTable(List<object> dataTable, Type classType);
    }
    /// <summary>
    /// 字节流序列化反序列化实现
    /// </summary>
    public class BinaryFormatterImp : FormatterImpBase
    {
        public BinaryFormatterImp(MultiLanguageExchanger multiLanguageWriter)
        {
            m_multiLanguageWriter = multiLanguageWriter;
        }

        public override List<T> DeSerializeDataTable<T>(Stream stream)
        {
            binaryParser = new ConfigData.BinaryParser(stream);
            List<T> tableData = binaryParser.ReadObjectList<T>();
            return tableData == null ? new List<T>() : tableData;
        }

        public override byte[] SerializeDataTable(List<object> dataTable, Type classType)
        {
            byte[] bytes = new byte[0];
            using (MemoryStream ms = new MemoryStream())
            {
                binaryFormatter = new ConfigData.BinaryFormatter(ms, m_multiLanguageWriter);
                binaryFormatter.WriteObjectNoGeneric(dataTable);
                bytes = ms.GetBuffer();
            }
            return bytes;
        }

        private MultiLanguageExchanger m_multiLanguageWriter;

        private ConfigData.BinaryParser binaryParser;

        private ConfigData.BinaryFormatter binaryFormatter;

    }

    public class Formatter : IFormatter
    {
        public static FormatterImpBase CreateFormatterImp(FormatterType formatterType, MultiLanguageExchanger multiLanguageWriter)
        {
            switch (formatterType)
            {
                case FormatterType.Binary:
                    return new BinaryFormatterImp(multiLanguageWriter);
            }
            return null;
        }

        private FormatterImpBase m_formatterImp;

        private FormatterType m_formatterType;

        public Formatter(FormatterType formatterType, MultiLanguageExchanger multiLanguageWriter)
        {
            m_formatterType = formatterType;
            m_formatterImp = CreateFormatterImp(formatterType, multiLanguageWriter);
        }

        public List<T> DeSerializeDataTable<T>(Stream stream) where T : ConfigData.IBinaryDeserializer, new()
        {
            return m_formatterImp.DeSerializeDataTable<T>(stream);
        }

        public byte[] SerializeDataTable(List<object> dataTable, Type classType)
        {
            return m_formatterImp.SerializeDataTable(dataTable, classType);
        }
    }
}
