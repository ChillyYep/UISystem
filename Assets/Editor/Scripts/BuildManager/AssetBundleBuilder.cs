using GameBase.Algorithm;
using GameBase.Asset;
using GameBase.Editor;
using GameBase.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
namespace GameBase.BundleBuilder
{
    /// <summary>
    /// 待解决问题：
    /// 1、热更新问题;
    /// 2、对于Material等资源需要在打包前剔除无用资源（多余的序列化数据）；
    /// 已解决：
    /// 1、循环引用问题(有了一个版本)；
    /// 2、增量打Bundle(只要不删除Bundle文件夹，Unity默认支持增量打Bundle);
    /// 3、依赖查询，有资源依赖了不在Bundle内的资源，报错
    /// </summary>
    public partial class AssetBundleBuilder
    {
        #region 阶段性函数
        /// <summary>
        /// 收集AB构建信息
        /// </summary>
        /// <returns></returns>
        private static bool CollectBundle2AssetListDict(BundleBuildSettings bundleBuildSettings, out Dictionary<string, List<string>> bundleName2AssetList, out Dictionary<string, List<string>> dependencies)
        {
            bundleName2AssetList = null;
            dependencies = null;

            var allBundleDescriptions = CollectAllBundleDesc(bundleBuildSettings);

            // 0、BundleDescription重新收集资源
            AutoRecollectAssetAndGenOutoutBundleName(allBundleDescriptions);

            // 1、检测BundleDescription是不是都符合要求
            if (!CheckBundleDescsLegality(allBundleDescriptions, bundleBuildSettings))
            {
                return false;
            }
            // 2、获取BundleName和资源列表的映射关系
            if (!CheckRepeatedAsset(allBundleDescriptions, bundleBuildSettings, out bundleName2AssetList, out var asset2BundleList))
            {
                return false;
            }
            // 3、一个Bundle内同名资源检测
            if (!CheckSameAssetInOneBundle(bundleName2AssetList, bundleBuildSettings))
            {
                return false;
            }
            // 4、检测是否存在依赖文件在资源文件夹外部
            if (!CheckOuterDependencies(allBundleDescriptions, bundleBuildSettings))
            {
                return false;
            }
            // 5、更新Bundle依赖图
            dependencies = UpdateDependencies(bundleName2AssetList, asset2BundleList, bundleBuildSettings);

            // 6、循环依赖查询
            if (!CheckCircularDependencyBundle(bundleName2AssetList, dependencies, bundleBuildSettings))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检测BundleDescription是否都符合要求
        /// </summary>
        /// <param name="descs"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <returns></returns>
        private static bool CheckBundleDescsLegality(List<BundleDescription> descs, BundleBuildSettings bundleBuildSettings)
        {
            var oneBundleOneBundleDesc = bundleBuildSettings.ContainMode(BundleBuildMode.OneBundleOneBundleDesc);
            var autoCullDontExistAsset = bundleBuildSettings.ContainMode(BundleBuildMode.AutoCullDontExistAssetInBundleDesc);
            Dictionary<string, List<BundleDescription>> bundle2BundleDesc = new Dictionary<string, List<BundleDescription>>();
            // 0、在完整打Bundle流程中，Auto的BundleDescription会重新生成OutputBundleName和AssetList，所以下面前两个检测一般都能通过，子BundleDesc收集全是自动进行的，不需要做检测
            try
            {
                const float totalSteps = 5f;
                // 1、同一文件夹（不包含子文件夹）只能有一个BundleDescription
                EditorUtility.DisplayProgressBar(nameof(CheckBundleDescsLegality), string.Format("0/{0:1}", totalSteps), 0f / totalSteps);
                Dictionary<string, List<string>> oneFolderDescs = new Dictionary<string, List<string>>();
                bool oneFolderOneDesc = true;
                foreach (var desc in descs)
                {
                    var descPath = AssetDatabase.GetAssetPath(desc);
                    var folderPath = ProjectWindowUtil.GetContainingFolder(descPath);
                    if (!oneFolderDescs.TryGetValue(folderPath, out var descPaths))
                    {
                        descPaths = new List<string>();
                        oneFolderDescs[folderPath] = descPaths;
                    }
                    oneFolderDescs[folderPath].Add(descPath);
                }
                foreach (var infos in oneFolderDescs)
                {
                    if (infos.Value.Count > 1)
                    {
                        oneFolderOneDesc = false;
                        Debug.LogError($"Exist multiple BundleDescription in one folder.BundleDescs:\"{string.Join("\",\"", infos.Value)}\"");
                    }
                }
                if (!oneFolderOneDesc)
                {
                    Debug.LogError("Fail to pass OneFolderOneDesc check!");
                    return false;
                }

                // 2、Bundle名称合法性检测
                EditorUtility.DisplayProgressBar(nameof(CheckBundleDescsLegality), string.Format("1/{0:1}", totalSteps), 1f / totalSteps);
                bool nameLegal = true;
                foreach (var desc in descs)
                {
                    // 名称不能为空
                    if (string.IsNullOrEmpty(desc.m_outputBundleName))
                    {
                        Debug.LogError($"Exist null or empty outputBundleName!BundleDesription path:{AssetDatabase.GetAssetPath(desc)}");
                        nameLegal = false;
                    }
                    // 必须是字母、数字和下划线的组合
                    if (!Regex.Match(desc.m_outputBundleName, "[a-zA-Z0-9_]+").Value.Equals(desc.m_outputBundleName))
                    {
                        Debug.LogError($"There should be only english alphabet or underline in outputbundlename !BundleDesription path:{AssetDatabase.GetAssetPath(desc)}");
                        nameLegal = false;
                    }
                }
                if (!nameLegal)
                {
                    Debug.LogError("Fail to pass bundleName legality check!");
                    return false;
                }

                // 3、BundleDescription资源列表各资源存在性检测
                EditorUtility.DisplayProgressBar(nameof(CheckBundleDescsLegality), string.Format("2/{0:1}", totalSteps), 2f / totalSteps);
                bool assetListLegal = true;
                List<int> removeAssets = new List<int>();
                foreach (var desc in descs)
                {
                    removeAssets.Clear();
                    for (int i = 0; i < desc.m_assetList.Count; ++i)
                    {
                        var asset = desc.m_assetList[i];
                        if (!File.Exists(asset))
                        {
                            if (autoCullDontExistAsset)
                            {
                                removeAssets.Add(i);
                                Debug.LogWarning($"Asset \"{asset}\" does't exist!AutoCull it from BundleDescription.BundleDesc:\"{AssetDatabase.GetAssetPath(desc)}\".");
                            }
                            else
                            {
                                assetListLegal = false;
                                Debug.LogError($"Asset \"{asset}\" does't exist!BundleDesc:\"{AssetDatabase.GetAssetPath(desc)}\".Try to enable {nameof(BundleBuildMode) + "." + nameof(BundleBuildMode.AutoCullDontExistAssetInBundleDesc)}");
                            }
                        }
                    }
                    if (removeAssets.Count > 0)
                    {
                        for (int i = removeAssets.Count - 1; i >= 0; --i)
                        {
                            desc.m_assetList.RemoveAt(removeAssets[i]);
                        }
                        EditorUtils.SaveAndReimport(desc);
                    }
                }
                if (!assetListLegal)
                {
                    Debug.LogError("Fail to pass bundle assetList legality check!");
                    return false;
                }

                // 4、清除资源列表为空的BundleDesc（不能删除BundleDesc资源，只是将其从打包流程中剔除，避开空Bundle的影响）
                // 从后往前删除
                for (int i = descs.Count - 1; i >= 0; --i)
                {
                    // 与打包的目标方案不同的BundleDesc会被剔除
                    if ((descs[i].m_solution & bundleBuildSettings.CurSolution) != bundleBuildSettings.CurSolution)
                    {
                        descs.RemoveAt(i);
                    }
                    else if (descs[i].m_assetList.Count <= 0)
                    {
                        Debug.Log($"BundleDescription's assetList is empty.BundleDesc:{AssetDatabase.GetAssetPath(descs[i])}.Skip this bundleDescription.");
                        descs.RemoveAt(i);
                    }
                }

                // 5、检测Bundle和BundleDescription是否一一对应
                EditorUtility.DisplayProgressBar(nameof(CheckBundleDescsLegality), string.Format("3/{0:1}", totalSteps), 3f / totalSteps);
                foreach (var desc in descs)
                {
                    var outputBundleName = desc.GetBundleNameWithSuffix();
                    if (!bundle2BundleDesc.TryGetValue(outputBundleName, out var descList))
                    {
                        descList = new List<BundleDescription>();
                        bundle2BundleDesc[outputBundleName] = descList;
                    }
                    descList.Add(desc);
                }
                StringBuilder sb = new StringBuilder();
                bool oneBundleOneBundleDescLegal = true;
                foreach (var pair in bundle2BundleDesc)
                {
                    Debug.Log($"Bundle {pair.Key} relates to \"{ string.Join("\",\"", pair.Value.Select(desc => AssetDatabase.GetAssetPath(desc)))}\"!");
                    if (pair.Value.Count > 1 && oneBundleOneBundleDesc)
                    {
                        oneBundleOneBundleDescLegal = false;
                        var bundleDescPaths = pair.Value.Select(bundleDesc => AssetDatabase.GetAssetPath(bundleDesc));
                        sb.Append(nameof(BundleBuildMode) + "." + nameof(BundleBuildMode.OneBundleOneBundleDesc) + $" only allow the bundle relating to \"{ string.Join("\",\"", pair.Value.Select(desc => AssetDatabase.GetAssetPath(desc)))}\"!");
                        sb.Append($"Bundle \"{pair.Key}\" relates to [\"{string.Join("\",\"", bundleDescPaths)}\"]");
                        Debug.LogError(sb.ToString());
                    }
                }
                if (!oneBundleOneBundleDescLegal)
                {
                    Debug.LogError("Fail to pass OneBundleOneBundleDesc Check!");
                    return false;
                }

                EditorUtility.DisplayProgressBar(nameof(CheckBundleDescsLegality), string.Format("4/{0:0}", totalSteps), 4f / totalSteps);

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return true;
        }

        /// <summary>
        /// 自动收集资源和重新生成Bundle名
        /// </summary>
        /// <param name="descs"></param>
        private static void AutoRecollectAssetAndGenOutoutBundleName(List<BundleDescription> descs)
        {
            // 根据文件夹目录排序，路径越长，排得越前，因为路径越长越有可能是更深的路径，对于有父子关系的文件夹尤其如此
            descs.Sort((a, b) =>
            {
                return Path.GetDirectoryName(AssetDatabase.GetAssetPath(b)).Length.CompareTo(Path.GetDirectoryName(AssetDatabase.GetAssetPath(a)).Length);
            });

            try
            {
                for (int i = 0; i < descs.Count; ++i)
                {
                    EditorUtility.DisplayProgressBar("自动收集资源列表和自动生成BundleName", $"{i}/{descs.Count}", (float)i / descs.Count);
                    // 不论其他的自动化操作有没有开启，子BundleDesc收集都是必须要进行的
                    descs[i].AutoCollectChildBundleDescs();
                    if (descs[i].ContainGenerateMode(BundleDescription.GenerateMode.AutoCollectAsset))
                    {
                        descs[i].AutoCollectAsset();
                    }
                    if (descs[i].ContainGenerateMode(BundleDescription.GenerateMode.AutoGenOutoutBundleName))
                    {
                        descs[i].AutoGenOutputBundleName();
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 资源重复性检测
        /// </summary>
        /// <param name="descs"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <param name="bundleName2AssetList"></param>
        /// <returns></returns>
        private static bool CheckRepeatedAsset(List<BundleDescription> descs, BundleBuildSettings bundleBuildSettings, out Dictionary<string, List<string>> bundleName2AssetList, out Dictionary<string, List<string>> asset2BundleDict)
        {
            bundleName2AssetList = new Dictionary<string, List<string>>();
            bool passCheck = true;
            asset2BundleDict = new Dictionary<string, List<string>>();
            var skipRepeatAsset = bundleBuildSettings.ContainMode(BundleBuildMode.AutoSkipRepeatAsset);
            // 收集bundle和Asset的映射信息
            try
            {
                for (int i = 0; i < descs.Count; ++i)
                {
                    var bundleDescription = descs[i];
                    var outputBundleName = bundleDescription.GetBundleNameWithSuffix();
                    EditorUtility.DisplayProgressBar(nameof(CheckRepeatedAsset), $"{i}/{descs.Count}", (float)i / descs.Count);
                    if (!bundleName2AssetList.TryGetValue(outputBundleName, out var assetList))
                    {
                        assetList = new List<string>();
                        bundleName2AssetList[outputBundleName] = assetList;
                    }

                    foreach (var asset in bundleDescription.m_assetList)
                    {
                        bool hasKey = false;
                        if (!asset2BundleDict.TryGetValue(asset, out var bundleList))
                        {
                            bundleList = new List<string>();
                            asset2BundleDict[asset] = bundleList;
                        }
                        else
                        {
                            hasKey = true;
                        }
                        bundleList.Add(outputBundleName);
                        // 没有重复的或者不自动跳过重复资源的，添加进资源列表
                        if (!hasKey || !skipRepeatAsset)
                        {
                            assetList.Add(asset);
                            // 有重复且不自动跳过重复资源，表示资源检测失败
                            if (hasKey && !skipRepeatAsset)
                            {
                                passCheck = false;
                            }
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // 打印日志
            const string firstLog = "There will be multiple bundles that includes asset \"{0}\".Multiple bundles:[\"{1}\"].";
            const string skippedLog = "It will be skipped except in first bundle.";
            const string trySkipLog = "If possible,try to enable " + nameof(BundleBuildMode) + "." + nameof(BundleBuildMode.AutoSkipRepeatAsset) + ".";
            StringBuilder sb = new StringBuilder();
            foreach (var asset2BundleList in asset2BundleDict)
            {
                sb.Clear();
                // 大于1表示有重复
                if (asset2BundleList.Value.Count > 1)
                {
                    if (skipRepeatAsset)
                    {
                        sb.Append(string.Format(firstLog, asset2BundleList.Key, string.Join("\",\"", asset2BundleList.Value)));
                        sb.Append(skippedLog);
                        Debug.Log(sb.ToString());
                    }
                    else
                    {
                        sb.Append(string.Format(firstLog, asset2BundleList.Key, string.Join("\",\"", asset2BundleList.Value)));
                        sb.Append(trySkipLog);
                        Debug.LogError(sb.ToString());
                    }
                }
                else if (asset2BundleList.Value.Count == 1)
                {
                    Debug.Log($"Asset \"{asset2BundleList.Key}\" will be added in bundle \"{asset2BundleList.Value[0]}\"");
                }
            }
            if (passCheck)
            {
                Debug.Log("Success to pass AssetRepeat Check!");
            }
            else
            {
                Debug.LogError("Fail to pass AssetRepeat Check!");
            }
            return passCheck;
        }

        /// <summary>
        /// 资源文件名（除开后缀）同名检测
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <returns></returns>
        private static bool CheckSameAssetInOneBundle(Dictionary<string, List<string>> bundleName2AssetList, BundleBuildSettings bundleBuildSettings)
        {
            bool passCheck = true;
            if (bundleBuildSettings.ContainMode(BundleBuildMode.SameNameAssetInOneBundleCheck))
            {
                try
                {
                    Dictionary<string, List<string>> assetName2SameNameAsset = new Dictionary<string, List<string>>();
                    int i = 0;
                    foreach (var pair in bundleName2AssetList)
                    {
                        EditorUtility.DisplayProgressBar(nameof(CheckSameAssetInOneBundle), $"{i}/{bundleName2AssetList.Count}", (float)i / bundleName2AssetList.Count);
                        assetName2SameNameAsset.Clear();
                        foreach (var asset in pair.Value)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(asset);
                            if (!assetName2SameNameAsset.TryGetValue(fileName, out var sameAssetList))
                            {
                                sameAssetList = new List<string>();
                                assetName2SameNameAsset[fileName] = sameAssetList;
                            }
                            sameAssetList.Add(asset);
                        }

                        foreach (var asset2AssetList in assetName2SameNameAsset)
                        {
                            var sameAssetList = asset2AssetList.Value;
                            if (sameAssetList.Count > 1)
                            {
                                passCheck = false;
                                Debug.LogError($"Exist same filename \"{asset2AssetList.Key}\" asset in the bundle \"{pair.Key}\".FileList:\"{string.Join("\",\"", sameAssetList)}\"");
                            }
                        }
                        i++;
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    if (passCheck)
                    {
                        Debug.Log("Success to pass SameAssetInOneBundle check!");
                    }
                }
            }
            return passCheck;
        }

        /// <summary>
        /// 检测资源外部依赖
        /// </summary>
        /// <param name="descs"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <returns></returns>
        private static bool CheckOuterDependencies(List<BundleDescription> descs, BundleBuildSettings bundleBuildSettings)
        {
            bool passCheck = true;
            if (bundleBuildSettings.ContainMode(BundleBuildMode.OuterDepedencyCheck))
            {
                HashSet<string> dependencies = new HashSet<string>();
                for (int i = 0; i < descs.Count; ++i)
                {
                    EditorUtility.DisplayProgressBar(nameof(CheckOuterDependencies), $"{i}/{descs.Count}", (float)i / descs.Count);
                    foreach (var asset in descs[i].m_assetList)
                    {
                        // 必须是Assets下的依赖项才会进入是否是外部资源的检测
                        var rawDependencies = AssetDatabase.GetDependencies(asset, false);
                        var depedencies = rawDependencies.Where(dependency => dependency.StartsWith("Assets") && !dependency.EndsWith(".cs"));
                        foreach (var dependency in depedencies)
                        {
                            dependencies.Add(dependency);
                        }
                    }
                }
                foreach (var dependency in dependencies)
                {
                    if (!bundleBuildSettings.IsAssetAllowedToBundle(dependency, out _))
                    {
                        passCheck = false;
                        Debug.LogError($"Asset \"{dependency}\" is out of the appointed directory \"{bundleBuildSettings.RuntimeAssetsDir}\"!");
                    }
                }
                EditorUtility.ClearProgressBar();
                if (passCheck)
                {
                    Debug.Log("Success to pass OuterDepedencies check!");
                }
            }
            return passCheck;
        }

        /// <summary>
        /// 更新依赖图
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        /// <param name="asset2BundleList"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <returns></returns>
        private static Dictionary<string, List<string>> UpdateDependencies(Dictionary<string, List<string>> bundleName2AssetList, Dictionary<string, List<string>> asset2BundleList, BundleBuildSettings bundleBuildSettings)
        {
            var bundle2DependencyBundles = new Dictionary<string, List<string>>();
            foreach (var bundleAssetPair in bundleName2AssetList)
            {
                var bundleName = bundleAssetPair.Key;
                HashSet<string> assetSet = new HashSet<string>();
                HashSet<string> dependencyBundleSet = new HashSet<string>();
                foreach (var asset in bundleAssetPair.Value)
                {
                    // 存在一些不在Assets目录下的依赖
                    var dependencies = AssetDatabase.GetDependencies(asset, false);
                    foreach (var dependency in dependencies)
                    {
                        assetSet.Add(dependency);
                    }
                }
                foreach (var asset in assetSet)
                {
                    if (asset2BundleList.TryGetValue(asset, out var bundles) && bundles.Count > 0)
                    {
                        dependencyBundleSet.Add(bundles[0]);
                    }
                }
                bundle2DependencyBundles[bundleName] = new List<string>(dependencyBundleSet);
            }
            return bundle2DependencyBundles;
        }
        /// <summary>
        /// 循环依赖检查，如果有循环依赖链发生交叉，只取其中一个列出
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <returns></returns>
        private static bool CheckCircularDependencyBundle(Dictionary<string, List<string>> bundleName2AssetList, Dictionary<string, List<string>> bundleDependencies, BundleBuildSettings bundleBuildSettings)
        {
            bool passCheck = true;
            if (bundleBuildSettings.ContainMode(BundleBuildMode.CircularDependencyBundleAndAssetCheck))
            {
                // 1、资源循环依赖查询
                var asset2Bundle = new Dictionary<string, string>();
                // 已经搜寻过的资源
                HashSet<string> nodeVisited = new HashSet<string>();
                // 一次依赖查询中搜寻过的资源
                //HashSet<string> assetDependencyVisited = new HashSet<string>();
                // 已经发现的在环上的资源
                HashSet<string> nodeOnTheRingVisited = new HashSet<string>();

                // 临时的路劲集合，用来查资源环和Bundle环都可以
                HashSet<string> pathNodeSet = new HashSet<string>();

                StringBuilder sb = new StringBuilder();

                // 资源或Bundle依赖查询时，为每一个节点执行的代码
                void ExecuteForeachNode(string node, IEnumerable<string> curPath)
                {
                    nodeVisited.Add(node);
                    // 检测有没有重复资源
                    string repeatedNode = string.Empty;
                    pathNodeSet.Clear();
                    foreach (var item in curPath)
                    {
                        if (!pathNodeSet.Add(item))
                        {
                            repeatedNode = item;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(repeatedNode))
                    {
                        return;
                    }

                    sb.Clear();
                    sb.Append("Exist circular asset or bundle dependency!");
                    bool start = false;
                    foreach (var pathNode in curPath.Reverse())
                    {
                        if (repeatedNode.Equals(pathNode))
                        {
                            if (!start)
                            {
                                start = true;
                            }
                            else
                            {
                                nodeOnTheRingVisited.Add(pathNode);
                                sb.Append(pathNode);
                                start = false;
                                break;
                            }
                        }
                        nodeOnTheRingVisited.Add(pathNode);
                        sb.Append(pathNode);
                        sb.Append("->");
                    }
                    Debug.LogError(sb.ToString());
                };

                // 有环即停止的条件
                bool StopWhen(string node) => nodeOnTheRingVisited.Count > 0;

                foreach (var bundlePair in bundleName2AssetList)
                {
                    foreach (var asset in bundlePair.Value)
                    {
                        asset2Bundle[asset] = bundlePair.Key;
                    }
                }

                foreach (var asset2BundlePair in asset2Bundle)
                {
                    nodeOnTheRingVisited.Clear();
                    var asset = asset2BundlePair.Key;
                    var bundleName = asset2BundlePair.Value;
                    // 已经访问过的资源不会再开启一次查询,这样会加快查询，但总有可能有环漏过查询，需要多次开启这个检测才能完全查询完
                    if (!nodeVisited.Contains(asset))
                    {
                        SearchHelper.DepthFirstSearch(asset,
                            ExecuteForeachNode,
                            node => AssetDatabase.GetDependencies(node, false).Where(dependency => dependency.StartsWith("Assets") && !dependency.EndsWith(".cs")),
                            // 发现环就终止本次查询
                            StopWhen);
                    }
                }

                // 2、Bundle循环依赖查询
                nodeVisited.Clear();
                foreach (var bundle2Dependencies in bundleDependencies)
                {
                    nodeOnTheRingVisited.Clear();

                    var bundleName = bundle2Dependencies.Key;

                    if (!nodeVisited.Contains(bundleName))
                    {
                        SearchHelper.DepthFirstSearch(bundleName, ExecuteForeachNode, node => bundleDependencies[node], StopWhen);
                    }
                }
            }
            return passCheck;
        }

        /// <summary>
        /// 过滤掉不需要更新的Bundle
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        private static Dictionary<string, List<string>> FilterNochangedBundle(Dictionary<string, List<string>> bundleName2AssetList, BundleBuildSettings settings, string bundleRoot)
        {
            var newBundleName2AssetList = new Dictionary<string, List<string>>(bundleName2AssetList);
            if (settings.ContainMode(Settings.BundleBuildMode.CustomIncrementBuildBundle))
            {
                var bundleBuildCache = BundleBuildCache.LoadBundleBuildCache();
                if (bundleBuildCache != null)
                {
                    bundleBuildCache.GetNeedUpdateBundle(newBundleName2AssetList, out var reBuildList, out var removeList);
                    List<string> unChangedList = new List<string>();
                    foreach (var pair in newBundleName2AssetList)
                    {
                        if (!reBuildList.Contains(pair.Key))
                        {
                            unChangedList.Add(pair.Key);
                        }
                    }
                    foreach (var unchangedBundle in unChangedList)
                    {
                        newBundleName2AssetList.Remove(unchangedBundle);
                    }
                    try
                    {
                        foreach (var removeBundle in removeList)
                        {
                            File.Delete(Path.Combine(bundleRoot, removeBundle));
                            File.Delete(Path.Combine(bundleRoot, $"{removeBundle}.manifest"));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
            return newBundleName2AssetList;
        }
        /// <summary>
        /// 打Bundle前的预处理
        /// </summary>
        private static void PreProcessBuildAssetBundles(BundleBuildSettings bundleBuildSettings, ResSettings resSettings, string bundleRootDir)
        {
            // 1、剔除材质球过期属性
            CullMaterialsExpiredProp(bundleBuildSettings);
        }

        /// <summary>
        /// 剔除材质球过期属性
        /// </summary>
        /// <param name="bundleBuildSettings"></param>
        private static void CullMaterialsExpiredProp(BundleBuildSettings bundleBuildSettings)
        {
            if (bundleBuildSettings.ContainMode(BundleBuildMode.PreprocessCullExpiredMaterialProps))
            {
                var materials = AssetDatabase.FindAssets("t:material", new string[] { bundleBuildSettings.RuntimeAssetsDir }).Select(matGuid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(matGuid)));
                List<int> removeList = new List<int>();
                foreach (var mat in materials)
                {
                    var serializedObject = new SerializedObject(mat);
                    serializedObject.Update();
                    var serializedProperty = serializedObject.FindProperty("m_SavedProperties");

                    // 删除过期属性
                    void RemoveExpiredPropertyAtTheArray(string arrayProp)
                    {
                        removeList.Clear();
                        var props = serializedProperty.FindPropertyRelative(arrayProp);
                        if (props.isArray)
                        {
                            for (int i = 0; i < props.arraySize; ++i)
                            {
                                var prop = props.GetArrayElementAtIndex(i);
                                if (!mat.HasProperty(prop.displayName))
                                {
                                    removeList.Add(i);
                                }
                            }
                            for (int i = removeList.Count - 1; i >= 0; --i)
                            {
                                Debug.LogError($"Material \"{AssetDatabase.GetAssetPath(mat)}\" delete propperty {props.GetArrayElementAtIndex(removeList[i]).displayName}!");
                                props.DeleteArrayElementAtIndex(removeList[i]);
                            }
                        }
                    }

                    RemoveExpiredPropertyAtTheArray("m_TexEnvs");
                    RemoveExpiredPropertyAtTheArray("m_Floats");
                    RemoveExpiredPropertyAtTheArray("m_Colors");

                    serializedObject.ApplyModifiedProperties();
                }

                AssetDatabase.SaveAssets();
            }
        }
        /// <summary>
        /// Bundle构建后的后处理
        /// </summary>
        /// <param name="assetBundleBuilds"></param>
        /// <param name="bundleDirRoot"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <param name="resSettings"></param>
        private static void PostProcessBuildAssetBundles(Dictionary<string, List<string>> bundleName2AssetList, Dictionary<string, List<string>> dependencies, string bundleDirRoot, BundleBuildSettings bundleBuildSettings, ResSettings resSettings)
        {
            // 重新创建AssetBundleMap
            RecreateAssetBundleMap(AssetBundleMap.AssetBundleMapPath, bundleName2AssetList, dependencies, bundleDirRoot, bundleBuildSettings, resSettings);
            // 复制AB文件到StreamingAssets目录下
            if (bundleBuildSettings.ContainMode(BundleBuildMode.AutoCopyStreamingAssets))
            {
                Copy2StreamingAssets(bundleDirRoot, resSettings);
            }
        }

        /// <summary>
        /// 重建AssetBundleMap
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        /// <param name="bundleDirRoot"></param>
        /// <param name="bundleBuildSettings"></param>
        /// <param name="resSettings"></param>
        private static void RecreateAssetBundleMap(string assetbundleMapPath, Dictionary<string, List<string>> bundleName2AssetList, Dictionary<string, List<string>> dependencies, string bundleDirRoot, BundleBuildSettings bundleBuildSettings, ResSettings resSettings)
        {
            const string AssetBundleManifestName = nameof(AssetBundleManifest);
            // 保存AssetBundleMap
            var map = AssetBundleMap.LoadAssetBundleMap();
            var newMap = ScriptableObject.CreateInstance<AssetBundleMap>();
            if (map != null)
            {
                AssetDatabase.DeleteAsset(assetbundleMapPath);
            }
            AssetDatabase.CreateAsset(newMap, assetbundleMapPath);
            // Manifest的特殊处理
            {
                bundleName2AssetList[resSettings.GetAssetBundleManifestName()] = new List<string>() { AssetBundleManifestName };
            }
            // 资源型AB
            foreach (var bundlePair in bundleName2AssetList)
            {
                var bundleName = bundlePair.Key;
                var assetList = bundlePair.Value;
                // 没有资源列表，说明是空Bundle，则跳过
                if (assetList.Count <= 0)
                {
                    continue;
                }
                foreach (var assetName in assetList)
                {
                    newMap.Add(assetName, bundleName);
                }
                var bundleInfo = newMap.GetSingleBundleInfo(bundleName);
                var assetBundlePath = Path.Combine(bundleDirRoot, bundleName);
                if (bundleInfo != null)
                {
                    // 使用自定义MD5 Hash算法，可以计算AssetBundleManifest的MD5值
                    if (!EncryptHelper.CalcMD5(assetBundlePath, out var md5Hash))
                    {
                        Debug.LogError($"Fail to compute hash \"{assetBundlePath}\"");
                        continue;
                    }
                    if (!BuildPipeline.GetCRCForAssetBundle(assetBundlePath, out var crc))
                    {
                        Debug.LogError($"Fail to compute crc \"{assetBundlePath}\"");
                        continue;
                    }
                    if (!md5Hash.Equals(bundleInfo.m_hash) || !crc.Equals(bundleInfo.m_crc))
                    {
                        bundleInfo.m_version += bundleBuildSettings.VersionStep;
                    }
                    // 更新Hash和CRC
                    bundleInfo.m_hash = md5Hash;
                    bundleInfo.m_crc = crc;
                    bundleInfo.m_size = new FileInfo(assetBundlePath).Length;
                }
            }

            newMap.UpdateDependencies(dependencies);
            // 更新BundleBuildCache
            var bundleBuildCache = BundleBuildCache.LoadBundleBuildCache();
            bundleBuildCache.UpdateCache(newMap);
            EditorUtils.SaveAndReimport(newMap);
        }

        /// <summary>
        /// 复制到StreamingAssets目录
        /// </summary>
        private static void Copy2StreamingAssets(string bundleDirRoot, ResSettings resSettings)
        {
            List<string> copyBundleNames = new List<string>();
            var assetBundleMap = AssetBundleMap.LoadAssetBundleMap();
            foreach (var item in assetBundleMap.GetBundleMap())
            {
                if (item.Value.m_setting.m_location == SetupLocation.StreamingAssets)
                {
                    copyBundleNames.Add(item.Key);
                }
            }
            var streamingAssetsDir = resSettings.GetStreamingAssetsByPlatform();
            if (EditorUtils.CreateDirectory(streamingAssetsDir, true))
            {
                try
                {
                    for (int i = 0; i < copyBundleNames.Count; ++i)
                    {
                        var abFile = copyBundleNames[i];
                        EditorUtility.DisplayProgressBar(nameof(Copy2StreamingAssets), $"{i}/{copyBundleNames.Count}", (float)i / copyBundleNames.Count);
                        File.Copy(Path.Combine(bundleDirRoot, abFile), Path.Combine(streamingAssetsDir, abFile));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
                // 生成StreamingAssets的AssetBundleMap
                var streamingAssetsAssetBundleMap = ScriptableObject.CreateInstance<AssetBundleMap>();
                foreach (var bundleName in copyBundleNames)
                {
                    var singleBundleInfo = assetBundleMap.GetSingleBundleInfo(bundleName);
                    if (singleBundleInfo != null)
                    {
                        streamingAssetsAssetBundleMap.Add(singleBundleInfo);
                    }
                }
                streamingAssetsAssetBundleMap.UpdateDependencies(assetBundleMap.DependenciesInfo);
                var streamingAssetsAssetBundleMapPath = AssetBundleMap.StreamingAssetsAssetBundleMapPath;
                if (File.Exists(streamingAssetsAssetBundleMapPath))
                {
                    AssetDatabase.DeleteAsset(streamingAssetsAssetBundleMapPath);
                }
                AssetDatabase.CreateAsset(streamingAssetsAssetBundleMap, streamingAssetsAssetBundleMapPath);
            }
            AssetDatabase.Refresh();
        }
        #endregion

        #region 辅助性函数
        /// <summary>
        /// 获取BundleBuildSettings
        /// </summary>
        /// <returns></returns>
        private static bool GetBundleBuildSettings(out BundleBuildSettings bundleBuildSettings, out string bundleDirRoot, out ResSettings resSettings)
        {
            var gameSettings = GameClientSettings.GetInstance();
            if (gameSettings == null)
            {
                bundleBuildSettings = null;
                bundleDirRoot = null;
                resSettings = null;
                return false;
            }
            bundleBuildSettings = gameSettings.m_bundleBuildSettings;

            resSettings = gameSettings.m_resPathSettings;

            bundleDirRoot = gameSettings.m_resPathSettings.GetBundleRootByPlatform();

            return true;
        }

        /// <summary>
        /// 转换成AssetBundleBuild数组
        /// </summary>
        /// <param name="bundleName2AssetList"></param>
        /// <returns></returns>
        private static AssetBundleBuild[] Change2AssetBundleBuilds(Dictionary<string, List<string>> bundleName2AssetList, BundleBuildSettings bundleBuildSettings)
        {
            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[bundleName2AssetList.Count];
            int i = 0;
            foreach (var bundle2AssetListPair in bundleName2AssetList)
            {
                assetBundleBuilds[i++] = new AssetBundleBuild()
                {
                    assetBundleName = bundle2AssetListPair.Key,
                    assetNames = bundle2AssetListPair.Value.ToArray()
                };
            }
            return assetBundleBuilds;
        }

        /// <summary>
        /// 收集所有BundleDescription
        /// </summary>
        /// <returns></returns>
        private static List<BundleDescription> CollectAllBundleDesc(BundleBuildSettings bundleBuildSettings)
        {
            return AssetDatabase.FindAssets("t:" + nameof(BundleDescription), bundleBuildSettings.AssetDirs.ToArray()).Select(bundleDescGuid => AssetDatabase.LoadAssetAtPath<BundleDescription>(AssetDatabase.GUIDToAssetPath(bundleDescGuid))).ToList();
        }
        #endregion
    }
}
