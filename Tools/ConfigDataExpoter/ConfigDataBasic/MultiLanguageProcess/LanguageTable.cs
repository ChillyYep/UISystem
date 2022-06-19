using System.Collections.Generic;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 一个翻译表
    /// </summary>
    public class LanguageTable
    {
        public string m_excelName;
        public string m_className;
        public const string IDField = "ID";
        public const string SourceText = "SourceText";
        public const string TranslateText = "TranslateText";
        public const string SourceInfoStr = "SourceInfoStr";
        public readonly Dictionary<string, LanguageItem> m_allLanguageItems = new Dictionary<string, LanguageItem>();
        //public readonly List<LanguageItem> m_allLanguageItems = new List<LanguageItem>();
    }
}
