using System;
using System.Collections.Generic;
// Test
namespace ConfigData
{
    public class TestObj : IConfigData, IBinaryDeserializer
    {
        public int id { get; protected set; }
        public void Deserialize(BinaryParser reader)
        {
            throw new NotImplementedException();
        }
    }
    public partial class ConfigDataLoader
    {
        public void LoadAllData()
        {
            TestDict = LoadConfigDataDict<TestObj>("xxx");
        }

        public Dictionary<int, TestObj> TestDict { get; private set; }

    }
}
