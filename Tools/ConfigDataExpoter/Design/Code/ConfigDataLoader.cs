using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigData
{
    public partial class ConfigDataLoader
    {
        public ConfigDataLoader(string directory, string suffix)
        {
            m_directory = directory;
            m_suffix = suffix;
        }

        private Dictionary<int, T> LoadConfigDataDict<T>(string filename) where T : IBinaryDeserializer, IConfigData, new()
        {
            var testDict = new Dictionary<int, T>();
            using (FileStream sr = new FileStream(Path.Combine(m_directory, string.Format("{0}.{1}", filename, m_suffix)), FileMode.Open, FileAccess.Read))
            {
                BinaryParser binaryParser = new BinaryParser(sr);
                var list = binaryParser.ReadObjectList<T>();
                foreach (var item in list)
                {
                    testDict[item.id] = item;
                }
            }
            return testDict;

        }

        public string m_directory;

        public string m_suffix;

    }
}
