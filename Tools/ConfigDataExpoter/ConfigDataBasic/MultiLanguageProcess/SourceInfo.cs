namespace ConfigDataExpoter
{
    /// <summary>
    /// 来源信息
    /// </summary>
    public class SourceInfo
    {
        public static SourceInfo ParseSourceIDStr(string sourceStr)
        {
            var sourceInfo = new SourceInfo();
            var strs = sourceStr.Split('-');
            sourceInfo.m_className = strs[0];
            sourceInfo.m_rowID = int.Parse(strs[1]);
            sourceInfo.m_fieldName = strs[2];
            sourceInfo.m_fieldListIndex = int.Parse(strs[3]);
            sourceInfo.m_nestedFieldName = strs[4];
            sourceInfo.m_nestedFieldListIndex = int.Parse(strs[5]);
            return sourceInfo;
        }

        /// <summary>
        /// 所属配置类
        /// </summary>
        public string m_className;

        /// <summary>
        /// 源文本
        /// </summary>
        public string m_sourceText;

        /// <summary>
        /// ID
        /// </summary>
        public int m_rowID;

        /// <summary>
        /// 一级域名
        /// </summary>
        public string m_fieldName;

        /// <summary>
        /// 内嵌类域名，也就是二级域名
        /// </summary>
        public string m_nestedFieldName;

        /// <summary>
        /// 字段如果是数组，启用该项区分，内嵌类内部字段已经禁止使用数组
        /// </summary>
        public int m_fieldListIndex;

        /// <summary>
        /// 内嵌类列表Index
        /// </summary>
        public int m_nestedFieldListIndex;

        public string GetIDStr()
        {
            return $"{m_className}-{m_rowID}-{m_fieldName}-{m_fieldListIndex}-{m_nestedFieldName}-{m_nestedFieldListIndex}";
        }
    }
}
