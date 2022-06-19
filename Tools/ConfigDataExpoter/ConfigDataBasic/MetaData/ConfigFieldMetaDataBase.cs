namespace ConfigDataExpoter
{
    /// <summary>
    /// 域元数据基类
    /// </summary>
    public abstract class ConfigFieldMetaDataBase : ConfigMetaData
    {
        /// <summary>
        /// 带下划线域名
        /// </summary>
        public string PrivateFieldName { get; private set; }

        private string m_fieldName;

        /// <summary>
        /// 列名
        /// </summary>
        public string FieldName
        {
            get
            {
                return m_fieldName;
            }
            set
            {
                m_fieldName = value;
                PrivateFieldName = "_" + m_fieldName;
            }
        }

        /// <summary>
        /// 注释/备注
        /// </summary>
        public string Comment;

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType OwnDataType;

        /// <summary>
        /// 是否是数组，None,List[0],List[1]……,List[x]可变长数组
        /// </summary>
        public string ListType;

        /// <summary>
        /// 实际名称
        /// </summary>
        public string RealTypeName;

        /// <summary>
        /// 所属类的类名
        /// </summary>
        public string BelongClassName;

    }
}
