using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    /// <summary>
    /// Excel数据解析类
    /// </summary>
    class ExcelDataRowParser : ExcelParserBase
    {
        public Dictionary<Type, List<object>> ParseAllExcelTableDatas(Assembly configDataAssembly, string directory, Dictionary<string, List<ConfigSheetData>> configSheetDict)
        {
            var allTableDatas = new Dictionary<Type, List<object>>();
            var files = Directory.GetFiles(directory, "*.xlsx", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!configSheetDict.TryGetValue(file, out var fileSheetDatas))
                {
                    throw new ParseExcelException("找不到目标文件的类型信息!");
                }
                ParseOneExcelTableData(configDataAssembly, file, fileSheetDatas, ref allTableDatas);
            }
            // 检测主键、外键合法性
            m_keyRelations.CheckForiegnKeySafty();
            return allTableDatas;
        }

        /// <summary>
        /// 解析Table数据
        /// </summary>
        /// <param name="configDataAssembly"></param>
        /// <param name="path"></param>
        /// <param name="sheetDatas"></param>
        /// <param name="allTableData"></param>
        public void ParseOneExcelTableData(Assembly configDataAssembly, string path, List<ConfigSheetData> sheetDatas, ref Dictionary<Type, List<object>> allTableData)
        {
            var sheets = GetSheets(path);
            if (sheets.Count <= 0)
            {
                return;
            }
            // 解析多个Sheet
            foreach (var sheet in sheets)
            {
                var sheetData = sheetDatas.Find(element => element.m_sheetName == sheet.SheetName);
                if (sheetData == null)
                {
                    continue;
                }
                if (sheetData.m_sheetType != SheetType.Class)
                {
                    continue;
                }
                var dataTable = ParseOneSheetData(configDataAssembly, sheet, sheetData, out var classType);
                if (allTableData.ContainsKey(classType))
                {
                    throw new ParseExcelException($"出现同名配置类{classType.FullName}");
                }
                allTableData[classType] = dataTable;
            }
        }

        /// <summary>
        /// 读取数据实例
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="sheet"></param>
        /// <param name="configSheetData"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        private List<object> ParseOneSheetData(Assembly assembly, ISheet sheet, ConfigSheetData configSheetData, out Type classType)
        {
            List<object> dataTable = new List<object>(Math.Max(sheet.LastRowNum + 1 - ConfigFieldMetaData.DataBeginRow, 0));
            classType = null;
            if (configSheetData.m_sheetType != SheetType.Class || !(configSheetData.m_configMetaData is ConfigClassMetaData classMetaData))
            {
                return dataTable;
            }
            // 配置类
            var classFullName = $"ConfigData.{classMetaData.m_classname}";
            classType = assembly.GetType(classFullName);
            if (classType == null)
            {
                throw new ParseExcelException($"不存在类型为{classFullName}的配置类");
            }
            // 提前解析类型信息
            ParseSheetFieldTypeInfos(assembly, classType, classMetaData);

            for (int i = ConfigFieldMetaData.DataBeginRow; i <= sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                // 一行数据一个实例
                var instance = ParseSingleRowData(row, classMetaData, classType);
                dataTable.Add(instance);
            }
            return dataTable;
        }

        /// <summary>
        /// 解析内嵌类信息
        /// </summary>
        private void ParseNestedClassExtraTypeInfos(Assembly assembly, ConfigFieldMetaData fieldMetaData, FieldTypeInfo fieldTypeInfo,
            Dictionary<string, List<FieldTypeInfo>> nestedClassFieldTypeInfos, Dictionary<string, Type> nestedClassTypes)
        {
            // 收集内嵌类信息，不管他是不是数组的
            var fullTypeName = ConfigFieldMetaData.GetFullTypeName(fieldMetaData, fieldMetaData.DataType, ConfigFieldMetaData.None);
            var nestedClassType = assembly.GetType(fullTypeName);
            if (nestedClassType == null)
            {
                throw new ParseExcelException("内嵌类解析失败");
            }
            nestedClassFieldTypeInfos[fieldMetaData.FieldName] = new List<FieldTypeInfo>();
            var fieldList = fieldMetaData.m_nestedClassMetaData.m_fieldsInfo;

            foreach (var nestedClassFieldTypeInfo in fieldList)
            {
                var typeinfo = new FieldTypeInfo()
                {
                    m_fieldName = nestedClassFieldTypeInfo.PrivateFieldName,
                    m_dataType = nestedClassFieldTypeInfo.DataType,
                    m_isNestedClassField = true,
                    m_keyType = KeyType.None //内嵌类的域不会是逐渐或外键 
                };
                typeinfo.m_listType = ConfigFieldMetaData.GetListType(nestedClassFieldTypeInfo.BelongClassName, nestedClassFieldTypeInfo.FieldName, nestedClassFieldTypeInfo.ListType, out var listCount);
                typeinfo.m_listCount = listCount;
                typeinfo.m_fieldInfo = nestedClassType.GetField(typeinfo.m_fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                typeinfo.m_fieldMetaData = nestedClassFieldTypeInfo;
                nestedClassFieldTypeInfos[fieldMetaData.FieldName].Add(typeinfo);
            }
            nestedClassTypes[fieldTypeInfo.m_fieldName] = nestedClassType;

            if (fieldTypeInfo.m_listType > ListType.None)
            {
                fieldTypeInfo.m_addMethod = fieldTypeInfo.m_fieldInfo.FieldType.GetMethod("Add", new Type[] { nestedClassType });
            }
        }

        /// <summary>
        /// 解析枚举类型信息
        /// </summary>
        /// <param name="fieldTypeInfo"></param>
        private void ParseEnumExtraTypeInfos(FieldTypeInfo fieldTypeInfo)
        {
            // 枚举数组需要Add方法
            if (fieldTypeInfo.m_listType > ListType.None)
            {
                var listType = fieldTypeInfo.m_fieldInfo.FieldType;
                if (listType.IsGenericType)
                {
                    var elementType = listType.GenericTypeArguments[0];
                    fieldTypeInfo.m_addMethod = fieldTypeInfo.m_fieldInfo.FieldType.GetMethod("Add", new Type[] { elementType });
                }
            }
        }

        /// <summary>
        /// 解析一些类型信息，为实例化行数据做准备
        /// </summary>
        private void ParseSheetFieldTypeInfos(Assembly assembly, Type classType, ConfigClassMetaData classMetaData)
        {
            m_fieldTypeInfos.Clear();
            m_nestedClassFieldTypeInfos.Clear();
            m_nestedClassTypes.Clear();

            foreach (var fieldMetaData in classMetaData.m_fieldsInfo)
            {
                if (m_fieldTypeInfos.TryGetValue(fieldMetaData.m_columnIndex, out var fieldTypeInfo))
                {
                    throw new ParseExcelException($"字段名称重复,Class:{classMetaData.m_classname},Field:{fieldMetaData.FieldName}");
                }
                fieldTypeInfo = new FieldTypeInfo();

                // 枚举是硬编码的，不走下面的主键或外键判断
                if (fieldMetaData.DataType != DataType.Enum)
                {
                    if (fieldMetaData.FieldName.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase))
                    {
                        if (fieldMetaData.DataType != DataType.Int32)
                        {
                            throw new ParseExcelException("主键类型只能是Int32的！");
                        }
                        fieldTypeInfo.m_keyType = KeyType.Primary;
                    }
                    else if (ConfigFieldMetaData.ParseForeignKey(fieldMetaData.m_foreignKey, out var foreignClass, out var foreignKey))
                    {
                        if (foreignKey.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase))
                        {
                            if (fieldMetaData.DataType != DataType.Int32)
                            {
                                throw new ParseExcelException("外键类型只能是Int32的！");
                            }
                            fieldTypeInfo.m_keyType = KeyType.Foreign;
                        }
                        else
                        {
                            throw new ParseExcelException($"配置类{classMetaData.m_classname}的{fieldMetaData.FieldName}外键没有使用{foreignClass}.ID的格式");
                        }
                    }
                }

                // 通用类型解析，为后面数据实例化做准备
                fieldTypeInfo.m_isNestedClassField = false;
                fieldTypeInfo.m_listType = ConfigFieldMetaData.GetListType(fieldMetaData.BelongClassName, fieldMetaData.FieldName, fieldMetaData.ListType, out var listCount);
                fieldTypeInfo.m_listCount = listCount;
                fieldTypeInfo.m_dataType = fieldMetaData.DataType;
                fieldTypeInfo.m_fieldName = fieldMetaData.PrivateFieldName;
                fieldTypeInfo.m_fieldInfo = classType.GetField(fieldTypeInfo.m_fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                fieldTypeInfo.m_fieldMetaData = fieldMetaData;

                if (fieldTypeInfo.m_fieldInfo == null)
                {
                    throw new ParseExcelException($"字段类型解析失败,Class:{classMetaData.m_classname},Field:{fieldMetaData.FieldName}");
                }
                // 内嵌类额外的信息
                if (fieldMetaData.DataType == DataType.NestedClass)
                {
                    ParseNestedClassExtraTypeInfos(assembly, fieldMetaData, fieldTypeInfo, m_nestedClassFieldTypeInfos, m_nestedClassTypes);
                }
                // 枚举额外的信息
                else if (fieldMetaData.DataType == DataType.Enum)
                {
                    ParseEnumExtraTypeInfos(fieldTypeInfo);
                }

                m_fieldTypeInfos[fieldMetaData.m_columnIndex] = fieldTypeInfo;
            }
        }

        /// <summary>
        /// 解析内嵌类
        /// </summary>
        private object ParseNestedClassData(Dictionary<string, List<FieldTypeInfo>> nestedClassFieldTypeInfos, string nestedClassInstanceStr, ConfigFieldMetaData fieldInfo, Type nestedFieldType)
        {
            var nestedClassInstance = Activator.CreateInstance(nestedFieldType);
            if (nestedClassInstance == null)
            {
                throw new ParseExcelException($"创建内嵌类{fieldInfo.FieldName}实例失败");
            }
            // 解析每一个字段
            var values = nestedClassInstanceStr.Split(ConfigFieldMetaData.NestedClassMetaData.seperator);
            if (!nestedClassFieldTypeInfos.TryGetValue(fieldInfo.FieldName, out var nestedClassFieldInfos))
            {
                throw new ParseExcelException($"内嵌类{fieldInfo.FieldName}获取域信息失败");
            }
            for (int x = 0; x < nestedClassFieldInfos.Count; ++x)
            {
                SetFieldValue(nestedClassFieldInfos[x], nestedClassInstance, values[x]);
            }
            return nestedClassInstance;
        }

        /// <summary>
        /// 解析单行数据
        /// </summary>
        private object ParseSingleRowData(IRow row, ConfigClassMetaData classMetaData, Type classType)
        {
            var instance = Activator.CreateInstance(classType);
            if (instance == null)
            {
                throw new ParseExcelException("创建数据实例出错");
            }

            foreach (var fieldMetaData in classMetaData.m_fieldsInfo)
            {
                int columnIndex = fieldMetaData.m_columnIndex;
                var cell = row.GetCell(columnIndex);
                if (m_fieldTypeInfos.TryGetValue(columnIndex, out var fieldTypeInfo))
                {
                    var fieldInfo = fieldTypeInfo.FieldMetaData;
                    var fieldType = fieldTypeInfo.m_fieldInfo;
                    var fieldName = fieldTypeInfo.m_fieldName;
                    if (fieldInfo.DataType == DataType.NestedClass)
                    {
                        if (!m_nestedClassTypes.TryGetValue(fieldName, out var nestedFieldType))
                        {
                            throw new ParseExcelException($"不存在该内嵌类{fieldInfo.FieldName}");
                        }
                        if (fieldTypeInfo.m_listType == ListType.None)
                        {
                            var nestedClassInstance = ParseNestedClassData(m_nestedClassFieldTypeInfos, NormalizeCellValue2String(cell), fieldInfo, nestedFieldType);
                            fieldType.SetValue(instance, nestedClassInstance);
                        }
                        else
                        {
                            var listInstance = Activator.CreateInstance(fieldType.FieldType);
                            var addMethod = fieldTypeInfo.m_addMethod;
                            if (addMethod == null)
                            {
                                throw new ParseExcelException($"{fieldTypeInfo.m_fieldName}字段不存在Add方法");
                            }
                            var listValue = NormalizeCellValue2String(cell).Split(ConfigFieldMetaData.ListSeperator);
                            foreach (var value in listValue)
                            {
                                var obj = ParseNestedClassData(m_nestedClassFieldTypeInfos, value, fieldInfo, nestedFieldType);
                                addMethod.Invoke(listInstance, new object[] { obj });
                            }
                            fieldType.SetValue(instance, listInstance);
                        }
                    }
                    else
                    {
                        var value = NormalizeCellValue2String(cell);

                        SetFieldValue(fieldTypeInfo, instance, value, fieldInfo.m_foreignKey, true);
                    }
                }
                else
                {
                    throw new ParseExcelException($"字段列号不匹配,Class:{classMetaData.m_classname}");
                }
            }
            return instance;
        }

        /// <summary>
        /// 统一转换成字符串
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private string NormalizeCellValue2String(ICell cell)
        {
            if (cell == null)
            {
                return string.Empty;
            }
            switch (cell.CellType)
            {
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Numeric:
                    var value = cell.NumericCellValue.ToString();
                    return value;
                case CellType.String:
                    return cell.StringCellValue;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 设置主键外键信息
        /// </summary>
        /// <param name="fieldTypeInfo"></param>
        /// <param name="value"></param>
        /// <param name="curClassName"></param>
        private void SetKeyInfo(FieldTypeInfo fieldTypeInfo, int value, string curClassName)
        {
            // 如果是主键，添加主键值，配置类有且仅有一个主键，且类型为整型
            if (fieldTypeInfo.m_keyType == KeyType.Primary)
            {
                m_keyRelations.AddPrimaryKey(curClassName, value);
            }
            else if (fieldTypeInfo.m_keyType == KeyType.Foreign)
            {
                if (ConfigFieldMetaData.ParseForeignKey(fieldTypeInfo.FieldMetaData.m_foreignKey, out var foreignClass, out var foreignKey))
                {
                    m_keyRelations.AddForeignKey(curClassName, foreignClass, value);
                }
            }
        }


        /// <summary>
        /// 设置域值
        /// </summary>
        private void SetFieldValue(FieldTypeInfo fieldTypeInfo, object instance, string value, string foreignKeys = "", bool checkKeys = false)
        {
            var fieldInfo = fieldTypeInfo.m_fieldInfo;
            var dataType = fieldTypeInfo.m_dataType;
            var isList = fieldTypeInfo.m_listType > ListType.None;
            const char seperator = ConfigFieldMetaData.ListSeperator;
            const string listStringSeperator = ConfigFieldMetaData.ListStringSeperator;
            string[] values = null;
            if (isList)
            {
                if (string.IsNullOrEmpty(value))
                {
                    values = new string[0];
                }
                else
                {
                    if (fieldTypeInfo.m_dataType == DataType.String || fieldTypeInfo.m_dataType == DataType.Text)
                    {
                        // 引号开头或引号结尾
                        if (value.StartsWith("\"") || value.EndsWith("\""))
                        {
                            // 但是首尾不是成对出现的
                            if (!value.StartsWith("\"") || !value.EndsWith("\""))
                            {
                                throw new ParseExcelException($"字符串数组，引号必须在首尾同时出现，列名:{fieldTypeInfo.m_fieldName}");
                            }
                            // 删除尾部引号
                            value = value.Substring(0, value.Length - 1);
                            // 删除首部引号
                            value = value.Substring(1, value.Length - 1);
                            values = value.Split(new string[] { listStringSeperator }, StringSplitOptions.None);
                        }
                        else
                        {
                            // 如果不是引号开头或结尾，默认以通用分隔符区分数组元素
                            //values = value.Split(seperator);
                            throw new ParseExcelException($"字符串数组，必须用引号隔开彼此，列名:{fieldTypeInfo.m_fieldName}");
                        }
                    }
                    else
                    {
                        values = value.Split(seperator);
                    }
                }
                // 固定长度列表限制长度
                if (fieldTypeInfo.m_listType == ListType.FixedLengthList && values.Length != fieldTypeInfo.m_listCount)
                {
                    throw new ParseExcelException($"{fieldTypeInfo.m_fieldName}的类型为固定长度列表，但长度不匹配，目标长度：{fieldTypeInfo.m_listCount}，实际长度{values.Length}");
                }
            }
            switch (dataType)
            {
                case DataType.Enum:
                    var fieldType = fieldInfo.FieldType;
                    if (ConfigFieldMetaData.ParseForeignKey(foreignKeys, out var foreignClass, out var foreignKey))
                    {
                        if (foreignKey.Equals(ConfigEnumMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase) ||
                            foreignKey.Equals(ConfigEnumMetaData.ValuePrimaryKey, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                if (isList)
                                {
                                    var enumList = Activator.CreateInstance(fieldType);
                                    if (fieldType.IsGenericType && enumList != null)
                                    {
                                        var elementType = fieldType.GenericTypeArguments[0];
                                        var addMethod = fieldTypeInfo.m_addMethod;
                                        if (addMethod == null)
                                        {
                                            throw new ParseExcelException($"{fieldTypeInfo.m_fieldName}字段不存在Add方法");
                                        }
                                        foreach (var element in values)
                                        {
                                            if (element.Contains(ConfigFieldMetaData.NestedClassMetaData.seperator))
                                            {
                                                throw new ParseExcelException($"枚举列表里不能有\"{ConfigFieldMetaData.NestedClassMetaData.seperator}\"");
                                            }
                                            var flag = Enum.Parse(elementType, element);
                                            addMethod.Invoke(enumList, new object[] { flag });
                                        }
                                    }
                                    fieldInfo.SetValue(instance, enumList);
                                }
                                else
                                {
                                    var flag = Enum.Parse(fieldType, value);
                                    if (flag == null)
                                    {
                                        throw new ParseExcelException($"枚举值{fieldTypeInfo.m_fieldName}不能为空！");
                                    }
                                    else
                                    {
                                        fieldInfo.SetValue(instance, flag);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                throw new ParseExcelException($"枚举类型转换失败,Enum:{foreignClass},value:{ value}不存在", e);
                            }
                        }
                    }
                    break;
                case DataType.Int8:
                    if (isList)
                    {
                        List<byte> realValues = new List<byte>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(byte.Parse(v));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, byte.Parse(value));
                    }
                    break;
                case DataType.Int16:
                    if (isList)
                    {
                        List<Int16> realValues = new List<Int16>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(Int16.Parse(v));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, Int16.Parse(value));
                    }
                    break;
                case DataType.Int32:
                    if (isList)
                    {
                        List<Int32> realValues = new List<Int32>(values.Length);
                        foreach (var v in values)
                        {
                            var intValue = Int32.Parse(v);
                            realValues.Add(intValue);
                            if (checkKeys)
                            {
                                SetKeyInfo(fieldTypeInfo, intValue, fieldTypeInfo.FieldMetaData.BelongClassName);
                            }
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        var intValue = Int32.Parse(value);
                        if (checkKeys)
                        {
                            SetKeyInfo(fieldTypeInfo, intValue, fieldTypeInfo.FieldMetaData.BelongClassName);
                        }
                        fieldInfo.SetValue(instance, intValue);
                    }
                    break;
                case DataType.Int64:
                    if (isList)
                    {
                        List<Int64> realValues = new List<Int64>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(Int64.Parse(v));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, Int64.Parse(value));
                    }
                    break;
                case DataType.Boolean:
                    if (isList)
                    {
                        List<Boolean> realValues = new List<Boolean>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(Boolean.Parse(v));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, Boolean.Parse(value));
                    }
                    break;
                case DataType.Float:
                    if (isList)
                    {
                        List<float> realValues = new List<float>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(float.Parse(v));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, float.Parse(value));
                    }
                    break;
                case DataType.Double:
                    if (isList)
                    {
                        List<Double> realValues = new List<Double>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(Double.Parse(v));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, Double.Parse(value));
                    }
                    break;
                case DataType.String:
                case DataType.Text:
                    if (isList)
                    {
                        List<string> realValues = new List<string>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(v);
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, value);
                    }
                    break;
                case DataType.NestedClass:
                    throw new ParseExcelException("无法直接解析内嵌类");
            }
        }

        public enum KeyType
        {
            None,
            /// <summary>
            /// 主键
            /// </summary>
            Primary,
            /// <summary>
            /// 外键
            /// </summary>
            Foreign
        }

        /// <summary>
        /// 域类型相关信息
        /// </summary>
        public class FieldTypeInfo
        {
            public bool m_isNestedClassField;
            public KeyType m_keyType;
            public DataType m_dataType;
            public FieldInfo m_fieldInfo;
            public string m_fieldName;
            public ListType m_listType;
            public int m_listCount = int.MaxValue;
            public MethodBase m_addMethod;
            public ConfigFieldMetaDataBase m_fieldMetaData;

            public ConfigFieldMetaData FieldMetaData
            {
                get
                {
                    if (!m_isNestedClassField)
                    {
                        return m_fieldMetaData as ConfigFieldMetaData;
                    }
                    return null;
                }
            }
            public ConfigFieldMetaData.NestClassFieldInfo NestedClassFieldMetaData
            {
                get
                {
                    if (m_isNestedClassField)
                    {
                        return m_fieldMetaData as ConfigFieldMetaData.NestClassFieldInfo;
                    }
                    return null;
                }
            }
        }

        private KeyRelations m_keyRelations = new KeyRelations();

        /// <summary>
        /// 每一列的域类型信息
        /// </summary>
        private Dictionary<int, FieldTypeInfo> m_fieldTypeInfos = new Dictionary<int, FieldTypeInfo>();

        /// <summary>
        /// 内嵌类及其域信息
        /// </summary>
        private Dictionary<string, List<FieldTypeInfo>> m_nestedClassFieldTypeInfos = new Dictionary<string, List<FieldTypeInfo>>();

        /// <summary>
        /// 内嵌类及其类信息
        /// </summary>
        private Dictionary<string, Type> m_nestedClassTypes = new Dictionary<string, Type>();
    }
}
