using System.Collections.Generic;

namespace ConfigData
{
    /// <summary>
    /// 翻译项加载器实现基类
    /// </summary>
    public abstract class LanguageLoaderImpBase : ILanguageLoader
    {
        public LanguageLoaderImpBase(Language language)
        {
            m_language = language;
        }

        /// <summary>
        /// 加载所有翻译文本
        /// </summary>
        public abstract void LoadAllTexts();

        /// <summary>
        /// id-翻译文本词典
        /// </summary>
        public readonly Dictionary<int, string> m_languageParser = new Dictionary<int, string>();

        protected Language m_language;
    }
}
