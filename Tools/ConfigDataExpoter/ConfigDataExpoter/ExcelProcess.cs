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
    class ExcelProcess
    {
        public ExcelProcess(string mainDirecotry, string exportCodeDiretory, string exportDataDirecotry, string exportCodeFileName)
        {
            m_mainDirectory = mainDirecotry;
            m_exportCodeDirectory = exportCodeDiretory;
            m_exportDataDirectory = exportDataDirecotry;
            m_exportCodePath = Path.Combine(exportCodeDiretory, exportCodeFileName);
        }

        private string GetClassOrEnumName(ConfigSheetData sheetData)
        {
            if (sheetData.m_sheetType == SheetType.Enum)
            {
                return (sheetData.m_configMetaData as ConfigEnumMetaData).m_name;
            }
            else if (sheetData.m_sheetType == SheetType.Class)
            {
                return (sheetData.m_configMetaData as ConfigClassMetaData).m_classname;
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
                        if (foreignClass.Equals(classMetaData.m_classname))
                        {
                            throw new ParseExcelException($"外键名不能与当前所属类型名相同,ClassName:{classMetaData.m_classname},Field:{fieldInfo.m_fieldName}");
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
                            throw new ParseExcelException($"不存在该外键类型，ForeignClass:{foreignClass},ClassName:{classMetaData.m_classname},FieldName:{fieldInfo.m_fieldName}");
                        }
                    }

                }
            }
        }

        public void ParseAllExcel()
        {
            // 1、从一个文件夹读取所有Excel文件，挨个解析ConfigSheetData列表
            CodeParser codeParser = new CodeParser();
            m_configSheetDict = codeParser.ParseAllMetaData(m_mainDirectory);

            // 2、验证头信息安全性
            CheckMetaDataSafety();

            // 3、导出代码
            CodeExpoter codeExporter = new CodeExpoter();
            var sheetDatas = new List<ConfigSheetData>();
            foreach (var fileSheetDatas in m_configSheetDict.Values)
            {
                sheetDatas.AddRange(fileSheetDatas);
            }
            codeExporter.Setup(sheetDatas);
            codeExporter.ExportCode(m_exportCodePath);

            // 4、编译代码
            var assembly = codeExporter.Compile(m_exportCodeDirectory);
            if (assembly == null)
            {
                throw new ParseExcelException("导出代码编译出错！");
            }

            // 5、读取数据体，序列化
            DataParser dataParser = new DataParser();
            var allTableDatas = dataParser.ParseAllTableDatas(assembly, m_mainDirectory, m_configSheetDict);

            // 6、导出数据
            DataExporter dataExporter = new DataExporter();
            dataExporter.Setup(allTableDatas, FormatterType.Binary);
            dataExporter.ExportData(m_exportDataDirectory);
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
