using System.Collections.Generic;
using System.IO;

namespace ConfigData
{
    /// <summary>
    /// 配置表加载器，自定义部分
    /// </summary>
    public partial class ConfigDataLoader
    {
        public ConfigDataLoader(string directory, string suffix, MutiLanguageReader mutiLanguageReader)
        {
            m_directory = directory;
            m_suffix = suffix;
            m_mutiLanguageReader = mutiLanguageReader;
        }

        private Dictionary<int, T> LoadConfigDataDict<T>(string filename) where T : IBinaryDeserializer, IConfigData, new()
        {
            var testDict = new Dictionary<int, T>();
            using (FileStream sr = new FileStream(Path.Combine(m_directory, string.Format("{0}.{1}", filename, m_suffix)), FileMode.Open, FileAccess.Read))
            {
                BinaryParser binaryParser = new BinaryParser(sr, m_mutiLanguageReader);
                var list = binaryParser.ReadObjectList<T>();
                foreach (var item in list)
                {
                    testDict[item.id] = item;
                }
            }
            return testDict;

        }

        private MutiLanguageReader m_mutiLanguageReader;

        private string m_directory;

        private string m_suffix;

    }
}
