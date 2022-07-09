using GameBase.Asset;
using GameBase.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameBase.Asset
{

    public class BundleGlobalSolution
    {
        /// <summary>
        /// 方案名
        /// </summary>
        public string solutionName;
    }

    [CustomEditor(typeof(AssetBundleMap))]
    public class AssetBundleMapInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit BundleInfo Settings"))
            {
                var assetBundleMap = target as AssetBundleMap;
                BundleSettingEditor.CreateWindow(assetBundleMap);
            }
            GUI.enabled = false;
            base.OnInspectorGUI();
        }
    }
    /// <summary>
    /// Bundle相关设置
    /// </summary>
    public class BundleSettingEditor : TabsWindow
    {
        protected override List<TabItem> EditorTools { get; set; }

        public static void CreateWindow(AssetBundleMap assetBundleMap)
        {
            var window = CreateWindow<BundleSettingEditor>();
            window.titleContent = new GUIContent("Bundle管理");
            window.Load(assetBundleMap);
            window.Show();
        }

        private void Load(AssetBundleMap assetBundleMap)
        {
            m_assetBundleMap = assetBundleMap;
            EditorTools = new List<TabItem>() {
                new TabItem("逐Bundle设置", 200f, new PerBundleSettingWindow(m_assetBundleMap)),
                new TabItem("Empty", 200f),
                new TabItem("Empty", 200f),
                new TabItem("Empty", 200f),
                new TabItem("Empty", 200f),
                new TabItem("Empty", 200f),
            };
        }

        private AssetBundleMap m_assetBundleMap;
    }

    /// <summary>
    /// 逐Bundle相关设置子页签
    /// </summary>
    public class PerBundleSettingWindow : TabSubWindowBase
    {
        public PerBundleSettingWindow(AssetBundleMap assetBundleMap)
        {
            m_assetBundleMap = assetBundleMap;
            m_bundleName2BundleInfo = assetBundleMap.GetBundleMap();
        }
        class PerBundleSetting
        {
            public string bundleName;
            public SingleBundleSetting setting;
        }

        private Dictionary<string, SingleBundleInfo> m_bundleName2BundleInfo;

        private List<PerBundleSetting> m_allBundleSettings;

        private AssetBundleMap m_assetBundleMap;

        private ReorderableList m_bundleList;

        private Vector2 m_scrollPos;

        private List<float> m_colWithList = new List<float>()
        {
            200f,150f
        };
        private float offset = 10f;

        public override void OnOpen()
        {
            if (m_allBundleSettings == null)
            {
                m_allBundleSettings = m_bundleName2BundleInfo.Select(pair => new PerBundleSetting() { bundleName = pair.Key, setting = pair.Value.m_setting }).ToList();
                m_bundleList = new ReorderableList(m_allBundleSettings, typeof(PerBundleSetting), false, true, false, false)
                {
                    drawHeaderCallback = (rect) =>
                    {
                        rect.width = m_colWithList[0];
                        EditorGUI.LabelField(rect, "Bundle名");
                        rect.x += m_colWithList[0] + offset;
                        rect.width = m_colWithList[1] - offset;
                        EditorGUI.LabelField(rect, "SetupLocation");
                    },
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                      {
                          EditorGUI.BeginChangeCheck();
                          rect.width = m_colWithList[0];
                          EditorGUI.LabelField(rect, m_allBundleSettings[index].bundleName);
                          rect.x += rect.width;
                          rect.width = m_colWithList[1];
                          m_allBundleSettings[index].setting.m_location = (SetupLocation)EditorGUI.EnumPopup(rect, m_allBundleSettings[index].setting.m_location);
                          if (EditorGUI.EndChangeCheck())
                          {
                              EditorUtils.SaveAndReimport(m_assetBundleMap);
                          }
                      }
                };
            }
        }
        public override void OnGUI()
        {
            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);

            m_bundleList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }
    }
}