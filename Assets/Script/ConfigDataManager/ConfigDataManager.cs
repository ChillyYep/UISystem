using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConfigData
{
    public class ConfigDataManager : Singleton_CSharp<ConfigDataManager>
    {
        public void Init()
        {
            ConfigDataLoader = new ConfigDataLoader("Assets/ConfigData", "txt");
            ConfigDataLoader.LoadAllTableData();
        }

        public void Uninit()
        {
            ConfigDataLoader = null;
        }

        public ConfigDataLoader ConfigDataLoader { get; private set; }
    }

}

