using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace GameBase.Log
{
    /// <summary>
    /// 日志名规则
    /// </summary>
    public enum LogNameRule
    {
        /// <summary>
        /// 年月日时分秒
        /// </summary>
        YMdHms,
        /// <summary>
        /// 年月日时分
        /// </summary>
        YMdHm,
        /// <summary>
        /// 年月日时
        /// </summary>
        YMdH,
        /// <summary>
        /// 年月日
        /// </summary>
        YMd
    }
    /// <summary>
    /// 日志文件输出
    /// </summary>
    public class FileLogger
    {
        public FileLogger(string fileRoot, LogNameRule logNameRule = LogNameRule.YMdHms)
        {
            LogFileDir = fileRoot;
            if (!Directory.Exists(fileRoot))
            {
                Directory.CreateDirectory(fileRoot);
            }
            switch (logNameRule)
            {
                case LogNameRule.YMd:
                    m_logFilePath = fileRoot + $"/Log_{DateTime.Now:yyyy_MMdd}.txt";
                    break;
                case LogNameRule.YMdH:
                    m_logFilePath = fileRoot + $"/Log_{DateTime.Now:yyyy_MMdd_HH}.txt";
                    break;
                case LogNameRule.YMdHm:
                    m_logFilePath = fileRoot + $"/Log_{DateTime.Now:yyyy_MMdd_HHmm}.txt";
                    break;
                default:
                    m_logFilePath = fileRoot + $"/Log_{DateTime.Now:yyyy_MMdd_HHmm_ss}.txt";
                    break;
            }

            m_fileWriter = new StreamWriter(m_logFilePath);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="logStr"></param>
        /// <param name="logType"></param>
        public void WriteLog(string logStr, string stackTrace, UnityEngine.LogType logType)
        {
            if (m_fileWriter == null)
            {
                return;
            }
            lock (m_fileWriter)
            {
                try
                {
                    var logPrefix = $"[{ GetLogLevelStrByLogType(logType)}][{ DateTime.Now:yyyy/MM/dd-HH:mm:ss}]";

                    m_fileWriter.WriteLine($"{logPrefix} {logStr}");

                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        switch (logType)
                        {
                            case LogType.Assert:
                            case LogType.Error:
                            case LogType.Exception:
                                m_stringBuilder.Clear();
                                var stackTraceItems = stackTrace.Split('\n');
                                foreach (var item in stackTraceItems)
                                {
                                    if (!string.IsNullOrEmpty(item))
                                    {
                                        m_stringBuilder.AppendLine("\t\t\t\t\t\t\t" + item);
                                    }
                                }
                                m_fileWriter.Write(m_stringBuilder.ToString());
                                break;
                        }
                    }
                    m_fileWriter.Flush();
                }
                catch
                {
                    UnityEngine.Debug.LogError("Fail to output log");
                    return;
                }
            }
        }

        /// <summary>
        /// 关闭文件输出
        /// </summary>
        public void Close()
        {
            if (m_fileWriter == null)
            {
                return;
            }
            lock (m_fileWriter)
            {
                try
                {
                    m_fileWriter.Close();
                }
                catch
                {
                    UnityEngine.Debug.LogError("Fail to close file's Output!");
                    return;
                }
            }
        }

        /// <summary>
        /// 获取日志级别字符串
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        private string GetLogLevelStrByLogType(LogType logType)
        {
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    return "Error";
                case LogType.Log:
                    return "Info";
                case LogType.Warning:
                    return "Warning";
            }
            return "NoLevel";
        }

        public readonly string LogFileDir;

        private string m_logFilePath;

        private StreamWriter m_fileWriter;

        private readonly StringBuilder m_stringBuilder = new StringBuilder();
    }
}
