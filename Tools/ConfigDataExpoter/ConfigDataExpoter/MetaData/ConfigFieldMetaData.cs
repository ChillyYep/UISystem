using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 类的域配置
    /// </summary>
    public class ConfigFieldMetaData : ConfigFieldMetaDataBase
    {
        /// <summary>
        /// 数组类别格式
        /// </summary>
        public const string ListFormat = @"List[{0}]";

        public const string None = "None";

        public const char ForeignKeySeperator = '.';

        public const char ListSeperator = ';';

        public const string ListStringSeperator = "\";\"";

        /// <summary>
        /// 数据开始行
        /// </summary>
        public const int DataBeginRow = (int)ConfigClassFieldHeader.ClassNestedClassFieldIsList + 1;

        /// <summary>
        /// 双端可见性
        /// </summary>
        public Visiblity OwnVisiblity;

        /// <summary>
        /// 外键（"类名.域名"的格式，对外键进行引用）
        /// </summary>
        public string ForeignKey;

        /// <summary>
        /// 表中第几列
        /// </summary>
        public int ColumnIndex;

        /// <summary>
        /// 内嵌类额外信息
        /// </summary>
        public NestedClassMetaData OwnNestedClassMetaData = new NestedClassMetaData();

        /// <summary>
        /// 内嵌类域信息
        /// </summary>
        public class NestClassFieldInfo : ConfigFieldMetaDataBase { }
        /// <summary>
        /// 内嵌类额外信息
        /// </summary>
        public class NestedClassMetaData : ConfigClassMetaDataBase
        {
            public const char seperator = ',';
            public int FieldNum => m_fieldsInfo.Count;
            public List<NestClassFieldInfo> m_fieldsInfo = new List<NestClassFieldInfo>();
        }

        /// <summary>
        /// 获取列表类型和列表长度
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="listCount"></param>
        /// <returns></returns>
        public static ListType GetListType(string belongtoClassName, string fieldName, string listType, out int listCount)
        {
            if (listType == None)
            {
                listCount = 0;
                return ConfigDataExpoter.ListType.None;
            }
            // 可变长
            if (listType.Equals("List[x]", StringComparison.OrdinalIgnoreCase))
            {
                listCount = int.MaxValue;
                return ConfigDataExpoter.ListType.VaraintLengthList;
            }
            // 固定长度
            if (Regex.Match(listType, @"List\[[0-9]+\]", RegexOptions.IgnoreCase).Length == listType.Length)
            {
                var match = Regex.Match(listType, @"[0-9]+");
                if (match.Success)
                {
                    listCount = int.Parse(match.Value);
                }
                else
                {
                    listCount = int.MaxValue;
                }
                return ConfigDataExpoter.ListType.FixedLengthList;
            }
            throw new ParseExcelException($"{belongtoClassName}的{fieldName}使用了非法的列表类型格式");
        }

        public static string GetTypeName(ConfigFieldMetaDataBase metaData, DataType dataType, string listTypeStr, bool isNestedClass = false)
        {
            var belongClassName = metaData == null ? "" : metaData.BelongClassName;
            var name = metaData == null ? "" : metaData.FieldName;
            var listType = GetListType(metaData.BelongClassName, metaData.FieldName, listTypeStr, out _);
            string elementType = string.Empty;
            switch (dataType)
            {
                case DataType.None:
                    throw new ParseExcelException($"{belongClassName}中{name}字段类型无效");
                case DataType.Enum:
                    {
                        ConfigFieldMetaData fieldMetaData = metaData as ConfigFieldMetaData;

                        var foreignKey = fieldMetaData == null ? "" : fieldMetaData.ForeignKey;
                        if (isNestedClass)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为内嵌类，该内嵌类中不能使用枚举字段");
                        }
                        if (string.IsNullOrEmpty(foreignKey))
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为Enum类型，必须指定好枚举外键");
                        }
                        var keys = foreignKey.Split(ForeignKeySeperator);
                        if (keys.Length != 2)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为枚举外键格式不正确，必须是[EnumKey].ID或[EnumValue].Value");
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
                case DataType.Boolean:
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
                    {
                        ConfigFieldMetaData fieldMetaData = metaData as ConfigFieldMetaData;
                        var nestedClassMetaData = fieldMetaData == null ? null : fieldMetaData.OwnNestedClassMetaData;
                        // 内嵌类
                        if (isNestedClass)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为内嵌类，其中不能再嵌套一个类");
                        }
                        if (nestedClassMetaData.FieldNum <= 0)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为内嵌类，但该列并没有定义内嵌类");
                        }
                        elementType = nestedClassMetaData.m_classname;
                        break;
                    }
            }
            if (listType == ConfigDataExpoter.ListType.None)
            {
                return elementType;
            }
            //else if (listType == ListType.FixedLengthList)
            //{
            //    return $"{elementType}[]";
            //}
            else
            {
                return $"List<{elementType}>";
            }
        }

        public static string GetFullTypeName(ConfigFieldMetaDataBase metaData, DataType dataType, string listTypeStr, bool isNestedClass = false)
        {
            var belongClassName = metaData.BelongClassName;
            var name = metaData.FieldName;
            var listType = GetListType(metaData.BelongClassName, metaData.FieldName, listTypeStr, out _);
            string elementType = string.Empty;
            switch (dataType)
            {
                case DataType.None:
                    throw new ParseExcelException($"{belongClassName}中{name}字段类型无效");
                case DataType.Enum:
                    {
                        ConfigFieldMetaData fieldMetaData = metaData as ConfigFieldMetaData;
                        var foreignKey = fieldMetaData == null ? "" : fieldMetaData.ForeignKey;
                        if (isNestedClass)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为内嵌类，该内嵌类中不能使用枚举字段");
                        }
                        if (string.IsNullOrEmpty(foreignKey))
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为Enum类型，必须指定好枚举外键");
                        }
                        var keys = foreignKey.Split(ForeignKeySeperator);
                        if (keys.Length != 2)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为枚举外键格式不正确，必须是[EnumKey].ID或[EnumValue].Value");
                        }
                        elementType = $"ConfigData.{keys[0]}";
                        break;
                    }
                case DataType.Int8:
                    elementType = typeof(byte).FullName;
                    break;
                case DataType.Int16:
                    elementType = typeof(Int16).FullName;
                    break;
                case DataType.Int32:
                    elementType = typeof(Int32).FullName;
                    break;
                case DataType.Int64:
                    elementType = typeof(Int64).FullName;
                    break;
                case DataType.Boolean:
                    elementType = typeof(Boolean).FullName;
                    break;
                case DataType.Float:
                    elementType = typeof(float).FullName;
                    break;
                case DataType.Double:
                    elementType = typeof(double).FullName;
                    break;
                case DataType.String:
                case DataType.Text:
                    elementType = typeof(string).FullName;
                    break;
                default:
                    {
                        ConfigFieldMetaData fieldMetaData = metaData as ConfigFieldMetaData;
                        var nestedClassMetaData = fieldMetaData == null ? null : fieldMetaData.OwnNestedClassMetaData;
                        // 内嵌类
                        if (isNestedClass)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为内嵌类，其中不能再嵌套一个类");
                        }
                        if (nestedClassMetaData.FieldNum <= 0)
                        {
                            throw new ParseExcelException($"{belongClassName}中字段{name}为内嵌类，但该列并没有定义内嵌类");
                        }
                        elementType = $"ConfigData.{belongClassName}+{nestedClassMetaData.m_classname}";
                        break;
                    }
            }
            if (listType == ConfigDataExpoter.ListType.None)
            {
                return elementType;
            }
            //else if (listType == ListType.FixedLengthList)
            //{
            //    return $"{elementType}[]";
            //}
            else
            {
                return $"System.Collections.Generic.List<{elementType}>";
            }
        }

        public bool SetValue(ConfigClassFieldHeader fieldType, string value)
        {
            switch (fieldType)
            {
                case ConfigClassFieldHeader.ClassFieldComment:
                    {
                        Comment = value;
                        return Comment != null;
                    }
                case ConfigClassFieldHeader.ClassFieldName:
                    {
                        FieldName = value.ToLower();
                        return FieldName != null;
                    }
                case ConfigClassFieldHeader.ClassFieldVisiblity:
                    {
                        OwnVisiblity = ParseEnum(value, Visiblity.Invalid);
                        return OwnVisiblity != Visiblity.Invalid;
                    }
                case ConfigClassFieldHeader.ClassFieldForeignKey:
                    {
                        ForeignKey = value;
                        return ForeignKey != null;
                    }
                case ConfigClassFieldHeader.ClassFieldIsList:
                    {
                        ListType = value;
                        return ListType != null;
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
            OwnNestedClassMetaData.m_fieldsInfo.Clear();
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
                        var listType = GetListType(OwnNestedClassMetaData.m_classname, nameArr[i], isListArr[i], out _);
                        if (listType > ConfigDataExpoter.ListType.None)
                        {
                            throw new ParseExcelException("内嵌类各字段不能是数组类型的，如需要数组类型应当另起一列");
                        }
                        var fieldInfo = new NestClassFieldInfo()
                        {
                            BelongClassName = OwnNestedClassMetaData.m_classname,
                            FieldName = nameArr[i].ToLower(),
                            Comment = commentArr[i],
                            ListType = isListArr[i],
                            OwnDataType = typeArr[i]
                        };
                        fieldInfo.RealTypeName = GetTypeName(fieldInfo, typeArr[i], isListArr[i], true);
                        OwnNestedClassMetaData.m_fieldsInfo.Add(fieldInfo);
                    }
                    return true;
                }
                else
                {
                    throw new ParseExcelException("无效类型，如果是内嵌类，内嵌类字段数量必须大于一，否则应当使用基础类型，且字段类型/备注/名称/列表类型应当一一对应，");
                }
            }

        }

        /// <summary>
        /// 解析外键，外键为None或格式不对则解析失败
        /// </summary>
        /// <param name="foreignKey"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ParseForeignKey(string foreignKey, out string type, out string key)
        {
            if (foreignKey.Equals(None))
            {
                type = key = None;
                return false;
            }
            var foreignKeys = foreignKey.Split(ForeignKeySeperator);
            if (foreignKeys.Length == 2)
            {
                type = foreignKeys[0];
                key = foreignKeys[1];
                return true;
            }
            throw new ParseExcelException($"外键格式不正确");
        }
    }
}
