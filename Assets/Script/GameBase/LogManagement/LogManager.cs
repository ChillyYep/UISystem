using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using UnityEngine;

namespace GameBase.Log
{
    /// <summary>
    /// 日志相关设置
    /// </summary>
    [Serializable]
    public class LogSettings
    {
        public bool IsNeedFileLog = false;
        public bool IsNeedEngineLog = false;
        public bool IsInPersistentDir = true;
        public LogNameRule LogNameRule = LogNameRule.YMd;
        public string LogFileDir = "../GameRunningLog";
    }

    /// <summary>
    /// 日志管理
    /// </summary>
    public class LogManager : Singleton_CSharp<LogManager>
    {
        public void Initialize(LogSettings logSettings)
        {
            NeedFileLog = logSettings.IsNeedFileLog;
            NeedEngineLog = logSettings.IsNeedEngineLog;
            if (NeedFileLog)
            {
                try
                {
#if UNITY_EDITOR
                    m_fileLogger = new FileLogger(logSettings.LogFileDir, logSettings.LogNameRule);
#else
                    m_fileLogger = new FileLogger(Path.Combine(logSettings.IsInPersistentDir ? Application.persistentDataPath : Application.temporaryCachePath, logSettings.LogFileDir), logSettings.LogNameRule);
#endif
                }
                catch
                {
                    m_fileLogger = null;
                    UnityEngine.Debug.LogError($"Fail to Create Directory:{logSettings.LogFileDir}");
                }
            }
            m_customLoggerHandler = new CustomLoggerHandler(UnityEngine.Debug.unityLogger.logHandler);
            UnityEngine.Debug.unityLogger.logHandler = m_customLoggerHandler;
        }

        public void Unintialize()
        {
            if (m_fileLogger != null)
            {
                m_fileLogger.Close();
                m_fileLogger = null;
            }
            if (m_customLoggerHandler != null)
            {
                UnityEngine.Debug.unityLogger.logHandler = m_customLoggerHandler.UnityLogHandler;
            }
        }
        public void OutputLog(string condition, string stackTrace, LogType type)
        {
            if (m_fileLogger == null)
            {
                return;
            }
            m_fileLogger.WriteLog(condition, stackTrace, type);
        }
        /// <summary>
        /// 需要文件日志
        /// </summary>
        public bool NeedFileLog { get; private set; }
        /// <summary>
        /// 需要引擎日志
        /// </summary>
        public bool NeedEngineLog { get; private set; }
        /// <summary>
        /// 日志文件输出
        /// </summary>
        private FileLogger m_fileLogger;
        /// <summary>
        /// 自定义的日志行为
        /// </summary>
        private CustomLoggerHandler m_customLoggerHandler;
    }

}
