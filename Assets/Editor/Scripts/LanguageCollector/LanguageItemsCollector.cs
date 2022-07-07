using ConfigData;
using ConfigDataExpoter;
using GameBase.Settings;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class LanguageItemsCollector
{
    /// <summary>
    /// 收集所有UIText
    /// </summary>
    [MenuItem(MenuItemCollection.ExcelProcess.CollectUIText)]
    public static void CollectAllUIText()
    {
        var dir = BundleBuildSettings.GetInstance().RuntimeAssetsDir;
        var prefabs = AssetDatabase.FindAssets("t:prefab", new string[] { dir }).Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));

        int incrementID = 0;
        // 先获得原先就有的
        var oldDict = LoadUITexts();
        var newDict = new Dictionary<string, UITranslateTextItem>();
        ISheet sheet = null;
        // 删除已经存在的
        if (File.Exists(UILangugateTextExcelFilePath))
        {
            File.Delete(UILangugateTextExcelFilePath);
        }
        // 重新创建
        sheet = CreateSheet(out var excelCreator);
        foreach (var translateItem in oldDict)
        {
            newDict[translateItem.Key] = new UITranslateTextItem(incrementID++, translateItem.Value.sourcetext, translateItem.Value.sourceinfo);
        }
        // 加载所有TranlateText
        HashSet<string> allSourceInfos = new HashSet<string>();
        foreach (var prefab in prefabs)
        {
            if (prefab == null)
            {
                continue;
            }
            var translateTexts = prefab.GetComponentsInChildren<TranslateText>();
            foreach (var translateText in translateTexts)
            {
                Debug.Log(AssetDatabase.GetAssetPath(prefab));
                var sourceText = translateText.SourceText;
                if (sourceText == null || string.IsNullOrEmpty(sourceText.text))
                {
                    translateText.m_uiTranslateID = translateText.m_translateID = -1;
                    continue;
                }
                var sourceInfo = GetSourceInfo(prefab, translateText);
                if (!allSourceInfos.Add(sourceInfo))
                {
                    Debug.LogError($"UI上的翻译项出现重复来源,Prefab:{prefab.name},node:{translateText.gameObject.name}，翻译项收集失败！");
                    return;
                }
                if (!newDict.TryGetValue(sourceInfo, out var uITranslateTextItem))
                {
                    uITranslateTextItem = new UITranslateTextItem(incrementID++, sourceText.text, sourceInfo);
                    newDict[sourceInfo] = uITranslateTextItem;
                }
                translateText.m_uiTranslateID = uITranslateTextItem.id;
                EditorUtility.SetDirty(prefab);
            }
        }
        // 收集过期Item
        List<string> removeList = new List<string>();
        foreach (var translateItem in newDict)
        {
            var sourceInfo = translateItem.Value.sourceinfo;
            if (!allSourceInfos.Contains(sourceInfo))
            {
                removeList.Add(sourceInfo);
            }
        }
        // 删除过期Item
        foreach (var removeitem in removeList)
        {
            newDict.Remove(removeitem);
        }
        var curRow = ConfigFieldMetaData.DataBeginRow - 1;
        foreach (var translateItem in newDict)
        {
            curRow++;
            var dataRow = sheet.CreateRow(curRow);
            dataRow.CreateCell(0).SetCellValue(translateItem.Value.id.ToString());
            dataRow.CreateCell(1).SetCellValue(translateItem.Value.sourcetext);
            dataRow.CreateCell(2).SetCellValue(translateItem.Value.sourceinfo);
        }
        excelCreator.Save();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
    /// <summary>
    /// 加载UITexts
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, UITranslateTextItem> LoadUITexts()
    {
        var sourceInfo2TranslateText = new Dictionary<string, UITranslateTextItem>();
        if (!File.Exists(UILangugateTextExcelFilePath))
        {
            return sourceInfo2TranslateText;
        }
        ExcelTypeMetaDataParser excelTypeMetaDataParser = new ExcelTypeMetaDataParser(CodeType.Client);
        var configSheetDatas = excelTypeMetaDataParser.ParseMetaData(UILangugateTextExcelFilePath);
        ExcelDataRowParser rowParser = new ExcelDataRowParser(null);
        List<object> translateItems = new List<object>();
        using (FileStream fs = new FileStream(UILangugateTextExcelFilePath, FileMode.Open, FileAccess.Read))
        {
            var workbook = new XSSFWorkbook(fs);
            foreach (var sheetData in configSheetDatas)
            {
                var sheet = workbook.GetSheet(sheetData.m_sheetName);
                if (sheetData.m_sheetType == SheetType.Class)
                {
                    translateItems.AddRange(rowParser.ParseOneSheetData(typeof(UITranslateTextItem).Assembly, sheet, sheetData, out _));
                }
            }
        }
        foreach (var translateItem in translateItems)
        {
            var uiTranslateItem = translateItem as UITranslateTextItem;
            if (uiTranslateItem == null)
            {
                continue;
            }
            sourceInfo2TranslateText[uiTranslateItem.sourceinfo] = uiTranslateItem;
        }
        return sourceInfo2TranslateText;
    }
    /// <summary>
    /// 创建Sheet
    /// </summary>
    /// <param name="creator"></param>
    /// <returns></returns>
    private static ISheet CreateSheet(out ExcelCreator creator)
    {
        creator = new ExcelCreator(UILangugateTextExcelFilePath);
        // 创建的UI翻译表域信息
        List<ConfigFieldMetaData> fieldsInfo = new List<ConfigFieldMetaData>()
        {
            new ConfigFieldMetaData()
            {
                ColumnIndex=0,
                FieldName="ID",
                Comment="ID",
                ListType="None",
                ForeignKey="None",
                OwnVisiblity=Visiblity.Client,
                OwnDataType=DataType.Int32,
                BelongClassName=nameof(UITranslateTextItem)
            },
            new ConfigFieldMetaData()
            {
                ColumnIndex=1,
                FieldName="SourceText",
                Comment="原始文本",
                ListType="None",
                ForeignKey="None",
                OwnVisiblity=Visiblity.Client,
                OwnDataType=DataType.Text,
                BelongClassName=nameof(UITranslateTextItem)
            },
            new ConfigFieldMetaData()
            {
                ColumnIndex=2,
                FieldName="SourceInfo",
                Comment="来源信息",
                ListType="None",
                ForeignKey="None",
                OwnVisiblity=Visiblity.Client,
                OwnDataType=DataType.String,
                BelongClassName=nameof(UITranslateTextItem)
            }};

        // 创建类型元数据
        ConfigClassMetaData classMetaData = new ConfigClassMetaData()
        {
            m_classname = nameof(UITranslateTextItem),
            m_comment = "来自UI的翻译项"
        };
        classMetaData.m_fieldsInfo.AddRange(fieldsInfo);

        ConfigSheetData configSheetData = new ConfigSheetData()
        {
            m_fileName = Path.GetFileNameWithoutExtension(UILangugateTextExcelFilePath),
            m_sheetName = nameof(UITranslateTextItem),
            m_filePath = UILangugateTextExcelFilePath,
            m_sheetType = SheetType.Class,
            m_configMetaData = classMetaData
        };
        // 创建Sheet
        var sheet = creator.CreateSheet(configSheetData);
        creator.Save();
        return sheet;

    }
    /// <summary>
    /// 获取来源信息
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string GetSourceInfo(GameObject prefab, TranslateText text)
    {
        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab)) + GetNodePath(text);
    }
    /// <summary>
    /// 获取预设内某节点的路径
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string GetNodePath(TranslateText text)
    {
        StringBuilder sb = new StringBuilder();
        Transform parent = text.transform;
        while (parent != null)
        {
            sb.Append("_");
            sb.Append(parent.name);
            parent = parent.parent;
        }
        return sb.ToString();
    }
    private static string UILangugateTextExcelFilePath = Path.Combine(Application.dataPath, "../Tools/ConfigDataExpoter/Design/UITranslateItems.xlsx");
}
