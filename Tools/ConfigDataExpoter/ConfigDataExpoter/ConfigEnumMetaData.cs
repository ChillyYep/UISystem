using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 枚举描述
    /// </summary>
    class ConfigEnumMetaData : ConfigMetaData
    {
        public class EnumData
        {
            public int m_ID;
            public string m_name;
            public string m_comment;
        }

        public string m_name;

        public string m_comment;

        public Visiblity m_visiblity;

        public const string IDPrimaryKey = "ID";
        public const string ValuePrimaryKey = "Value";
        public const string CommentCell = "Comment";

        public readonly Dictionary<int, EnumData> m_enumNameValue = new Dictionary<int, EnumData>();
    }
}
