using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBase.ObjectUtils
{
    public class ObjectRefer<T> where T : UnityEngine.Object
    {
        private T res;
        public int refCount { get; private set; }
        private Action<T> removeAction;
        private bool dieFlag;
        public ObjectRefer(T res, Action<T> removeAction = null)
        {
            this.refCount = 1;
            this.res = res;
            this.removeAction = removeAction;
            dieFlag = false;
        }
        public T Get()
        {
            ++refCount;
            return res;
        }
        public void Release()
        {
            if (dieFlag)
            {
                Debug.LogError("引用已经失效,请移除该引用！");
                return;
            }
            --refCount;
            if (refCount == 0)
            {
                dieFlag = true;
                removeAction?.Invoke(res);
            }
        }
    }

}
