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
    /// 数据解析类
    /// </summary>
    class DataParser : ExcelParserBase
    {
        public class TypeInfo
        {
            public DataType m_dataType;
            public FieldInfo m_fieldInfo;
            public string m_fieldName;
            public bool m_isList;
        }

        public Dictionary<Type, List<object>> ParseAllTableDatas(Assembly configDataAssembly, string directory, Dictionary<string, List<ConfigSheetData>> configSheetDict)
        {
            var allTableDatas = new Dictionary<Type, List<object>>();
            var files = Directory.GetFiles(directory, "*.xlsx", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!configSheetDict.TryGetValue(file, out var fileSheetDatas))
                {
                    throw new ParseExcelException("找不到目标文件的类型信息!");
                }
                ParseTableData(configDataAssembly, file, fileSheetDatas, ref allTableDatas);
            }
            return allTableDatas;
        }

        /// <summary>
        /// 解析Table数据
        /// </summary>
        /// <param name="configDataAssembly"></param>
        /// <param name="path"></param>
        /// <param name="sheetDatas"></param>
        /// <param name="allTableData"></param>
        public void ParseTableData(Assembly configDataAssembly, string path, List<ConfigSheetData> sheetDatas, ref Dictionary<Type, List<object>> allTableData)
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
                var dataTable = ReadDataTable(configDataAssembly, sheet, sheetData, out var classType);
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
        private List<object> ReadDataTable(Assembly assembly, ISheet sheet, ConfigSheetData configSheetData, out Type classType)
        {
            List<object> dataTable = new List<object>(Math.Max(sheet.LastRowNum + 1 - ConfigFieldMetaData.DataBeginRow, 0));
            classType = null;
            if (configSheetData.m_sheetType != SheetType.Class || !(configSheetData.m_configMetaData is ConfigClassMetaData classMetaData))
            {
                return dataTable;
            }
            var classFullName = $"ConfigData.{classMetaData.m_classname}";
            classType = assembly.GetType(classFullName);
            if (classType == null)
            {
                throw new ParseExcelException($"不存在类型为{classFullName}的配置类");
            }
            var fieldTypeInfos = new Dictionary<int, TypeInfo>();

            var fieldMetaInfos = new Dictionary<int, ConfigFieldMetaData>();

            var nestedClassFieldTypeInfos = new Dictionary<string, List<TypeInfo>>();

            var nestedClassTypes = new Dictionary<string, Type>();

            foreach (var fieldMetaData in classMetaData.m_fieldsInfo)
            {
                if (fieldTypeInfos.TryGetValue(fieldMetaData.m_columnIndex, out var fieldTypeInfo))
                {
                    throw new ParseExcelException($"字段名称重复,Class:{classMetaData.m_classname},Field:{fieldMetaData.m_fieldName}");
                }
                fieldTypeInfo = new TypeInfo();
                fieldTypeInfo.m_isList = ConfigFieldMetaData.GetListType(fieldMetaData.m_listType) != ListType.None;
                fieldTypeInfo.m_dataType = fieldMetaData.m_dataType;
                fieldTypeInfo.m_fieldName = "_" + fieldMetaData.m_fieldName;
                fieldTypeInfo.m_fieldInfo = classType.GetField(fieldTypeInfo.m_fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (fieldTypeInfo.m_fieldInfo == null)
                {
                    throw new ParseExcelException($"字段类型解析失败,Class:{classMetaData.m_classname},Field:{fieldMetaData.m_fieldName}");
                }
                if (fieldMetaData.m_dataType == DataType.NestedClass)
                {
                    var fullTypeName = ConfigFieldMetaData.GetFullTypeName(fieldMetaData, fieldMetaData.m_dataType, fieldMetaData.m_listType);
                    var nestedClassType = assembly.GetType(fullTypeName);
                    if (nestedClassType == null)
                    {
                        throw new ParseExcelException("内嵌类解析失败");
                    }
                    nestedClassFieldTypeInfos[fieldMetaData.m_fieldName] = new List<TypeInfo>();
                    var fieldList = fieldMetaData.m_nestedClassMetaData.m_fieldsInfo;

                    foreach (var nestedClassFieldTypeInfo in fieldList)
                    {
                        var typeinfo = new TypeInfo()
                        {
                            m_fieldName = "_" + nestedClassFieldTypeInfo.m_fieldName,
                            m_dataType = nestedClassFieldTypeInfo.m_dataType
                        };
                        typeinfo.m_isList = ConfigFieldMetaData.GetListType(nestedClassFieldTypeInfo.m_listType) != ListType.None;
                        typeinfo.m_fieldInfo = nestedClassType.GetField(typeinfo.m_fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        nestedClassFieldTypeInfos[fieldMetaData.m_fieldName].Add(typeinfo);
                    }
                    nestedClassTypes[fieldTypeInfo.m_fieldName] = nestedClassType;
                }
                fieldTypeInfos[fieldMetaData.m_columnIndex] = fieldTypeInfo;
                fieldMetaInfos[fieldMetaData.m_columnIndex] = fieldMetaData;
            }

            for (int i = ConfigFieldMetaData.DataBeginRow; i <= sheet.LastRowNum; ++i)
            {
                var instance = Activator.CreateInstance(classType);
                if (instance == null)
                {
                    throw new ParseExcelException("创建数据实例出错");
                }
                var row = sheet.GetRow(i);
                for (int j = 0; j < row.LastCellNum; ++j)
                {
                    var cell = row.GetCell(j);
                    if (cell == null)
                    {
                        continue;
                    }
                    if (fieldTypeInfos.TryGetValue(j, out var fieldTypeInfo))
                    {
                        var fieldInfo = fieldMetaInfos[j];
                        var fieldType = fieldTypeInfo.m_fieldInfo;
                        var fieldName = fieldTypeInfo.m_fieldName;
                        if (fieldInfo.m_dataType == DataType.NestedClass)
                        {
                            if (fieldTypeInfo.m_isList)
                            {
                                throw new ParseExcelException($"内嵌类不支持数组化");
                            }
                            if (!nestedClassTypes.TryGetValue(fieldName, out var nestedFieldType))
                            {
                                throw new ParseExcelException($"不存在该内嵌类{fieldInfo.m_fieldName}");
                            }
                            var nestedClassInstance = Activator.CreateInstance(nestedFieldType);
                            if (nestedClassInstance == null)
                            {
                                throw new ParseExcelException($"创建内嵌类{fieldInfo.m_fieldName}实例失败");
                            }
                            var nestMetaData = fieldInfo.m_nestedClassMetaData;
                            var values = cell.StringCellValue.Split(ConfigFieldMetaData.NestedClassMetaData.seperator);
                            if (!nestedClassFieldTypeInfos.TryGetValue(fieldInfo.m_fieldName, out var nestedClassFieldInfos))
                            {
                                throw new ParseExcelException($"内嵌类{fieldInfo.m_fieldName}获取域信息失败");
                            }
                            for (int x = 0; x < nestedClassFieldInfos.Count; ++x)
                            {
                                var nestedFieldInfo = nestedClassFieldInfos[x].m_fieldInfo;
                                var dataType = nestedClassFieldInfos[x].m_dataType;
                                var nestedFieldName = nestedClassFieldInfos[x].m_fieldName;
                                SetFieldValue(nestedFieldInfo, nestedClassInstance, values[x], dataType);
                            }
                            fieldType.SetValue(instance, nestedClassInstance);
                        }
                        else
                        {
                            SetFieldValue(fieldType, instance, NormalizeCellValue2String(cell), fieldInfo.m_dataType, fieldTypeInfo.m_isList, fieldInfo.m_foreignKey);
                        }
                    }
                    else
                    {
                        throw new ParseExcelException($"字段列号不匹配,Class:{classMetaData.m_classname}");
                    }
                }
                dataTable.Add(instance);
            }
            return dataTable;
        }

        /// <summary>
        /// 统一转换成字符串
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private string NormalizeCellValue2String(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Numeric:
                    return cell.NumericCellValue.ToString();
                case CellType.String:
                    return cell.StringCellValue;
                default:
                    return "None";
            }
        }

        /// <summary>
        /// 设置域值
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <param name="dataType"></param>
        /// <param name="isList"></param>
        /// <param name="foreignKeys"></param>
        private void SetFieldValue(FieldInfo fieldInfo, object instance, string value, DataType dataType, bool isList = false, string foreignKeys = "")
        {
            const char seperator = ConfigFieldMetaData.ListSeperator;
            string[] values = null;
            if (isList)
            {
                values = value.Split(seperator);
            }
            switch (dataType)
            {
                case DataType.Enum:
                    var fieldType = fieldInfo.FieldType;
                    if (ConfigFieldMetaData.ParseForeignKey(foreignKeys, out var foreignClass, out var foreignKey))
                    {
                        if (foreignKey == ConfigEnumMetaData.IDPrimaryKey)
                        {
                            //if (values != null)
                            //{
                            //    List<int> realValues = new List<int>(values.Length);
                            //    foreach (var v in values)
                            //    {
                            //        realValues.Add(int.Parse(v));
                            //    }
                            //    fieldInfo.SetValue(instance, realValues);
                            //}
                            //else
                            //{
                            //    fieldInfo.SetValue(instance, Enum.Parse(fieldType, value));
                            //}
                            if (values != null)
                            {
                                throw new ParseExcelException("Enum类型不支持数组化");
                            }
                            fieldInfo.SetValue(instance, Enum.Parse(fieldType, value));
                        }
                    }
                    break;
                case DataType.Int8:
                    if (values != null)
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
                    if (values != null)
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
                    if (values != null)
                    {
                        List<Int32> realValues = new List<Int32>(values.Length);
                        foreach (var v in values)
                        {
                            realValues.Add(Int32.Parse(v));
                        }
                        fieldInfo.SetValue(instance, realValues);
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, Int32.Parse(value));
                    }
                    break;
                case DataType.Int64:
                    if (values != null)
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
                    if (values != null)
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
                    if (values != null)
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
                    if (values != null)
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
                    if (values != null)
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

    }
}
