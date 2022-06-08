using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    /// <summary>
    /// Excel解析异常
    /// </summary>
    public class ParseExcelException : Exception
    {
        public ParseExcelException() : base()
        {
        }

        public ParseExcelException(string message) : base(message)
        {
        }

        public ParseExcelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParseExcelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    class ParseExcelProcess
    {
        public ParseExcelProcess(string mainDirecotry, string exportCodeDiretory, string exportDataDirecotry, string exportCodeFileName)
        {
            m_mainDirectory = mainDirecotry;
            m_exportCodeDirectory = exportCodeDiretory;
            m_exportDataDirectory = exportDataDirecotry;
            m_exportCodePath = Path.Combine(exportCodeDiretory, exportCodeFileName);
        }

        /// <summary>
        /// 解析一个Excel
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<ConfigSheetData> ParseMetaData(string path)
        {
            var sheetDatas = new List<ConfigSheetData>();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = null;
                if (path.EndsWith(".xlsx"))
                {
                    workbook = new XSSFWorkbook(fs);
                }
                else if (path.EndsWith(".xls"))
                {
                    workbook = new HSSFWorkbook(fs);
                }
                if (workbook != null)
                {
                    // 解析多个Sheet
                    for (int i = 0; i < workbook.NumberOfSheets; ++i)
                    {
                        var sheet = workbook.GetSheetAt(i);
                        var sheetData = ParseSheet(sheet);
                        sheetData.m_filePath = path;
                        sheetData.m_fileName = Path.GetFileNameWithoutExtension(path);
                        sheetData.m_sheetName = sheet.SheetName;
                        sheetDatas.Add(sheetData);
                    }
                }
            }
            return sheetDatas;
        }

        private string GetClassOrEnumName(ConfigSheetData sheetData)
        {
            if (sheetData.m_sheetType == SheetType.Enum)
            {
                return (sheetData.m_configMetaData as ConfigEnumMetaData).m_name;
            }
            else if (sheetData.m_sheetType == SheetType.Class)
            {
                return (sheetData.m_configMetaData as ConfigClassMetaData).m_name;
            }
            return string.Empty;
        }

        public void CheckMetaDataSafety()
        {
            Dictionary<string, ConfigSheetData> className2SheetData = new Dictionary<string, ConfigSheetData>();
            //1、类名不重复;
            foreach (var excelMetaDatas in m_configSheetDict)
            {
                foreach (var metaData in excelMetaDatas.Value)
                {
                    var className = GetClassOrEnumName(metaData);
                    if (string.IsNullOrEmpty(className))
                    {
                        throw new ParseExcelException("配置类数据异常！");
                    }
                    if (!className2SheetData.ContainsKey(className))
                    {
                        className2SheetData[className] = metaData;
                    }
                    else
                    {
                        throw new ParseExcelException($"出现重复类名！ClassName:{className},Excel:\"{className2SheetData[className].m_fileName}\",\"{metaData.m_fileName}\"");
                    }

                }
            }
            //2、外键关联正确，即能够查询到正确的外键,枚举不能数组化;
            foreach (var sheetData in className2SheetData)
            {
                if (sheetData.Value.m_sheetType == SheetType.Class)
                {
                    var classMetaData = sheetData.Value.m_configMetaData as ConfigClassMetaData;
                    foreach (var fieldInfo in classMetaData.m_fieldsInfo)
                    {
                        if (!ConfigFieldMetaData.ParseForeignKey(fieldInfo.m_foreignKey, out var foreignClass, out var foreignField))
                        {
                            continue;

                        }
                        if (foreignClass.Equals(classMetaData.m_name))
                        {
                            throw new ParseExcelException($"外键名不能与当前所属类型名相同,ClassName:{classMetaData.m_name},Field:{fieldInfo.m_name}");
                        }

                        if (fieldInfo.m_dataType == DataType.Enum)
                        {
                            var listType = ConfigFieldMetaData.GetListType(fieldInfo.m_listType);
                            if (listType != ListType.None)
                            {
                                throw new ParseExcelException($"Enum不能数组化");
                            }
                        }
                        if (className2SheetData.TryGetValue(foreignClass, out var foreignMetaData))
                        {
                            if (foreignMetaData.m_sheetType == SheetType.Enum)
                            {
                                if (!foreignField.Equals(ConfigEnumMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase) && !foreignField.Equals(ConfigEnumMetaData.ValuePrimaryKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new ParseExcelException($"枚举不存在列名为{foreignField}的数据");
                                }
                            }
                            else if (foreignMetaData.m_sheetType == SheetType.Class)
                            {
                                if (!foreignField.Equals(ConfigClassMetaData.IDPrimaryKey))
                                {
                                    throw new ParseExcelException($"只能以{foreignClass}的ID列为外键");
                                }
                            }
                        }
                        else
                        {
                            throw new ParseExcelException($"不存在该外键类型，ForeignClass:{foreignClass},ClassName:{classMetaData.m_name},FieldName:{fieldInfo.m_name}");
                        }
                    }

                }
            }
        }

        public void ParseRealData(Assembly configDataAssembly, string path)
        {
            var tables = new List<DataTable>();
            if (!m_configSheetDict.TryGetValue(path, out var sheetDatas))
            {
                throw new ParseExcelException("找不到目标文件的类型信息!");
            }
            try
            {
                if (Directory.Exists(m_exportDataDirectory))
                {
                    Directory.Delete(m_exportDataDirectory, true);
                }
                Directory.CreateDirectory(m_exportDataDirectory);
            }
            catch (Exception e)
            {
                throw new ParseExcelException(e.Message);
            }
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = null;
                if (path.EndsWith(".xlsx"))
                {
                    workbook = new XSSFWorkbook(fs);
                }
                else if (path.EndsWith(".xls"))
                {
                    workbook = new HSSFWorkbook(fs);
                }
                if (workbook != null)
                {
                    // 解析多个Sheet
                    for (int i = 0; i < workbook.NumberOfSheets; ++i)
                    {
                        var sheet = workbook.GetSheetAt(i);
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
                        var bytes = SerializeDataTable(configDataAssembly, dataTable, classType);
                        var streamWriter = File.CreateText(Path.Combine(m_exportDataDirectory, classType.Name + ".txt"));
                        streamWriter.Write(System.Text.Encoding.Default.GetString(bytes));
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
            }
        }

        public byte[] SerializeDataTable(Assembly configDataAssembly, List<object> dataTable, Type classType)
        {
            // 1、数据能否正确转换成对应类型(包含定长数组和不定长数组)
            byte[] bytes = new byte[0];
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (var item in dataTable)
                {
                    binaryFormatter.Serialize(ms, item);
                }
                bytes = ms.GetBuffer();
            }
            return bytes;
        }

        public void ParseAllExcel()
        {
            m_configSheetDict.Clear();
            List<ConfigSheetData> sheetDatas = new List<ConfigSheetData>();
            // 1、从一个文件夹读取所有Excel文件，挨个解析ConfigSheetData列表
            var files = Directory.GetFiles(m_mainDirectory, "*.xlsx", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                m_configSheetDict[file] = ParseMetaData(file);
                sheetDatas.AddRange(m_configSheetDict[file]);
            }
            // 2、验证头信息安全性
            CheckMetaDataSafety();
            // 3、导出代码
            CodeExpoter codeExporter = new CodeExpoter();
            codeExporter.SetConfigSheetData(sheetDatas);
            codeExporter.ExportCode(m_exportCodePath);
            // 4、编译代码
            var assembly = codeExporter.Compile(m_exportCodeDirectory);
            if (assembly == null)
            {
                throw new ParseExcelException("导出代码编译出错！");
            }
            // 5、读取数据体，序列化
            foreach (var file in files)
            {
                ParseRealData(assembly, file);
            }
        }

        /// <summary>
        /// 解析表格数据
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private ConfigSheetData ParseSheet(ISheet sheet)
        {
            int rowOffset;
            var sheetType = ParseSheetType(sheet, out rowOffset);
            switch (sheetType)
            {
                case SheetType.Enum:
                    {
                        var enumMetaData = ParseEnumHeader(sheet);
                        ParseEnumValues(sheet, enumMetaData);
                        return new ConfigSheetData()
                        {
                            m_sheetType = sheetType,
                            m_configMetaData = enumMetaData
                        };
                    }
                case SheetType.Class:
                    {
                        var classMetaData = ParseClassHeader(sheet);
                        ParseClassFields(sheet, classMetaData);
                        return new ConfigSheetData()
                        {
                            m_sheetType = sheetType,
                            m_configMetaData = classMetaData
                        };
                    }
            }
            return new ConfigSheetData()
            {
                m_sheetType = SheetType.Invalid,
            };

        }
        public class TypeInfo
        {
            public DataType m_dataType;
            public FieldInfo m_fieldInfo;
            public string m_fieldName;
            public bool m_isList;
        }

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
        private string ChangeCell2String(ICell cell)
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
        private List<object> ReadDataTable(Assembly assembly, ISheet sheet, ConfigSheetData configSheetData, out Type classType)
        {
            List<object> dataTable = new List<object>(Math.Max(sheet.LastRowNum + 1 - ConfigFieldMetaData.DataBeginRow, 0));
            classType = null;
            if (configSheetData.m_sheetType != SheetType.Class || !(configSheetData.m_configMetaData is ConfigClassMetaData classMetaData))
            {
                return dataTable;
            }
            var classFullName = $"ConfigData.{classMetaData.m_name}";
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
                    throw new ParseExcelException($"字段名称重复,Class:{classMetaData.m_name},Field:{fieldMetaData.m_name}");
                }
                fieldTypeInfo = new TypeInfo();
                fieldTypeInfo.m_isList = ConfigFieldMetaData.GetListType(fieldMetaData.m_listType) != ListType.None;
                fieldTypeInfo.m_dataType = fieldMetaData.m_dataType;
                fieldTypeInfo.m_fieldName = "_" + fieldMetaData.m_name;
                fieldTypeInfo.m_fieldInfo = classType.GetField(fieldTypeInfo.m_fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (fieldTypeInfo.m_fieldInfo == null)
                {
                    throw new ParseExcelException($"字段类型解析失败,Class:{classMetaData.m_name},Field:{fieldMetaData.m_name}");
                }
                if (fieldMetaData.m_dataType == DataType.NestedClass)
                {
                    var fullTypeName = ConfigFieldMetaData.GetFullTypeName(fieldMetaData, fieldMetaData.m_dataType, fieldMetaData.m_listType);
                    var nestedClassType = assembly.GetType(fullTypeName);
                    if (nestedClassType == null)
                    {
                        throw new ParseExcelException("内嵌类解析失败");
                    }
                    nestedClassFieldTypeInfos[fieldMetaData.m_name] = new List<TypeInfo>();
                    var fieldList = fieldMetaData.m_nestedClassMetaData.m_fieldsInfo.Values.ToList();

                    foreach (var nestedClassFieldTypeInfo in fieldList)
                    {
                        var typeinfo = new TypeInfo()
                        {
                            m_fieldName = "_" + nestedClassFieldTypeInfo.m_fieldName,
                            m_dataType = nestedClassFieldTypeInfo.m_dataType
                        };
                        typeinfo.m_isList = ConfigFieldMetaData.GetListType(nestedClassFieldTypeInfo.m_listType) != ListType.None;
                        typeinfo.m_fieldInfo = nestedClassType.GetField(typeinfo.m_fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        nestedClassFieldTypeInfos[fieldMetaData.m_name].Add(typeinfo);
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
                                throw new ParseExcelException($"不存在该内嵌类{fieldInfo.m_name}");
                            }
                            var nestedClassInstance = Activator.CreateInstance(nestedFieldType);
                            if (nestedClassInstance == null)
                            {
                                throw new ParseExcelException($"创建内嵌类{fieldInfo.m_name}实例失败");
                            }
                            var nestMetaData = fieldInfo.m_nestedClassMetaData;
                            var values = cell.StringCellValue.Split(ConfigFieldMetaData.NestedClassMetaData.seperator);
                            if (!nestedClassFieldTypeInfos.TryGetValue(fieldInfo.m_name, out var nestedClassFieldInfos))
                            {
                                throw new ParseExcelException($"内嵌类{fieldInfo.m_name}获取域信息失败");
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
                            SetFieldValue(fieldType, instance, ChangeCell2String(cell), fieldInfo.m_dataType, fieldTypeInfo.m_isList, fieldInfo.m_foreignKey);
                        }
                    }
                    else
                    {
                        throw new ParseExcelException($"字段列号不匹配,Class:{classMetaData.m_name}");
                    }
                }
                dataTable.Add(instance);
            }
            return dataTable;
        }

        /// <summary>
        /// 解析表类型
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowOffset"></param>
        /// <returns></returns>
        private SheetType ParseSheetType(ISheet sheet, out int rowOffset)
        {
            rowOffset = 1;
            var row = sheet.GetRow(ConfigSheetData.TypeRowIndex);
            var cell = row.GetCell(0);
            var sheetType = cell.StringCellValue;

            if (Enum.TryParse<SheetType>(sheetType, true, out var result))
            {
                return result;
            }
            else
            {
                throw new ParseExcelException($"解析表类型失败,Sheet:{sheet.SheetName}");
            }
        }
        /// <summary>
        /// 解析表头
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="beginOffset"></param>
        /// <param name="rowOffset"></param>
        /// <returns></returns>
        private ConfigEnumMetaData ParseEnumHeader(ISheet sheet)
        {
            ConfigEnumMetaData enumMetaData = new ConfigEnumMetaData();

            var row = sheet.GetRow(ConfigSheetData.EnumHeaderIndex);
            var nameCell = row.GetCell(ConfigSheetData.EnumNameCellIndex);
            var commentCell = row.GetCell(ConfigSheetData.EnumCommentCellIndex);
            var visiblityCell = row.GetCell(ConfigSheetData.EnumVisiblityCellIndex);

            enumMetaData.m_name = nameCell.StringCellValue;
            enumMetaData.m_comment = commentCell.StringCellValue;
            var visiblity = enumMetaData.ParseEnum(visiblityCell.StringCellValue, Visiblity.Invalid);
            if (visiblity != Visiblity.Invalid)
            {
                enumMetaData.m_visiblity = visiblity;
            }
            else
            {
                throw new ParseExcelException($"可见性解析失败，Sheet:{sheet.SheetName}");
            }
            return enumMetaData;
        }

        private void ParseEnumValues(ISheet sheet, ConfigEnumMetaData enumMetaData)
        {
            for (int i = ConfigSheetData.EnumBodyIndex; i <= sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                var idCell = row.GetCell(ConfigSheetData.EnumFlagIDCellIndex);
                var nameCell = row.GetCell(ConfigSheetData.EnumFlagNameCellIndex);
                var commentCell = row.GetCell(ConfigSheetData.EnumFlagCommentCellIndex);
                if (idCell == null)
                {
                    throw new ParseExcelException("请填写Enum行数据");
                }

                var id = (int)idCell.NumericCellValue;

                if (!enumMetaData.m_enumNameValue.TryGetValue(id, out var flagData))
                {
                    if (nameCell != null && !string.IsNullOrEmpty(nameCell.StringCellValue))
                    {
                        // 同名检测
                        foreach (var enumData in enumMetaData.m_enumNameValue.Values)
                        {
                            if (enumData.m_name.Equals(nameCell.StringCellValue))
                            {
                                throw new ParseExcelException($"Enum 不能有同名枚举 {nameCell.StringCellValue}");
                            }
                        }

                        flagData = new ConfigEnumMetaData.EnumData()
                        {
                            m_ID = id,
                            m_name = nameCell.StringCellValue,
                            m_comment = commentCell == null ? "" : commentCell.StringCellValue // 可空
                        };
                        enumMetaData.m_enumNameValue[id] = flagData;
                    }
                    else
                    {
                        throw new ParseExcelException("Enum Name不能为Empty Or Null");
                    }
                }
                else
                {
                    throw new ParseExcelException("Enum ID重复");
                }
            }
        }

        /// <summary>
        /// 解析类头
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private ConfigClassMetaData ParseClassHeader(ISheet sheet)
        {
            var classMetaData = new ConfigClassMetaData();
            var headerRow = sheet.GetRow(ConfigSheetData.ClassHeaderIndex);
            var nameCell = headerRow.GetCell(ConfigSheetData.ClassNameCellIndex);
            var commentCell = headerRow.GetCell(ConfigSheetData.ClassCommentCellIndex);
            if (nameCell != null && !string.IsNullOrEmpty(nameCell.StringCellValue))
            {
                classMetaData.m_name = nameCell.StringCellValue;
                classMetaData.m_comment = commentCell == null ? "" : commentCell.StringCellValue;
            }
            else
            {
                throw new ParseExcelException($"类名异常,Sheet:{sheet.SheetName}");
            }
            return classMetaData;
        }

        /// <summary>
        /// 解析类的域名
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="classMetaData"></param>
        /// <returns></returns>
        private void ParseClassFields(ISheet sheet, ConfigClassMetaData classMetaData)
        {
            var fieldsDict = new Dictionary<string, ConfigFieldMetaData>();
            var fieldNameRow = sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldName);
            var fieldTypeRow = sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldDataType);
            // 排除了特殊处理的列表
            var headers = (Enum.GetValues(typeof(ConfigClassFieldHeader)) as ConfigClassFieldHeader[]).Except(new ConfigClassFieldHeader[]{ConfigClassFieldHeader.ClassFieldName,
                                ConfigClassFieldHeader.ClassFieldDataType ,
                                ConfigClassFieldHeader.ClassNestedClassFieldComments ,
                                ConfigClassFieldHeader.ClassNestedClassFieldIsList ,
                                ConfigClassFieldHeader.ClassNestedClassFieldNames ,
                                ConfigClassFieldHeader.ClassNestedClassFieldTypes});

            // 首先获得域名
            for (int i = 0; i < fieldNameRow.LastCellNum; ++i)
            {
                var fieldNameCell = fieldNameRow.GetCell(i);
                var fieldTypeCell = fieldTypeRow.GetCell(i);
                var tempFieldDataType = classMetaData.ParseEnum(fieldTypeCell.StringCellValue, DataType.None);

                if (fieldNameCell != null && !string.IsNullOrEmpty(fieldNameCell.StringCellValue))
                {
                    if (!fieldsDict.ContainsKey(fieldNameCell.StringCellValue))
                    {
                        var fieldMetaData = new ConfigFieldMetaData()
                        {
                            m_belongClassName = classMetaData.m_name,
                            m_name = fieldNameCell.StringCellValue.ToLower(),
                        };
                        fieldsDict[fieldMetaData.m_name] = fieldMetaData;
                        // 解析内嵌类，如果不是内嵌类，则这四个单元格的数据应当也是None的
                        var nestedClassNamesCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldNames).GetCell(i);
                        var nestedClassTypesCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldTypes).GetCell(i);
                        var nestedClassCommentsCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldComments).GetCell(i);
                        var nestedClassIsListCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldIsList).GetCell(i);

                        if (nestedClassNamesCell != null && !string.IsNullOrEmpty(nestedClassNamesCell.StringCellValue) &&
                            nestedClassTypesCell != null && !string.IsNullOrEmpty(nestedClassTypesCell.StringCellValue) &&
                            nestedClassCommentsCell != null && !string.IsNullOrEmpty(nestedClassCommentsCell.StringCellValue) &&
                            nestedClassIsListCell != null && !string.IsNullOrEmpty(nestedClassIsListCell.StringCellValue))
                        {
                            bool isNestedClass = fieldMetaData.ParseNestedClass(tempFieldDataType, nestedClassNamesCell.StringCellValue, nestedClassTypesCell.StringCellValue,
                                nestedClassCommentsCell.StringCellValue, nestedClassIsListCell.StringCellValue);
                            if (isNestedClass)
                            {
                                fieldMetaData.m_dataType = DataType.NestedClass;
                                fieldMetaData.m_nestedClassMetaData.m_className = fieldTypeCell.StringCellValue;
                            }
                            else
                            {
                                fieldMetaData.m_dataType = tempFieldDataType;
                            }
                        }
                        else
                        {
                            throw new ParseExcelException($"内嵌类配置错误，请检查[{fieldNameCell.StringCellValue}]");
                        }
                        // 通常处理
                        foreach (var headerInfo in headers)
                        {
                            ConfigClassFieldHeader header = headerInfo;
                            int rowIndex = (int)headerInfo;
                            var row = sheet.GetRow(rowIndex);
                            var headerCell = row.GetCell(i);
                            // 枚举或字符串
                            if (!fieldMetaData.SetValue(header, headerCell.StringCellValue))
                            {
                                throw new ParseExcelException($"域信息解析异常，域名:{fieldNameCell.StringCellValue}，域信息类型：{header}");
                            }
                        }
                        // 域的实际类名
                        fieldMetaData.m_realTypeName = ConfigFieldMetaData.GetTypeName(fieldMetaData, fieldMetaData.m_dataType, fieldMetaData.m_listType);
                        // 表中第几列
                        fieldMetaData.m_columnIndex = i;
                    }
                    else
                    {
                        throw new ParseExcelException($"一个配置类里不能有重名的字段名,域名:{fieldNameCell.StringCellValue}");
                    }
                }
                else
                {
                    throw new ParseExcelException($"域名不得为空，Sheet:{sheet.SheetName}");
                }
            }

            var fieldList = fieldsDict.Values.ToList();
            fieldList.Sort((a, b) =>
            {
                if (a.m_name.Length != b.m_name.Length)
                {
                    return a.m_name.Length.CompareTo(b.m_name.Length);
                }
                return a.m_name.CompareTo(b.m_name);
            });
            classMetaData.m_fieldsInfo.Clear();
            classMetaData.m_fieldsInfo.AddRange(fieldList);
        }

        /// <summary>
        /// Excel路径/名称为key
        /// </summary>
        public Dictionary<string, List<ConfigSheetData>> m_configSheetDict = new Dictionary<string, List<ConfigSheetData>>();
        /// <summary>
        /// Excel文件所在目录
        /// </summary>
        private string m_mainDirectory;
        /// <summary>
        /// 代码导出位置
        /// </summary>
        private string m_exportCodePath;

        /// <summary>
        /// 代码导出目录
        /// </summary>
        private string m_exportCodeDirectory;

        /// <summary>
        /// 数据导出目录
        /// </summary>
        private string m_exportDataDirectory;
    }
}
