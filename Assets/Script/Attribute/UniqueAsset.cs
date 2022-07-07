using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 唯一资源地址，尽在Resources文件夹下有效
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class UniqueResourcesAsset : Attribute
{
    private const string Suffix = "asset";
    public UniqueResourcesAsset(string uniquePath)
    {
        if (Suffix.Equals(Path.GetExtension(uniquePath)))
        {
            UniquePath = uniquePath;
            UniquePathWithoutExtension = uniquePath.Substring(0, uniquePath.LastIndexOf(Suffix) - 1);
        }
        else
        {
            UniquePathWithoutExtension = uniquePath;
            UniquePath = uniquePath + "." + Suffix;
        }
    }
    public string UniquePath { get; private set; }
    public string UniquePathWithoutExtension { get; private set; }
}
