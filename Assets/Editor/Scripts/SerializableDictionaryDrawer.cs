using GameBase.Asset;
using GameBase.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomPropertyDrawer(typeof(ISerializableDictionary), true)]
//public class SerializableDictionaryDrawer : PropertyDrawer
//{
//    private void Initialize()
//    {
//        m_initialized = true;
//        var guiEnabledAttrs = fieldInfo.GetCustomAttributes(typeof(GUIEnableAttribute), true) as GUIEnableAttribute[];
//        var guiEnabledAttr = guiEnabledAttrs.Length > 0 ? guiEnabledAttrs[0] : null;
//        m_guiEnabled = guiEnabledAttr != null ? guiEnabledAttr.Enabled : true;
//    }
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("pairs"), label);

//        //return base.GetPropertyHeight(property, label);
//    }
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        if (!m_initialized)
//        {
//            Initialize();
//        }
//        GUI.enabled = m_guiEnabled;
//        var pairs = property.FindPropertyRelative("pairs");
//        //EditorGUI.BeginProperty(position, label, property);
//        EditorGUI.PropertyField(position, pairs, label);
//        //EditorGUI.EndProperty();
//    }

//    private bool m_initialized;

//    private bool m_guiEnabled;
//}
