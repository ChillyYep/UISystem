using GameBase.Asset;
using GameBase.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameBase.Settings
{
    /// <summary>
    /// 游戏内设置
    /// </summary>
    [CreateAssetMenu(fileName = nameof(GameClientSettings), menuName = nameof(GameClientSettings) + "/" + nameof(GameClientSettings))]
    [UniqueResourcesAsset("Resources/Settings/GameClientSetting")]
    public class GameClientSettings : Singleton_ScriptableObject<GameClientSettings>
    {
        public LogSettings m_logSetting;

        public ResSettings m_resPathSettings;

        public BundleBuildSettings m_bundleBuildSettings;
    }
}
