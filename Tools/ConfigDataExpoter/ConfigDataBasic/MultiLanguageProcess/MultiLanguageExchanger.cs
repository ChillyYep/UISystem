using ConfigData;
using System.Collections.Generic;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 多语言sourceInfo-id转换器
    /// </summary>
    public class MultiLanguageExchanger : ITextWriter
    {
        public void AddIDTextPair(int id, string sourceInfo)
        {
            if (m_sourceInfo2ID.ContainsKey(sourceInfo))
            {
                throw new ParseExcelException($"存在重复来源翻译项\"{sourceInfo}\"");
            }
            m_sourceInfo2ID[sourceInfo] = id;
        }

        public int Encode(string sourceInfo)
        {
            if (m_sourceInfo2ID.TryGetValue(sourceInfo, out var id))
            {
                return id;
            }
            throw new ParseExcelException($"不存在该来源的翻译项ID\"{sourceInfo}\"");
        }

        public readonly Dictionary<string, int> m_sourceInfo2ID = new Dictionary<string, int>();
    }
}
