using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
/* 说明：
 * 以文本方式重构场景/预设结构，如果有对场景进行某些合规性检测的需求，
 * 这种方式可以替换直接加载整个场景(场景加载非常耗时)
*/
public class BaseInfo
{
    public long m_fileId;

    public string m_propertiesText;

    private readonly static Regex s_firstPropertyMatches = new Regex(" {2}[_0-9a-zA-Z]+");

    private Dictionary<string, string> m_firstPropertyInfos = new Dictionary<string, string>();

    public string GetProperty(string propertyName)
    {
        return m_firstPropertyInfos.TryGetValue(propertyName, out var value) ? value : string.Empty;
    }

    public IEnumerable<string> GetIterator()
    {
        return m_firstPropertyInfos.Keys;
    }

    public void UpdateFirstProperty(bool forceUpdate = false)
    {
        if (m_firstPropertyInfos.Count > 0 && !forceUpdate)
        {
            return;
        }
        m_firstPropertyInfos.Clear();
        List<int> propertyBeginIndices = new List<int>();
        var matches = s_firstPropertyMatches.Matches(m_propertiesText);
        for (int i = 0; i < matches.Count; ++i)
        {
            if (matches[i].Index == 0 || (matches[i].Index > 0 && m_propertiesText[matches[i].Index - 1] != ' '))
            {
                propertyBeginIndices.Add(matches[i].Index + 2);
            }
        }
        for (int i = 0; i < propertyBeginIndices.Count; ++i)
        {
            var curIndex = propertyBeginIndices[i];
            string subStr;
            if (i < propertyBeginIndices.Count - 1)
            {
                var nextIndex = propertyBeginIndices[i + 1];
                subStr = m_propertiesText.Substring(curIndex, nextIndex - curIndex - 2);
            }
            else
            {
                subStr = m_propertiesText.Substring(curIndex);
            }
            var match = Regex.Match(subStr, "^[_0-9a-zA-Z]+");
            if (match != null && !string.IsNullOrEmpty(match.Value))
            {
                m_firstPropertyInfos[match.Value] = subStr.Substring(match.Length + 1);
            }
        }
    }
}

public class OwnerInfo : BaseInfo
{
    public TransformInfo m_transformInfo;
}

public class GameObjectInfo : OwnerInfo
{
    public string m_name;
    public List<NativeComponentInfo> m_nativeComponentInfos = new List<NativeComponentInfo>();
    public List<MonoComponentInfo> m_monoComponentInfos = new List<MonoComponentInfo>();
}

public class TransformInfo : BaseInfo
{
    public OwnerInfo m_ownerReference;
    public TransformInfo m_parentTransformInfo;
    public readonly List<TransformInfo> m_childrenTransfroms = new List<TransformInfo>();
}

public class MonoComponentInfo : BaseInfo
{
    public GameObjectInfo m_gameObjectReference;
    public string m_guid;
}

public class NativeComponentInfo : BaseInfo
{
    public GameObjectInfo m_gameObjectReference;
    public string m_type;
}

public class PrefabInstanceInfo : OwnerInfo
{
    private readonly static Regex s_modificationsRegex = new Regex(" {4}[_0-9a-zA-Z]+");
    private readonly static Regex s_guidRegex = new Regex("guid:[0-9a-f]+");
    private Dictionary<string, string> m_modifications = new Dictionary<string, string>();
    public string GetSourcePrefabGuid()
    {
        UpdateFirstProperty();
        var propertyValueStr = GetProperty("m_SourcePrefab");
        if (!string.IsNullOrEmpty(propertyValueStr))
        {
            var match = s_guidRegex.Match(propertyValueStr);
            if (match != null && !string.IsNullOrEmpty(match.Value))
            {
                var guidStr = match.Value.Substring(5);
                return guidStr;
            }
        }
        return string.Empty;
    }

    public string GetModificationProperty(string propertyName)
    {
        return m_modifications.TryGetValue(propertyName, out var propertyValue) ? propertyValue : string.Empty;
    }

