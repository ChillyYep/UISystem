using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBase.UI
{
    public enum UIModuleEnum
    {
        UIPage, //页面管理模块
    }
    public interface IUIState
    {
        bool isActive { get; }
    }
    public interface IUIModule : IUIState
    {
        UIModuleEnum uiModuleType { get; }
        void Init(IUICentreState state);
        void ResetModule();
        void Open(UIModuleParam moduleParam);
        void Hide();
        void Show();
    }
    public class UIModuleParam
    {
        public string name;
        public object uiParams;
    }
    /// <summary>
    /// 通用UI模块接口，包括Page系统，弹窗系统，HUD系统等
    /// </summary>
    public abstract class UIModule : MonoBehaviour, IUIModule
    {
        protected IUICentreState uICentre;//UI总管理器状态接口

        public bool isActive { get; protected set; }

        public abstract UIModuleEnum uiModuleType { get; }

        public virtual void Init(IUICentreState state)
        {
            uICentre = state;
        }
        public virtual void Hide()
        {
            isActive = false;
        }
        public virtual void Show()
        {
            isActive = true;
        }
        public abstract void Open(UIModuleParam moduleParam);

        public abstract void ResetModule();
    }
}
