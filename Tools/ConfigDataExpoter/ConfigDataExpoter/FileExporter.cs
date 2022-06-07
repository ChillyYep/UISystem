using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    class FileExporter
    {
        public void ExportFile(string filePath, string code)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            try
            {
                if (Directory.Exists(directoryName))
                {
                    Directory.Delete(directoryName, true);
                }
                Directory.CreateDirectory(directoryName);
                var streamWriter = File.CreateText(filePath);
                streamWriter.Write(code);
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch (Exception e)
            {
                throw new ParseExcelException(e.Message);
            }
        }
    }
}
