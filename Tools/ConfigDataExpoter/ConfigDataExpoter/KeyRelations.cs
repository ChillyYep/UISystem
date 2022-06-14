using System;
using System.Collections.Generic;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 主键外键关系
    /// </summary>
    public class KeyRelations
    {
        /// <summary>
        /// 一个Sheet最多一个主键，主键ID-Int32应该够用了
        /// </summary>
        public readonly Dictionary<string, HashSet<Int32>> m_primaryKeys = new Dictionary<string, HashSet<int>>();

        /// <summary>
        /// 一个Sheet可能有多个外键字段，每个外键字段关联另一个表的列数据
        /// </summary>
        public readonly Dictionary<string, Dictionary<string, List<Int32>>> m_foreignKeys = new Dictionary<string, Dictionary<string, List<Int32>>>();

        /// <summary>
        /// 添加主键
        /// </summary>
        public void AddPrimaryKey(string className, Int32 id)
        {
            if (!m_primaryKeys.TryGetValue(className, out var primaryKeySet))
            {
                primaryKeySet = new HashSet<int>();
                m_primaryKeys[className] = primaryKeySet;
            }

            if (!primaryKeySet.Add(id))
            {
                throw new ParseExcelException($"{className}主键ID不能重复");
            }
        }

        /// <summary>
        /// 添加外键
        /// </summary>
        public void AddForeignKey(string curClassName, string foreignClassName, Int32 id)
        {
            if (!m_foreignKeys.TryGetValue(curClassName, out var allForeignKeyDict))
            {
                allForeignKeyDict = new Dictionary<string, List<Int32>>();
                m_foreignKeys[curClassName] = allForeignKeyDict;
            }
            if (!allForeignKeyDict.TryGetValue(foreignClassName, out var oneForeignKeyDict))
            {
                oneForeignKeyDict = new List<int>();
                allForeignKeyDict[foreignClassName] = oneForeignKeyDict;
            }
            oneForeignKeyDict.Add(id);
        }

        public void CheckForiegnKeySafty()
        {
            foreach (var foreignKeys in m_foreignKeys)
            {
                foreach (var foreignKey in foreignKeys.Value)
                {
                    var keyClass = foreignKey.Key;
                    var datas = foreignKey.Value;
                    if (!m_primaryKeys.TryGetValue(keyClass, out var primaryKeys))
                    {
                        throw new ParseExcelException($"{keyClass}主键缺失");
                    }
                    foreach (var data in datas)
                    {
                        if (!primaryKeys.Contains(data))
                        {
                            throw new ParseExcelException($"表{keyClass}不存在{data}的主键");
                        }
                    }
                }
            }
        }
    }
}
