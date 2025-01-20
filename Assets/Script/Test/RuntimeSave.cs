using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RuntimeSave : MonoBehaviour
{
    public int m_syncValue = 10;
    ScriptableObjectTest m_scriptAsset;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        m_scriptAsset = AssetDatabase.LoadAssetAtPath<ScriptableObjectTest>("Assets/RuntimeAssets/ScriptableObjectTest.asset");
    }

    // Update is called once per frame
    void Update()
    {
        m_scriptAsset.m_configId = m_syncValue;
    }
    [ContextMenu("Save")]
    void Save()
    {
        AssetDatabase.SaveAssets();
    }
}
