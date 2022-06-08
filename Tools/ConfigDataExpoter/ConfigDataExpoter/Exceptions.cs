using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
