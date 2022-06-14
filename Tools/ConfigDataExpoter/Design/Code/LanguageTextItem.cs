using System;
using System.Collections.Generic;

namespace ConfigData
{
    public enum Language
    {
        CN = 0,
        EN = 1
    }

    public class LanguageTextItem : IBinarySerializer, IBinaryDeserializer
    {
        public int m_id;
        public string m_translateText;

        public void Deserialize(BinaryParser reader)
        {
            m_id = reader.ReadInt32();
            m_translateText = reader.ReadString();
        }

        public void Serialize(BinaryFormatter formatter)
        {
            formatter.WriteInt32(m_id);
            formatter.WriteString(m_translateText);
        }
    }
}
