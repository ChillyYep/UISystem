using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    class DataExporter : FileExporter
    {
        public void Setup(Dictionary<Type, List<object>> allTableDatas)
        {
            m_allTableDatas = allTableDatas;
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
                var bytes = SerializeDataTable(item.Value, item.Key);
                ExportFile(Path.Combine(directory, item.Key.Name + ".txt"), System.Text.Encoding.Default.GetString(bytes));
            }

        }
        public byte[] SerializeDataTable(List<object> dataTable, Type classType)
        {
            byte[] bytes = new byte[0];
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (var item in dataTable)
                {
                    binaryFormatter.Serialize(ms, item);
                }
                bytes = ms.GetBuffer();
            }
            return bytes;
        }

        public Dictionary<Type, List<object>> m_allTableDatas;
    }
}
