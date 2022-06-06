using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.UI
{
    //1、页面本身开关事件、内部处理事件
    //2、页面管理器逻辑事件,如Cover相关
    //3、外部事件，调用游戏通用事件管理器
    //4、页面间数据通信
    public class Page : MonoBehaviour
    {
        //private Dictionary<int, Action<object>> eventHandlers = new Dictionary<int, Action<object>>();
        public PageLayer pageLayer;
        public int pageIndex { get; private set; }
        public PageType pageType { get; private set; }//打开后就不会再变化
        public bool IsOpened { get; private set; }
        private bool _initialized = false;
        private Action<string> closeHandler;
        private Predicate<string> checkClosable;
        private Action<string, object, PageType, bool> openHandler;
        private Intent intent;
        public object Params { get; set; }
        public Intent GetIntent()
        {
            return intent;
        }
        private PageManagementLayer manager;

        #region 数据初始化/更新
        public void Init(PageManagementLayer pageInitializer, string pageName, int pageIndex, PageType pageType, Action<string, object, PageType, bool> openHandler, Action<string> closeHandler, Predicate<string> checkClosable)
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;
            this.name = pageName;
            this.pageIndex = pageIndex;
            this.pageType = pageType;
            this.openHandler = openHandler;
            this.closeHandler = closeHandler;
            this.checkClosable = checkClosable;
            manager = pageInitializer;
            intent = new Intent(pageName);
        }
        public void UpdatePageIndex(int pageIndex)
        {
            this.pageIndex = pageIndex;
        }
        #endregion

        #region 通用方法
        public T FindPage<T>() where T : Page
        {
            return manager.FindPage<T>();
        }
        public Page FindPageByName(string pageName)
        {
            return manager.FindPageByName(pageName);
        }
        public Page FindPageByIndex(int index)
        {
            return manager.FindPageByIndex(index);
        }
        public void ClosePage(string pageName)
        {
            manager.ClosePage(pageName);
        }
        public void ClosePage<T>() where T : Page
        {
            manager.ClosePage<T>();
        }
        public void CloseAll()
        {
            manager.CloseAllPage();
        }
        #endregion

        #region 外部事件处理
        //protected virtual void RegisterListener(Dictionary<int, Action<object>> eventHandlers)
        //{

        //}
        //private void UnRegisterListener()
        //{
        //    //EventManager.RemoveListener
        //    eventHandlers.Clear();
        //}
        #endregion

        #region 页面打开/关闭事件
        /// <summary>
        /// 页面打开时要进行的行为
        /// </summary>
        protected virtual void DoOpen()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 页面关闭时要进行的行为
        /// </summary>
        protected virtual void DoClose()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
        protected void Open(string pageName, object Params, PageType pageType, bool fullScreenReplace = false)
        {
            openHandler?.Invoke(pageName, Params, pageType, fullScreenReplace);
        }
        /// <summary>
        /// 打开其他页面
        /// </summary>
        /// <param name="pageName"></param>
        public virtual void OpenPage(string pageName)
        {
            //Open(pageName, null, PageType.Cover);
        }
        /// <summary>
        /// 由页面栈调用
        /// </summary>
        public void Open()
        {
            if (IsOpened)
            {
                return;
            }
            //RegisterListener(eventHandlers);
            IsOpened = true;
            DoOpen();
        }
        public void Close()
        {
            bool closeable = checkClosable == null ? true : (checkClosable(name));
            if (closeable)
            {
                CloseWithoutDestroy();
                intent.pageEvent = PageEvent.PageClose;
                closeHandler?.Invoke(name);
            }
        }
        public void CloseWithoutDestroy()
        {
            if (!IsOpened)
            {
                return;
            }
            //UnRegisterListener();
            IsOpened = false;
            DoClose();
        }
        #endregion

        #region 处理页面间传递事件
        /// <summary>
        /// 有页面覆盖在当前页面上时本页面要进行的行为
        /// </summary>
        protected virtual void OnPageOpened(Intent intent)
        {

        }
        /// <summary>
        /// 覆盖在本页面上的页面关闭时要进行的行为
        /// </summary>
        protected virtual void OnPageClosed(Intent intent)
        {

        }
        /// <summary>
        /// 处理所有页面间传递的数据，常用的页面打开/关闭事件则转发给OnPageCover和ObPageUnCover
        /// </summary>
        /// <param name="intent"></param>
        public virtual void OnPageNotify(Intent intent)
        {
            if (intent == null)
            {
                return;
            }
            switch (intent.pageEvent)
            {
                case PageEvent.PageOpen://有页面在上层打开
                    OnPageOpened(intent);
                    break;
                case PageEvent.PageClose://有上层页面关闭
                    OnPageClosed(intent);
                    break;
            }
        }
        #endregion
    }
}
