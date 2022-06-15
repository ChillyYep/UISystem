namespace ConfigDataExpoter
{
    /// <summary>
    /// 配置类元数据行每行的类型枚举
    /// </summary>
    public enum ConfigClassFieldHeader
    {
        ClassFieldName = 2,
        ClassFieldComment = 3,
        ClassFieldVisiblity = 4,
        ClassFieldDataType = 5,
        ClassFieldForeignKey = 6,
        ClassFieldIsList = 7,
        ClassNestedClassFieldNames = 8,
        ClassNestedClassFieldComments = 9,
        ClassNestedClassFieldTypes = 10,
        ClassNestedClassFieldIsList = 11
    }
}
