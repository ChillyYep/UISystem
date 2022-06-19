namespace ConfigDataExpoter
{
    /// <summary>
    /// 数据类型或数组元素类型
    /// </summary>
    public enum DataType
    {
        None,
        Int8,
        Int16,
        Int32,
        Int64,
        Boolean,
        Float,
        Double,
        String,
        /// <summary>
        /// 可翻译的字符串
        /// </summary>
        Text,
        Enum,
        /// <summary>
        /// 自定义内嵌类
        /// </summary>
        NestedClass
    }
}
