using GameBase.Asset;
using GameBase.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameBase.Asset
{
    [CustomEditor(typeof(AssetBundleMap))]
    public class AssetBundleMapInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit BundleInfo Settings"))
            {
                var assetBundleMap = target as AssetBundleMap;
                AssetBundleMapBundleSettingEditor.CreateWindow(assetBundleMap);
            }
            GUI.enabled = false;
            base.OnInspectorGUI();
        }
    }

    public class AssetBundleMapBundleSettingEditor : EditorWindow
    {
        public static void CreateWindow(AssetBundleMap assetBundleMap)
        {
            var window = CreateWindow<AssetBundleMapBundleSettingEditor>();
            window.Load(assetBundleMap);
            window.Show();
        }

        private void Load(AssetBundleMap assetBundleMap)
        {
            m_assetBundleMap = assetBundleMap;
            m_bundleName2BundleInfo = assetBundleMap.GetBundleMap();
        }

        private void Initialize()
        {
            m_initialized = true;
            m_foldouts.Clear();
            foreach (var pair in m_bundleName2BundleInfo)
            {
                m_foldouts[pair.Key] = false;
            }

        }
        private void OnGUI()
        {
            if (!m_initialized)
            {
                Initialize();
            }
            m_changed = false;
            SingleBundleSetting tempSettings;
            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
            foreach (var bundlePair in m_bundleName2BundleInfo)
            {
                var settings = bundlePair.Value.m_setting;
                m_foldouts[bundlePair.Key] = EditorGUILayout.Foldout(m_foldouts[bundlePair.Key], bundlePair.Key, true);
                if (m_foldouts[bundlePair.Key])
                {
                    try
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginVertical();
                        tempSettings = settings;
                        settings.m_location = (SetupLocation)EditorGUILayout.EnumPopup(nameof(SetupLocation), settings.m_location);
                        if (!tempSettings.IsSame(settings))
                        {
                            m_changed = true;
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();
                    }
                    finally
                    {
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUI.changed && m_changed)
            {
                EditorUtils.SaveAndReimport(m_assetBundleMap);
            }
        }
        private Vector2 m_scrollPos;

        private AssetBundleMap m_assetBundleMap;

        private Dictionary<string, SingleBundleInfo> m_bundleName2BundleInfo;

        private bool m_initialized;

        private bool m_changed;

        private Dictionary<string, bool> m_foldouts = new Dictionary<string, bool>();
    }
}