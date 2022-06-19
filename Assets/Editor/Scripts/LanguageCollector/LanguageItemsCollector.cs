using ConfigData;
using GameBase.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UITranslateTextItem : IBinarySerializer, IBinaryDeserializer, IConfigData
{
    public int m_id;
    public string m_sourceText;
    public string m_sourceInfo;

    public int id => m_id;

    public void Deserialize(BinaryParser reader)
    {
        m_id = reader.ReadInt32();
        m_sourceText = reader.ReadString();
        m_sourceInfo = reader.ReadString();
    }

    public void Serialize(BinaryFormatter formatter)
    {
        formatter.WriteInt32(m_id);
        formatter.WriteString(m_sourceText);
        formatter.WriteString(m_sourceInfo);
    }
}

public static class LanguageItemsCollector
{
    public static void CollectAllUIText()
    {
        var dir = GameClientSettings.LoadMainGameClientSettings().m_bundleBuildSettings.RuntimeAssetsDir;
        var prefabs = AssetDatabase.FindAssets("t:prefab", new string[] { dir }).Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));

        int incrementID = 0;
        foreach (var prefab in prefabs)
        {
            if (prefab == null)
            {
                continue;
            }
            var translateTexts = prefab.GetComponentsInChildren<TranslateText>();
            foreach (var translateText in translateTexts)
            {

                GetSourceInfo(prefab, translateText);
            }
        }
    }

    //private static Dictionary<string, UITranslateTextItem> LoadUITexts()
    //{
    //    Dictionary<string, UITranslateTextItem> sourceInfo2TranslateText = new Dictionary<string, UITranslateTextItem>();
    //    ConfigDataExpoter.ExcelTypeMetaDataParser excelTypeMetaDataParser = new ConfigDataExpoter.ExcelTypeMetaDataParser(ConfigDataExpoter.CodeType.Client);
    //    var configSheetDatas = excelTypeMetaDataParser.ParseMetaData(UILangugateTextExcelFilePath);
    //    foreach (var sheetData in configSheetDatas)
    //    {
    //        if (sheetData.m_sheetType == ConfigDataExpoter.SheetType.Class)
    //        {
    //        }
    //    }
    //}
    private static string GetSourceInfo(GameObject prefab, TranslateText text)
    {
        return string.Empty;
    }
    private const string UILangugateTextExcelFilePath = "";
}
