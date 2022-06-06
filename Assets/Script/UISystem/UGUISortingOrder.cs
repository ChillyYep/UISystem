using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, DisallowMultipleComponent]
//[RequireComponent(typeof(Renderer), typeof(Canvas))]
public class UGUISortingOrder : MonoBehaviour
{
    [SerializeField]
    int sortingOrder;
    [SerializeField]
    string sortingLayer = "Default";
    private void Awake()
    {
        OnValidate();
    }
    private void OnValidate()
    {
        Renderer m_renderer = GetComponent<Renderer>();
        if (m_renderer != null)
        {
            m_renderer.sortingOrder = sortingOrder;
            m_renderer.sortingLayerName = sortingLayer;
        }
        else
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
                canvas.sortingLayerName = sortingLayer;
            }
        }
    }
}
