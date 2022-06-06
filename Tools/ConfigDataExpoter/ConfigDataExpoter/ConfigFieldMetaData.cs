using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 数据类型或数组元素类型
    /// </summary>
    public enum DataType
    {
        Invalid,
        Int8,
        Int16,
        Int32,
        Int64,
        Float,
        Double,
        /// <summary>
        /// 可翻译的字符串
        /// </summary>
        Text,
        String,
        Enum,
        /// <summary>
        /// 自定义内嵌类
        /// </summary>
        NestedClass
    }
    /// <summary>
    /// 可见性
    /// </summary>
    public enum Visiblity
    {
        Invalid,
        None,
        Client,
        Server,
        Both
    }

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

    /// <summary>
    /// 类的域配置
    /// </summary>
    class ConfigFieldMetaData : ConfigMetaData
    {
        /// <summary>
        /// 数组类别格式
        /// </summary>
        public const string ListFormat = @"List[{0}]";

        public const string None = "None";

        public const char ForeignKeySeperator = '.';

        /// <summary>
        /// 列名
        /// </summary>
        public string m_name;

        /// <summary>
        /// 注释/备注
        /// </summary>
        public string m_comment;

        /// <summary>
        /// 双端可见性
        /// </summary>
        public Visiblity m_visiblity;

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType m_dataType;

        /// <summary>
        /// 外键（"类名.域名"的格式，对外键进行引用）
        /// </summary>
        public string m_foreignKey;

        /// <summary>
        /// 是否是数组，None,List[0],List[1]……,List[x]可变长数组
        /// </summary>
        public string m_listType;

        /// <summary>
        /// 内嵌类额外信息
        /// </summary>
        public NestedClassMetaData m_nestedClassMetaData = new NestedClassMetaData();

        /// <summary>
        /// 内嵌类域信息
        /// </summary>
        public class NestClassFieldInfo
        {
            public string m_fieldName;
            public string m_comment;
            public DataType m_dataType;
            public string m_listType;
        }
        /// <summary>
        /// 内嵌类额外信息
        /// </summary>
        public class NestedClassMetaData
        {
            public const char seperator = ',';
            public int FieldNum => m_fieldsInfo.Count;
            public Dictionary<string, NestClassFieldInfo> m_fieldsInfo = new Dictionary<string, NestClassFieldInfo>();
        }

        public bool SetValue(ConfigClassFieldHeader fieldType, string value)
        {
            switch (fieldType)
            {
                case ConfigClassFieldHeader.ClassFieldComment:
                    {
                        m_comment = value;
                        return m_comment != null;
                    }
                case ConfigClassFieldHeader.ClassFieldDataType:
                    {
                        m_dataType = ParseEnum(value, DataType.Invalid);
                        return m_dataType != DataType.Invalid;
                    }
                case ConfigClassFieldHeader.ClassFieldName:
                    {
                        m_name = value;
                        return m_name != null;
                    }
                case ConfigClassFieldHeader.ClassFieldVisiblity:
                    {
                        m_visiblity = ParseEnum(value, Visiblity.Invalid);
                        return m_visiblity != Visiblity.Invalid;
                    }
                case ConfigClassFieldHeader.ClassFieldForeignKey:
                    {
                        m_foreignKey = value;
                        return m_foreignKey != null;
                    }
                case ConfigClassFieldHeader.ClassFieldIsList:
                    {
                        m_listType = value;
                        return m_listType != null;
                    }
            }
            return false;
        }
        /// <summary>
        /// 解析内嵌类
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="names"></param>
        /// <param name="types"></param>
        /// <param name="comments"></param>
        /// <param name="isLists"></param>
        public void ParseNestedClass(DataType dataType, string names, string types, string comments, string isLists)
        {
            m_nestedClassMetaData.m_fieldsInfo.Clear();
            const char seperator = NestedClassMetaData.seperator;
            const string none = None;
            var nameArr = names.Split(seperator);
            var typeArr = types.Split(seperator).Select(type => Enum.TryParse<DataType>(type, out var realType) ? realType : DataType.Invalid).ToArray();
            var commentArr = comments.Split(seperator);
            var isListArr = isLists.Split(seperator);
            if (dataType == DataType.NestedClass)
            {
                // 字段大于一个内嵌类才算有意义
                if (nameArr.Length > 1 && nameArr.Length == typeArr.Length && nameArr.Length == commentArr.Length && nameArr.Length == isListArr.Length)
                {
                    for (int i = 0; i < nameArr.Length; ++i)
                    {
                        m_nestedClassMetaData.m_fieldsInfo[nameArr[i]] = new NestClassFieldInfo()
                        {
                            m_fieldName = nameArr[i],
                            m_comment = commentArr[i],
                            m_listType = isListArr[i],
                            m_dataType = typeArr[i]
                        };
                    }
                }
                else
                {
                    throw new ParseExcelException("无效内嵌类，内嵌类四行头数据字段数量必须大于一且四行数据一一对应");
                }
            }
            else
            {
                // 如果不是内嵌类，而是原生支持的类型，这些单元格的值都应该是None的
                if (nameArr.Length == 1 && typeArr.Length == 1 && commentArr.Length == 1 && isListArr.Length == 1 &&
                    nameArr[0].Equals(none) && typeArr[0].Equals(none) && commentArr[0].Equals(none) && isListArr[0].Equals(none))
                {
                    return;
                }
                else
                {
                    throw new ParseExcelException("非内嵌类型的字段，内嵌类相关的四行头数据必须是None的");
                }
            }

        }

    }
}
