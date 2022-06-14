using ConfigData;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 每一个翻译项信息
    /// </summary>
    public class LanguageItem
    {
        public const char fieldSeperator = '.';
        public int m_id;
        public SourceInfo m_source;
        public string m_translateText;
    }

    /// <summary>
    /// 一个翻译表
    /// </summary>
    public class LanguageTable
    {
        public string m_excelName;
        public string m_className;
        public const string IDField = "ID";
        public const string SourceText = "SourceText";
        public const string TranslateText = "TranslateText";
        public const string SourceInfoStr = "SourceInfoStr";
        public readonly Dictionary<string, LanguageItem> m_allLanguageItems = new Dictionary<string, LanguageItem>();
        //public readonly List<LanguageItem> m_allLanguageItems = new List<LanguageItem>();
    }

    /// <summary>
    /// 来源信息
    /// </summary>
    public class SourceInfo
    {
        public static SourceInfo ParseSourceIDStr(string sourceStr)
        {
            var sourceInfo = new SourceInfo();
            var strs = sourceStr.Split('-');
            sourceInfo.m_className = strs[0];
            sourceInfo.m_rowID = int.Parse(strs[1]);
            sourceInfo.m_fieldName = strs[2];
            sourceInfo.m_fieldListIndex = int.Parse(strs[3]);
            sourceInfo.m_nestedFieldName = strs[4];
            sourceInfo.m_nestedFieldListIndex = int.Parse(strs[5]);
            return sourceInfo;
        }

        /// <summary>
        /// 所属配置类
        /// </summary>
        public string m_className;

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
            return $"{m_className}-{m_rowID}-{m_fieldName}-{m_fieldListIndex}-{m_nestedFieldName}-{m_nestedFieldListIndex}";
        }
    }

    /// <summary>
    /// 多语言处理，语言表数据行单行格式:id(string，标明了来源),原文本,翻译文本，第一行标记语言表，第二行标明来源配置类
    /// </summary>
    public class MultiLanguageCollector : ExcelParserBase
    {
        public const string MutiLanguageMark = "MutiLanguage";

        public MultiLanguageCollector(Language language, MultiLanguageExchanger multiLanguageWriter)
        {
            m_multiLanguageWriter = multiLanguageWriter;
            m_language = language;
            m_curID = 0;
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
                // 第三行是域名，不用读取，跳过

                // 从第四行开始是真正的数据
                for (int j = 3; j <= sheet.LastRowNum; ++j)
                {
                    var idCell = sheet.GetRow(j).GetCell(0);
                    var sourceTextCell = sheet.GetRow(j).GetCell(1);
                    var translateTextCell = sheet.GetRow(j).GetCell(2);
                    var sourceInfoCell = sheet.GetRow(j).GetCell(3);
                    var translateText = (translateTextCell == null || translateTextCell.CellType != CellType.String) ? string.Empty : translateTextCell.StringCellValue;

                    if (idCell == null || sourceTextCell == null || idCell.CellType != CellType.String ||
                        sourceTextCell.CellType != CellType.String)
                    {
                        continue;
                    }
                    var sourceInfo = SourceInfo.ParseSourceIDStr(sourceInfoCell.StringCellValue);
                    sourceInfo.m_sourceText = sourceTextCell.StringCellValue;
                    languageItems[sourceInfoCell.StringCellValue] = new LanguageItem()
                    {
                        m_id = ++m_curID,
                        m_source = sourceInfo,
                        m_translateText = translateText
                    };
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
            if (!languageTable.m_allLanguageItems.TryGetValue(source, out var languageItem))
            {
                languageItem = new LanguageItem()
                {
                    m_id = ++m_curID,
                    m_source = sourceInfo,
                    m_translateText = string.Empty
                };
                languageTable.m_allLanguageItems[source] = languageItem;
            }
            // 添加KeyValuePair
            m_multiLanguageWriter.AddIDTextPair(languageItem.m_id, source);
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
                    // 3、设置域名
                    var fieldNameRow = sheet.CreateRow(2);

                    var idFieldNameCell = fieldNameRow.CreateCell(0);
                    var sourceTextFieldNameCell = fieldNameRow.CreateCell(1);
                    var translateTextFieldNameCell = fieldNameRow.CreateCell(2);
                    var sourceInfoFieldNameCell = fieldNameRow.CreateCell(3);

                    idFieldNameCell.SetCellValue(LanguageTable.IDField);
                    sourceTextFieldNameCell.SetCellValue(LanguageTable.SourceText);
                    translateTextFieldNameCell.SetCellValue(LanguageTable.TranslateText);
                    sourceInfoFieldNameCell.SetCellValue(LanguageTable.SourceInfoStr);

                    int curRow = 2;
                    // 4、设置数据
                    foreach (var item in languageTable.m_allLanguageItems.Values)
                    {
                        curRow++;
                        var dataRow = sheet.CreateRow(curRow);

                        var idCell = dataRow.CreateCell(0);
                        var sourceTextCell = dataRow.CreateCell(1);
                        var translateTextCell = dataRow.CreateCell(2);
                        var sourceInfoCell = dataRow.CreateCell(3);

                        idCell.SetCellValue(item.m_id.ToString());
                        sourceTextCell.SetCellValue(item.m_source.m_sourceText);
                        translateTextCell.SetCellValue(item.m_translateText);
                        sourceInfoCell.SetCellValue(item.m_source.GetIDStr());
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

        private MultiLanguageExchanger m_multiLanguageWriter;

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

        /// <summary>
        /// 获取语言表目录
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public string[] GetLanguageDirectories(string directory)
        {
            var languageNames = Enum.GetNames(typeof(Language));
            var directories = languageNames.Select(languageName => Path.Combine(directory, languageName)).ToArray();
            return directories;
        }

        private readonly Dictionary<string, LanguageTable> m_languageCollector = new Dictionary<string, LanguageTable>();

        private Language m_language;

        private int m_curID = 0;
    }
}
