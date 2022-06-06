using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Algorithm
{
    public static class SearchHelper
    {
        /// <summary>
        /// 深度优先搜索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void DepthFirstSearch<T>(T root, Action<T, IEnumerable<T>> executeForeach, Func<T, IEnumerable<T>> getChildsFunc, Predicate<T> stopCondition = null) where T : class
        {
            if (root == null || getChildsFunc == null || executeForeach == null)
            {
                return;
            }
            Stack<T> stack = new Stack<T>();
            // 记录根节点到当前节点的路径
            Stack<T> path = new Stack<T>();
            stack.Push(root);
            bool lastNodeIsLeafNode = false;
            while (stack.Count > 0)
            {
                var element = stack.Pop();

                // 叶子节点
                if (lastNodeIsLeafNode)
                {
                    path.Pop();
                }
                // 路径获取
                path.Push(element);

                if (stopCondition != null && stopCondition(element))
                {
                    break;
                }

                executeForeach(element, path);
                // 获得子节点
                var childs = getChildsFunc(element);
                int count = 0;
                foreach (var item in childs)
                {
                    count++;
                    stack.Push(item);
                }
                lastNodeIsLeafNode = count == 0;
            }
        }

        /// <summary>
        /// 广度优先搜索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void BreathFirstSearch<T>(T root, Action<T> executeForeach, Func<T, IEnumerable<T>> getChildsFunc, Predicate<T> stopCondition = null) where T : class
        {
            if (root == null || getChildsFunc == null || executeForeach == null)
            {
                return;
            }
            Queue<T> queue = new Queue<T>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                if (stopCondition != null && stopCondition(element))
                {
                    break;
                }
                executeForeach(element);
                try
                {
                    var childs = getChildsFunc(element);
                    foreach (var item in childs)
                    {
                        queue.Enqueue(item);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
    }

}
