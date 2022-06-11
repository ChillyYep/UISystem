using GameBase.CoroutineHelper;
using GameBase.Log;
using GameBase.Settings;
using GameBase.TimeUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameBase.Asset
{
    /// <summary>
    /// AB缓存项
    /// </summary>
    public class BundleCacheItem
    {
        public BundleCacheItem(string bundleName, AssetBundle bundle)
        {
            m_bundleName = bundleName;
            m_assetBundle = bundle;
            m_refCount = 0;
        }
        public readonly string m_bundleName;
        public readonly AssetBundle m_assetBundle;
        public int m_refCount;
    }

    public class BundleLoadState
    {
        public string m_bundleName;

        public DateTime m_expiredTime;

    }
    /// <summary>
    /// 加载现场，用于保存加载的一些状态
    /// </summary>
    public class BundleLoadingContext
    {
        public void Init()
        {
            m_loadingBundles.Clear();
        }
        public void RegistBundleLoading(string bundleName)
        {
            if (!m_loadingBundles.ContainsKey(bundleName))
            {
                m_loadingBundles[bundleName] = new BundleLoadState()
                {
                    m_bundleName = bundleName,
                    m_expiredTime = DateTime.Now.AddSeconds(m_loadTimeout)
                };
            }
            else
            {
                Debug.LogError($"Bundle {bundleName} is loading.Can't Load it again!");
            }
        }

        public void UnRegistBundleLoading(string bundleName)
        {
            m_loadingBundles.Remove(bundleName);
        }

        public bool IsBundleInLoading(string bundleName)
        {
            return m_loadingBundles.ContainsKey(bundleName);
        }

        /// <summary>
        /// 获取加载状态
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public BundleLoadState GetBundleLoadState(string bundleName)
        {
            return m_loadingBundles.TryGetValue(bundleName, out var state) ? state : null;
        }

        /// <summary>
        /// 加载10秒即超时
        /// </summary>
        public float m_loadTimeout = 10f;

        private readonly Dictionary<string, BundleLoadState> m_loadingBundles = new Dictionary<string, BundleLoadState>();
    }

    /// <summary>
    /// 资源加载管理Bundle模式
    /// </summary>
    public class BundleModeAssetManager : AssetManagerImp
    {
        public override void Initialize(ResSettings resSettings, ICouroutineHelper couroutineHelper)
        {
            base.Initialize(resSettings, couroutineHelper);
            if (resSettings.RuntimeReadBundleInStreamingAssets)
            {
                m_assetBundleMap = AssetBundleMap.LoadStreamingAssetsAssetBundleMap();
                m_assetBundleMap.SetLocation(Location.StreamingAssetDir);
            }
            else
            {
                m_assetBundleMap = AssetBundleMap.LoadAssetBundleMap();
                m_assetBundleMap.SetLocation(Location.AssetBundleDir);
            }
            m_bundleCacheDict.Clear();
            m_bundleLoadingContext.Init();
            // 加载AssetBundleManifest
            //var assetBundleManifestPath = resSettings.GetAssetBundleManifestPath();

            m_dependenciesGraph = m_assetBundleMap.DependenciesInfo;
        }

        public override void UnInitialize()
        {
            // todo
            m_bundleCacheDict.Clear();
        }

        public override T LoadAssetSync<T>(string assetPath)
        {
            // 1、Asset缓存有，则直接取
            if (TryLoadAssetFromCache<T>(assetPath, out var asset))
            {
                return asset;
            }
            // 2、映射获得Bundle路径
            var assetBundleName = m_assetBundleMap.GetAssetBundleName(assetPath);
            if (string.IsNullOrEmpty(assetBundleName))
            {
                Debug.LogError($"LoadAssetSync:Fail to map AssetPath to BundlePath!AssetPath:{assetPath},BundleName:{assetBundleName}.");
                return null;
            }
            // 3、查看Bundle缓存，无Bundle缓存，则需先加载Bundle，并保存该Bundle缓存
            BundleCacheItem bundleCacheItem = null;
            var iter = _LoadBundleChainWorker(assetBundleName, (cacheItem, result) =>
               {
                   bundleCacheItem = cacheItem;
               }, false);
            iter.MoveNext();
            // 4、从Bundle中加载资源，并缓存资源
            if (bundleCacheItem == null)
            {
                Debug.LogError($"LoadAssetSync:Fail to Get BundleCacheItem!BundleName:{assetBundleName}.");
                return null;
            }
            if (bundleCacheItem.m_assetBundle == null)
            {
                Debug.LogError($"LoadAssetSync:Fail to Get AssetBundle From BundleCacheItem!BundleName{assetBundleName}");
                return null;
            }
            // 如果该资源依赖了其他的资源，Unity会自动给帮我们加载依赖项资源（前提是依赖项资源对应的AssetBundle已经被加载进内存）
            // 已经加载过的资源，不会被重复加载，这个由AssetBundle管理，假如我们卸载了AssetBundle却没卸载其管理的Asset，则资源就可能在内存中重复
            asset = bundleCacheItem.m_assetBundle.LoadAsset<T>(assetPath);
            if (asset != null)
            {
                // 新的资源加载，会使Bundle缓存计数+1
                bundleCacheItem.m_refCount++;
                m_assetCache[assetPath] = new AssetCacheItem(assetPath, asset);
            }
            else
            {
                Debug.LogError($"LoadAssetSync:Fail to Load Asset From AssetBundle!BundleName{assetBundleName},AssetPath:{assetPath}");
            }
            return asset;
        }

        public override void LoadAssetAsync<T>(string assetPath, Action<T, bool> OnLoadEnd)
        {
            m_couroutineHelper.StartCoroutine(LoadAssetAsyncWorker(assetPath, OnLoadEnd));
        }

        /// <summary>
        /// 异步加载资源协程辅助
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="OnLoadEnd"></param>
        /// <returns></returns>
        private IEnumerator LoadAssetAsyncWorker<T>(string assetPath, Action<T, bool> OnLoadEnd) where T : UnityEngine.Object
        {
            // 1、Asset缓存有，则直接取
            if (TryLoadAssetFromCache<T>(assetPath, out var asset))
            {
                OnLoadEnd?.Invoke(asset, true);
                yield break;

            }
            // 2、映射获得Bundle路径
            var assetBundleName = m_assetBundleMap.GetAssetBundleName(assetPath);
            if (string.IsNullOrEmpty(assetBundleName))
            {
                Debug.LogError($"LoadAssetSync:Fail to map AssetPath to BundlePath!AssetPath:{assetPath},BundleName:{assetBundleName}.");
                OnLoadEnd?.Invoke(null, false);
                yield break;
            }
            // 3、加载Bundle，并保存该Bundle缓存
            BundleCacheItem bundleCacheItem = null;
            var iter = _LoadBundleChainWorker(assetBundleName, (cacheItem, result) =>
              {
                  if (result)
                  {
                      bundleCacheItem = cacheItem;
                  }
              }, true);
            while (iter.MoveNext())
            {
                yield return null;
            }
            // 4、从Bundle中加载资源，并缓存资源
            if (bundleCacheItem == null)
            {
                Debug.LogError($"LoadAssetSync:Fail to Get BundleCacheItem!BundleName:{assetBundleName}.");
                OnLoadEnd?.Invoke(null, false);
                yield break;
            }
            if (bundleCacheItem.m_assetBundle == null)
            {
                Debug.LogError($"LoadAssetSync:Fail to Get AssetBundle From BundleCacheItem!BundleName{assetBundleName}");
                OnLoadEnd?.Invoke(null, false);
                yield break;
            }
            var assetBundleReq = bundleCacheItem.m_assetBundle.LoadAssetAsync<T>(assetPath);
            while (!assetBundleReq.isDone)
            {
                yield return null;
            }
            asset = assetBundleReq.asset as T;
            if (asset == null)
            {
                Debug.LogError($"LoadAssetSync:Fail to Load Asset From AssetBundle!BundleName{assetBundleName},AssetPath:{assetPath}");
                OnLoadEnd?.Invoke(null, false);
                yield break;
            }
            else
            {
                // 新的资源加载，会使Bundle缓存计数+1
                bundleCacheItem.m_refCount++;
                m_assetCache[assetPath] = new AssetCacheItem(assetPath, asset);
            }
            OnLoadEnd?.Invoke(asset, true);
        }

        /// <summary>
        /// 收集Bundle及其依赖的所有Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        private List<string> CollectAllNeedBundle(string bundleName)
        {
            HashSet<string> allBundles = new HashSet<string>();
            allBundles.Add(bundleName);
            var rawDependencies = m_dependenciesGraph.GetAllDependencies(bundleName);
            foreach (var dependency in rawDependencies)
            {
                // 已经加载了的，从待加载项中排除
                if (!m_bundleCacheDict.ContainsKey(dependency))
                {
                    allBundles.Add(dependency);
                }
            }
            return new List<string>(allBundles);
        }

        /// <summary>
        /// 同步加载单一Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        private BundleCacheItem _LoadOneBundleSync(string bundleName)
        {
            BundleCacheItem bundleCacheItem = null;
            var iter = _LoadOneBundleWorker(bundleName, (cacheItem, result) =>
            {
                if (result)
                {
                    bundleCacheItem = cacheItem;
                }
            }, false);
            while (iter.MoveNext()) { }
            if (bundleCacheItem == null)
            {
                Debug.LogError($"Fail to LoadBundeSync.BundleName:{bundleName}");
            }
            return bundleCacheItem;
        }

        /// <summary>
        /// 异步加载单个Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="OnLoadEnd"></param>
        private void _LoadOneBundleAsync(string bundleName, Action<BundleCacheItem, bool> OnLoadEnd)
        {
            m_couroutineHelper.StartCoroutine(_LoadOneBundleWorker(bundleName, OnLoadEnd));
        }

        /// <summary>
        /// 链式加载Bundle及其依赖Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="OnLoadEnd"></param>
        /// <param name="loadAsync"></param>
        /// <returns></returns>
        private IEnumerator _LoadBundleChainWorker(string bundleName, Action<BundleCacheItem, bool> OnLoadEnd, bool loadAsync)
        {
            int workerCount = m_resSettings.MaxBundleWorkers;
            List<string> loadSuccessBundles = new List<string>();
            List<string> loadFailBundles = new List<string>();
            // 1、缓存中不存在Bundle，则尝试加载
            if (!m_bundleCacheDict.TryGetValue(bundleName, out var bundleCacheItem))
            {
                // 2、收集所有依赖的Bundle，并依次加载
                List<IEnumerator> loadWorkers = new List<IEnumerator>();
                List<int> removeWorkerIndices = new List<int>();
                var allNeedLoadBundles = CollectAllNeedBundle(bundleName);
                if (loadAsync)
                {
                    // 3.1.1、创建多协程
                    for (int i = 0; i < allNeedLoadBundles.Count; ++i)
                    {
                        var iter = _LoadOneBundleWorker(allNeedLoadBundles[i], (cacheItem, result) =>
                            {
                                if (result)
                                {
                                    loadSuccessBundles.Add(cacheItem.m_bundleName);
                                }
                                else
                                {
                                    loadFailBundles.Add(cacheItem.m_bundleName);
                                }
                            });
                        loadWorkers.Add(iter);
                    }
                    // 3.1.2、运行多协程
                    while (loadWorkers.Count > 0)
                    {
                        // 同时工作的协程
                        for (int i = 0; i < workerCount; ++i)
                        {
                            if (i >= loadWorkers.Count)
                            {
                                break;
                            }
                            if (!loadWorkers[i].MoveNext())
                            {
                                removeWorkerIndices.Add(i);
                            }
                        }
                        // 删除已经完成的工作协程
                        for (int i = removeWorkerIndices.Count - 1; i >= 0; --i)
                        {
                            loadWorkers.RemoveAt(removeWorkerIndices[i]);
                        }
                        yield return null;
                    }

                }
                else
                {
                    // 3.2、同步加载所有Bundle
                    foreach (var oneBundle in allNeedLoadBundles)
                    {
                        _LoadOneBundleSync(oneBundle);
                    }
                }
            }

            // 4.打印日志
            foreach (var item in loadSuccessBundles)
            {
                Debug.Log($"LoadBundle Success!BundleName:{item}.");
            }

            foreach (var item in loadFailBundles)
            {
                Debug.LogError($"LoadBundle Failed!BundleName:{item}.");
            }

            // 5.成功or失败
            if (m_bundleCacheDict.TryGetValue(bundleName, out var newBundleCacheItem))
            {
                OnLoadEnd?.Invoke(newBundleCacheItem, true);
            }
            else
            {
                OnLoadEnd?.Invoke(null, false);
            }
        }

        /// <summary>
        /// 异步加载单一Bundle迭代器
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="OnLoadEnd"></param>
        /// <returns></returns>
        private IEnumerator _LoadOneBundleWorker(string bundleName, Action<BundleCacheItem, bool> OnLoadEnd, bool loadAsync = true)
        {
            Debug.Log($"[Load]_LoadOneBundleWorker:Load \"{bundleName}\" Start!");
            //查看Bundle缓存，无Bundle缓存，则需先加载Bundle，并保存该Bundle缓存
            BundleCacheItem bundleCacheItem;
            if (!m_bundleCacheDict.TryGetValue(bundleName, out bundleCacheItem))
            {
                // 没有正在加载中
                if (!m_bundleLoadingContext.IsBundleInLoading(bundleName))
                {
                    m_bundleLoadingContext.RegistBundleLoading(bundleName);
                    var filePath = Path.Combine(Application.dataPath, m_resSettings.GetBundleRootByPlatform(), bundleName);
                    var bundleLoadState = m_bundleLoadingContext.GetBundleLoadState(bundleName);
                    AssetBundle assetBundle = null;
                    if (loadAsync)
                    {
                        var assetBundleCreateReq = AssetBundle.LoadFromFileAsync(filePath);
                        while (!assetBundleCreateReq.isDone)
                        {
                            // 加载超时
                            if (TimerManager.Instance.ClientRealTime >= bundleLoadState.m_expiredTime)
                            {
                                Debug.LogError("Load Bundle Timeout!");
                                OnLoadEnd?.Invoke(null, false);
                            }
                            yield return null;
                        }
                        assetBundle = assetBundleCreateReq.assetBundle;
                    }
                    else
                    {
                        assetBundle = AssetBundle.LoadFromFile(filePath);
                    }
                    if (assetBundle == null)
                    {
                        Debug.LogError($"LoadAssetSync:Fail to Load AssetBundle!BundleName:{bundleName}.");
                        m_bundleLoadingContext.UnRegistBundleLoading(bundleName);
                        OnLoadEnd?.Invoke(null, false);
                        yield break;
                    }
                    bundleCacheItem = new BundleCacheItem(bundleName, assetBundle);
                    m_bundleCacheDict[bundleName] = bundleCacheItem;
                    m_bundleLoadingContext.UnRegistBundleLoading(bundleName);
                }
                // 正在加载中
                else
                {
                    while (m_bundleLoadingContext.IsBundleInLoading(bundleName))
                    {
                        yield return null;
                    }
                    bundleCacheItem = m_bundleCacheDict[bundleName];
                }
            }

            Debug.Log($"[Load]_LoadOneBundleWorker:Load \"{bundleName}\" End!");
            OnLoadEnd?.Invoke(bundleCacheItem, true);
        }

        public override void UnloadUnusedAsset()
        {
            base.UnloadUnusedAsset();
        }
        /// <summary>
        /// Asset与Bundle映射表
        /// </summary>
        private AssetBundleMap m_assetBundleMap;

        private DependenciesGraph m_dependenciesGraph;

        private readonly BundleLoadingContext m_bundleLoadingContext = new BundleLoadingContext();

        /// <summary>
        /// AB缓存项
        /// </summary>
        private readonly Dictionary<string, BundleCacheItem> m_bundleCacheDict = new Dictionary<string, BundleCacheItem>();
    }
}
