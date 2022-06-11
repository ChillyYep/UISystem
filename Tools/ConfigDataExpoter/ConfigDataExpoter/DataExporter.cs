using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigDataExpoter
{
    class DataExporter : FileExporter
    {
        public void Setup(Dictionary<Type, List<object>> allTableDatas, FormatterType formatterType)
        {
            m_allTableDatas = allTableDatas;
            formatter = new Formatter(formatterType);
        }

        public void ExportData(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
                Directory.CreateDirectory(directory);
            }
            catch (Exception e)
            {
                throw new ParseExcelException(e.Message);
            }
            foreach (var item in m_allTableDatas)
            {
                var str = SerializeDataTable(item.Value, item.Key);
                var filePath = Path.Combine(directory, item.Key.Name + ".txt");
                ExportFile(filePath, str, false);
            }

        }

        public byte[] SerializeDataTable(List<object> dataTable, Type classType)
        {
            return formatter.SerializeDataTable(dataTable, classType);
        }

        public List<T> DeSerializeDataTable<T>(Stream stream) where T : ConfigData.IBinaryDeserializer, new()
        {
            return formatter.DeSerializeDataTable<T>(stream);
        }

        public Dictionary<Type, List<object>> m_allTableDatas;

        private Formatter formatter;
    }
}
