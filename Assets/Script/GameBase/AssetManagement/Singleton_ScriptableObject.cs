using GameBase.Asset;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 目前仅能够给Resources下的唯一序列化资源使用
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton_ScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance;
    public static T GetInstance()
    {
        if (instance == null)
        {
            var assetType = typeof(T);
            var attrs = assetType.GetCustomAttributes(typeof(UniqueResourcesAsset), true) as UniqueResourcesAsset[];
            if (attrs != null && attrs.Length > 0)
            {
                if (attrs[0].UniquePath.StartsWith("Resources/") || attrs[0].UniquePath.IndexOf("/Resources/") > 0)
                {
                    var path = attrs[0].UniquePathWithoutExtension.Substring(attrs[0].UniquePath.LastIndexOf("Resources/") + 10);
                    instance = Resources.Load<T>(path);
                    if (instance == null)
                    {
                        Debug.Log($"{assetType.Name}不存在，请先在{attrs[0].UniquePath}创建该资源");
                    }
                }
            }
        }
        return instance;
    }
}
