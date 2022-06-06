using GameBase.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBase.TimeUtils
{
    /// <summary>
    /// 计时器管理器接口
    /// </summary>
    public interface ITimerManager
    {
        /// <summary>
        /// 带了游戏时间尺度的时间(秒)
        /// </summary>
        float GameTime { get; }
        /// <summary>
        /// 当前帧的实际时间
        /// </summary>
        DateTime ClientRealTime { get; }

        void CreateTimerTask(TimerTask timerTask, TimerScaleType timerScaleType);

        void DestroyTimerTask(long tickID);

        void DestroyTimerTask(TimerTask timerTask);

        void DestroyTimerTask(TimerUseType timerType);
    }

    /// <summary>
    /// 计时工具
    /// </summary>
    public class TimerManager : Singleton_CSharp<TimerManager>, ITimerManager
    {
        public void Initialize()
        {
            // todo
        }

        public void UnInitialize()
        {
            // todo
        }

        public void Tick()
        {
            // DeltaTime和真实时间
            GameTime += Time.deltaTime;
            ClientRealTime = DateTime.Now;

            m_ready2DestroyInTick.Clear();

            // 先执行所有计时器任务
            foreach (var pair in m_timerTaskDict)
            {
                // Tick尝试执行
                var slot = pair.Value;
                if (slot != null)
                {
                    slot.Tick(this);
                }
                // 执行完毕/过期的计时器任务槽要删除
                if (slot.IsExpired)
                {
                    m_ready2DestroyInTick.Add(slot);
                }
            }
            // 删除计时器任务槽
            foreach (var slot in m_ready2DestroyInTick)
            {
                m_timerTaskDict.Remove(slot.InnerTimerTask.m_tickID);
            }
        }

        #region 创建/销毁计时器任务
        /// <summary>
        /// 添加计时任务
        /// </summary>
        /// <param name="timerTask"></param>
        public void CreateTimerTask(TimerTask timerTask, TimerScaleType timerScaleType = TimerScaleType.GameTime)
        {
            if (m_timerTaskDict.ContainsKey(timerTask.m_tickID))
            {
                Debug.LogError("Same TickID's timerTask has already exist!");
                return;
            }
            m_timerTaskDict[timerTask.m_tickID] = CreateTimerTaskSlot(timerTask, timerScaleType);
        }

        /// <summary>
        /// 销毁计时任务
        /// </summary>
        public void DestroyTimerTask(long tickID)
        {
            if (m_timerTaskDict.ContainsKey(tickID))
            {
                m_timerTaskDict.Remove(tickID);
            }
        }

        /// <summary>
        /// 销毁计时任务
        /// </summary>
        public void DestroyTimerTask(TimerTask timerTask)
        {
            DestroyTimerTask(timerTask.m_tickID);
        }

        /// <summary>
        /// 销毁特定类型的计时任务
        /// </summary>
        /// <param name="timerType"></param>
        public void DestroyTimerTask(TimerUseType timerType)
        {
            List<TimerTask> ready2Destroy = new List<TimerTask>();
            foreach (var timerTaskPair in m_timerTaskDict)
            {
                if (timerTaskPair.Value.InnerTimerTask.m_timerUseType == timerType)
                {
                    ready2Destroy.Add(timerTaskPair.Value.InnerTimerTask);
                }
            }
            foreach (var timerTask in ready2Destroy)
            {
                m_timerTaskDict.Remove(timerTask.m_tickID);
            }
        }

        #endregion

        /// <summary>
        /// 创建计时任务槽
        /// </summary>
        /// <param name="timerTask"></param>
        /// <param name="timerScaleType"></param>
        /// <returns></returns>
        private TimerTaskSlotBase CreateTimerTaskSlot(TimerTask timerTask, TimerScaleType timerScaleType)
        {
            switch (timerScaleType)
            {
                case TimerScaleType.GameTime:
                    return new TimerTaskGameTimeSlot(timerTask);
                case TimerScaleType.ClientRealTime:
                    return new TimerTaskClientRealTimeSlot(timerTask);
            }
            return new DefaultTimerTaskSlot(timerTask);
        }

        /// <summary>
        /// 计时任务字典表
        /// </summary>
        private readonly Dictionary<long, TimerTaskSlotBase> m_timerTaskDict = new Dictionary<long, TimerTaskSlotBase>();

        /// <summary>
        /// 准备销毁的计时器任务，只在Tick中使用
        /// </summary>
        private readonly List<TimerTaskSlotBase> m_ready2DestroyInTick = new List<TimerTaskSlotBase>();

        public float GameTime { get; private set; }

        public DateTime ClientRealTime { get; private set; }
    }
}

