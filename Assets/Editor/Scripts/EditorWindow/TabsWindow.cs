using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// 多页签窗口
/// </summary>
public abstract class TabsWindow : EditorWindow
{
    private int lastIndex = -1;
    protected int TabIndex { get; private set; }
    protected abstract List<TabItem> EditorTools { get; set; }

    protected virtual void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < EditorTools.Count; ++i)
        {
            if (GUILayout.Button(EditorTools[i].TabName, GUILayout.MaxWidth(EditorTools[i].Width)))
            {
                TabIndex = i;
            }
        }
        EditorGUILayout.EndHorizontal();
        if (TabIndex >= 0 && TabIndex < EditorTools.Count)
        {
            if (TabIndex != lastIndex)
            {
                if (lastIndex >= 0 && lastIndex < EditorTools.Count)
                {
                    EditorTools[lastIndex].SubWindow?.OnClose();
                }
                lastIndex = TabIndex;
                EditorTools[TabIndex].SubWindow?.OnOpen();
            }
            EditorTools[TabIndex].SubWindow?.OnGUI();
        }
    }
}

public struct TabItem
{
    public TabItem(string tabName, float width, TabSubWindowBase subWindow = null)
    {
        TabName = tabName;
        Width = width;
        SubWindow = subWindow;
    }

    /// <summary>
    /// Tab名称
    /// </summary>
    public readonly string TabName;
    /// <summary>
    /// Tab按钮宽度
    /// </summary>
    public readonly float Width;
    /// <summary>
    /// 子页签窗口
    /// </summary>
    public readonly TabSubWindowBase SubWindow;
}

/// <summary>
/// 子页签窗口
/// </summary>
public abstract class TabSubWindowBase
{
    public virtual void OnOpen() { }
    public virtual void OnClose() { }
    public abstract void OnGUI();
}
