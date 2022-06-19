namespace ConfigData
{
    /// <summary>
    /// 输出后翻译项，包含ID和翻译文本
    /// </summary>
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
