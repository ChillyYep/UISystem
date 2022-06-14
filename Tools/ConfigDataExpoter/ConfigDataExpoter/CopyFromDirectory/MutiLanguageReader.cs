using System;
using System.Collections.Generic;

namespace ConfigData
{
    public interface ILanguageLoader
    {
        void LoadAllTexts();
    }

    public abstract class LanguageLoaderImpBase : ILanguageLoader
    {
        public LanguageLoaderImpBase(Language language)
        {
            m_language = language;
        }

        public abstract void LoadAllTexts();

        /// <summary>
        /// key:className.ID.FieldName.FieldListIndex.NestedClassFieldIndex.NestdClassFieldListIndex
        /// </summary>
        public readonly Dictionary<int, string> m_languageParser = new Dictionary<int, string>();

        protected Language m_language;
    }

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
