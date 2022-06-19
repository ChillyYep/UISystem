using System;
using System.Collections.Generic;
/// <summary>
/// 程序自动生成的配置代码
/// </summary>
namespace ConfigData
{
    public partial class ConfigDataLoader
    {
        /// <summary>
        /// 加载所有表数据
        /// </summary>
        public void LoadAllTableData()
        {

            ConfigDataConfigPPTable = LoadConfigDataDict<ConfigPP>("ConfigPP");
            ConfigDataConfigPP2Table = LoadConfigDataDict<ConfigPP2>("ConfigPP2");
            ConfigDataUITranslateTextItemTable = LoadConfigDataDict<UITranslateTextItem>("UITranslateTextItem");
        }

        /// <summary>
        /// 测试数据类
        /// </summary>
        public Dictionary<int, ConfigPP> ConfigDataConfigPPTable { get; private set; }
        /// <summary>
        /// 测试数据类
        /// </summary>
        public Dictionary<int, ConfigPP2> ConfigDataConfigPP2Table { get; private set; }
        /// <summary>
        /// 来自UI的翻译项
        /// </summary>
        public Dictionary<int, UITranslateTextItem> ConfigDataUITranslateTextItemTable { get; private set; }
    }
}