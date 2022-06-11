using GameBase.Algorithm;
using GameBase.Collections;
using GameBase.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameBase.Asset
{
    [Serializable]
    public class DependenciesGraph
    {
        /// <summary>
        /// 更新依赖
        /// </summary>
        /// <param name="bundle2Dependencies"></param>
        public void Update(Dictionary<string, List<string>> bundle2Dependencies)
        {
            m_dependenciesDict.Clear();
            foreach (var bundle2AssetListPair in bundle2Dependencies)
            {
                m_dependenciesDict.Add(bundle2AssetListPair.Key, bundle2AssetListPair.Value);
            }
        }

        /// <summary>
        /// 获取直接依赖
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public List<string> GetDirectDependencies(string bundleName)
        {
            if (m_dependenciesDict.TryGetValue(bundleName, out var dependencies))
            {
                return dependencies;
            }
            return new List<string>();
        }

        /// <summary>
        /// 获取所有依赖
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public List<string> GetAllDependencies(string bundleName)
        {
            HashSet<string> bundlesSet = new HashSet<string>();
            SearchHelper.DepthFirstSearch(bundleName, (node, nodePath) => bundlesSet.Add(node), node => GetDirectDependencies(node).Where(dependency => !bundlesSet.Contains(dependency)));
            bundlesSet.Remove(bundleName);
            return new List<string>(bundlesSet);
        }

        [Serializable]
        public class Pair : Pair<string, List<string>>
        {
            public Pair(string key, List<string> value) : base(key, value)
            {
            }
        }

        [Serializable]
        public class DependenciesDict : SerializableDictionary<string, List<string>, Pair> { }

        [SerializeField]
        private DependenciesDict m_dependenciesDict = new DependenciesDict();
    }
    public enum Location
    {
        StreamingAssetDir,
        AssetBundleDir
    }
    /// <summary>
    /// AssetBundle 映射表
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AssetBundleMap), menuName = nameof(AssetBundleMap))]
    public class AssetBundleMap : ScriptableObject
    {
        /// <summary>
        /// 初始化映射
        /// </summary>
        public void SetLocation(Location location)
        {
            var gameClientSettings = GameClientSettings.LoadMainGameClientSettings();
            var resSetting = gameClientSettings.m_resPathSettings;

            var useManifest = gameClientSettings.m_bundleBuildSettings.UseAssetBundleManifestCollectDependencies;
            switch (location)
            {
                case Location.AssetBundleDir:
                    {
                        var ab = AssetBundle.LoadFromFile(Path.Combine(resSetting.GetStreamingAssetsByPlatform(), resSetting.GetAssetBundleManifestName()));
                        m_manifest = ab.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
                    }
                    break;
                case Location.StreamingAssetDir:
                    {
                        var path = Path.Combine(Application.dataPath, resSetting.EditorBundleDirectoryRelativeToAssets, resSetting.GetPlatformFolder());
                        path = Path.Combine(path, resSetting.GetAssetBundleManifestName());
                        var ab = AssetBundle.LoadFromFile(path);
                        m_manifest = ab.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
                    }
                    break;
            }
            if (m_manifest == null && useManifest)
            {
                Debug.LogError("Fail to load AssetBundleManifest!");
            }
        }

        /// <summary>
        /// 获取资源对应的AssetBundle名称
        /// </summary>
        /// <param name="originAssetPath"></param>
        /// <returns></returns>
        public string GetAssetBundleName(string originAssetPath)
        {
            if (m_inverseBundleMap.TryGetValue(originAssetPath, out var bundleName))
            {
                return bundleName;
            }
            return "";
        }

        /// <summary>
        /// 获取BundleMap
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, SingleBundleInfo> GetBundleMap()
        {
            return m_bundleMap.Dict;
        }

        public void Add(string assetPath, string bundleName)
        {
            m_inverseBundleMap.Add(assetPath, bundleName);
            if (!m_bundleMap.TryGetValue(bundleName, out var singleBundleInfo))
            {
                singleBundleInfo = new SingleBundleInfo()
                {
                    m_bundleNameWithSuffix = bundleName
                };
                m_bundleMap[bundleName] = singleBundleInfo;
            }
            if (!singleBundleInfo.m_assetList.Contains(assetPath))
            {
                singleBundleInfo.m_assetList.Add(assetPath);
            }
        }

        public void Add(SingleBundleInfo singleBundleInfo)
        {
            var bundleName = singleBundleInfo.m_bundleNameWithSuffix;
            m_bundleMap[bundleName] = singleBundleInfo;
            foreach (var asset in singleBundleInfo.m_assetList)
            {
                m_inverseBundleMap.Add(asset, bundleName);
            }
        }

        /// <summary>
        /// 更新依赖图，浅拷贝
        /// </summary>
        /// <param name="dependenciesGraph"></param>
        public void UpdateDependencies(DependenciesGraph dependenciesGraph)
        {
            m_dependencies = dependenciesGraph;
        }

        /// <summary>
        /// 更新依赖图
        /// </summary>
        /// <param name="dependenciesGraph"></param>
        public void UpdateDependencies(Dictionary<string, List<string>> dependencies)
        {
            m_dependencies.Update(dependencies);
        }

        /// <summary>
        /// 获取直接依赖
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public List<string> GetDirectDependencies(string bundleName)
        {
            if (m_manifest != null && m_useManifest)
            {
                return new List<string>(m_manifest.GetDirectDependencies(bundleName));
            }
            return m_dependencies.GetDirectDependencies(bundleName);
        }

        /// <summary>
        /// 获取所有依赖
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public List<string> GetAllDependencies(string bundleName)
        {
            if (m_manifest != null && m_useManifest)
            {
                return new List<string>(m_manifest.GetAllDependencies(bundleName));
            }
            return m_dependencies.GetAllDependencies(bundleName);
        }

        /// <summary>
        /// 删除资源
        /// </summary>
        /// <param name="assetPath"></param>
        public void RemoveAsset(string assetPath)
        {
            if (m_inverseBundleMap.TryGetValue(assetPath, out var bundleName))
            {
                if (m_bundleMap.TryGetValue(bundleName, out var bundleInfo))
                {
                    bundleInfo.m_assetList.Remove(assetPath);
                    m_inverseBundleMap.Remove(assetPath);
                    if (bundleInfo.m_assetList.Count <= 0)
                    {
                        m_bundleMap.Remove(bundleName);
                    }
                }
            }

        }

        /// <summary>
        /// 删除Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        public void RemoveBundle(string bundleName)
        {
            if (m_bundleMap.TryGetValue(bundleName, out var singleBundleInfo))
            {
                foreach (var asset in singleBundleInfo.m_assetList)
                {
                    if (m_inverseBundleMap.TryGetValue(asset, out var checkBundleName))
                    {
                        if (checkBundleName.Equals(bundleName))
                        {
                            m_inverseBundleMap.Remove(asset);
                        }
                        else
                        {
                            Debug.LogError("AssetBundleMap is dirty!Try to clear data and recreate it!");
                        }
                    }
                }
            }

            m_bundleMap.Remove(bundleName);

        }
        /// <summary>
        /// 获取单个BundleInfo
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public SingleBundleInfo GetSingleBundleInfo(string bundleName)
        {
            return m_bundleMap.TryGetValue(bundleName, out var bundleInfo) ? bundleInfo : null;
        }

        public void Clear()
        {
            m_inverseBundleMap.Dict.Clear();
            m_bundleMap.Dict.Clear();
        }

        /// <summary>
        /// 加载AssetBundleMap
        /// </summary>
        /// <returns></returns>
        public static AssetBundleMap LoadAssetBundleMap()
        {
            var gameClientSettings = GameClientSettings.LoadMainGameClientSettings();
            return CommonUtils.LoadResource<AssetBundleMap>(gameClientSettings.m_resPathSettings.AssetBundleMapPathRelativeResources);
        }

        /// <summary>
        /// 加载StreamingAssetsAssetBundleMap
        /// </summary>
        /// <returns></returns>
        public static AssetBundleMap LoadStreamingAssetsAssetBundleMap()
        {
            var gameClientSettings = GameClientSettings.LoadMainGameClientSettings();
            return CommonUtils.LoadResource<AssetBundleMap>(gameClientSettings.m_resPathSettings.StreamingAssetsAssetBundleMapPathRelativeResources);
        }

        public static string AssetBundleMapPath
        {
            get
            {
                var gameClientSettings = GameClientSettings.LoadMainGameClientSettings();
                return Path.Combine(gameClientSettings.m_resPathSettings.ResourcesDir, gameClientSettings.m_resPathSettings.AssetBundleMapPathRelativeResources + ".asset");
            }
        }

        public static string StreamingAssetsAssetBundleMapPath
        {
            get
            {
                var gameClientSettings = GameClientSettings.LoadMainGameClientSettings();
                return Path.Combine(gameClientSettings.m_resPathSettings.ResourcesDir, gameClientSettings.m_resPathSettings.StreamingAssetsAssetBundleMapPathRelativeResources + ".asset");
            }
        }

        [Serializable]
        public class PairStr2Str : Pair<string, string>
        {
            public PairStr2Str(string key, string value) : base(key, value) { }
        }

        [Serializable]
        public class SerializableDictionaryStr2Str : SerializableDictionary<string, string, PairStr2Str> { }

        [Serializable]
        public class PairStr2SingleBundleInfo : Pair<string, SingleBundleInfo>
        {
            public PairStr2SingleBundleInfo(string key, SingleBundleInfo value) : base(key, value)
            {
            }
        }

        [Serializable]
        public class SerializableDictionaryStr2SingleBundleInfo : SerializableDictionary<string, SingleBundleInfo, PairStr2SingleBundleInfo> { }

        private bool m_useManifest = false;

        private AssetBundleManifest m_manifest;

        [SerializeField]
        private SerializableDictionaryStr2Str m_inverseBundleMap = new SerializableDictionaryStr2Str();

        [SerializeField]
        private SerializableDictionaryStr2SingleBundleInfo m_bundleMap = new SerializableDictionaryStr2SingleBundleInfo();

        [SerializeField]
        private DependenciesGraph m_dependencies = new DependenciesGraph();

        public DependenciesGraph DependenciesInfo => m_dependencies;
    }
}
