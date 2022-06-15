namespace ConfigData
{
    /// <summary>
    /// 支持的多语言类型
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// 默认语言表，其他所有语种的翻译项ID及来源信息都以该语言表为准，由Default翻译项同步到其他语种翻译项
        /// </summary>
        Defaut,
        /// <summary>
        /// 中文语言表
        /// </summary>
        CN,
        /// <summary>
        /// 英文语言表
        /// </summary>
        EN,
        /// <summary>
        /// 标记语种数量
        /// </summary>
        Count
    }
}
