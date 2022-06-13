using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigData
{
    public enum Language
    {
        CN,
        EN
    }

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
        public Dictionary<string, string> m_languageParser = new Dictionary<string, string>();

        protected Language m_language;
    }

    class MutiLanguageReader : ITextReader
    {
        public MutiLanguageReader(LanguageLoaderImpBase languageLoaderImp)
        {
            m_languageLoaderImp = languageLoaderImp;
            m_languageLoaderImp.LoadAllTexts();
        }

        public string DeCode(string codeStr)
        {
            if (m_languageLoaderImp.m_languageParser.TryGetValue(codeStr, out var classText))
            {
                return classText;
            }
            return $"{codeStr} does't have translate text";
        }

        private LanguageLoaderImpBase m_languageLoaderImp;
    }
}