    public void UpdateModifications(bool forceUpdate = false)
    {
        if (m_modifications.Count > 0 && !forceUpdate)
        {
            return;
        }
        m_modifications.Clear();
        UpdateFirstProperty();
        var modificationPropValue = GetProperty("m_Modification");
        if (string.IsNullOrEmpty(modificationPropValue))
        {
            return;
        }
        List<int> subPropertyBeginIndices = new List<int>();
        var matches = s_modificationsRegex.Matches(modificationPropValue);
        for (int i = 0; i < matches.Count; ++i)
        {
            var match = matches[i];
            if (match.Index == 0 || (match.Index > 0 && modificationPropValue[match.Index - 1] != ' '))
            {
                subPropertyBeginIndices.Add(match.Index + 4);
            }
        }
        for (int i = 0; i < subPropertyBeginIndices.Count; ++i)
        {
            var curIndex = subPropertyBeginIndices[i];
            string subStr;
            if (i < subPropertyBeginIndices.Count - 1)
            {
                var nextIndex = subPropertyBeginIndices[i + 1];
                subStr = modificationPropValue.Substring(curIndex, nextIndex - curIndex - 4);
            }
            else
            {
                subStr = modificationPropValue.Substring(curIndex);
            }
            var match = Regex.Match(subStr, "^[_0-9a-zA-Z]+");
            if (match != null && !string.IsNullOrEmpty(match.Value))
            {
                m_modifications[match.Value] = subStr.Substring(match.Length + 1);
            }
        }
    }
}

public class MetaInfo
{
    public MetaInfo() { m_instanceId = s_counter; s_counter++; }

    public MetaInfo(MetaInfo other)
    {
        m_instanceId = other.m_instanceId;
        m_fileId = other.m_fileId;
        m_type = other.m_type;
        m_data = other.m_data;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(m_fileId) && !string.IsNullOrEmpty(m_type) && !string.IsNullOrEmpty(m_data);
    }

    public override string ToString()
    {
        return $"instanceId:{m_instanceId},fileId:{m_fileId ?? string.Empty},type:{m_type ?? string.Empty},data:{m_data ?? string.Empty}";
    }

    public string m_fileId;
    public string m_type;
    public string m_data;
    private static int s_counter = 0;
    private int m_instanceId;

}

public enum MetaInfoGeneratorState
{
    /// <summary>
    /// 解析MetaInfo的FileId
    /// </summary>
    ParseHead,
    /// <summary>
    /// 解析MetaInfo的类型
    /// </summary>
    ParseType,
    /// <summary>
    /// 读取MetaInfo内容
    /// </summary>
    CollectContent,
    /// <summary>
    /// 等待生成MetaInfo
    /// </summary>
    WaitGenerate,
    /// <summary>
    /// 成功生成MetaInfo
    /// </summary>
    Success,
    /// <summary>
    /// 生成MetaInfo失败
    /// </summary>
    Failed,
}

/// <summary>
/// MetaInfo生成器
/// </summary>
public class MetaInfoGenerator
{
    public StringBuilder m_content = new StringBuilder();
    public MetaInfoGeneratorState m_metaInfoGeneratorState = MetaInfoGeneratorState.ParseHead;
    private static readonly MetaInfo s_emptyMetaInfo = new MetaInfo();
    public MetaInfo m_curMetaInfo = s_emptyMetaInfo;

    public Regex m_headRegex = new Regex("--- !u![0-9]+ &[0-9]+");
    public Regex m_typeRegex = new Regex("^[a-zA-Z]+:");

    public void Collect(string line)
    {
        // 成功后，切换到解析下一个MetaInfo的状态
        if (m_metaInfoGeneratorState == MetaInfoGeneratorState.Success)
        {
            m_metaInfoGeneratorState = MetaInfoGeneratorState.ParseHead;
        }
        var prevGeneratorState = m_metaInfoGeneratorState;
        switch (m_metaInfoGeneratorState)
        {
            case MetaInfoGeneratorState.ParseHead:
                {
                    var match = m_headRegex.Match(line);
                    var matchValue = match.Value;
                    if (match != null && !string.IsNullOrEmpty(matchValue))
                    {
                        m_metaInfoGeneratorState = MetaInfoGeneratorState.ParseType;
                        m_curMetaInfo = new MetaInfo();
                        m_curMetaInfo.m_fileId = matchValue.Substring(matchValue.IndexOf('&') + 1);
                    }
                    else
                    {
                        m_curMetaInfo = s_emptyMetaInfo;
                        m_metaInfoGeneratorState = MetaInfoGeneratorState.Failed;
                    }
                    break;
                }
            case MetaInfoGeneratorState.ParseType:
                {
                    var match = m_typeRegex.Match(line);
                    var matchValue = match.Value;
                    if (match != null && !string.IsNullOrEmpty(matchValue))
                    {
                        m_metaInfoGeneratorState = MetaInfoGeneratorState.CollectContent;
                        m_curMetaInfo.m_type = matchValue.Substring(0, matchValue.Length - 1);
                    }
                    else
                    {
                        m_curMetaInfo = s_emptyMetaInfo;
                        m_metaInfoGeneratorState = MetaInfoGeneratorState.Failed;
                    }
                    break;
                }
            case MetaInfoGeneratorState.CollectContent:
                {
                    var match = m_headRegex.Match(line);
                    var matchValue = match.Value;
                    // 下一个MetaInfo解析开始的标志
                    if (match != null && !string.IsNullOrEmpty(matchValue))
                    {
                        m_metaInfoGeneratorState = MetaInfoGeneratorState.WaitGenerate;
                    }
                    else
                    {
                        m_content.Append(line);
                    }
                    break;
                }
            case MetaInfoGeneratorState.Success:
            case MetaInfoGeneratorState.Failed:
                {
                    break;
                }
        }
        if (prevGeneratorState != MetaInfoGeneratorState.Failed && m_metaInfoGeneratorState == MetaInfoGeneratorState.Failed)
        {
            Debug.LogError($"Parse MetaInfo Error!FailedState:{prevGeneratorState},MetaInfo:{m_curMetaInfo?.ToString() ?? string.Empty}");
        }
    }

