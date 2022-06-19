using System;
using System.IO;

namespace ConfigDataExpoter
{
    public enum CopyDirectoryOp
    {
        None,
        CreateIfDoesntExist,
        RecreateIfExist
    }
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

        public void CopyDirectory(string srcDirectory, string dstDirectory, CopyDirectoryOp copyDirectoryOp)
        {
            if (!Directory.Exists(dstDirectory))
            {
                if (copyDirectoryOp == CopyDirectoryOp.CreateIfDoesntExist)
                {
                    Directory.CreateDirectory(dstDirectory);
                }
                else
                {
                    throw new ParseExcelException($"目标目录不存在{dstDirectory}");
                }
            }
            else if (copyDirectoryOp == CopyDirectoryOp.RecreateIfExist)
            {
                Directory.Delete(dstDirectory, true);
                Directory.CreateDirectory(dstDirectory);
            }
            var srcFiles = Directory.GetFiles(srcDirectory);
            foreach (var file in srcFiles)
            {
                File.Copy(file, Path.Combine(dstDirectory, Path.GetFileName(file)), true);
            }
        }
    }
}
