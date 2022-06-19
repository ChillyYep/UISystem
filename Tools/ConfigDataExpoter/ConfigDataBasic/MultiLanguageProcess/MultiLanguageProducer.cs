using ConfigData;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 解析所有多语言表，转换成多种语言的翻译项列表
    /// </summary>
    public class MultiLanguageProducer : ExcelParserBase
    {
        /// <summary>
        /// 解析所有多语言表，转换成多种语言的翻译项列表
        /// </summary>
        /// <param name="srcDirectory"></param>
        /// <returns></returns>
        public Dictionary<string, List<LanguageTextItem>> ProduceMultiLanguageItems(string srcDirectory)
        {
            var languageTextItemsDict = new Dictionary<string, List<LanguageTextItem>>();
            List<ISheet> sheets = new List<ISheet>();
            for (int language = 0; language < (int)Language.Count; ++language)
            {
                var languageName = ((Language)language).ToString();
                sheets.Clear();
                // 某种语言
                var languageDirPath = Path.Combine(srcDirectory, languageName);

                var langugateTextItems = new List<LanguageTextItem>();

                if (Directory.Exists(languageDirPath))
                {
                    var files = GetAllDirectoryExcelFiles(languageDirPath);
                    foreach (var file in files)
                    {
                        sheets.AddRange(GetSheets(file));
                    }
                }
                foreach (var sheet in sheets)
                {
                    var markRow = sheet.GetRow(0);
                    if (markRow == null)
                    {
                        continue;
                    }
                    var markCell = markRow.GetCell(0);
                    if (markCell == null || markCell.CellType != CellType.String)
                    {
                        continue;
                    }
                    if (!markCell.StringCellValue.Equals(MultiLanguageCollector.MutiLanguageMark, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    var classNameRow = sheet.GetRow(1);
                    if (classNameRow == null)
                    {
                        continue;
                    }
                    var classNameCell = classNameRow.GetCell(0);
                    if (classNameCell == null || classNameCell.CellType != CellType.String)
                    {
                        continue;
                    }
                    var classname = classNameCell.StringCellValue;
                    for (int i = 3; i <= sheet.LastRowNum; ++i)
                    {
                        var curRow = sheet.GetRow(i);
                        var idCell = curRow.GetCell(0);
                        var translateTextCell = curRow.GetCell(2);
                        if (idCell == null || idCell.CellType != CellType.String)
                        {
                            throw new ParseExcelException($"{classname}ID解析失败");
                        }
                        var translateText = translateTextCell == null ? string.Empty : translateTextCell.StringCellValue;
                        langugateTextItems.Add(new LanguageTextItem()
                        {
                            m_id = int.Parse(idCell.StringCellValue),
                            m_translateText = translateText
                        });
                    }
                }
                languageTextItemsDict[languageName] = langugateTextItems;
            }
            return languageTextItemsDict;
        }
    }
}
