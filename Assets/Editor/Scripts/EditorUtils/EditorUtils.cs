using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameBase.Editor
{
    /// <summary>
    /// 自定义一些编辑器快捷工具
    /// </summary>
    public static class EditorUtils
    {
        /// <summary>
        /// 安全创建文件夹
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="deleteIfExist"></param>
        /// <returns></returns>
        public static bool CreateDirectory(string dirPath, bool deleteIfExist = false)
        {
            if (Directory.Exists(dirPath))
            {
                if (deleteIfExist)
                {
                    try
                    {
                        Directory.Delete(dirPath, true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            try
            {
                Directory.CreateDirectory(dirPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            return true;

        }

        /// <summary>
        /// 重新导入资源
        /// </summary>
        /// <param name="assetPath"></param>
        public static void SaveAndReimport(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset != null)
            {
                var assetImporter = AssetImporter.GetAtPath(assetPath);
                EditorUtility.SetDirty(asset);
                assetImporter.SaveAndReimport();
            }
            else
            {
                Debug.LogError($"File {assetPath} Doesn't Exist!");
            }
        }

        /// <summary>
        /// 重新导入资源
        /// </summary>
        /// <param name="asset"></param>
        public static void SaveAndReimport(UnityEngine.Object asset)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var assetImporter = AssetImporter.GetAtPath(assetPath);
                EditorUtility.SetDirty(asset);
                assetImporter.SaveAndReimport();
            }
            else
            {
                Debug.LogError("Object is't a asset!");
            }
        }
    }
}
