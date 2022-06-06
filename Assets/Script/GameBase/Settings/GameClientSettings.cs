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
    [CreateAssetMenu(fileName = nameof(GameClientSettings), menuName = nameof(GameClientSettings))]
    public class GameClientSettings : ScriptableObject
    {
        public LogSettings m_logSetting;

        public ResSettings m_resPathSettings;

        public BundleBuildSettings m_bundleBuildSettings;

        public const string MainGameClientSettingsAssetPath = "Settings/GameClientSetting";

        public static GameClientSettings m_instance;

        public static GameClientSettings LoadMainGameClientSettings()
        {
            if (m_instance == null)
            {
                m_instance = CommonUtils.LoadResource<GameClientSettings>(MainGameClientSettingsAssetPath);
            }
            return m_instance;
        }
    }
}
