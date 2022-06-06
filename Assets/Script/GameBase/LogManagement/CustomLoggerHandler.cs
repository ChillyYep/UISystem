using GameBase.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBase.Log
{
    /// <summary>
    /// 自定义Log行为
    /// </summary>
    public class CustomLoggerHandler : ILogHandler
    {
        public CustomLoggerHandler(ILogHandler unityLogHandler)
        {
            UnityLogHandler = unityLogHandler;
        }
        public void LogException(Exception exception, UnityEngine.Object context)
        {
            if (LogManager.Instance.NeedEngineLog)
            {
                UnityLogHandler.LogException(exception, context);
            }

            if (LogManager.Instance.NeedFileLog && exception != null && Application.isPlaying)
            {
                LogManager.Instance.OutputLog(exception.Message, exception.StackTrace, LogType.Exception);
            }
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (LogManager.Instance.NeedEngineLog)
            {
                UnityLogHandler.LogFormat(logType, context, format, args);
            }
            if (LogManager.Instance.NeedFileLog && Application.isPlaying)
            {
                LogManager.Instance.OutputLog(string.Format(format, args), "", logType);
            }
        }
        /// <summary>
        /// Unity使用的日志接口
        /// </summary>
        public readonly ILogHandler UnityLogHandler;
    }
}

