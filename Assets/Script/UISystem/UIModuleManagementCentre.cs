using GameBase.Asset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameBase.UI
{
    public interface IUICentreState : IUIState { }
    public class UIModuleManagementCentre : Singleton_Unity<UIModuleManagementCentre>, IUICentreState
    {
        [Header("游戏内页面管理")]
        public PageManagementLayer pageModule;
        private IAssetManager m_assetManager;
        private Dictionary<UIModuleEnum, UIModule> uiModules = new Dictionary<UIModuleEnum, UIModule>();
        public bool isActive { get; private set; }

        public void OpenUI(UIModuleEnum uIModuleType, UIModuleParam uiParam)
        {
            if (uiModules.TryGetValue(uIModuleType, out UIModule uiModule))
            {
                uiModule.Open(uiParam);
            }
        }
        public void ResetModule(UIModuleEnum uIModuleType)
        {
            if (uiModules.TryGetValue(uIModuleType, out UIModule uiModule))
            {
                uiModule.ResetModule();
            }
        }
        public void Hide()
        {
            isActive = false;
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public void Init(IAssetManager assetManager)
        {
            m_assetManager = assetManager;
            pageModule.Init(this, m_assetManager);
            pageModule.Show();
            uiModules[pageModule.uiModuleType] = pageModule;
        }

        public void Show()
        {
            isActive = true;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }
}
