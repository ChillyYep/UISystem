using GameBase.Algorithm;
using GameBase.Asset;
using GameBase.Collections;
using GameBase.Editor;
using GameBase.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameBase.BundleBuilder
{
    [Serializable]
    public class SingleAssetCache
    {
        public string m_assetPath;
        public long m_assetSize;
        public string m_assetHash;
    }
    [Serializable]
    public class SingleBundleCache
    {
        public string m_bundleName;

        public List<SingleAssetCache> m_singleAssetCaches = new List<SingleAssetCache>();

        public bool HasDiff(SingleBundleCache other)
        {
            // 不是同一个Bundle，有差异
            if (!m_bundleName.Equals(other.m_bundleName))
            {
                return true;
            }
            // 资源列表长度不一致，有差异
            if (m_singleAssetCaches.Count != other.m_singleAssetCaches.Count)
            {
                return true;
            }
            Dictionary<string, SingleAssetCache> assetSet = new Dictionary<string, SingleAssetCache>();
            foreach (var assetCache in m_singleAssetCaches)
            {
                assetSet.Add(assetCache.m_assetPath, assetCache);
            }

            foreach (var assetCache in other.m_singleAssetCaches)
            {
                // 存在不一样的资源，有差异
                if (!assetSet.TryGetValue(assetCache.m_assetPath, out var myAsset))
                {
                    return true;
                }
                // 资源大小不一样，有差异
                if (myAsset.m_assetSize != assetCache.m_assetSize)
                {
                    return true;
                }
                // 文件Hash不一样，有差异
                if (!myAsset.m_assetPath.Equals(assetCache.m_assetHash))
                {
                    return true;
                }

            }
            return false;
        }
    }
    /// <summary>
    /// 用于记录每次Bundle构建完毕后的信息
    /// </summary>
    [CreateAssetMenu(fileName = nameof(BundleBuildCache), menuName = nameof(BundleBuildCache))]
    public class BundleBuildCache : ScriptableObject
    {
        public static BundleBuildCache LoadBundleBuildCache()
        {
            var bundleBuildSettings = BundleBuildSettings.GetInstance();
            var buildCachePath = bundleBuildSettings.BuildCachePath;
            if (!File.Exists(buildCachePath))
            {
                var directory = Path.GetDirectoryName(buildCachePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                AssetDatabase.CreateAsset(CreateInstance<BundleBuildCache>(), buildCachePath);
                AssetDatabase.Refresh();
            }
            return AssetDatabase.LoadAssetAtPath<BundleBuildCache>(buildCachePath);
        }
        /// <summary>
        /// 获取需要更新的Bundle列表
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        /// <param name="rebuildList"></param>
        /// <param name="removeList"></param>
        public void GetNeedUpdateBundle(Dictionary<string, List<string>> bundleName2AssetList, out List<string> rebuildList, out List<string> removeList)
        {
            var bundleCacheDict = Change2BundleCacheDict(bundleName2AssetList);
            GetNeedUpdateBundle(bundleCacheDict, out rebuildList, out removeList);
        }

        /// <summary>
        /// 转变成Bundle缓存格式
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        /// <returns></returns>
        private Dictionary<string, SingleBundleCache> Change2BundleCacheDict(Dictionary<string, List<string>> bundleName2AssetList)
        {
            var bundleCaches = new Dictionary<string, SingleBundleCache>();
            foreach (var pair in bundleName2AssetList)
            {
                var assetCaches = new List<SingleAssetCache>(pair.Value.Count);
                foreach (var asset in pair.Value)
                {
                    if (asset.Equals(nameof(AssetBundleManifest)))
                    {
                        continue;
                    }
                    if (EncryptHelper.CalcMD5(asset, out var hash))
                    {
                        assetCaches.Add(new SingleAssetCache()
                        {
                            m_assetPath = asset,
                            m_assetSize = new FileInfo(asset).Length,
                            m_assetHash = hash
                        });
                    }
                    else
                    {
                        Debug.LogError($"Compuete Hash Error!Asset:\"{asset}\"");
                        return new Dictionary<string, SingleBundleCache>();
                    }
                }
                bundleCaches[pair.Key] = new SingleBundleCache()
                {
                    m_bundleName = pair.Key,
                    m_singleAssetCaches = assetCaches
                };
            }
            return bundleCaches;
        }

        /// <summary>
        /// 获取需要更新的Bundle列表
        /// </summary>
        /// <param name="allBundleCaches"></param>
        /// <param name="rebuildList"></param>
        /// <param name="removeList"></param>
        private void GetNeedUpdateBundle(Dictionary<string, SingleBundleCache> allBundleCaches, out List<string> rebuildList, out List<string> removeList)
        {
            removeList = new List<string>();
            rebuildList = new List<string>();
            foreach (var oldBundle in m_bundleCaches)
            {
                var oldBundleName = oldBundle.Key;
                var bundleCache = oldBundle.Value;
                if (allBundleCaches.TryGetValue(oldBundleName, out var newBundleCache))
                {
                    // 两边都有的，且比对不上的，就是需要修改的
                    if (bundleCache.HasDiff(newBundleCache))
                    {
                        rebuildList.Add(oldBundleName);
                    }
                }
                else
                {
                    // 删除的
                    removeList.Add(oldBundleName);
                }
            }
            foreach (var newBundle in allBundleCaches)
            {
                var newBundleName = newBundle.Key;
                // 增加的
                if (!m_bundleCaches.TryGetValue(newBundleName, out _))
                {
                    rebuildList.Add(newBundleName);
                }
            }
        }

        public void UpdateCache(AssetBundleMap assetBundleMap)
        {
            m_bundleCaches.Clear();
            var bundleMap = assetBundleMap.GetBundleMap();
            foreach (var bundlePair in bundleMap)
            {
                var assetCahces = new List<SingleAssetCache>(bundlePair.Value.m_assetList.Count);
                foreach (var asset in bundlePair.Value.m_assetList)
                {
                    if (asset.Equals(nameof(AssetBundleManifest)))
                    {
                        continue;
                    }
                    if (EncryptHelper.CalcMD5(asset, out var hash))
                    {
                        assetCahces.Add(new SingleAssetCache()
                        {
                            m_assetPath = asset,
                            m_assetSize = new FileInfo(asset).Length,
                            m_assetHash = hash
                        });
                    }
                    else
                    {
                        Debug.LogError($"Compuete Hash Error!Asset:\"{asset}\"");
                        m_bundleCaches.Clear();
                        return;
                    }
                }
                m_bundleCaches.Add(bundlePair.Key, new SingleBundleCache()
                {
                    m_bundleName = bundlePair.Key,
                    m_singleAssetCaches = assetCahces
                });
            }
            EditorUtils.SaveAndReimport(this);
        }
        [Serializable]
        public class Pair : Pair<string, SingleBundleCache>
        {
            public Pair(string key, SingleBundleCache value) : base(key, value)
            {
            }
        }
        [Serializable]
        public class Str2SingleBundleCacheDictionary : SerializableDictionary<string, SingleBundleCache, Pair>
        {

        }

        public Str2SingleBundleCacheDictionary m_bundleCaches = new Str2SingleBundleCacheDictionary();
    }


    [CustomEditor(typeof(BundleBuildCache))]
    public class BundleBuildCacheInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
        }
    }
}
