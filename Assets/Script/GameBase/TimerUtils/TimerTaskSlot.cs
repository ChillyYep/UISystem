using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBase.TimeUtils
{
    /// <summary>
    /// 计时器任务插槽
    /// </summary>
    public abstract class TimerTaskSlotBase
    {
        public TimerTaskSlotBase(TimerTask timerTask)
        {
            InnerTimerTask = timerTask;
            m_hasStarted = false;
        }
        protected abstract void OnStart(ITimerManager timerManager);
        protected abstract void OnTick(ITimerManager timerManager);
        public void Tick(ITimerManager timerManager)
        {
            if (!m_hasStarted)
            {
                OnStart(timerManager);
                m_hasStarted = true;
            }
            else
            {
                OnTick(timerManager);
            }
        }

        public bool IsExpired => InnerTimerTask.IsExpired;

        public readonly TimerTask InnerTimerTask;

        protected bool m_hasStarted;
    }

    /// <summary>
    /// 默认的不会执行任何计时任务的Slot
    /// </summary>
    public class DefaultTimerTaskSlot : TimerTaskSlotBase
    {
        public DefaultTimerTaskSlot(TimerTask timerTask) : base(timerTask) { }

        protected override void OnStart(ITimerManager timerManager) { }
        protected override void OnTick(ITimerManager timerManager) { }
    }

    /// <summary>
    /// GameTime计时器任务插槽
    /// </summary>
    public class TimerTaskGameTimeSlot : TimerTaskSlotBase
    {
        public TimerTaskGameTimeSlot(TimerTask timerTask) : base(timerTask) { }

        protected override void OnStart(ITimerManager timerManager)
        {
            m_nextExecGameTime = timerManager.GameTime + InnerTimerTask.m_interval;
        }

        protected override void OnTick(ITimerManager timerManager)
        {
            if (timerManager.GameTime >= m_nextExecGameTime && !InnerTimerTask.IsExpired)
            {
                InnerTimerTask.Execute();
                m_nextExecGameTime += InnerTimerTask.m_interval;
            }
        }

        /// <summary>
        /// 游戏时间，下次执行的时间节点
        /// </summary>
        private float m_nextExecGameTime;
    }

    /// <summary>
    /// 客户端实际时间计时器任务插槽
    /// </summary>
    public class TimerTaskClientRealTimeSlot : TimerTaskSlotBase
    {
        public TimerTaskClientRealTimeSlot(TimerTask timerTask) : base(timerTask) { }

        protected override void OnStart(ITimerManager timerManager)
        {
            m_nextExecClientRealTime = timerManager.ClientRealTime.AddSeconds(InnerTimerTask.m_interval);
        }

        protected override void OnTick(ITimerManager timerManager)
        {
            if (timerManager.ClientRealTime >= m_nextExecClientRealTime && !InnerTimerTask.IsExpired)
            {
                InnerTimerTask.Execute();
                m_nextExecClientRealTime = m_nextExecClientRealTime.AddSeconds(InnerTimerTask.m_interval);
            }
        }
        /// <summary>
        /// 下次执行的客户端实际时间
        /// </summary>
        private DateTime m_nextExecClientRealTime;
    }
}