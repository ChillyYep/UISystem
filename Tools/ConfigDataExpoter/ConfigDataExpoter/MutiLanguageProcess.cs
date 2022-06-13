using ConfigData;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ConfigDataExpoter
{
    public class LanguageItem
    {
        public const char fieldSeperator = '.';
        public SourceInfo m_source;
        public string m_translateText;
    }
    public class LanguageTable
    {
        public string m_excelName;
        public string m_className;
        public readonly List<LanguageItem> m_allLanguageItems = new List<LanguageItem>();
    }
    /// <summary>
    /// ID_Field1.fieldIndex_NestedField.nestedFieldIndex
    /// </summary>
    public class SourceInfo
    {
        public static SourceInfo ParseSourceIDStr(string sourceStr)
        {
            var sourceInfo = new SourceInfo();
            var strs = sourceStr.Split('.');
            sourceInfo.m_rowID = int.Parse(strs[0]);
            sourceInfo.m_fieldName = strs[1];
            sourceInfo.m_fieldListIndex = int.Parse(strs[2]);
            sourceInfo.m_nestedFieldName = strs[3];
            sourceInfo.m_nestedFieldListIndex = int.Parse(strs[4]);
            return sourceInfo;
        }
        /// <summary>
        /// 源文本
        /// </summary>
        public string m_sourceText;

        /// <summary>
        /// ID
        /// </summary>
        public int m_rowID;

        /// <summary>
        /// 一级域名
        /// </summary>
        public string m_fieldName;

        /// <summary>
        /// 内嵌类域名，也就是二级域名
        /// </summary>
        public string m_nestedFieldName;

        /// <summary>
        /// 字段如果是数组，启用该项区分，内嵌类内部字段已经禁止使用数组
        /// </summary>
        public int m_fieldListIndex;

        /// <summary>
        /// 内嵌类列表Index
        /// </summary>
        public int m_nestedFieldListIndex;

        public string GetIDStr()
        {
            return $"{m_rowID}.{m_fieldName}.{m_fieldListIndex}.{m_nestedFieldName}.{m_nestedFieldListIndex}";
        }
    }
    /// <summary>
    /// 多语言处理，语言表数据行单行格式:id(string，标明了来源),原文本,翻译文本，第一行标记语言表，第二行标明来源配置类
    /// </summary>
    public class MutiLanguageProcess : ExcelParserBase
    {
        public const string MutiLanguageMark = "MutiLanguage";

        public MutiLanguageProcess(Language language)
        {
            m_language = language;
        }

        public void Load(string directory)
        {
            m_languageCollector.Clear();

            directory = GetLanguageDirectory(directory);
            if (!Directory.Exists(directory))
            {
                return;
            }
            var files = Directory.GetFiles(directory);
            List<ISheet> allLanguageSheets = new List<ISheet>();
            List<string> excelNames = new List<string>();
            foreach (var file in files)
            {
                excelNames.Add(Path.GetFileNameWithoutExtension(file));
                allLanguageSheets.AddRange(GetSheets(file));
            }
            // 筛除非语言表的Sheet
            for (int i = allLanguageSheets.Count - 1; i >= 0; --i)
            {
                var cell = allLanguageSheets[i].GetRow(0).GetCell(0);
                if (cell == null)
                {
                    allLanguageSheets.RemoveAt(i);
                    excelNames.RemoveAt(i);
                    continue;
                }
                if (cell.CellType != CellType.String)
                {
                    allLanguageSheets.RemoveAt(i);
                    excelNames.RemoveAt(i);
                    continue;
                }
                if (cell.StringCellValue != MutiLanguageMark)
                {
                    allLanguageSheets.RemoveAt(i);
                    excelNames.RemoveAt(i);
                    continue;
                }
            }
            // 读取翻译数据
            for (int i = 0; i < allLanguageSheets.Count; ++i)
            {
                var sheet = allLanguageSheets[i];
                var filename = excelNames[i];
                // 语言表配置类来源，及创建字典表
                var classCell = sheet.GetRow(1).GetCell(0);
                if (classCell == null || classCell.CellType != CellType.String)
                {
                    throw new ParseExcelException($"{sheet.SheetName}语言表第二行标记配置类来源，现格式不正确");
                }
                var languageTable = new LanguageTable()
                {
                    m_excelName = filename,
                    m_className = classCell.StringCellValue
                };
                var languageItems = languageTable.m_allLanguageItems;

                m_languageCollector[languageTable.m_className] = languageTable;

                // 从第三行开始是真正的数据
                for (int j = 2; j < sheet.LastRowNum; ++j)
                {
                    var idCell = sheet.GetRow(j).GetCell(0);
                    var sourceTextCell = sheet.GetRow(j).GetCell(1);
                    var translateTextCell = sheet.GetRow(j).GetCell(2);
                    var translateText = (translateTextCell == null || translateTextCell.CellType != CellType.String) ? string.Empty : translateTextCell.StringCellValue;

                    if (idCell == null || sourceTextCell == null || idCell.CellType != CellType.String ||
                        sourceTextCell.CellType != CellType.String)
                    {
                        var sourceInfo = SourceInfo.ParseSourceIDStr(idCell.StringCellValue);
                        sourceInfo.m_sourceText = sourceTextCell.StringCellValue;
                        languageItems.Add(new LanguageItem()
                        {
                            m_source = sourceInfo,
                            m_translateText = translateText
                        });
                    }

                }
            }
        }

        /// <summary>
        /// 添加需要翻译的文本
        /// </summary>
        /// <param name="excelName"></param>
        /// <param name="className"></param>
        /// <param name="sourceInfo"></param>
        /// <returns></returns>
        public string AddText(string excelName, string className, SourceInfo sourceInfo)
        {
            if (!m_languageCollector.TryGetValue(className, out var languageTable))
            {
                languageTable = new LanguageTable()
                {
                    m_excelName = $"{excelName}_{m_language.ToString()}",
                    m_className = className
                };
                m_languageCollector[className] = languageTable;
            }
            var source = sourceInfo.GetIDStr();
            languageTable.m_allLanguageItems.Add(new LanguageItem()
            {
                m_source = sourceInfo,
                m_translateText = string.Empty
            });
            return source;
        }

        /// <summary>
        /// 保存翻译表
        /// </summary>
        /// <param name="directory"></param>
        public void Save(string directory)
        {
            directory = GetLanguageDirectory(directory);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Dictionary<string, List<string>> excelName2ClassNames = new Dictionary<string, List<string>>();
            foreach (var item in m_languageCollector)
            {
                var languageExcelName = item.Value.m_excelName;
                if (!excelName2ClassNames.TryGetValue(languageExcelName, out var classNames))
                {
                    classNames = new List<string>();
                    excelName2ClassNames[languageExcelName] = classNames;
                }
                classNames.Add(item.Value.m_className);
            }
            foreach (var excelName2ClassNamesPair in excelName2ClassNames)
            {
                var excelName = excelName2ClassNamesPair.Key;
                var excelPath = Path.Combine(directory, excelName + ".xlsx");
                var classNames = excelName2ClassNamesPair.Value;
                XSSFWorkbook workBook = new XSSFWorkbook();
                foreach (var className in classNames)
                {
                    // 1、创建Sheet
                    var sheet = workBook.CreateSheet(className);
                    var languageTable = m_languageCollector[className];
                    // 2、并设置标记行和配置类来源
                    var markRow = sheet.CreateRow(0);
                    var markCell = markRow.CreateCell(0);
                    markCell.SetCellValue(MutiLanguageMark);
                    var classRow = sheet.CreateRow(1);
                    var classCell = classRow.CreateCell(0);
                    classCell.SetCellValue(className);
                    int curRow = 1;
                    // 3、设置数据
                    foreach (var item in languageTable.m_allLanguageItems)
                    {
                        curRow++;
                        var dataRow = sheet.CreateRow(curRow);
                        var idCell = dataRow.CreateCell(0);
                        var sourceTextCell = dataRow.CreateCell(1);
                        var translateTextCell = dataRow.CreateCell(2);
                        idCell.SetCellValue(item.m_source.GetIDStr());
                        sourceTextCell.SetCellValue(item.m_source.m_sourceText);
                        translateTextCell.SetCellValue(item.m_translateText);
                    }
                }
                // 如果存在，则需要删除重新创建
                if (File.Exists(excelPath))
                {
                    File.Delete(excelPath);
                }
                using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
                {
                    workBook.Write(fs);
                }
            }
        }

        public string GetLanguageDirectory(string directory)
        {
            if (m_language == Language.CN)
            {
                directory = Path.Combine(directory, "CN");
            }
            else if (m_language == Language.EN)
            {
                directory = Path.Combine(directory, "EN");
            }
            return directory;
        }

        private readonly Dictionary<string, LanguageTable> m_languageCollector = new Dictionary<string, LanguageTable>();

        private Language m_language;
    }
}
