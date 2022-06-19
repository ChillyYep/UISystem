using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MenuItemCollection
{
    public const string ProjectCustomMotherMenuItem = "ProjectCustomTools";

    public const string AssetMenuItem = "Assets";

    /// <summary>
    /// 资源加载模式切换
    /// </summary>
    public static class AssetLoadModeChange
    {
        public const string EditorAssetdatabase = ProjectCustomMotherMenuItem + "/" + nameof(AssetLoadModeChange) + "/" + nameof(EditorAssetdatabase);
        public const string EditorAssetBundle = ProjectCustomMotherMenuItem + "/" + nameof(AssetLoadModeChange) + "/" + nameof(EditorAssetBundle);
    }

    public static class AssetBundleBuilder
    {
        public const string BuildAssetBundle = ProjectCustomMotherMenuItem + "/" + nameof(AssetBundleBuilder) + "/" + "All Steps : " + nameof(BuildAssetBundle);
        public const string PreProcessBuildAssetBundles = ProjectCustomMotherMenuItem + "/" + nameof(AssetBundleBuilder) + "/" + "Split Step 0 : " + nameof(PreProcessBuildAssetBundles);
        public const string RecollectBundleDesciption = ProjectCustomMotherMenuItem + "/" + nameof(AssetBundleBuilder) + "/" + "Split Step 1 : " + nameof(RecollectBundleDesciption);
        public const string CheckBundleDescsLegality = ProjectCustomMotherMenuItem + "/" + nameof(AssetBundleBuilder) + "/" + "Split Step 2 : " + nameof(CheckBundleDescsLegality);
        public const string CheckAssetLegality = ProjectCustomMotherMenuItem + "/" + nameof(AssetBundleBuilder) + "/" + "Split Step 3 : " + nameof(CheckAssetLegality);
        public const string Copy2StreamingAssets = ProjectCustomMotherMenuItem + "/" + nameof(AssetBundleBuilder) + "/" + "Split Step 4 : " + nameof(Copy2StreamingAssets);
    }

    public static class ExcelProcess
    {
        public const string CollectUIText = ProjectCustomMotherMenuItem + "/" + nameof(ExcelProcess) + "/" + nameof(CollectUIText);

    }

    public static class Create
    {
        public const string BundleDescription = AssetMenuItem + "/" + nameof(Create) + "/" + nameof(BundleDescription);
    }

}
