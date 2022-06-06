using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.UI
{
    /// <summary>
    /// 页面间传递的数据，通常只是为了让表现效果可以更丰富
    /// </summary>
    public class Intent
    {
        public string sender;//sender PageName
        public PageEvent pageEvent;
        public bool notifyAllOpened;
        public string[] recievers;
        public object param;//扩展数据
        public Intent(string sender, PageEvent pageEvent = PageEvent.PageNone, object param = null, bool notifyAllOpened = true, string[] recievers = null)
        {
            this.sender = sender;
            this.pageEvent = pageEvent;
            this.param = param;
            this.notifyAllOpened = notifyAllOpened;
            this.recievers = recievers;
        }
    }
}
