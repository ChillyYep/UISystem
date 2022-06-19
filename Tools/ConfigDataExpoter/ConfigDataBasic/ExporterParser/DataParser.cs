using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConfigDataExpoter
{
    /// <summary>
    /// Excel数据解析类
    /// </summary>
    public class ExcelDataRowParser : ExcelParserBase
    {
        public ExcelDataRowParser(MultiLanguageCollector mutiLanguageProcess = null)
        {
            m_mutiLanguageProcess = mutiLanguageProcess;
        }

        public Dictionary<Type, List<object>> ParseAllExcelTableDatas(Assembly configDataAssembly, string directory, Dictionary<string, List<ConfigSheetData>> configSheetDict)
        {
            var allTableDatas = new Dictionary<Type, List<object>>();
            var files = GetAllTopDirectoryExcelFiles(directory);
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
            // 当前正在解析的表
            m_context.m_curParsingExcelName = Path.GetFileNameWithoutExtension(path);

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
            m_context.m_curParsingExcelName = string.Empty;
        }

        /// <summary>
        /// 读取数据实例
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="sheet"></param>
        /// <param name="configSheetData"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public List<object> ParseOneSheetData(Assembly assembly, ISheet sheet, ConfigSheetData configSheetData, out Type classType)
        {
            List<object> dataTable = new List<object>(Math.Max(sheet.LastRowNum + 1 - ConfigFieldMetaData.DataBeginRow, 0));
            classType = null;
            if (configSheetData.m_sheetType != SheetType.Class || !(configSheetData.m_configMetaData is ConfigClassMetaData classMetaData))
            {
                return dataTable;
            }
            // 正在解析的类型
            m_context.m_curParsingClassName = classMetaData.m_classname;

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
            m_context.m_curParsingClassName = string.Empty;
            return dataTable;
        }

        /// <summary>
        /// 解析内嵌类信息
        /// </summary>
        private void ParseNestedClassExtraTypeInfos(Assembly assembly, ConfigFieldMetaData fieldMetaData, FieldTypeInfo fieldTypeInfo,
            Dictionary<string, List<FieldTypeInfo>> nestedClassFieldTypeInfos, Dictionary<string, Type> nestedClassTypes)
        {
            // 收集内嵌类信息，不管他是不是数组的
            var fullTypeName = ConfigFieldMetaData.GetFullTypeName(fieldMetaData, fieldMetaData.OwnDataType, ConfigFieldMetaData.None);
            var nestedClassType = assembly.GetType(fullTypeName);
            if (nestedClassType == null)
            {
                throw new ParseExcelException("内嵌类解析失败");
            }
            nestedClassFieldTypeInfos[fieldMetaData.FieldName] = new List<FieldTypeInfo>();
            var fieldList = fieldMetaData.OwnNestedClassMetaData.m_fieldsInfo;

            foreach (var nestedClassFieldTypeInfo in fieldList)
            {
                var typeinfo = new FieldTypeInfo()
                {
                    m_fieldName = nestedClassFieldTypeInfo.PrivateFieldName,
                    m_dataType = nestedClassFieldTypeInfo.OwnDataType,
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
                if (m_fieldTypeInfos.TryGetValue(fieldMetaData.ColumnIndex, out var fieldTypeInfo))
                {
                    throw new ParseExcelException($"字段名称重复,Class:{classMetaData.m_classname},Field:{fieldMetaData.FieldName}");
                }
                fieldTypeInfo = new FieldTypeInfo();

                // 枚举是硬编码的，不走下面的主键或外键判断
                if (fieldMetaData.OwnDataType != DataType.Enum)
                {
                    if (fieldMetaData.FieldName.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase))
                    {
                        if (fieldMetaData.OwnDataType != DataType.Int32)
                        {
                            throw new ParseExcelException("主键类型只能是Int32的！");
                        }
                        fieldTypeInfo.m_keyType = KeyType.Primary;
                    }
                    else if (ConfigFieldMetaData.ParseForeignKey(fieldMetaData.ForeignKey, out var foreignClass, out var foreignKey))
                    {
                        if (foreignKey.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase))
                        {
                            if (fieldMetaData.OwnDataType != DataType.Int32)
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
                fieldTypeInfo.m_dataType = fieldMetaData.OwnDataType;
                fieldTypeInfo.m_fieldName = fieldMetaData.PrivateFieldName;
                fieldTypeInfo.m_fieldInfo = classType.GetField(fieldTypeInfo.m_fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                fieldTypeInfo.m_fieldMetaData = fieldMetaData;

                if (fieldTypeInfo.m_fieldInfo == null)
                {
                    throw new ParseExcelException($"字段类型解析失败,Class:{classMetaData.m_classname},Field:{fieldMetaData.FieldName}");
                }
                // 内嵌类额外的信息
                if (fieldMetaData.OwnDataType == DataType.NestedClass)
                {
                    ParseNestedClassExtraTypeInfos(assembly, fieldMetaData, fieldTypeInfo, m_nestedClassFieldTypeInfos, m_nestedClassTypes);
                }
                // 枚举额外的信息
                else if (fieldMetaData.OwnDataType == DataType.Enum)
                {
                    ParseEnumExtraTypeInfos(fieldTypeInfo);
                }

                m_fieldTypeInfos[fieldMetaData.ColumnIndex] = fieldTypeInfo;
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
                var fieldTypeInfo = nestedClassFieldInfos[x];
                m_context.m_curParsingNestedClassFieldName = fieldTypeInfo.NestedClassFieldMetaData.FieldName;
                SetFieldValue(fieldTypeInfo, nestedClassInstance, values[x]);
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
            // 先解析ID
            var idFieldMetaData = classMetaData.m_fieldsInfo.Find(fieldMetaData => fieldMetaData.FieldName.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase));
            if (idFieldMetaData == null)
            {
                throw new ParseExcelException("该行数据不存在ID列");
            }
            int idColumnIndex = idFieldMetaData.ColumnIndex;
            var idcell = row.GetCell(idColumnIndex);
            if (m_fieldTypeInfos.TryGetValue(idColumnIndex, out var idFieldTypeInfo))
            {
                var realValue = NormalizeCellValue2String(idcell);
                SetFieldValue(idFieldTypeInfo, instance, realValue);
                m_context.m_rowID = int.Parse(realValue);
            }

            foreach (var fieldMetaData in classMetaData.m_fieldsInfo)
            {
                if (fieldMetaData == idFieldMetaData)
                {
                    continue;
                }
                int columnIndex = fieldMetaData.ColumnIndex;
                var cell = row.GetCell(columnIndex);
                if (m_fieldTypeInfos.TryGetValue(columnIndex, out var fieldTypeInfo))
                {
                    m_context.m_curParsingFieldName = fieldMetaData.FieldName;
                    SetFieldValue(fieldTypeInfo, instance, NormalizeCellValue2String(cell));
                    m_context.ClearFieldCache();
                }
                else
                {
                    throw new ParseExcelException($"字段列号不匹配,Class:{classMetaData.m_classname}");
                }
            }
            m_context.m_rowID = -1;
            return instance;
        }

        private SourceInfo CreateSourceInfo(string sourceText)
        {
            return new SourceInfo()
            {
                m_className = m_context.m_curParsingClassName,
                m_rowID = m_context.m_rowID,
                m_sourceText = sourceText,
                m_fieldName = m_context.m_curParsingFieldName,
                m_fieldListIndex = m_context.m_curFieldListIndex,
                m_nestedFieldName = m_context.m_curParsingNestedClassFieldName,
                m_nestedFieldListIndex = m_context.m_curNestedClassFieldListIndex,
            };
        }

        private string AddLanguageSourceKey(string sourceText)
        {
            if (m_mutiLanguageProcess == null)
            {
                return sourceText;
            }
            if (string.IsNullOrEmpty(m_context.m_curParsingExcelName) || string.IsNullOrEmpty(m_context.m_curParsingClassName))
            {
                throw new ParseExcelException("当前没有正在解析的Excel或Class型的Sheet!");
            }
            return m_mutiLanguageProcess.AddText(m_context.m_curParsingExcelName, m_context.m_curParsingClassName, CreateSourceInfo(sourceText));
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
        private void SetKeyInfoIfPossible(FieldTypeInfo fieldTypeInfo, int value, string curClassName)
        {
            // 如果是主键，添加主键值，配置类有且仅有一个主键，且类型为整型
            if (fieldTypeInfo.m_keyType == KeyType.Primary)
            {
                m_keyRelations.AddPrimaryKey(curClassName, value);
            }
            else if (fieldTypeInfo.m_keyType == KeyType.Foreign)
            {
                if (ConfigFieldMetaData.ParseForeignKey(fieldTypeInfo.FieldMetaData.ForeignKey, out var foreignClass, out var foreignKey))
                {
                    m_keyRelations.AddForeignKey(curClassName, foreignClass, value);
                }
            }
        }


        /// <summary>
        /// 设置域值
        /// </summary>
        private void SetFieldValue(FieldTypeInfo fieldTypeInfo, object instance, string value)
        {
            var fieldInfo = fieldTypeInfo.m_fieldInfo;
            var dataType = fieldTypeInfo.m_dataType;
            var fieldMetaData = fieldTypeInfo.FieldMetaData;
            var fieldName = fieldTypeInfo.m_fieldName;
            var foreignKeys = fieldTypeInfo.FieldMetaData == null ? ConfigFieldMetaData.None : fieldTypeInfo.FieldMetaData.ForeignKey;

            var isList = fieldTypeInfo.m_listType > ListType.None;
            const char seperator = ConfigFieldMetaData.ListSeperator;
            const string listStringSeperator = ConfigFieldMetaData.ListStringSeperator;
            string[] values = null;
            // 如果是数组，需要分解字符串
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

            // 每种类型都分成List和非List两种处理
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
                            SetKeyInfoIfPossible(fieldTypeInfo, intValue, fieldTypeInfo.m_fieldMetaData.BelongClassName);
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        var intValue = Int32.Parse(value);
                        SetKeyInfoIfPossible(fieldTypeInfo, intValue, fieldTypeInfo.m_fieldMetaData.BelongClassName);
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
                case DataType.Text:
                    if (isList)
                    {
                        List<string> realValues = new List<string>(values.Length);
                        for (int i = 0; i < values.Length; ++i)
                        {
                            if (fieldTypeInfo.m_isNestedClassField)
                            {
                                m_context.m_curNestedClassFieldListIndex = i;
                            }
                            else
                            {
                                m_context.m_curFieldListIndex = i;
                            }
                            realValues.Add(AddLanguageSourceKey(values[i]));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, AddLanguageSourceKey(value));
                    }
                    break;
                case DataType.NestedClass:
                    if (!m_nestedClassTypes.TryGetValue(fieldName, out var nestedFieldType))
                    {
                        throw new ParseExcelException($"不存在该内嵌类{fieldMetaData.FieldName}");
                    }
                    // 暂存状态
                    if (isList)
                    {
                        var listInstance = Activator.CreateInstance(fieldInfo.FieldType);
                        var addMethod = fieldTypeInfo.m_addMethod;
                        if (addMethod == null)
                        {
                            throw new ParseExcelException($"{fieldTypeInfo.m_fieldName}字段不存在Add方法");
                        }
                        for (int i = 0; i < values.Length; ++i)
                        {
                            m_context.m_curFieldListIndex = i;
                            var obj = ParseNestedClassData(m_nestedClassFieldTypeInfos, values[i], fieldMetaData, nestedFieldType);
                            // 恢复状态
                            addMethod.Invoke(listInstance, new object[] { obj });
                        }
                        fieldInfo.SetValue(instance, listInstance);
                    }
                    else
                    {
                        var nestedClassInstance = ParseNestedClassData(m_nestedClassFieldTypeInfos, value, fieldMetaData, nestedFieldType);
                        // 恢复状态
                        fieldInfo.SetValue(instance, nestedClassInstance);
                    }
                    break;
                    //throw new ParseExcelException("无法直接解析内嵌类");
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

        public class Context
        {
            #region 逐表
            public string m_curParsingExcelName;
            #endregion

            #region 逐Sheet
            public string m_curParsingClassName;
            #endregion

            #region 逐行
            /// <summary>
            /// 行ID
            /// </summary>
            public int m_rowID;
            #endregion

            public string m_curParsingFieldName;

            public int m_curFieldListIndex;

            public string m_curParsingNestedClassFieldName;

            public int m_curNestedClassFieldListIndex;

            public void ClearFieldCache()
            {
                m_curParsingFieldName = "";
                m_curParsingNestedClassFieldName = "";
                m_curFieldListIndex = 0;
                m_curNestedClassFieldListIndex = 0;
            }
        }

        private readonly Context m_context = new Context();

        /// <summary>
        /// 主键外键关系收集
        /// </summary>
        private readonly KeyRelations m_keyRelations = new KeyRelations();

        /// <summary>
        /// 多语言处理
        /// </summary>
        private MultiLanguageCollector m_mutiLanguageProcess;

        /// <summary>
        /// 每一列的域类型信息
        /// </summary>
        private readonly Dictionary<int, FieldTypeInfo> m_fieldTypeInfos = new Dictionary<int, FieldTypeInfo>();

        /// <summary>
        /// 内嵌类及其域信息
        /// </summary>
        private readonly Dictionary<string, List<FieldTypeInfo>> m_nestedClassFieldTypeInfos = new Dictionary<string, List<FieldTypeInfo>>();

        /// <summary>
        /// 内嵌类及其类信息
        /// </summary>
        private readonly Dictionary<string, Type> m_nestedClassTypes = new Dictionary<string, Type>();
    }
}
