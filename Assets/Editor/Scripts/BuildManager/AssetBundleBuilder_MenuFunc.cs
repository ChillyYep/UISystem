using GameBase.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameBase.BundleBuilder
{
    public partial class AssetBundleBuilder
    {
        [MenuItem(MenuItemCollection.AssetBundleBuilder.BuildAssetBundle, validate = true)]
        private static bool BuildAssetBundlesValidate()
        {
            return !Application.isPlaying;
        }

        [MenuItem(MenuItemCollection.AssetBundleBuilder.BuildAssetBundle, priority = -1)]
        public static void BuildAssetBundles()
        {
            // 获取BundleBuildSettings
            if (!GetBundleBuildSettings(out var bundleBuildSettings, out var bundleRoot, out var resSettings))
            {
                Debug.LogError("Fail to get bundleBuildSettings!");
                return;
            }
            // 在打Bundle前需要进行预处理，如剪除Material中的遗留属性
            PreProcessBuildAssetBundles(bundleBuildSettings, resSettings, bundleRoot);
            if (!EditorUtils.CreateDirectory(bundleRoot, false))
            {
                return;
            }
            if (CollectBundle2AssetListDict(bundleBuildSettings, out var bundleName2AssetList, out var dependencies))
            {
                Dictionary<string, List<string>> filteredBundleName2AssetList;
                // 过滤掉没有改变的Bundle
                filteredBundleName2AssetList = FilterNochangedBundle(bundleName2AssetList, bundleBuildSettings, bundleRoot);

                var assetBundleBuilds = Change2AssetBundleBuilds(filteredBundleName2AssetList, bundleBuildSettings);
                var manifest = BuildPipeline.BuildAssetBundles(bundleRoot, assetBundleBuilds, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode,
                    EditorUserBuildSettings.activeBuildTarget);
                if (manifest != null)
                {
                    PostProcessBuildAssetBundles(bundleName2AssetList, dependencies, bundleRoot, bundleBuildSettings, resSettings);
                }
            }
        }
        [MenuItem(MenuItemCollection.AssetBundleBuilder.PreProcessBuildAssetBundles, priority = 10)]
        private static void PreProcessBuildAssetBundlesMenuFunc()
        {
            if (GetBundleBuildSettings(out var buildSettings, out var bundleRootDir, out var resSettings))
            {
                PreProcessBuildAssetBundles(buildSettings, resSettings, bundleRootDir);
            }
        }


        [MenuItem(MenuItemCollection.AssetBundleBuilder.RecollectBundleDesciption, priority = 11)]
        private static void AutoRecollectAssetAndGenOutputBundleNameMenuFunc()
        {
            AutoRecollectAssetAndGenOutoutBundleName(CollectAllBundleDesc());
        }

        [MenuItem(MenuItemCollection.AssetBundleBuilder.CheckBundleDescsLegality, priority = 12)]
        private static void CheckBundleDescsLegalityMenuFunc()
        {
            if (GetBundleBuildSettings(out var buildSettings, out _, out _))
            {
                CheckBundleDescsLegality(CollectAllBundleDesc(), buildSettings);
            }
        }

        [MenuItem(MenuItemCollection.AssetBundleBuilder.CheckAssetLegality, priority = 13)]
        private static void CheckAssetLegalityMenuFunc()
        {
            if (GetBundleBuildSettings(out var buildSettings, out _, out _))
            {
                var bundleDescs = CollectAllBundleDesc();
                if (CheckRepeatedAsset(bundleDescs, buildSettings, out var bundleName2AssetList, out var asset2BundleList)
                    && CheckSameAssetInOneBundle(bundleName2AssetList, buildSettings)
                    && CheckOuterDependencies(bundleDescs, buildSettings))
                {
                    var dependencies = UpdateDependencies(bundleName2AssetList, asset2BundleList, buildSettings);
                    CheckCircularDependencyBundle(bundleName2AssetList, dependencies, buildSettings);
                }
            }
        }
        [MenuItem(MenuItemCollection.AssetBundleBuilder.Copy2StreamingAssets, priority = 14)]
        private static void Copy2StreamingAssetsMenuFunc()
        {
            if (GetBundleBuildSettings(out var _, out var bundleDirRoot, out var resSetting))
            {
                Copy2StreamingAssets(bundleDirRoot, resSetting);
            }
        }
    }
}
