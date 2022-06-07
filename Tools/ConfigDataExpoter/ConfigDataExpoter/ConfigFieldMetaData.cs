using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    public enum ListType
    {
        None,
        FixedLengthList,
        VaraintLengthList
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
        /// 所属类的类名
        /// </summary>
        public string m_belongClassName;

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
        /// 实际名称
        /// </summary>
        public string m_realTypeName;

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
            public string m_realTypeName;
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
            public string m_className;
            public const string Comment = "NestedClass";
            public int FieldNum => m_fieldsInfo.Count;
            public Dictionary<string, NestClassFieldInfo> m_fieldsInfo = new Dictionary<string, NestClassFieldInfo>();
        }

        public ListType GetListType(string listType)
        {
            if (listType == None)
            {
                return ListType.None;
            }
            // 可变长
            if (listType.Equals("List[x]"))
            {
                return ListType.VaraintLengthList;
            }
            // 固定长度
            if (Regex.Match(listType, @"List\[[0-9]+\]").Length == listType.Length)
            {
                return ListType.FixedLengthList;
            }
            return ListType.None;
        }

        public string GetType(DataType dataType, string listTypeStr, bool isNestedClass = false)
        {
            var listType = GetListType(listTypeStr);
            string elementType = string.Empty;
            switch (dataType)
            {
                case DataType.None:
                    throw new ParseExcelException($"{m_belongClassName}中{m_name}字段类型无效");
                case DataType.Enum:
                    {
                        if (isNestedClass)
                        {
                            throw new ParseExcelException($"{m_belongClassName}中字段{m_name}为内嵌类，该内嵌类中不能使用枚举字段");
                        }
                        if (string.IsNullOrEmpty(m_foreignKey))
                        {
                            throw new ParseExcelException($"{m_belongClassName}中字段{m_name}为Enum类型，必须指定好枚举外键");
                        }
                        var keys = m_foreignKey.Split(ForeignKeySeperator);
                        if (keys.Length != 2)
                        {
                            throw new ParseExcelException($"{m_belongClassName}中字段{m_name}为枚举外键格式不正确，必须是[EnumKey].ID或[EnumValue].Value");
                        }
                        elementType = keys[0];
                        break;
                    }
                case DataType.Int8:
                    elementType = typeof(byte).Name;
                    break;
                case DataType.Int16:
                case DataType.Int32:
                case DataType.Int64:
                    elementType = dataType.ToString();
                    break;
                case DataType.Float:
                    elementType = typeof(float).Name;
                    break;
                case DataType.Double:
                    elementType = typeof(double).Name;
                    break;
                case DataType.String:
                case DataType.Text:
                    elementType = typeof(string).Name;
                    break;
                default:
                    // 内嵌类
                    if (isNestedClass)
                    {
                        throw new ParseExcelException($"{m_belongClassName}中字段{m_name}为内嵌类，其中不能再嵌套一个类");
                    }
                    if (m_nestedClassMetaData.m_fieldsInfo.Count <= 0)
                    {
                        throw new ParseExcelException($"{m_belongClassName}中字段{m_name}为内嵌类，但该列并没有定义内嵌类");
                    }
                    elementType = m_nestedClassMetaData.m_className;
                    break;
            }
            if (listType == ListType.None)
            {
                return elementType;
            }
            else if (listType == ListType.FixedLengthList)
            {
                return $"{elementType}[]";
            }
            else
            {
                return $"List<{elementType}>";
            }
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
                //case ConfigClassFieldHeader.ClassFieldDataType:
                //    {
                //        m_dataType = ParseEnum(value, DataType.None);
                //        if (m_dataType == DataType.None && m_nestedClassMetaData.m_fieldsInfo.Count > 0)
                //        {
                //            m_dataType = DataType.NestedClass;
                //            m_nestedClassMetaData.m_className = value;
                //            return true;
                //        }
                //        return m_dataType != DataType.None;
                //    }
                case ConfigClassFieldHeader.ClassFieldName:
                    {
                        m_name = value.ToLower();
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
        public bool ParseNestedClass(DataType dataType, string names, string types, string comments, string isLists)
        {
            m_nestedClassMetaData.m_fieldsInfo.Clear();
            const char seperator = NestedClassMetaData.seperator;
            const string none = None;
            var nameArr = names.Split(seperator);
            var typeArr = types.Split(seperator).Select(type => Enum.TryParse<DataType>(type, out var realType) ? realType : DataType.NestedClass).ToArray();
            var commentArr = comments.Split(seperator);
            var isListArr = isLists.Split(seperator);

            if (dataType < DataType.NestedClass && dataType > DataType.None)
            {
                // 如果不是内嵌类，而是原生支持的类型，这些单元格的值都应该是None的
                if (nameArr.Length == 1 && typeArr.Length == 1 && commentArr.Length == 1 && isListArr.Length == 1 &&
                    nameArr[0].Equals(none) && typeArr[0] == DataType.None && commentArr[0].Equals(none) && isListArr[0].Equals(none))
                {
                    return false;
                }
                else
                {
                    throw new ParseExcelException("非内嵌类型的字段，内嵌类相关的四行头数据必须是None的");
                }
            }
            else
            {
                // 字段大于一个内嵌类才算有意义
                if (nameArr.Length > 1 && nameArr.Length == typeArr.Length && nameArr.Length == commentArr.Length && nameArr.Length == isListArr.Length)
                {
                    for (int i = 0; i < nameArr.Length; ++i)
                    {
                        m_nestedClassMetaData.m_fieldsInfo[nameArr[i]] = new NestClassFieldInfo()
                        {
                            m_realTypeName = GetType(typeArr[i], isListArr[i], true),
                            m_fieldName = nameArr[i].ToLower(),
                            m_comment = commentArr[i],
                            m_listType = isListArr[i],
                            m_dataType = typeArr[i]
                        };
                    }
                    return true;
                }
                else
                {
                    throw new ParseExcelException("无效类型，如果是内嵌类，内嵌类四行头数据字段数量必须大于一且四行数据一一对应，否则应当是基础类型");
                }
            }

        }

    }
}
