﻿using System;
using System.Collections.Generic;
using System.IO;
using ConfigData;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 多语言表导出
    /// </summary>
    public class SerializedMultiLanguageExporter : FileExporter
    {
        public SerializedMultiLanguageExporter(string srcDirectory)
        {
            m_srcDirectory = srcDirectory;
            var multiLanguageParser = new MultiLanguageProducer();
            m_languageTextItemsDict = multiLanguageParser.ProduceMultiLanguageItems(srcDirectory);
        }

        public void ExportData(string dstDirectory)
        {
            var languageNames = Enum.GetNames(typeof(Language));
            foreach (var languageName in languageNames)
            {
                var languageDir = Path.Combine(dstDirectory, languageName);
                // 一个语言一个二进制文件
                using (MemoryStream ms = new MemoryStream())
                {
                    m_formatter = new BinaryFormatter(ms);
                    if (m_languageTextItemsDict.TryGetValue(languageName, out var languageTextItems))
                    {
                        m_formatter.WriteObjectNoGeneric(languageTextItems);
                    }
                    m_formatter.Flush();
                    m_formatter.Close();
                    ExportFile(Path.Combine(languageDir, languageName + ".txt"), ms.GetBuffer());
                }
            }
        }

        private readonly Dictionary<string, List<LanguageTextItem>> m_languageTextItemsDict;

        private BinaryFormatter m_formatter;

        private string m_srcDirectory;
    }
}
