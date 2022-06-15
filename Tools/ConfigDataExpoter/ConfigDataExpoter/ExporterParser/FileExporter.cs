using System;
using System.IO;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 文件导出工具
    /// </summary>
    public class FileExporter
    {
        public void ExportFile(string filePath, string code, bool recreateDirectory = true)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            try
            {
                if (Directory.Exists(directoryName) && recreateDirectory)
                {
                    Directory.Delete(directoryName, true);
                    Directory.CreateDirectory(directoryName);
                }
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

        public void ExportFile(string filePath, byte[] bytes, bool recreateDirectory = true)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            try
            {
                if (Directory.Exists(directoryName))
                {
                    if (recreateDirectory)
                    {
                        Directory.Delete(directoryName, true);
                        Directory.CreateDirectory(directoryName);
                    }
                }
                else
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                throw new ParseExcelException(e.Message);
            }
        }

        public void CopyDirectory(string srcDirectory, string dstDirectory, bool createIfDoesntExist = true)
        {
            if (!Directory.Exists(dstDirectory))
            {
                if (createIfDoesntExist)
                {
                    Directory.CreateDirectory(dstDirectory);
                }
                else
                {
                    throw new ParseExcelException($"目标目录不存在{dstDirectory}");
                }
            }
            var srcFiles = Directory.GetFiles(srcDirectory);
            foreach (var file in srcFiles)
            {
                File.Copy(file, Path.Combine(dstDirectory, Path.GetFileName(file)), true);
            }
        }
    }
}
