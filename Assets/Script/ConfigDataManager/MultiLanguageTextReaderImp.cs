using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ConfigData
{
    /// <summary>
    /// 多语言加载，翻译转换逻辑
    /// </summary>
    public class MultiLanguageTextReaderImp : ConfigData.LanguageLoaderImpBase
    {
        public MultiLanguageTextReaderImp(ConfigData.Language language, string directory, string suffix) : base(language)
        {
            m_filename = language.ToString();
            m_suffix = suffix;
            m_directory = Path.Combine(directory, m_filename);
        }

        public override void LoadAllTexts()
        {
            m_languageParser.Clear();
            using (FileStream sr = new FileStream(Path.Combine(m_directory, string.Format("{0}.{1}", m_filename, m_suffix)), FileMode.Open, FileAccess.Read))
            {
                BinaryParser binaryParser = new BinaryParser(sr);
                var list = binaryParser.ReadObjectList<LanguageTextItem>();
                foreach (var item in list)
                {
                    m_languageParser[item.m_id] = item.m_translateText;
                }
            }
        }
        private string m_filename;
        private string m_directory;
        private string m_suffix;
    }
}
