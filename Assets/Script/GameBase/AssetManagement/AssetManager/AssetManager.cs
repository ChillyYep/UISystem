using GameBase.CoroutineHelper;
using GameBase.TimeUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameBase.Asset
{
    /// <summary>
    /// 资源路径类型
    /// </summary>
    public enum AssetPathType
    {
        DefaultPrefab,
        UIPagePrefab
    }

    /// <summary>
    /// 资源相关设置
    /// </summary>
    [Serializable]
    public class ResSettings
    {
        [SerializeField]
        private string EditorBundleDirectoryRelativeToAssets = @"../AssetBundle";
        [SerializeField]
        private string StreamingAssetsRelativeToAssets = @"StreamingAssets";

        public string ResourcesDir = "Assets/Resources";

        public string AssetBundleMapPathRelativeResources = "Settings/AssetBundleMap";

        public string StreamingAssetsAssetBundleMapPathRelativeResources = "Settings/StreamingAssetsAssetBundleMap";

        public bool RuntimeReadBundleInStreamingAssets = false;
        public string PageResPath = @"Assets/Prefabs/Page";
        public string PrefabPath = @"Assets/Prefabs";
        /// <summary>
        /// 是否开启定时回收无用资源计时器
        /// </summary>
        public bool StartUnloadUnusedAssetTimer = false;
        /// <summary>
        /// 回收无用资源时间间隔（如果计时器开启）
        /// </summary>
        public float UnloadUnusedAssetInverval = 60f;
        /// <summary>
        /// 异步Bundle加载同时间最大工作协程数
        /// </summary>
        public int MaxBundleWorkers = 1;

        /// <summary>
        /// 获取平台相关的Bundle文件夹
        /// </summary>
        /// <returns></returns>
        public string GetBundleRootByPlatform()
        {
#if UNITY_EDITOR
            if (Application.isPlaying && RuntimeReadBundleInStreamingAssets)
            {
                return GetStreamingAssetsByPlatform();
            }
            return Path.Combine(Application.dataPath, EditorBundleDirectoryRelativeToAssets, GetPlatformFolder());
#else
            return GetStreamingAssetsByPlatform();
#endif

        }

        /// <summary>
        /// 获取平台相关StreamingAssets文件夹
        /// </summary>
        /// <returns></returns>
        public string GetStreamingAssetsByPlatform()
        {
            return Path.Combine(Application.dataPath, StreamingAssetsRelativeToAssets, GetPlatformFolder());
        }

        /// <summary>
        /// 获取平台相关的Bundle文件夹
        /// </summary>
        /// <returns></returns>
        public string GetPlatformFolder()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "IOS";
                case RuntimePlatform.OSXPlayer:
                    return "StandaloneOSXIntel";
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return IntPtr.Size == 8 ? "StandaloneWindows64" : "StandaloneWindows";
            }
            Debug.LogError($"Platform:{Application.platform}.GetPlatformFolder Error!");
            return "Other";
        }

        /// <summary>
        /// 获取AssetBundleManifest的Bundle名（平台相关）
        /// </summary>
        /// <returns></returns>
        public string GetAssetBundleManifestName()
        {
            return GetPlatformFolder();
        }

        /// <summary>
        /// 获取AssetBundleManifest的Bundle路径
        /// </summary>
        /// <returns></returns>
        public string GetAssetBundleManifestPath()
        {
            return Path.Combine(GetBundleRootByPlatform(), GetAssetBundleManifestName());
        }

        /// <summary>
        /// 依据资源路径类型和资源名获取完整路径
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetPath(AssetPathType assetType, string assetName)
        {
            switch (assetType)
            {
                case AssetPathType.UIPagePrefab:
                    return string.Format("{0}/{1}.prefab", PageResPath, assetName);
                case AssetPathType.DefaultPrefab:
                    return string.Format("{0}/{1}.prefab", PrefabPath, assetName);
            }
            return "";
        }
    }

    /// <summary>
    /// 资源缓存项
    /// </summary>
    public class AssetCacheItem
    {
        public AssetCacheItem(string assetPath, UnityEngine.Object assetObj)
        {
            m_assetPath = assetPath;
            m_weakAssetRef = new WeakReference<UnityEngine.Object>(assetObj);
        }
        public readonly string m_assetPath;
        public readonly WeakReference<UnityEngine.Object> m_weakAssetRef;
    }

    /// <summary>
    /// 资源管理器接口
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// 资源管理器初始化
        /// </summary>
        void Initialize(ResSettings resSettings, ICouroutineHelper couroutineHelper);

        /// <summary>
        /// 卸载
        /// </summary>
        void UnInitialize();

        /// <summary>
        /// 加载Resource资源
        /// </summary>
        T LoadAssetInResource<T>(string assetPath) where T : UnityEngine.Object;

        /// <summary>
        /// 加载Resource所有资源
        /// </summary>
        /// <returns></returns>
        T[] LoadAllAssetInResource<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 异步加载Resource资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        void LoadAssetInResourceAsync<T>(string assetPath, Action<T, bool> onLoadEnd) where T : UnityEngine.Object;

        /// <summary>
        /// 根据资源路径类型同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPathType"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        T LoadAssetSync<T>(AssetPathType assetPathType, string assetName) where T : UnityEngine.Object;

        /// <summary>
        /// 根据资源路径同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        T LoadAssetSync<T>(string assetPath) where T : UnityEngine.Object;

        /// <summary>
        /// 根据资源路径类型同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPathType"></param>
        /// <param name="assetName"></param>
        /// <param name="OnLoadEnd"></param>
        void LoadAssetAsync<T>(AssetPathType assetPathType, string assetName, Action<T, bool> OnLoadEnd) where T : UnityEngine.Object;

        /// <summary>
        /// 根据资源路径异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        void LoadAssetAsync<T>(string assetPath, Action<T, bool> OnLoadEnd) where T : UnityEngine.Object;

        /// <summary>
        /// 手动的Unload不能彻底清理Asset，需要调用UnloadUsedAssets来清理
        /// </summary>
        void UnloadUnusedAsset();

    }

    /// <summary>
    /// 资源管理器，所谓资源通常是独一份的，同一资源不应该在内存中存在多份，
    /// 卸载资源
    /// </summary>
    public class AssetManager : Singleton_CSharp<AssetManager>, IAssetManager
    {
        public void Initialize(ResSettings resSettings, ICouroutineHelper couroutineHelper)
        {
#if UNITY_EDITOR && !IN_BUNDLE
            m_assetManagerImp = new EditorModeAssetManager();
#else
            m_assetManagerImp = new BundleModeAssetManager();
#endif
            m_assetManagerImp.Initialize(resSettings, couroutineHelper);
            StartUnloadUnusedAssetTimer(resSettings.StartUnloadUnusedAssetTimer, resSettings.UnloadUnusedAssetInverval);
        }

        public void UnInitialize()
        {
            StartUnloadUnusedAssetTimer(false);
            m_assetManagerImp.UnInitialize();
            m_assetManagerImp = null;
        }

        public T LoadAssetSync<T>(AssetPathType assetPathType, string assetName) where T : UnityEngine.Object
        {
            return m_assetManagerImp.LoadAssetSync<T>(assetPathType, assetName);
        }

        public T LoadAssetSync<T>(string assetPath) where T : UnityEngine.Object
        {
            return m_assetManagerImp.LoadAssetSync<T>(assetPath);
        }

        public T LoadAssetInResource<T>(string assetPath) where T : UnityEngine.Object
        {
            return m_assetManagerImp.LoadAssetInResource<T>(assetPath);
        }

        public T[] LoadAllAssetInResource<T>(string path) where T : UnityEngine.Object
        {
            return m_assetManagerImp.LoadAllAssetInResource<T>(path);
        }

        public void LoadAssetInResourceAsync<T>(string assetPath, Action<T, bool> onLoadEnd) where T : UnityEngine.Object
        {
            m_assetManagerImp.LoadAssetInResourceAsync<T>(assetPath, onLoadEnd);
        }

        public void LoadAssetAsync<T>(AssetPathType assetPathType, string assetName, Action<T, bool> OnLoadEnd) where T : UnityEngine.Object
        {
            m_assetManagerImp.LoadAssetAsync<T>(assetPathType, assetName, OnLoadEnd);
        }

        public void LoadAssetAsync<T>(string assetPath, Action<T, bool> OnLoadEnd) where T : UnityEngine.Object
        {
            m_assetManagerImp.LoadAssetAsync<T>(assetPath, OnLoadEnd);
        }

        /// <summary>
        /// 手动的Unload不能彻底清理Asset，需要调用UnloadUsedAssets来清理
        /// </summary>
        public void UnloadUnusedAsset()
        {
            m_assetManagerImp.UnloadUnusedAsset();
        }

        /// <summary>
        /// 开关定时清理无用资源
        /// </summary>
        /// <param name="start"></param>
        /// <param name="unloadInverval"></param>
        private void StartUnloadUnusedAssetTimer(bool start, float unloadInverval = 60f)
        {
            if (start)
            {
                if (m_timerTask != null)
                {
                    TimerManager.Instance.DestroyTimerTask(m_timerTask);
                    m_timerTask = null;
                }
                m_timerTask = new TimerTask(UnloadUnusedAsset, TimerUseType.AssetManger, unloadInverval, TimerTask.InfinityTimes);
                TimerManager.Instance.CreateTimerTask(m_timerTask);
            }
            else
            {
                if (m_timerTask != null)
                {
                    TimerManager.Instance.DestroyTimerTask(m_timerTask);
                    m_timerTask = null;
                }
            }
        }

        /// <summary>
        /// 资源管理器实现类
        /// </summary>
        private AssetManagerImp m_assetManagerImp;

        /// <summary>
        /// 定时任务
        /// </summary>
        private TimerTask m_timerTask;
    }
}
