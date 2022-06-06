#if UNITY_EDITOR && !IN_BUNDLE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameBase.Asset
{
    /// <summary>
    /// 资源加载管理Editor模式
    /// </summary>
    public class EditorModeAssetManager : AssetManagerImp
    {
        public override void UnInitialize()
        {
            // todo
        }

        public override void LoadAssetAsync<T>(string assetPath, Action<T, bool> OnLoadEnd)
        {
            var asset = LoadAssetSync<T>(assetPath);
            OnLoadEnd?.Invoke(asset, asset != null);
        }

        public override T LoadAssetSync<T>(string assetPath)
        {
            // 1、Asset缓存有，则直接取
            if (TryLoadAssetFromCache<T>(assetPath, out var asset))
            {
                return asset;
            }
            // 2、缓存中没有，则加载
            asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning($"Fail to Load Asset!AssetPath:{assetPath}");
                return null;
            }
            AssetCacheItem assetObject = new AssetCacheItem(assetPath, asset);
            m_assetCache[assetPath] = assetObject;
            return asset;
        }
    }

}
#endif