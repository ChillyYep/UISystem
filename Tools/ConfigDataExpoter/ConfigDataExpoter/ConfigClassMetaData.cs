using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    public abstract class ConfigMetaData
    {
        public T ParseEnum<T>(string value, T defaultValue) where T : struct
        {
            var dataTypeStr = value;
            if (string.IsNullOrEmpty(dataTypeStr))
            {
                return defaultValue;
            }
            if (Enum.TryParse<T>(dataTypeStr, out var result))
            {
                return result;
            }
            return defaultValue;
        }
    }
    /// <summary>
    /// 表数据类信息
    /// </summary>
    class ConfigClassMetaData : ConfigMetaData
    {
        /// <summary>
        /// 类名
        /// </summary>
        public string m_name;
        /// <summary>
        /// 类注释
        /// </summary>
        public string m_comment;

        public const string IDPrimaryKey = "ID";
        /// <summary>
        /// 域名信息
        /// </summary>
        public readonly List<ConfigFieldMetaData> m_fieldsInfo = new List<ConfigFieldMetaData>();

    }
}
