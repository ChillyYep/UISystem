using ConfigData;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 多语言处理，语言表数据行单行格式:id(string，标明了来源),原文本,翻译文本，第一行标记语言表，第二行标明来源配置类
    /// </summary>
    public class MultiLanguageCollector : ExcelParserBase
    {
        public MultiLanguageCollector(MultiLanguageExchanger multiLanguageWriter, bool autoRemoveExpiredLanguageItem)
        {
            m_autoRemoveExpiredLanguageItem = autoRemoveExpiredLanguageItem;
            m_multiLanguageWriter = multiLanguageWriter;
            m_curID = 0;
        }

        /// <summary>
        /// 加载翻译文件夹下所有翻译文件
        /// </summary>
        /// <param name="directory"></param>
        public void Load(string directory)
        {
            // 以默认翻译表为基准，其ID和sourceInfo同步到其他翻译表
            LoadOneLanguage(Language.Defaut, directory, true);
            for (int i = 1; i < (int)Language.Count; ++i)
            {
                LoadOneLanguage((Language)i, directory, false);
            }
        }

        /// <summary>
        /// 检测语言表合法性
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="excelNames"></param>
        /// <param name="allLanguageSheets"></param>
        private void CheckLanguageTableLegality(string directory, out List<string> excelNames, out List<ISheet> allLanguageSheets)
        {
            var files = GetAllTopDirectoryExcelFiles(directory);
            allLanguageSheets = new List<ISheet>();
            excelNames = new List<string>();
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
        }

        /// <summary>
        /// 获取或如果不存在则创建语言表
        /// </summary>
        /// <returns></returns>
        private LanguageTable GetOrCreateLanguageTable(string className, string filename, bool enableCreate)
        {
            if (!m_languageCollector.TryGetValue(className, out var languageTable) && enableCreate)
            {
                languageTable = new LanguageTable()
                {
                    m_excelName = filename,
                    m_className = className
                };
                m_languageCollector[className] = languageTable;
            }
            return languageTable;
        }

        /// <summary>
        /// 获取或如果不存在则创建翻译项
        /// </summary>
        /// <param name="languageItems"></param>
        /// <param name="sourceInfoStr"></param>
        /// <param name="sourceInfo"></param>
        /// <param name="enableCreate"></param>
        /// <returns></returns>
        private LanguageItem GetOrCreateLanguageItem(Dictionary<string, LanguageItem> languageItems, string sourceInfoStr, SourceInfo sourceInfo, bool enableCreate)
        {
            if (!languageItems.TryGetValue(sourceInfoStr, out var languageItem) && enableCreate)
            {
                languageItem = new LanguageItem()
                {
                    m_id = ++m_curID,
                    m_source = sourceInfo
                };
                languageItems[sourceInfoStr] = languageItem;
            }
            return languageItem;
        }

        /// <summary>
        /// 加载某一种语言的翻译表
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private void LoadOneLanguage(Language language, string directory, bool enableCreate)
        {
            var languageName = language.ToString();
            directory = Path.Combine(directory, languageName);
            if (!Directory.Exists(directory))
            {
                return;
            }
            CheckLanguageTableLegality(directory, out var excelNames, out var allLanguageSheets);
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
                var languageTable = GetOrCreateLanguageTable(classCell.StringCellValue, filename, enableCreate);
                if (languageTable == null)
                {
                    continue;
                }
                // 去除后缀，防止后续重复添加后缀
                var suffix = "_" + languageName;
                if (languageTable.m_excelName.EndsWith(suffix))
                {
                    languageTable.m_excelName = languageTable.m_excelName.Substring(0, languageTable.m_excelName.LastIndexOf(suffix));
                }

                var languageItems = languageTable.m_allLanguageItems;

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
                    var languageItem = GetOrCreateLanguageItem(languageItems, sourceInfoCell.StringCellValue, sourceInfo, enableCreate);
                    // 根据Default表创建LanguageItem，如果GetOrCreateLanguageItem的结果为空，说明不同语种的翻译表与Default表不同步
                    if (languageItem == null)
                    {
                        continue;
                    }
                    languageItem.SetTranslateTextIfEmpty(language, translateText);
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
            var source = sourceInfo.GetIDStr();
            var id = AddTextForAllLanguage(excelName, className, sourceInfo, source);
            // 添加KeyValuePair
            m_multiLanguageWriter.AddIDTextPair(id, source);
            return source;
        }

        /// <summary>
        /// 向所有语种的翻译表添加翻译项
        /// </summary>
        /// <param name="excelName"></param>
        /// <param name="className"></param>
        /// <param name="sourceInfo"></param>
        /// <param name="sourceinfoStr"></param>
        /// <returns></returns>
        private int AddTextForAllLanguage(string excelName, string className, SourceInfo sourceInfo, string sourceinfoStr)
        {
            var languageTable = GetOrCreateLanguageTable(className, excelName, true);
            var languageItem = GetOrCreateLanguageItem(languageTable.m_allLanguageItems, sourceinfoStr, sourceInfo, true);
            for (int i = 0; i < (int)Language.Count; ++i)
            {
                languageItem.SetTranslateTextIfEmpty((Language)i, string.Empty);
                languageItem.m_source.m_sourceText = sourceInfo.m_sourceText;

            }
            return languageItem.m_id;
        }

        /// <summary>
        /// 保存翻译表
        /// </summary>
        /// <param name="directory"></param>
        public void Save(string directory)
        {
            if (m_autoRemoveExpiredLanguageItem)
            {
                // 保存前删除过期的翻译项，不会修改翻译项的ID，所以不影响数据读取，在下次导出时会重新整理ID，自动解决零碎的ID分布
                RemoveExpiredLanguageItems();
            }
            for (int i = 0; i < (int)Language.Count; ++i)
            {
                SaveOneLanguage((Language)i, directory);
            }
        }

        /// <summary>
        /// 保存某种语言的翻译表
        /// </summary>
        /// <param name="language"></param>
        /// <param name="directory"></param>
        private void SaveOneLanguage(Language language, string directory)
        {
            directory = Path.Combine(directory, language.ToString());
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
                var excelPath = Path.Combine(directory, $"{excelName}_{language}.xlsx");
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
                        translateTextCell.SetCellValue(item.GetTranslateText(language));
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

        /// <summary>
        /// 删除过期翻译项
        /// </summary>
        private void RemoveExpiredLanguageItems()
        {
            // className-sourceInfoList键值对
            var wait2Remove = new Dictionary<string, List<string>>();
            // 按表收集过期项
            foreach (var languagePair in m_languageCollector)
            {
                var className = languagePair.Key;
                var languageTable = languagePair.Value;
                foreach (var languageItemPair in languageTable.m_allLanguageItems)
                {
                    var sourceInfoStr = languageItemPair.Key;
                    if (!m_multiLanguageWriter.m_sourceInfo2ID.ContainsKey(sourceInfoStr))
                    {
                        if (!wait2Remove.TryGetValue(className, out var removeList))
                        {
                            removeList = new List<string>();
                            wait2Remove[className] = removeList;
                        }
                        removeList.Add(sourceInfoStr);
                    }
                }
            }
            // 按表删除过期项
            foreach (var removeItemPair in wait2Remove)
            {
                var removeTable = removeItemPair.Key;
                var removeSourceInfoList = removeItemPair.Value;
                var targetLanguageTable = m_languageCollector[removeTable];
                foreach (var waitRemoveSourceInfo in removeSourceInfoList)
                {
                    if (targetLanguageTable.m_allLanguageItems.ContainsKey(waitRemoveSourceInfo))
                    {
                        targetLanguageTable.m_allLanguageItems.Remove(waitRemoveSourceInfo);
                    }
                }
            }
        }

        public const string MutiLanguageMark = "MutiLanguage";

        private MultiLanguageExchanger m_multiLanguageWriter;

        private Dictionary<string, LanguageTable> m_languageCollector = new Dictionary<string, LanguageTable>();

        private int m_curID = 0;

        private bool m_autoRemoveExpiredLanguageItem;
    }
}
