using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public struct GraphNodeInfo
{
    public string m_name;
    public string[] m_inputPort;
    public string[] m_outputPort;
    public Vector2 m_screenPos;
}
public class GraphSearchWindowProvider : ScriptableObject, ISearchWindowProvider
{
    public void Setup(Action<GraphNodeInfo> onCreation)
    {
        m_onCreation = onCreation;
    }
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        return new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Nodes")),
            new SearchTreeGroupEntry(new GUIContent("Test 1"),1),
            new SearchTreeEntry(new GUIContent("Pass A"))
            {
                level = 2,
                userData = "Pass A"
            },
            new SearchTreeGroupEntry(new GUIContent("Test 2"),1),
            new SearchTreeEntry(new GUIContent("Pass B"))
            {
                level = 2,
                userData = "Pass B"
            }
        };
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        if (SearchTreeEntry.content.text == "Pass A")
        {
            m_onCreation?.Invoke(new GraphNodeInfo()
            {
                m_name = "Pass A",
                m_inputPort = new string[] { "Input1", "Input2" },
                m_outputPort = new string[] { "Output1", "Output2" },
                m_screenPos = context.screenMousePosition
            });
        }
        else if (SearchTreeEntry.content.text == "Pass B")
        {
            m_onCreation?.Invoke(new GraphNodeInfo()
            {
                m_name = "Pass B",
                m_inputPort = new string[] { "Input1", "Input2", "Input3" },
                m_outputPort = new string[] { "Output1", "Output2" },
                m_screenPos = context.screenMousePosition
            });
        }
        return true;
    }

    private Action<GraphNodeInfo> m_onCreation;
}
