namespace ConfigData
{
    /// <summary>
    /// 多语言解码，读取器
    /// </summary>
    public class MutiLanguageReader : ITextReader
    {
        public MutiLanguageReader(LanguageLoaderImpBase languageLoaderImp)
        {
            m_languageLoaderImp = languageLoaderImp;
            m_languageLoaderImp.LoadAllTexts();
        }

        public string DeCode(int id)
        {
            string classText;
            if (m_languageLoaderImp.m_languageParser.TryGetValue(id, out classText))
            {
                return classText;
            }
            return string.Format("{0} does't have translate text", id);
        }

        private LanguageLoaderImpBase m_languageLoaderImp;
    }
}
