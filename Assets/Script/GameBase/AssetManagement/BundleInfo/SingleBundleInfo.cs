using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Asset
{
    /// <summary>
    /// 安装时位置
    /// </summary>
    public enum SetupLocation
    {
        StreamingAssets,
        Remote
    }
    [Serializable]
    public class SingleBundleSetting
    {
        public SetupLocation m_location;

        public bool IsSame(SingleBundleSetting other)
        {
            return m_location == other.m_location;
        }
    }
    /// <summary>
    /// 单个Bundle的关联信息
    /// </summary>
    [Serializable]
    public class SingleBundleInfo
    {
        /// <summary>
        /// 带后缀Bundle名称
        /// </summary>
        public string m_bundleNameWithSuffix;

        /// <summary>
        /// 资源列表
        /// </summary>
        public List<string> m_assetList = new List<string>();

        /// <summary>
        /// CRC校验值
        /// </summary>
        public uint m_crc;

        /// <summary>
        /// Hash值
        /// </summary>
        public string m_hash;

        /// <summary>
        /// 版本号
        /// </summary>
        public int m_version;

        /// <summary>
        /// Byte大小
        /// </summary>
        public long m_size;

        /// <summary>
        /// 设置相关
        /// </summary>
        public SingleBundleSetting m_setting = new SingleBundleSetting();
    }
}
