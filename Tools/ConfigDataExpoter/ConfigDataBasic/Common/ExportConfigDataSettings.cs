using ConfigData;

namespace ConfigDataExpoter
{
    public enum CodeType
    {
        Client,
        Server
    }
    /// <summary>
    /// 程序配置
    /// </summary>
    public class ExportConfigDataSettings : IBinarySerializer, IBinaryDeserializer
    {
        public string ExportRootDirectoryPath = "../../../Design";
        public string ExportCodeDirectoryName = "Code";
        public string ExportDataDirectoryName = "Data";
        public string ExportConfigDataName = "ConfigData.cs";
        public string ExportLoaderCodeName = "ConfigDataLoader_AutoGen.cs";
        public string ExportTypeEnumCodeName = "ConfigDataTypeEnum.cs";
        public string CopyFromDirectoryPath = "../../CopyFromDirectory";
        public string UnityCodeDirectory = "";
        public string UnityDataDirectory = "";
        public CodeType CodeVisiblity = CodeType.Client;
        public string ExportLanguageDirectoryName = "Language";
        public string UnityLanaguageDirectory = "";
        public bool RemoveExpiredLanguageItem = true;

        private string ReadString(BinaryParser reader)
        {
            try
            {
                return reader.ReadString();
            }
            catch
            {
                return string.Empty;
            }
        }
        private int ReadEnum(BinaryParser reader)
        {
            try
            {
                return reader.ReadEnum();
            }
            catch
            {
                return default(int);
            }
        }
        private bool ReadBoolean(BinaryParser reader)
        {
            try
            {
                return reader.ReadBoolean();
            }
            catch
            {
                return default(bool);
            }
        }
        public void Deserialize(BinaryParser reader)
        {
            ExportRootDirectoryPath = ReadString(reader);
            ExportCodeDirectoryName = ReadString(reader);
            ExportDataDirectoryName = ReadString(reader);
            ExportConfigDataName = ReadString(reader);
            ExportLoaderCodeName = ReadString(reader);
            ExportTypeEnumCodeName = ReadString(reader);
            CopyFromDirectoryPath = ReadString(reader);
            UnityCodeDirectory = ReadString(reader);
            UnityDataDirectory = ReadString(reader);
            CodeVisiblity = (CodeType)ReadEnum(reader);
            ExportLanguageDirectoryName = ReadString(reader);
            UnityLanaguageDirectory = ReadString(reader);
            RemoveExpiredLanguageItem = ReadBoolean(reader);
        }

        public void Serialize(BinaryFormatter formatter)
        {
            formatter.WriteString(ExportRootDirectoryPath);
            formatter.WriteString(ExportCodeDirectoryName);
            formatter.WriteString(ExportDataDirectoryName);
            formatter.WriteString(ExportConfigDataName);
            formatter.WriteString(ExportLoaderCodeName);
            formatter.WriteString(ExportTypeEnumCodeName);
            formatter.WriteString(CopyFromDirectoryPath);
            formatter.WriteString(UnityCodeDirectory);
            formatter.WriteString(UnityDataDirectory);
            formatter.WriteEnum((int)CodeVisiblity);
            formatter.WriteString(ExportLanguageDirectoryName);
            formatter.WriteString(UnityLanaguageDirectory);
            formatter.WriteBoolean(RemoveExpiredLanguageItem);
        }
    }
}
