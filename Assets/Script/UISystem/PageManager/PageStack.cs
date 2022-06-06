using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBase.UI
{
    /// <summary>
    /// 栈底为一个FullScreen页面，其他元素都为Cover页面
    /// </summary>
    public class PageStack
    {
        public PageStack(int stackIndex)
        {
            this.StackIndex = stackIndex;
            this.name = "NoInit";
        }
        // 用栈底的FullScreen页面来命名
        public string name { get; private set; }
        public int Count => pageGroups.Count;
        public List<Page> pageGroups = new List<Page>();
        public int StackIndex { get; set; }
        public void Push(Page page)
        {
            var findPage = FindPageByName(page.name);
            if (findPage != null)
            {
                findPage.Close();
            }
            page.Open();
            if (page.pageType == PageType.Cover)
            {
                var intent = page.GetIntent();
                if (intent != null)
                {
                    intent.pageEvent = PageEvent.PageOpen;
                    NotifyAllOpendStackPage(intent);
                }
            }
            //必须在通知完后把新的Page加入
            pageGroups.Add(page);
            if (pageGroups.Count == 1)
            {
                name = pageGroups[0].name;
            }
            ReOrderInHierachy();
        }
        public void Pop(Page page)
        {
            var findPage = FindPageByName(page.name);
            if (findPage == null)
            {
                return;
            }
            if (findPage.pageType == PageType.FullScreen && pageGroups.Count > 1)
            {
                Debug.LogError("关闭FullScreen页面前请先关闭页面栈上其他Cover页面");
                return;
            }
            var intent = findPage.GetIntent();
            pageGroups.RemoveAt(findPage.pageIndex);
            //findPage.Close();
            ReOrderInHierachy();
            //必须在页面关闭，且从栈中删除后才能通知其他Page
            if (findPage.pageType == PageType.Cover)
            {
                intent.pageEvent = PageEvent.PageClose;
                NotifyAllOpendStackPage(intent);
            }

        }
        public void LazyPush(Page page)
        {
            var findPage = FindPageByName(page.name);
            if (findPage != null)
            {
                pageGroups.RemoveAt(findPage.pageIndex);
            }
            //必须在通知完后把新的Page加入
            pageGroups.Add(page);
            if (pageGroups.Count == 1)
            {
                name = pageGroups[0].name;
            }
            ReOrderInHierachy();
        }
        public void LazyPop(Page page)
        {
            var findPage = FindPageByName(page.name);
            if (findPage == null)
            {
                return;
            }
            pageGroups.RemoveAt(findPage.pageIndex);
            ReOrderInHierachy();
        }
        public void PushToRecoverStack()
        {
            for (int i = pageGroups.Count - 1; i >= 0; --i)
            {
                pageGroups[i].CloseWithoutDestroy();
            }
        }
        public void ReOrderInHierachy()
        {
            for (int i = 0; i < pageGroups.Count; ++i)
            {
                pageGroups[i].UpdatePageIndex(i);
                pageGroups[i].transform.SetSiblingIndex(i);
            }
        }
        public void PopFromRecoverStack()
        {
            ReOrderInHierachy();
            for (int i = 0; i < pageGroups.Count; ++i)
            {
                pageGroups[i].Open();
            }
        }
        public void NotifyAllOpendStackPage(Intent intent)
        {
            if (intent == null)
            {
                return;
            }
            if (intent.notifyAllOpened)
            {
                for (int i = pageGroups.Count - 1; i >= 0; --i)
                {
                    pageGroups[i].OnPageNotify(intent);
                }
            }
            else if (intent.recievers != null)
            {
                for (int i = 0; i < intent.recievers.Length; ++i)
                {
                    var page = FindPageByName(intent.recievers[i]);
                    if (page != null)
                    {
                        page.OnPageNotify(intent);
                    }
                }
            }
        }
        public void CloseAllPage(bool closeFullScreen = true)
        {
            int first = closeFullScreen ? 0 : 1;
            //从后往前，性能更优
            for (int i = pageGroups.Count - 1; i >= first; --i)
            {
                pageGroups[i].Close();
            }
            //pageGroups.Clear();
        }
        public void ClosePage(string pageName)
        {
            var page = FindPageByName(pageName);
            if (page == null)
            {
                return;
            }
            page.Close();
        }
        public void ClosePage<T>() where T : Page
        {
            var page = FindPage<T>();
            if (page == null)
            {
                return;
            }
            page.Close();
        }
        public void PushNewFullScreen(Page page)
        {
            if (page.pageType == PageType.FullScreen)
            {
                CloseAllPage(false);
                //var curFullScreenPage = pageGroups[0];

            }
        }
        public T FindPage<T>() where T : Page
        {
            for (int i = 0; i < pageGroups.Count; ++i)
            {
                if (pageGroups[i] is T originPage)
                {
                    return originPage;
                }
            }
            return default;
        }
        public Page FindPageByName(string pageName)
        {
            for (int i = 0; i < pageGroups.Count; ++i)
            {
                if (pageGroups[i].name == pageName)
                {
                    return pageGroups[i];
                }
            }
            return null;
        }
        public Page FindPageByIndex(int pageIndex)
        {
            if (pageIndex < pageGroups.Count && pageIndex >= 0)
            {
                return pageGroups[pageIndex];
            }
            return null;
        }
    }
}
