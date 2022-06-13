using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
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

        public Dictionary<string, Dictionary<string, string>> m_languageParser = new Dictionary<string, Dictionary<string, string>>();

        private Language m_language;
    }

    class MutiLanguageReader
    {
        public MutiLanguageReader(LanguageLoaderImpBase languageLoaderImp)
        {
            m_languageLoaderImp = languageLoaderImp;
            m_languageLoaderImp.LoadAllTexts();
        }

        public string GetText(string className, string source)
        {
            if (m_languageLoaderImp.m_languageParser.TryGetValue(className, out var classTexts))
            {
                if (classTexts.TryGetValue(source, out var text))
                {
                    return text;
                }
            }
            return $"{className} does't have {source} translate text";
        }

        private LanguageLoaderImpBase m_languageLoaderImp;
    }
}
