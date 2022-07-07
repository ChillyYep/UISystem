using GameBase.Asset;
using GameBase.ObjectUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameBase.UI
{
    //1、资源读取、释放管理
    //2、功能->打开/关闭
    //3、缓存
    public enum PageLayer
    {
        UI2D_LayerA,
        UI3D_LayerA
    }
    public enum PageType
    {
        FullScreen,
        Cover
    }
    /// <summary>
    /// 打开页面需要的参数配置
    /// </summary>
    public class PageModuleParam : UIModuleParam
    {
        public PageType pageType;
    }
    /// <summary>
    /// 使用多个页面栈管理页面
    /// 1、每个FullScreen页面作为栈底，其他栈元素都为Cover,同一时间只有一个页面栈在显示
    /// 2、多个页面栈之间可以“借用”Cover类型的页面，页面栈关闭后会归还页面，页面层级不变
    /// 3、当前页面栈关闭，就会启动缓存中最近的页面栈
    /// 4、如果缓存中没有其他页面栈，则不允许当前页面栈关闭，会保留一个FullScreen页面
    /// 5、可以隐藏页面管理系统，以使场景上不显示页面
    /// 6、若要关闭唯一页面栈的最底层页面，只能使用替换的方式，用一个新的FullScreen页面替换旧的FullScreen
    /// </summary>
    public class PageManagementLayer : UIModule
    {
        public RectTransform[] pageLayers;
        private PageStack openedPageStack;
        private List<PageStack> readyToReOpenStacks = new List<PageStack>();
        private Dictionary<string, ObjectRefer<Page>> pageCache = new Dictionary<string, ObjectRefer<Page>>();

        public override UIModuleEnum uiModuleType => UIModuleEnum.UIPage;

        #region Page开关核心逻辑
        /// <summary>
        /// 页面开启的入口
        /// 1、加载UI资源/取已有的UI资源
        /// 2、实例化UI
        /// 3、初始化页面上管理相关的数据
        /// 4、将页面压入页面栈中，并打开页面
        /// 5、缓存页面，仅在对页面的引用为0时释放资源
        /// </summary>
        /// <param name="forceCloseCurPageStack">打开FullScreen页面时，是否强制替换当前栈</param>
        private void _OpenPage(string pageName, object Params, PageType pageType, bool forceCloseCurPageStack = false)
        {
            Assert.IsTrue(isActive);
            if (pageCache.TryGetValue(pageName, out ObjectRefer<Page> pageRef))
            {
                var page = pageRef.Get();
                if (pageType == PageType.FullScreen || page.pageType == PageType.FullScreen)
                {
                    pageRef.Release();
                    Debug.Log("管理器中存在同名Page，只能重复打开同为Cover的页面");
                    return;
                }
                page.Params = Params;
                openedPageStack.Push(page);
            }
            else
            {
                //排除所有异常情况
                if (pageType == PageType.Cover && openedPageStack.Count == 0)
                {
                    Debug.LogError("当前页面栈为空，不能添加Cover类型的页面");
                    return;
                }
                var obj = m_assetManager.LoadAssetSync<GameObject>(AssetPathType.UIPagePrefab, pageName);
                if (obj == null)
                {
                    Debug.LogError($"页面资源不存在:{pageName}");
                    return;
                }
                var objInst = GameObject.Instantiate(obj);
                var page = objInst.GetComponent<Page>();
                int layer = (int)page.pageLayer;
                if (layer >= pageLayers.Length)
                {
                    GameObject.Destroy(objInst);
                    Debug.LogError("缺少PageLayer:" + page.pageLayer.ToString());
                    return;
                }
                RectTransform parent = pageLayers[layer];
                page.transform.SetParent(parent);
                page.transform.localScale = Vector3.one;
                page.transform.rotation = Quaternion.identity;
                page.transform.localPosition = Vector3.zero;

                if (pageType == PageType.FullScreen && openedPageStack.Count > 0)
                {
                    //是否强制关闭当前栈，并且打开一个新栈
                    if (!forceCloseCurPageStack)
                    {
                        //当前栈压入缓存
                        var oldPageStack = openedPageStack;
                        readyToReOpenStacks.Add(oldPageStack);
                        //New
                        openedPageStack = new PageStack(readyToReOpenStacks.Count);
                        page.Init(this, pageName, 0, pageType, _OpenPage, _ClosePage, _CheckClosable);
                        page.Params = Params;
                        openedPageStack.Push(page);
                        //Old
                        oldPageStack.PushToRecoverStack();
                    }
                    else
                    {
                        //删除当前栈
                        var newPageStack = new PageStack(readyToReOpenStacks.Count);
                        page.Init(this, pageName, 0, pageType, _OpenPage, _ClosePage, _CheckClosable);
                        page.Params = Params;
                        newPageStack.LazyPush(page);
                        readyToReOpenStacks.Add(newPageStack);
                        openedPageStack.CloseAllPage();
                    }
                }
                else
                {
                    page.Init(this, pageName, openedPageStack.Count, pageType, _OpenPage, _ClosePage, _CheckClosable);
                    page.Params = Params;
                    openedPageStack.Push(page);
                }
                pageCache[pageName] = new ObjectRefer<Page>(page, _DestroyPage);
            }
        }
        /// <summary>
        /// 页面调用Close时会调用该方法
        /// 1、关闭页面，将页面弹出栈
        /// 2、释放页面引用，如果计数为0则卸载资源、销毁物体
        /// </summary>
        /// <param name="intent"></param>
        private void _ClosePage(string pageName)
        {
            Assert.IsTrue(isActive);
            ObjectRefer<Page> pageRef;
            var page = FindPageByName(pageName);
            if (!pageCache.TryGetValue(pageName, out pageRef) || page == null)
            {
                Debug.LogWarning(string.Format("名为{0}的页面不存在当前的页面栈中！", pageName));
                return;
            }
            if (openedPageStack.Count > 1)
            {
                openedPageStack.Pop(page);
                pageRef.Release();
            }
            else if (openedPageStack.Count == 1)
            {
                if (readyToReOpenStacks.Count > 0)
                {
                    int index = readyToReOpenStacks.Count - 1;
                    openedPageStack.Pop(page);
                    pageRef.Release();
                    openedPageStack = readyToReOpenStacks[index];
                    readyToReOpenStacks.RemoveAt(index);
                    openedPageStack.PopFromRecoverStack();
                }
                else
                {
                    Debug.LogWarning("关闭页面无效，页面管理器中必须有至少一个页面");
                }
            }
        }
        /// <summary>
        /// 销毁页面，释放资源
        /// </summary>
        /// <param name="page"></param>
        private void _DestroyPage(Page page)
        {
            Assert.IsTrue(isActive);
            pageCache.Remove(page.name);
            GameObject.Destroy(page.gameObject);
            //AssetManager.Instance.UnLoad(AssetPathType.UIPagePrefab, page.name);
            Debug.Log(string.Format("页面{0}释放资源", page.name));
        }
        /// <summary>
        /// 关闭FullScreen页面请先关闭其他Cover页面，最底部的页面栈无法关闭FullScreen页面，只能尝试在OpenPage中启用Replace
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        private bool _CheckClosable(string pageName)
        {
            var page = FindPageByName(pageName);
            if (page == null)
            {
                Debug.LogWarning(string.Format("名为{0}的页面不存在当前的页面栈中！", pageName));
                return false;
            }
            //如果关闭FullScreen，且缓存中还有其他页面栈，要先关闭其他Cover页面
            if (page.pageType == PageType.FullScreen)
            {
                if (openedPageStack.Count > 1)
                {
                    Debug.Log("请先关闭当前页面栈中的其他Cover页面！");
                    return false;
                }
                if (readyToReOpenStacks.Count == 0)
                {
                    Debug.Log(string.Format("{0}是FullScreen类型的页面，且缓存中不存在其他页面栈，无法关闭！可以尝试隐藏页面管理系统以替代关闭唯一FullScreen页面", pageName));
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 关闭
        public void CloseAllPage(bool allPageStack = false)
        {
            PageStack onlyOne = null;
            if (allPageStack)
            {
                while (readyToReOpenStacks.Count > 0)
                {
                    openedPageStack.CloseAllPage();
                }
                onlyOne = openedPageStack;
            }
            else
            {
                if (readyToReOpenStacks.Count > 0)
                {
                    openedPageStack.CloseAllPage();
                }
                else
                {
                    onlyOne = openedPageStack;
                }
            }
            if (onlyOne != null)
            {
                onlyOne.CloseAllPage(false);
            }
        }
        public void ClosePage(string pageName)
        {
            openedPageStack.ClosePage(pageName);
        }
        public void ClosePage<T>() where T : Page
        {
            openedPageStack.ClosePage<T>();
        }
        #endregion

        #region 查找
        public T FindPage<T>() where T : Page
        {
            return openedPageStack.FindPage<T>();
        }
        public Page FindPageByName(string pageName)
        {
            return openedPageStack.FindPageByName(pageName);
        }
        public Page FindPageByIndex(int pageIndex)
        {
            return openedPageStack.FindPageByIndex(pageIndex);
        }
        #endregion

        #region UI模块通用接口
        public override void Init(IUICentreState state, IAssetManager assetManager)
        {
            base.Init(state, assetManager);
            openedPageStack = new PageStack(0);
        }
        public override void Hide()
        {
            base.Hide();
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
        public override void Show()
        {
            base.Show();
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public override void Open(UIModuleParam moduleParam)
        {
            var pageParam = moduleParam as PageModuleParam;
            if (pageParam == null)
            {
                Debug.LogError("打开页面参数类型指定错误");
                return;
            }
            _OpenPage(pageParam.name, pageParam.uiParams, pageParam.pageType);
        }

        public override void ResetModule()
        {
            CloseAllPage(true);
        }
        #endregion
    }
}