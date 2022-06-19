using System;

namespace ConfigData
{
    /// <summary>
    /// 所有ConfigData配置类都需要ID
    /// </summary>
    public interface IConfigData
    {
        Int32 id { get; }
    }
}
