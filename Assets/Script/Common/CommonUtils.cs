using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonUtils
{
    public static T LoadResource<T>(string path, string suffix = "asset") where T : UnityEngine.Object
    {
        if (Application.isPlaying)
        {
#if UNITY_EDITOR && !IN_BUNDLE
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/Resources/{path}.{suffix}");
#else
            return Resources.Load<T>(path);
#endif
        }
        else
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/Resources/{path}.{suffix}");
#else
            return Resources.Load<T>(path);
#endif
        }
    }
}
