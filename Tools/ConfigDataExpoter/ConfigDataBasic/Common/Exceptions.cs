using System;
using System.Runtime.Serialization;

namespace ConfigDataExpoter
{
    /// <summary>
    /// Excel解析异常
    /// </summary>
    public class ParseExcelException : Exception
    {
        public ParseExcelException() : base()
        {
        }

        public ParseExcelException(string message) : base(message)
        {
        }

        public ParseExcelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParseExcelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
