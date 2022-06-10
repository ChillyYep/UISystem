using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ConfigDataExpoter
{
    public enum FormatterType
    {
        Json,
        Binary,
        CSV,
        XML
    }

    public interface IFormatter
    {
        string SerializeDataTable(List<object> dataTable, Type classType);
        List<object> DeSerializeDataTable(Stream stream, Type classType);
    }

    public abstract class FormatterImpBase : IFormatter
    {
        public abstract List<object> DeSerializeDataTable(Stream stream, Type classType);

        public abstract string SerializeDataTable(List<object> dataTable, Type classType);
    }

    public class BinaryFormatterImp : FormatterImpBase
    {
        public override List<object> DeSerializeDataTable(Stream stream, Type classType)
        {
            List<object> tableData = null;
            //using (MemoryStream ms = new MemoryStream())
            //{
                //binaryParser = new ConfigData.BinaryParser(stream);
                //var bytes = System.Text.Encoding.Default.GetBytes(stream);
                //ms.Write(bytes, 0, bytes.Length);
                //tableData = binaryFormatter.Deserialize(ms) as List<object>;
            //}
            return tableData == null ? new List<object>() : tableData;
        }

        public override string SerializeDataTable(List<object> dataTable, Type classType)
        {
            byte[] bytes = new byte[0];
            using (MemoryStream ms = new MemoryStream())
            {
                binaryFormatter = new ConfigData.BinaryFormatter(ms);
                binaryFormatter.WriteObject(dataTable, dataTable.GetType());
                //ProtoBuf.Meta.RuntimeTypeModel.Create().Serialize(ms, dataTable);
                //binaryFormatter.Serialize(ms, dataTable);
                bytes = ms.GetBuffer();
            }
            return System.Text.Encoding.Default.GetString(bytes);
        }

        private ConfigData.BinaryParser binaryParser;

        private ConfigData.BinaryFormatter binaryFormatter;

    }

    class Formatter : IFormatter
    {
        public static FormatterImpBase CreateFormatterImp(FormatterType formatterType)
        {
            switch (formatterType)
            {
                case FormatterType.Binary:
                    return new BinaryFormatterImp();
            }
            return null;
        }

        private FormatterImpBase m_formatterImp;

        private FormatterType m_formatterType;

        public Formatter(FormatterType formatterType)
        {
            m_formatterType = formatterType;
            m_formatterImp = CreateFormatterImp(formatterType);
        }

        public List<object> DeSerializeDataTable(Stream stream, Type classType)
        {
            return m_formatterImp.DeSerializeDataTable(stream, classType);
        }

        public string SerializeDataTable(List<object> dataTable, Type classType)
        {
            return m_formatterImp.SerializeDataTable(dataTable, classType);
        }
    }
}
