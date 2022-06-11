using GameBase.Asset;
using GameBase.CoroutineHelper;
using GameBase.Log;
using GameBase.Settings;
using GameBase.TimeUtils;
using GameBase.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    void Start()
    {
        GameSettings = GameClientSettings.LoadMainGameClientSettings();
        // 1、日志
        LogManager.Instance.Initialize(GameSettings.m_logSetting);
        // 2、计时器
        TimerManager.Instance.Initialize();
        // 3、资源管理器
        AssetManager.Instance.Initialize(GameSettings.m_resPathSettings, IndepedentCoroutineHelper.Instance);
        // 4、配置数据
        ConfigData.ConfigDataManager.Instance.Init();
        foreach (var item in ConfigData.ConfigDataManager.Instance.ConfigDataLoader.ConfigDataConfigPPTable)
        {
            Debug.LogError($"id:{item.Value.id},name:{item.Value.name},ratio:{string.Join(",", item.Value.ratio)}");
        }
        // 5、UI模块
        InitUIModules();
        Debug.Log("Game Start!");
    }

    private void Update()
    {
        TimerManager.Instance.Tick();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Game Quit!");
        // 与初始化顺序相反
        UnInitUIModules();
        ConfigData.ConfigDataManager.Instance.Uninit();
        AssetManager.Instance.UnInitialize();
        TimerManager.Instance.UnInitialize();
        LogManager.Instance.Unintialize();
    }
    private void InitUIModules()
    {
        UIModuleManagementCentre.Instance.Init();
        UIModuleManagementCentre.Instance.Show();
    }
    private void UnInitUIModules()
    {

    }
    [ContextMenu("ResetUIModule")]
    private void ResetUIModule()
    {
        UIModuleManagementCentre.Instance.ResetModule(UIModuleEnum.UIPage);
    }

    public GameClientSettings GameSettings { get; private set; }
}
