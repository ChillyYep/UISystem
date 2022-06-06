using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.CoroutineHelper
{
    public interface ICouroutineHelper
    {
        Coroutine StartCoroutine(IEnumerator routine);

        /// <summary>
        /// 开启唯一协程
        /// </summary>
        /// <param name="coroutineID"></param>
        /// <param name="enumerator"></param>
        /// <param name="stopPrevCoroutineIfConflict"></param>
        void StartUniqueCoroutine(CoroutineID coroutineID, IEnumerator enumerator, bool stopPrevCoroutineIfConflict = false);
    }
    public enum CoroutineID
    {
        UnloadUnusedAsset,
    }
    /// <summary>
    /// 独立携程管理
    /// </summary>
    public class IndepedentCoroutineHelper : Singleton_Unity<IndepedentCoroutineHelper>, ICouroutineHelper
    {
        /// <summary>
        /// 开启唯一协程
        /// </summary>
        /// <param name="coroutineID"></param>
        /// <param name="enumerator"></param>
        /// <param name="stopPrevCoroutineIfConflict"></param>
        public void StartUniqueCoroutine(CoroutineID coroutineID, IEnumerator enumerator, bool stopPrevCoroutineIfConflict = false)
        {
            if (m_uniqueRunningCoroutine.ContainsKey(coroutineID))
            {
                if (stopPrevCoroutineIfConflict)
                {
                    var prevCoroutine = m_uniqueRunningCoroutine[coroutineID];
                    if (prevCoroutine != null)
                    {
                        StopCoroutine(prevCoroutine);
                        m_uniqueRunningCoroutine.Remove(coroutineID);
                    }
                }
                else
                {
                    Debug.LogError($"There are running same coroutine!CoroutineID:{coroutineID}.");
                    return;
                }
            }
            var coroutine = StartCoroutine(CoroutineWrapper(coroutineID, enumerator));
            m_uniqueRunningCoroutine[coroutineID] = coroutine;
        }

        /// <summary>
        /// 封装协程
        /// </summary>
        /// <param name="coroutineID"></param>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        private IEnumerator CoroutineWrapper(CoroutineID coroutineID, IEnumerator enumerator)
        {
            yield return enumerator;
            m_uniqueRunningCoroutine.Remove(coroutineID);

        }

        private readonly Dictionary<CoroutineID, Coroutine> m_uniqueRunningCoroutine = new Dictionary<CoroutineID, Coroutine>();
    }
}