    public bool IsReadyGenerate()
    {
        return m_metaInfoGeneratorState == MetaInfoGeneratorState.WaitGenerate;
    }

    public bool IsFailed()
    {
        return m_metaInfoGeneratorState == MetaInfoGeneratorState.Failed;
    }

    public bool CanMoveNextLine()
    {
        // 成功，需要等下一个开始信号
        return m_metaInfoGeneratorState != MetaInfoGeneratorState.Success &&
            // 失败，整个状态机都要暂停
            m_metaInfoGeneratorState != MetaInfoGeneratorState.Failed;
    }
    private MetaInfo GenerateMetaInfoInternal()
    {
        m_curMetaInfo.m_data = m_content.ToString();
        m_content.Clear();
        var metaInfo = m_curMetaInfo;
        m_curMetaInfo = s_emptyMetaInfo;
        return new MetaInfo(metaInfo);
    }

    public MetaInfo GenerateMetaInfo()
    {
        if (m_metaInfoGeneratorState == MetaInfoGeneratorState.Failed)
        {
            Debug.LogError("GenerateMetaInfo Fail!");
            return null;
        }
        if (m_metaInfoGeneratorState == MetaInfoGeneratorState.WaitGenerate)
        {
            m_metaInfoGeneratorState = MetaInfoGeneratorState.Success;
            return GenerateMetaInfoInternal();
        }
        Debug.LogError($"Parse Failed!CurState:{m_metaInfoGeneratorState}");
        return null;
    }

    public MetaInfo ForceGenerateMetaInfo()
    {
        if (!ReferenceEquals(m_curMetaInfo, s_emptyMetaInfo) && (m_metaInfoGeneratorState == MetaInfoGeneratorState.CollectContent ||
            m_metaInfoGeneratorState == MetaInfoGeneratorState.WaitGenerate))
        {
            m_metaInfoGeneratorState = MetaInfoGeneratorState.Success;
            return GenerateMetaInfoInternal();
        }
        return null;
    }
}

/// <summary>
/// 以文本方式解析场景结构
/// </summary>
public class SerializedContext
{
    public Dictionary<long, GameObjectInfo> m_gameObjects = new Dictionary<long, GameObjectInfo>();

    public Dictionary<long, PrefabInstanceInfo> m_prefabInstances = new Dictionary<long, PrefabInstanceInfo>();

    private Dictionary<long, TransformInfo> m_transforms = new Dictionary<long, TransformInfo>();

    private Dictionary<long, MonoComponentInfo> m_monoComponents = new Dictionary<long, MonoComponentInfo>();

    private Dictionary<long, NativeComponentInfo> m_nativeComponents = new Dictionary<long, NativeComponentInfo>();

    private readonly static Regex s_fileIDKeyValue = new Regex("fileID: [0-9]+");

    private string PickGameObjectName(GameObjectInfo gameObjectInfo)
    {
        gameObjectInfo.UpdateFirstProperty();
        return gameObjectInfo.GetProperty("m_Name");
    }

    private long[] PickFileIDs(string propertyLine)
    {
        long[] fileIds = new long[0];
        if (string.IsNullOrEmpty(propertyLine))
        {
            return fileIds;
        }
        var matches = s_fileIDKeyValue.Matches(propertyLine);
        fileIds = new long[matches.Count];
        for (int i = 0; i < matches.Count; ++i)
        {
            var fileIdStr = matches[i].Value.Substring(8);
            if (fileIdStr != null && !string.IsNullOrEmpty(fileIdStr) && long.TryParse(fileIdStr, out var fileId))
            {
                fileIds[i] = fileId;
            }
        }
        return fileIds;
    }

