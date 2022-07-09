using GameBase.Editor;
using GameBase.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameBase.Asset
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum AssetType
    {
        Texture = 1,
        Material = 1 << 1,
        Prefab = 1 << 2,
        AnimationClip = 1 << 3,
        AnimatorController = 1 << 4,
        PlayableAsset = 1 << 5,
        AudioClip = 1 << 6,
        AudioMixer = 1 << 7,
        Font = 1 << 8,
        Shader = 1 << 9,
        ComputeShader = 1 << 10,
        RenderTexture = 1 << 11,
        Mesh = 1 << 12,
        TextAsset = 1 << 13,
        Default = Texture | Material | Prefab | AnimationClip | AnimatorController | PlayableAsset | Mesh,
        All = Texture | Material | Prefab | AnimationClip | AnimatorController | PlayableAsset | AudioMixer | AudioClip | Font | Shader | ComputeShader | RenderTexture | Mesh | TextAsset
    }

    /// <summary>
    /// 打Bundle依赖的设置
    /// </summary>
    public class BundleDescription : ScriptableObject
    {
        #region Fields
        public GenerateMode m_generateMode = GenerateMode.AutoCollectAsset | GenerateMode.AutoGenOutoutBundleName;

        public Solution m_solution = Solution.SolutionA;

        public string m_bundleDescExplain = "普通Bundle";

        public string m_outputBundleName = "";

        public List<string> m_assetList = new List<string>();

        public List<BundleDescription> m_childBundleDescs = new List<BundleDescription>();

        public AssetType m_searchAssetTypes = AssetType.Default;

        #endregion

        #region class
        [Flags]
        public enum GenerateMode
        {
            Custom = 0,
            AutoCollectAsset = 1,
            AutoGenOutoutBundleName = 2
        }

        #endregion

        [MenuItem(MenuItemCollection.Create.BundleDescription, priority = 0)]
        public static void CreateBundleDescription()
        {
            var obj = Selection.activeObject;
            string folderPath;
            if (ProjectWindowUtil.IsFolder(obj.GetInstanceID()))
            {
                folderPath = AssetDatabase.GetAssetPath(obj);
            }
            else
            {
                folderPath = ProjectWindowUtil.GetContainingFolder(AssetDatabase.GetAssetPath(obj));
            }
            var assetPath = folderPath + "/BundleDescription.asset";
            var allAssetPaths = Directory.GetFiles(folderPath, "*.asset");
            foreach (var asset in allAssetPaths)
            {
                var bundleDesc = AssetDatabase.LoadAssetAtPath<BundleDescription>(asset);
                if (bundleDesc != null)
                {
                    Debug.LogError($"There have exsited a BundleDescription.Asset \"{asset}\".");
                    return;
                }

            }
            if (!File.Exists(assetPath))
            {
                var bundleDesc = CreateInstance<BundleDescription>();
                AssetDatabase.CreateAsset(bundleDesc, assetPath);
                bundleDesc.AutoGenOutputBundleName();
                bundleDesc.AutoCollectAsset();
            }
        }

        public string GetBundleNameWithSuffix()
        {
            var bundleBuildSetting = BundleBuildSettings.GetInstance();
            return m_outputBundleName + "." + bundleBuildSetting.BundleSuffix;
        }
        /// <summary>
        /// 清除数据
        /// </summary>
        public void Clear()
        {
            m_generateMode = GenerateMode.AutoCollectAsset | GenerateMode.AutoGenOutoutBundleName;
            m_searchAssetTypes = AssetType.Default;
            m_outputBundleName = "";
            m_assetList.Clear();
        }

        public void AutoCollectChildBundleDescs()
        {
            var descPath = AssetDatabase.GetAssetPath(this);
            var dir = Path.GetDirectoryName(descPath);
            // 获得同目录下的其他BundleDesc
            var childBundleDesc = AssetDatabase.FindAssets("t:" + nameof(BundleDescription), new string[] { dir }).Where(guid => !AssetDatabase.GUIDToAssetPath(guid).Equals(descPath));
            m_childBundleDescs.Clear();
            m_childBundleDescs.AddRange(childBundleDesc.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Select(path => AssetDatabase.LoadAssetAtPath<BundleDescription>(path)));
        }

        /// <summary>
        /// 获取搜索关键词
        /// </summary>
        /// <returns></returns>
        private string GetSearchPattern()
        {
            var allAssetType = Enum.GetValues(typeof(AssetType));
            StringBuilder sb = new StringBuilder();
            foreach (var assetTypeObj in allAssetType)
            {
                var assetType = (AssetType)assetTypeObj;
                if (assetType == AssetType.All)
                {
                    continue;
                }
                if ((m_searchAssetTypes & assetType) == assetType)
                {
                    sb.Append($"t:{assetType} ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 自动收集资源列表，收集资源前会先收集子BundleDesc,该资源列表不包含子BundleDesc的资源列表
        /// </summary>
        public void AutoCollectAsset()
        {
            AutoCollectChildBundleDescs();
            var descPath = AssetDatabase.GetAssetPath(this);
            var dir = Path.GetDirectoryName(descPath);

            // 获得子BundleDesc的资源列表
            HashSet<string> childResPaths = new HashSet<string>();
            foreach (var bundleDesc in m_childBundleDescs)
            {
                foreach (var asset in bundleDesc.m_assetList)
                {
                    childResPaths.Add(asset);
                }
            }
            var searchPattern = GetSearchPattern();
            m_assetList.Clear();
            if (!string.IsNullOrEmpty(searchPattern))
            {
                var assetPaths = AssetDatabase.FindAssets(searchPattern, new string[] { dir });
                // 当前BundleDesc需要排除子BundleDesc里的资源列表
                m_assetList.AddRange(assetPaths.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Where(path => !childResPaths.Contains(path)));
            }
            EditorUtils.SaveAndReimport(this);
        }

        /// <summary>
        /// 按路径自动生成Bundle名称
        /// </summary>
        public void AutoGenOutputBundleName()
        {
            var bundleBuildSettings = BundleBuildSettings.GetInstance();
            var defaultABName = bundleBuildSettings.DefaultAbName;
            var descAssetPath = AssetDatabase.GetAssetPath(this);
            // 必须是RuntimeAssetsDir之下的
            if (!bundleBuildSettings.IsAssetAllowedToBundle(descAssetPath, out var outputBundleDdirectory))
            {
                Debug.LogError($"Fail to {nameof(AutoGenOutputBundleName)}!\"{descAssetPath}\" is't under \"{string.Join("\",\"", bundleBuildSettings.AssetDirs)}\".");
                return;
            }
            var assetFolder = Path.GetDirectoryName(descAssetPath);
            outputBundleDdirectory = outputBundleDdirectory.Replace("\\", "/");
            assetFolder = assetFolder.Replace("\\", "/");
            int index = assetFolder.IndexOf(outputBundleDdirectory);
            if (outputBundleDdirectory.Equals(assetFolder))
            {
                m_outputBundleName = Path.GetFileName(assetFolder).ToLower();
            }
            else
            {
                m_outputBundleName = assetFolder.Substring(index + outputBundleDdirectory.Length + 1).Replace('/', '_').Replace("\\", "_").ToLower();
            }
            EditorUtils.SaveAndReimport(this);
        }

        /// <summary>
        /// 是否包含该模式
        /// </summary>
        /// <param name="generateMode"></param>
        /// <returns></returns>
        public bool ContainGenerateMode(GenerateMode generateMode)
        {
            return (m_generateMode & generateMode) == generateMode;
        }
    }

    [CustomEditor(typeof(BundleDescription))]
    public class BundleDescriptionInspector : UnityEditor.Editor
    {
        private void Init()
        {
            m_initialized = true;
            m_serializedObject = new SerializedObject(target);
        }
        public override void OnInspectorGUI()
        {
            if (!m_initialized)
            {
                Init();
            }
            m_serializedObject.Update();
            var bundleDesc = (target as BundleDescription);
            if (GUILayout.Button("Clear"))
            {
                bundleDesc.Clear();
            }
            if (GUILayout.Button("AutoCollectChildBundleDescs"))
            {
                bundleDesc.AutoCollectChildBundleDescs();
            }
            if (GUILayout.Button("AutoGenOutputBundleName"))
            {
                bundleDesc.AutoGenOutputBundleName();
            }
            if (GUILayout.Button("AutoCollectAsset"))
            {
                bundleDesc.AutoCollectAsset();
            }
            bundleDesc.m_bundleDescExplain = EditorGUILayout.TextField("BundleDesc解释说明", bundleDesc.m_bundleDescExplain);
            bundleDesc.m_solution = (Solution)EditorGUILayout.EnumFlagsField("参与方案", bundleDesc.m_solution);
            bundleDesc.m_searchAssetTypes = (AssetType)EditorGUILayout.EnumFlagsField("收集的资源类型", bundleDesc.m_searchAssetTypes);
            bundleDesc.m_generateMode = (BundleDescription.GenerateMode)EditorGUILayout.EnumFlagsField("BundleDesc生成模式", bundleDesc.m_generateMode);
            bundleDesc.m_outputBundleName = EditorGUILayout.TextField("Bundle名称", bundleDesc.m_outputBundleName);
            EditorGUILayout.PropertyField(m_serializedObject.FindProperty(nameof(bundleDesc.m_assetList)), new GUIContent("资源列表"));
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_serializedObject.FindProperty(nameof(bundleDesc.m_childBundleDescs)), new GUIContent("子BundleDescription列表"));
            m_serializedObject.ApplyModifiedProperties();
        }

        bool m_initialized = false;
        SerializedObject m_serializedObject;
    }
}
