using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    /// <summary>
    /// Excel解析工具
    /// </summary>
    public abstract class ExcelParserBase
    {
        public List<ISheet> GetSheets(string path)
        {
            List<ISheet> sheets = new List<ISheet>();
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
                        sheets.Add(workbook.GetSheetAt(i));
                    }
                }
            }
            return sheets;
        }

        public string[] GetAllTopDirectoryExcelFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.xlsx", SearchOption.TopDirectoryOnly);
        }
    }
}
