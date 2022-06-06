using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PlayerSettingsTools
{
    public const string InBundle = "IN_BUNDLE";
    public const string separator = ";";
    static void Select(string totalSymbols, string exceptSymbol, string expectSymbol)
    {
        if (totalSymbols.Contains(exceptSymbol))
        {
            if (totalSymbols.Contains(separator + exceptSymbol))
            {
                totalSymbols.Replace(separator + exceptSymbol, "");
            }
            else if (totalSymbols.Contains(exceptSymbol + separator))
            {
                totalSymbols.Replace(exceptSymbol + separator, "");
            }
            else
            {
                totalSymbols.Replace(exceptSymbol, "");
            }
        }
        totalSymbols.Concat(separator + expectSymbol);
    }
    static void SwitchBundleModel(bool inBundle)
    {
        string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        if (inBundle)
        {
            if (!defineSymbols.Contains(InBundle))
            {
                defineSymbols += separator + InBundle;
            }
        }
        else
        {
            if (defineSymbols.Contains(InBundle))
            {
                if (defineSymbols.Contains(separator + InBundle))
                {
                    defineSymbols = defineSymbols.Replace(separator + InBundle, "");
                }
                else if (defineSymbols.Contains(InBundle + separator))
                {
                    defineSymbols = defineSymbols.Replace(InBundle + separator, "");
                }
                else
                {
                    defineSymbols = defineSymbols.Replace(InBundle, "");
                }
            }
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defineSymbols);
    }
    [MenuItem(MenuItemCollection.AssetLoadModeChange.EditorAssetBundle)]
    static void SwitchToBundleMode()
    {
        SwitchBundleModel(true);
    }
    [MenuItem(MenuItemCollection.AssetLoadModeChange.EditorAssetdatabase)]
    static void SwitchToEditorMode()
    {
        SwitchBundleModel(false);
    }
}
