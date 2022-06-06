using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Collections
{
    public class GUIEnableAttribute : Attribute
    {
        public bool Enabled { get; set; }
    }
    public abstract class Pair<TKey, TValue> : IComparable<Pair<TKey, TValue>> where TKey : IComparable<TKey>
    {
        public Pair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
        public TKey key;
        public TValue value;

        public int CompareTo(Pair<TKey, TValue> other)
        {
            return key.CompareTo(other.key);
        }
    }

    public interface ISerializableDictionary { }

    public abstract class SerializableDictionary<TKey, TValue, TPair> : ISerializableDictionary, IEnumerable<KeyValuePair<TKey, TValue>>, ISerializationCallbackReceiver where TPair : Pair<TKey, TValue> where TKey : IComparable<TKey>
    {
        public readonly Dictionary<TKey, TValue> Dict = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get => Dict[key];
            set => Dict[key] = value;
        }

        public void Add(TKey key, TValue value)
        {
            Dict.Add(key, value);
        }

        public void Remove(TKey key)
        {
            Dict.Remove(key);
        }

        public void Clear()
        {
            Dict.Clear();
            pairs.Clear();
        }

        public bool ContainKey(TKey key)
        {
            return Dict.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dict.TryGetValue(key, out value);
        }
        [SerializeField]
        private List<TPair> pairs = new List<TPair>();

        public void OnBeforeSerialize()
        {
            pairs.Clear();
            foreach (var pair in Dict)
            {
                pairs.Add(Activator.CreateInstance(typeof(TPair), pair.Key, pair.Value) as TPair);
            }
            // 从稳定性考虑，需要排序保证每次序列化结果一致
            pairs.Sort();
        }

        public void OnAfterDeserialize()
        {
            Dict.Clear();
            foreach (var pair in pairs)
            {
                Dict[pair.key] = pair.value;
            }
            pairs.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dict.GetEnumerator();
        }
    }
}
