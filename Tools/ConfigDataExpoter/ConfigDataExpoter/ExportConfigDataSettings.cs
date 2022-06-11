using ConfigData;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace ConfigDataExpoter
{
    [Serializable]
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

        public void Deserialize(BinaryParser reader)
        {
            ExportRootDirectoryPath = reader.ReadString();
            ExportCodeDirectoryName = reader.ReadString();
            ExportDataDirectoryName = reader.ReadString();
            ExportConfigDataName = reader.ReadString();
            ExportLoaderCodeName = reader.ReadString();
            ExportTypeEnumCodeName = reader.ReadString();
            CopyFromDirectoryPath = reader.ReadString();
            UnityCodeDirectory = reader.ReadString();
            UnityDataDirectory = reader.ReadString();
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
        }
    }
}
