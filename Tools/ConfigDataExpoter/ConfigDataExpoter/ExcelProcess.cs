﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigDataExpoter
{
    /// <summary>
    /// Excel数据处理
    /// </summary>
    public class ExcelProcess
    {
        public ExcelProcess(string baseDirecotry)
        {
            m_baseDirectory = baseDirecotry;
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
        /// <summary>
        /// 检测Meta数据安全性
        /// </summary>
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
                    bool existID = false;
                    var classMetaData = sheetData.Value.m_configMetaData as ConfigClassMetaData;
                    foreach (var fieldInfo in classMetaData.m_fieldsInfo)
                    {
                        // 暂时不支持枚举数组
                        //if (fieldInfo.DataType == DataType.Enum)
                        //{
                        //    var listType = ConfigFieldMetaData.GetListType(fieldInfo.ListType);
                        //    if (listType != ListType.None)
                        //    {
                        //        throw new ParseExcelException($"Enum不能数组化");
                        //    }
                        //}
                        // 判断类型有ID列
                        if (fieldInfo.FieldName.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase))
                        {
                            if (ConfigFieldMetaData.GetListType(fieldInfo.BelongClassName, fieldInfo.FieldName, fieldInfo.ListType, out _) > ListType.None)
                            {
                                throw new ParseExcelException("主键ID列不能是列表类型");
                            }
                            existID = true;
                        }
                        if (!ConfigFieldMetaData.ParseForeignKey(fieldInfo.ForeignKey, out var foreignClass, out var foreignField))
                        {
                            continue;
                        }
                        // 外键不能是当前类自己
                        if (foreignClass.Equals(classMetaData.m_classname))
                        {
                            throw new ParseExcelException($"外键名不能与当前所属类型名相同,ClassName:{classMetaData.m_classname},Field:{fieldInfo.FieldName}");
                        }

                        // 外键必须是存在类型的ID或枚举的ID
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
                                if (!foreignField.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new ParseExcelException($"只能以{foreignClass}的ID列为外键");
                                }
                            }
                        }
                        else
                        {
                            throw new ParseExcelException($"不存在该外键类型，ForeignClass:{foreignClass},ClassName:{classMetaData.m_classname},FieldName:{fieldInfo.FieldName}");
                        }
                    }
                    if (!existID)
                    {
                        throw new ParseExcelException("配置类必须具有ID列,ID 将被当作数据表的主键来使用。");
                    }
                }
            }
        }

        private void Init(ExportConfigDataSettings settings)
        {
            m_mainDirectory = Path.Combine(m_baseDirectory, settings.ExportRootDirectoryPath);
            m_exportCodeDirectory = Path.Combine(m_mainDirectory, settings.ExportCodeDirectoryName);
            m_exportDataDirectory = Path.Combine(m_mainDirectory, settings.ExportDataDirectoryName);
            m_exportLanguageDirectory = Path.Combine(m_mainDirectory, settings.ExportLanguageDirectoryName);
            m_exportCodePath = Path.Combine(m_exportCodeDirectory, settings.ExportConfigDataName);
            m_exportTypeEnumCodePath = Path.Combine(m_exportCodeDirectory, settings.ExportTypeEnumCodeName);
            m_copyFromDirectory = Path.Combine(m_baseDirectory, settings.CopyFromDirectoryPath);
            m_configDataLoaderAutoGenPath = Path.Combine(m_exportCodeDirectory, settings.ExportLoaderCodeName);
            m_unityCodeDirectory = Path.Combine(m_mainDirectory, settings.UnityCodeDirectory);
            m_unityDataDirectory = Path.Combine(m_mainDirectory, settings.UnityDataDirectory);
        }

        public void ParseAllExcel(ExportConfigDataSettings settings)
        {
            Init(settings);
            // 1、从一个文件夹读取所有Excel文件，挨个解析ConfigSheetData列表
            ExcelTypeMetaDataParser metaDataParser = new ExcelTypeMetaDataParser(settings);
            m_configSheetDict = metaDataParser.ParseAllMetaData(m_mainDirectory);

            // 2、验证头信息安全性
            CheckMetaDataSafety();

            // 3、导出代码
            var sheetDatas = new List<ConfigSheetData>();
            foreach (var fileSheetDatas in m_configSheetDict.Values)
            {
                sheetDatas.AddRange(fileSheetDatas);
            }

            CodeExpoter codeExporter = new CodeExpoter(sheetDatas);
            codeExporter.ExportConfigCode(m_exportCodePath);
            codeExporter.ExportTypeEnumCode(m_exportTypeEnumCodePath);
            codeExporter.ExportConfigDataLoaderCode(m_configDataLoaderAutoGenPath);
            codeExporter.CopyDirectory(m_copyFromDirectory, m_exportCodeDirectory);
            // 4、编译代码
            var assembly = codeExporter.Compile(m_exportCodeDirectory);
            if (assembly == null)
            {
                throw new ParseExcelException("导出代码编译出错！");
            }

            codeExporter.CopyDirectory(m_exportCodeDirectory, m_unityCodeDirectory, false);

            // 5、读取数据体，穿插多语言收集
            MultiLanguageExchanger languageWriter = new MultiLanguageExchanger();
            MultiLanguageCollector mutiLanguageProcess = new MultiLanguageCollector(languageWriter, settings.RemoveExpiredLanguageItem);
            mutiLanguageProcess.Load(m_exportLanguageDirectory);

            ExcelDataRowParser dataParser = new ExcelDataRowParser(mutiLanguageProcess);
            var allTableDatas = dataParser.ParseAllExcelTableDatas(assembly, m_mainDirectory, m_configSheetDict);

            // 6、导出数据，序列化
            DataExporter dataExporter = new DataExporter(allTableDatas, FormatterType.Binary, languageWriter);
            dataExporter.ExportData(m_exportDataDirectory);
            dataExporter.CopyDirectory(m_exportDataDirectory, m_unityDataDirectory, false);

            // 7、多语言处理
            mutiLanguageProcess.Save(m_exportLanguageDirectory);
        }

        /// <summary>
        /// Excel路径/名称为key
        /// </summary>
        public Dictionary<string, List<ConfigSheetData>> m_configSheetDict = new Dictionary<string, List<ConfigSheetData>>();

        private string m_baseDirectory;

        /// <summary>
        /// Excel文件所在目录
        /// </summary>
        private string m_mainDirectory;

        /// <summary>
        /// 代码导出位置
        /// </summary>
        private string m_exportCodePath;

        /// <summary>
        /// 复制代码目录
        /// </summary>
        private string m_copyFromDirectory;

        /// <summary>
        /// 自动生成的Loader文件
        /// </summary>
        private string m_configDataLoaderAutoGenPath;

        private string m_unityCodeDirectory;

        private string m_unityDataDirectory;

        private string m_exportTypeEnumCodePath;

        /// <summary>
        /// 代码导出目录
        /// </summary>
        private string m_exportCodeDirectory;

        /// <summary>
        /// 数据导出目录
        /// </summary>
        private string m_exportDataDirectory;

        /// <summary>
        /// 语言表导出目录
        /// </summary>
        private string m_exportLanguageDirectory;
    }
}
