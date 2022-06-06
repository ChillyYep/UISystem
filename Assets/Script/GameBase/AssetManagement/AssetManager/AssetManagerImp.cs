using GameBase.CoroutineHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Asset
{
    /// <summary>
    /// 资源管理器实现类基类
    /// </summary>
    public abstract class AssetManagerImp : IAssetManager
    {
        public virtual void Initialize(ResSettings resSettingss, ICouroutineHelper couroutineHelper)
        {
            m_resSettings = resSettingss;
            m_couroutineHelper = couroutineHelper;
            m_assetCache.Clear();
        }

        public abstract void UnInitialize();

        public T LoadAssetInResource<T>(string assetPath) where T : UnityEngine.Object
        {
            var asset = Resources.Load<T>(assetPath);
            if (asset == null)
            {
                Debug.LogError($"Fail to load asset!AssetPath:{assetPath}.");
            }
            return asset;
        }

        public T[] LoadAllAssetInResource<T>(string path) where T : UnityEngine.Object
        {
            var asset = Resources.LoadAll<T>(path);
            return asset;
        }

        public void LoadAssetInResourceAsync<T>(string assetPath, Action<T, bool> onLoadEnd) where T : UnityEngine.Object
        {
            m_couroutineHelper.StartCoroutine(LoadAssetInResourceAsyncCoroutine(assetPath, onLoadEnd));
        }

        private IEnumerator LoadAssetInResourceAsyncCoroutine<T>(string assetPath, Action<T, bool> onLoadEnd) where T : UnityEngine.Object
        {
            var req = Resources.LoadAsync<T>(assetPath);
            while (!req.isDone)
            {
                yield return null;
            }
            var asset = req.asset as T;
            onLoadEnd?.Invoke(asset, asset != null);
        }

        public T LoadAssetSync<T>(AssetPathType assetPathType, string assetName) where T : UnityEngine.Object
        {
            string path = m_resSettings.GetPath(assetPathType, assetName);
            return LoadAssetSync<T>(path);
        }

        public abstract T LoadAssetSync<T>(string assetPath) where T : UnityEngine.Object;

        public void LoadAssetAsync<T>(AssetPathType assetPathType, string assetName, Action<T, bool> OnLoadEnd) where T : UnityEngine.Object
        {
            string path = m_resSettings.GetPath(assetPathType, assetName);
            LoadAssetAsync<T>(path, OnLoadEnd);
        }

        public abstract void LoadAssetAsync<T>(string assetPath, Action<T, bool> OnLoadEnd) where T : UnityEngine.Object;

        /// <summary>
        /// 从缓存中获取资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        protected bool TryLoadAssetFromCache<T>(string assetPath, out T asset) where T : UnityEngine.Object
        {
            asset = null;
            if (m_assetCache.TryGetValue(assetPath, out var assetCacheItem))
            {
                // 弱引用还存在，直接取资源引用
                if (assetCacheItem.m_weakAssetRef.TryGetTarget(out var target))
                {
                    asset = target as T;
                    return asset != null;
                }
                // 弱引用不在了，删除资源引用
                m_assetCache.Remove(assetPath);
            }
            return false;
        }

        public virtual void UnloadUnusedAsset()
        {
            m_couroutineHelper.StartUniqueCoroutine(CoroutineID.UnloadUnusedAsset, UnloadUnusedAssetWorker());
        }

        private IEnumerator UnloadUnusedAssetWorker()
        {
            // 清除无用资源
            var unloadWorker = Resources.UnloadUnusedAssets();
            while (!unloadWorker.isDone)
            {
                yield return null;
            }
            // 删除无效AssetCacheItem
            foreach (var assetCacheItem in m_assetCache)
            {
                // 表示资源已经被回收，则需要删除AssetCahceItem
                if (!assetCacheItem.Value.m_weakAssetRef.TryGetTarget(out _))
                {
                    m_wait2RemoveAssetCache.Add(assetCacheItem.Key);
                }
            }
            foreach (var wait2RemoveItem in m_wait2RemoveAssetCache)
            {
                m_assetCache.Remove(wait2RemoveItem);
            }
            m_wait2RemoveAssetCache.Clear();

        }
        /// <summary>
        /// 协程助手
        /// </summary>
        protected ICouroutineHelper m_couroutineHelper;

        protected readonly List<string> m_wait2RemoveAssetCache = new List<string>();
        /// <summary>
        /// 资源缓存
        /// </summary>
        protected readonly Dictionary<string, AssetCacheItem> m_assetCache = new Dictionary<string, AssetCacheItem>();

        protected ResSettings m_resSettings;
    }
}
