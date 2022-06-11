using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Settings
{
    /// <summary>
    /// 各种Bundle构建模式，可组合
    /// </summary>
    [Flags]
    public enum BundleBuildMode
    {
        /// <summary>
        /// 同一个资源是否允许被打进多个不同的Bundle里
        /// </summary>
        AutoSkipRepeatAsset = 1,

        /// <summary>
        /// 一个Bundle对应一个BundleDescription
        /// </summary>
        OneBundleOneBundleDesc = 1 << 1,

        /// <summary>
        /// 自动剔除BundleDescription中不存在的资源
        /// </summary>
        AutoCullDontExistAssetInBundleDesc = 1 << 2,

        /// <summary>
        /// 自动拷贝Bundle到StreamingAssets中
        /// </summary>
        AutoCopyStreamingAssets = 1 << 3,

        /// <summary>
        /// 一个Bundle中的同名资源检测（据说老版本的Unity同名资源打包时会报错，姑且加个检测一定程度上兼容老版本）
        /// </summary>
        SameNameAssetInOneBundleCheck = 1 << 4,

        /// <summary>
        /// 如果资源引用了RuntimeAssets文件夹外的资源，可选择开启此外部依赖检测禁止引用外部资源（不开启此选项，
        /// 当前版本的Unity也会正确打包，资源将被保存在某一个直接依赖的Bundle中）
        /// </summary>
        OuterDepedencyCheck = 1 << 5,

        /// <summary>
        /// 循环Bundle依赖和Asset依赖检查（目前支持加载循环依赖的Bundle，仅作为性能优化或逻辑优化等目的而开启的检测）
        /// </summary>
        CircularDependencyBundleAndAssetCheck = 1 << 6,

        /// <summary>
        /// 增量构建Bundle(只要不删除Bundle文件夹，Unity默认支持增量打Bundle)，所以不用开启该选项
        /// </summary>
        CustomIncrementBuildBundle = 1 << 7,

        /// <summary>
        /// 预处理阶段剔除材质球上的过期属性
        /// </summary>
        PreprocessCullExpiredMaterialProps = 1 << 8

    }

    [Serializable]
    public class BundleBuildSettings
    {
        public BundleBuildMode BundleBuildMode;

        public string BundleSuffix = "ab";

        public int VersionStep = 1;

        public bool UseAssetBundleManifestCollectDependencies = false;

        public List<string> AssetDirs = new List<string>()
        {
            "Assets/RuntimeAssets",
            "Assets/ConfigData"
        };

        public string RuntimeAssetsDir = "Assets/RuntimeAssets";

        public string DefaultAbName = "runtimeassets";

        public string BuildCachePath = "Assets/Editor/Cache/BundleBuildCache.asset";

        public bool IsFileAllowedInBundle(string assetPath, out string outputBundleDdirectory)
        {
            foreach (var dir in AssetDirs)
            {
                if (assetPath.IndexOf(dir) >= 0)
                {
                    outputBundleDdirectory = dir;
                    return true;
                }
            }
            outputBundleDdirectory = string.Empty;
            return false;
        }

        public bool ContainMode(BundleBuildMode bundleBuildMode)
        {
            return (BundleBuildMode & bundleBuildMode) == bundleBuildMode;
        }
    }
}