    private long PickFileIDInProperty(BaseInfo baseInfo, string propertyName)
    {
        baseInfo.UpdateFirstProperty();
        var propertyLine = baseInfo.GetProperty(propertyName);
        var fileIDs = PickFileIDs(propertyLine);
        return fileIDs.Length > 0 ? fileIDs[0] : 0;
    }

    private long[] PickChildrenInTransformProperty(TransformInfo transformInfo)
    {
        transformInfo.UpdateFirstProperty();
        var childrenProperty = transformInfo.GetProperty("m_Children");
        return PickFileIDs(childrenProperty);
    }

    private long PickPrefabInstanceParentTransform(PrefabInstanceInfo prefabInstanceInfo)
    {
        prefabInstanceInfo.UpdateModifications();
        var transformParentValue = prefabInstanceInfo.GetModificationProperty("m_TransformParent");
        var fileIDs = PickFileIDs(transformParentValue);
        return fileIDs.Length > 0 ? fileIDs[0] : 0;
    }

    private void CreateBaseInfos(List<MetaInfo> metaInfos)
    {
        foreach (var metainfo in metaInfos)
        {
            CreateBaseInfo(metainfo);
        }
    }

    private void CreateBaseInfo(MetaInfo metaInfo)
    {
        if (!long.TryParse(metaInfo.m_fileId, out var fieldId))
        {
            return;
        }
        switch (metaInfo.m_type)
        {
            case "PrefabInstance":
                if (!m_prefabInstances.ContainsKey(fieldId))
                {
                    m_prefabInstances[fieldId] = new PrefabInstanceInfo()
                    {
                        m_fileId = fieldId,
                        m_propertiesText = metaInfo.m_data
                    };
                }
                break;
            case nameof(GameObject):
                if (!m_gameObjects.ContainsKey(fieldId))
                {
                    var goInfo = new GameObjectInfo()
                    {
                        m_fileId = fieldId,
                        m_propertiesText = metaInfo.m_data,
                    };
                    goInfo.m_name = PickGameObjectName(goInfo);
                    m_gameObjects[fieldId] = goInfo;
                }
                break;
            case nameof(Transform):
                if (!m_transforms.ContainsKey(fieldId))
                {
                    m_transforms[fieldId] = new TransformInfo()
                    {
                        m_fileId = fieldId,
                        m_propertiesText = metaInfo.m_data
                    };
                }
                break;
            case nameof(MonoBehaviour):
                if (!m_monoComponents.ContainsKey(fieldId))
                {
                    m_monoComponents[fieldId] = new MonoComponentInfo()
                    {
                        m_fileId = fieldId,
                        m_propertiesText = metaInfo.m_data
                    };
                }
                break;
            default:
                if (!m_nativeComponents.ContainsKey(fieldId))
                {
                    m_nativeComponents[fieldId] = new NativeComponentInfo()
                    {
                        m_fileId = fieldId,
                        m_type = metaInfo.m_type,
                        m_propertiesText = metaInfo.m_data
                    };
                }
                break;
        }
    }

