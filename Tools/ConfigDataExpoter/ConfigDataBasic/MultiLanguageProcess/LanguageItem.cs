using ConfigData;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 每一个翻译项信息,所有语言的翻译项共用一个LanguageItem，以保证各语种翻译表id和来源信息同步
    /// </summary>
    public class LanguageItem
    {
        public LanguageItem()
        {
            m_translateText = new string[(int)Language.Count];
            for (int i = 0; i < m_translateText.Length; ++i)
            {
                m_translateText[i] = string.Empty;
            }
        }

        public const char fieldSeperator = '.';
        public int m_id;
        public SourceInfo m_source;
        private string[] m_translateText;

        public void SetTranslateTextIfEmpty(Language language, string translateText)
        {
            var index = (int)language;
            if (index < m_translateText.Length)
            {
                // 不能覆盖已经有的翻译，要相关人员手动填表修改
                if (string.IsNullOrEmpty(m_translateText[index]))
                {
                    m_translateText[index] = translateText;
                }
            }
        }

        public string GetTranslateText(Language language)
        {
            var index = (int)language;
            if (index < m_translateText.Length)
            {
                return m_translateText[index];
            }
            return string.Empty;
        }
    }
}
