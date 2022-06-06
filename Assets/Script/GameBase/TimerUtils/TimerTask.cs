using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.TimeUtils
{
    /// <summary>
    /// 计时器使用类型
    /// </summary>
    public enum TimerUseType
    {
        Default,
        AssetManger
    }

    /// <summary>
    /// 时间尺度类型
    /// </summary>
    public enum TimerScaleType
    {
        /// <summary>
        ///  游戏尺度时间
        /// </summary>
        GameTime,
        /// <summary>
        /// 现实尺度时间
        /// </summary>
        ClientRealTime,
        /// <summary>
        /// 服务器尺度时间
        /// </summary>
        ServerRealTime
    }
    /// <summary>
    /// 计时任务
    /// </summary>
    public class TimerTask
    {
        public TimerTask(Action execAct, TimerUseType timerUseType = TimerUseType.Default, float interval = 0f, uint executeTimes = 0u, TimerScaleType timerScaleType = TimerScaleType.GameTime)
        {
            m_timerUseType = timerUseType;
            m_interval = interval;
            m_executeTimes = executeTimes;
            m_execAct = execAct;
            m_curRemainTimes = executeTimes;
            m_timerScaleType = timerScaleType;
            m_tickID = DateTime.Now.Ticks;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Execute()
        {
            // 还有剩余次数才能执行
            if (!IsExpired)
            {
                m_execAct?.Invoke();
                if (m_executeTimes != InfinityTimes)
                {
                    m_curRemainTimes--;
                }
            }
        }

        /// <summary>
        /// 执行无穷次数
        /// </summary>
        public const uint InfinityTimes = uint.MaxValue;

        /// <summary>
        /// 计时器任务ID
        /// </summary>
        public readonly long m_tickID;

        /// <summary>
        /// 计时器类型
        /// </summary>
        public readonly TimerUseType m_timerUseType;

        /// <summary>
        /// 时间尺度类型
        /// </summary>
        public readonly TimerScaleType m_timerScaleType;

        /// <summary>
        /// 执行间隔时间(秒)
        /// </summary>
        public readonly float m_interval;

        /// <summary>
        /// 总的需要执行的次数
        /// </summary>
        public readonly uint m_executeTimes;

        /// <summary>
        /// 执行某行为
        /// </summary>
        public readonly Action m_execAct;

        /// <summary>
        /// 是否已经可以废弃
        /// </summary>
        public bool IsExpired => m_curRemainTimes <= 0;

        /// <summary>
        /// 剩余执行次数
        /// </summary>
        private uint m_curRemainTimes;

    }
}