    private void BuildReferences()
    {
        // Transform和GameObject建立联系
        foreach (var transformInfo in m_transforms.Values)
        {
            var fileId = PickFileIDInProperty(transformInfo, "m_GameObject");
            if (fileId == 0)
            {
                continue;
            }
            if (m_gameObjects.TryGetValue(fileId, out var gameObjectInfo))
            {
                gameObjectInfo.m_transformInfo = transformInfo;
                transformInfo.m_ownerReference = gameObjectInfo;
            }
        }
        // Transform和PrefabInstance建立联系
        foreach (var transformInfo in m_transforms.Values)
        {
            var fileId = PickFileIDInProperty(transformInfo, "m_PrefabInstance");
            if (fileId == 0)
            {
                continue;
            }
            if (m_prefabInstances.TryGetValue(fileId, out var prefabInstance))
            {
                prefabInstance.m_transformInfo = transformInfo;
                transformInfo.m_ownerReference = prefabInstance;
            }
        }
        // Transform之间的联系
        foreach (var transformInfo in m_transforms.Values)
        {
            var childrenFileIds = PickChildrenInTransformProperty(transformInfo);
            foreach (var fileId in childrenFileIds)
            {
                if (m_transforms.TryGetValue(fileId, out var childInfo))
                {
                    childInfo.m_parentTransformInfo = transformInfo;
                    transformInfo.m_childrenTransfroms.Add(childInfo);
                }
            }
        }

        // PrefabInstance的Transfrom
        foreach (var prefabInstance in m_prefabInstances.Values)
        {
            var fileID = PickPrefabInstanceParentTransform(prefabInstance);
            if (fileID == 0)
            {
                prefabInstance.m_transformInfo = new TransformInfo()
                {
                    m_ownerReference = prefabInstance
                };
                continue;
            }
            if (m_transforms.TryGetValue(fileID, out var parentInfo) && prefabInstance.m_transformInfo != null)
            {
                prefabInstance.m_transformInfo.m_parentTransformInfo = parentInfo;
                parentInfo.m_childrenTransfroms.Add(prefabInstance.m_transformInfo);
            }
        }

        // 建立MonoComponent和GameObjectInfo的联系
        foreach (var monoComponent in m_monoComponents.Values)
        {
            var fileId = PickFileIDInProperty(monoComponent, "m_GameObject");
            if (fileId == 0)
            {
                continue;
            }
            if (m_gameObjects.TryGetValue(fileId, out var gameObjectInfo))
            {
                gameObjectInfo.m_monoComponentInfos.Add(monoComponent);
                monoComponent.m_gameObjectReference = gameObjectInfo;
            }
        }
        // 建立一些NativeComponent和GameObjectInfo的联系
        foreach (var nativeComponent in m_nativeComponents.Values)
        {
            var fileId = PickFileIDInProperty(nativeComponent, "m_GameObject");
            if (fileId == 0)
            {
                continue;
            }
            if (m_gameObjects.TryGetValue(fileId, out var gameObjectInfo))
            {
                gameObjectInfo.m_nativeComponentInfos.Add(nativeComponent);
                nativeComponent.m_gameObjectReference = gameObjectInfo;
            }
        }

    }
    /// <summary>
    /// 获取场景根节点
    /// </summary>
    /// <returns></returns>
    public IEnumerable<OwnerInfo> GetRootGameObject()
    {
        return m_gameObjects.Values.Select(go => go as OwnerInfo).Union(m_prefabInstances.Values.Select(go => go as OwnerInfo)).Where(gameobjectInfo =>
        {
            if (gameobjectInfo == null)
            {
                return false;
            }
            if (gameobjectInfo.m_transformInfo == null)
            {
                return false;
            }
            return gameobjectInfo.m_transformInfo.m_parentTransformInfo == null;
        });
    }
    /// <summary>
    /// 读取场景/预设文件
    /// </summary>
    /// <param name="path"></param>
    public void ReadContext(string path)
    {
        // ClearAll
        m_gameObjects.Clear();
        m_prefabInstances.Clear();
        m_transforms.Clear();
        m_monoComponents.Clear();
        m_nativeComponents.Clear();

        // 1、生成MetaInfo
        MetaInfoGenerator metaInfoGenerator = new MetaInfoGenerator();
        List<MetaInfo> metaInfos = new List<MetaInfo>();
        using (StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
        {
            MetaInfo metaInfo = null;
            // 1、跳过前两行
            sr.ReadLine();
            sr.ReadLine();
            // 2、生成MetaInfo
            string lineStr = string.Empty;
            do
            {
                if (metaInfoGenerator.CanMoveNextLine())
                {
                    lineStr = sr.ReadLine();
                }
                metaInfoGenerator.Collect(lineStr);
                if (metaInfoGenerator.IsReadyGenerate())
                {
                    metaInfo = metaInfoGenerator.GenerateMetaInfo();
                    if (metaInfo != null)
                    {
                        metaInfos.Add(metaInfo);
                    }
                }
                else if (metaInfoGenerator.IsFailed())
                {
                    Debug.LogError("Generate Failed!");
                    break;
                }
            }
            while (!sr.EndOfStream);
            // 结束后大概率还剩一个MetaInfo未解析
            metaInfo = metaInfoGenerator.ForceGenerateMetaInfo();
            if (metaInfo != null)
            {
                metaInfos.Add(metaInfo);
            }
        }

        // 2、创建基础信息
        CreateBaseInfos(metaInfos);

        // 3、构造引用连接
        BuildReferences();

        // 4、测试场景文本方式重构
        var roots = new List<OwnerInfo>(GetRootGameObject());
        Debug.Log($"---------------------------------------");
    }
}

//public class AssetPostProcessTest : AssetPostprocessor
//{
//    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//    {
//        var imports = importedAssets.Concat(movedAssets).ToList();
//        SerializedContext serializedContext = new SerializedContext();
//        foreach (var asset in imports)
//        {
//            if (Path.GetExtension(asset).Equals(".unity"))
//            {
//                serializedContext.ReadContext(asset);
//            }
//        }
//    }
//}
