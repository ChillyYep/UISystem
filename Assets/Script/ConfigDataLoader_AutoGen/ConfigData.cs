using System;
using System.Collections.Generic;
/// <summary>
/// 程序自动生成的配置代码
/// </summary>
namespace ConfigData
{
    /// <summary>
    /// 颜色
    /// </summary>
    public enum Color
    {
        /// <summary>
        /// 红
        /// </summary>
        Red = 1,
        /// <summary>
        /// 绿
        /// </summary>
        Green = 2,
        /// <summary>
        /// 蓝
        /// </summary>
        Blue = 3,

    }
    /// <summary>
    /// 测试数据类
    /// </summary>
    [Serializable]
    public partial class ConfigPP: IConfigData, IBinarySerializer, IBinaryDeserializer
    {
        [Serializable]
        public class NestedClasss: IBinarySerializer, IBinaryDeserializer
        {
            public NestedClasss()
            {
            }
            public NestedClasss(Int32 id, String name)
            {
				this._id = id;
				this._name = name;
            }
            public void Deserialize(BinaryParser reader)
            {
				_id = reader.ReadInt32();
				_name = reader.ReadString();
            }
            public void Serialize(BinaryFormatter writer)
            {
				writer.WriteInt32(_id);
				writer.WriteString(_name);
            }
            private Int32 _id;
            /// <summary>
            /// Id
            /// </summary>
            public Int32 id
            {
                get
                {
                    return _id;
                }
                private set
                {
                    _id = value;
                }
            }
            private String _name;
            /// <summary>
            /// 姓名
            /// </summary>
            public String name
            {
                get
                {
                    return _name;
                }
                private set
                {
                    _name = value;
                }
            }

        }
        public ConfigPP()
        {
        }
        public ConfigPP(Int32 id, String name, Color color, List<Boolean> flags, List<Single> ratio, List<NestedClasss> nesttt)
        {
			this._id = id;
			this._name = name;
			this._color = color;
			this._flags = flags;
			this._ratio = ratio;
			this._nesttt = nesttt;
        }
        public void Deserialize(BinaryParser reader)
        {
			_id = reader.ReadInt32();
			_name = reader.ReadString();
			_color = (Color)reader.ReadEnum();
			_flags = reader.ReadBooleanList();
			_ratio = reader.ReadSingleList();
			_nesttt = reader.ReadObjectList<NestedClasss>();
        }
        public void Serialize(BinaryFormatter writer)
        {
			writer.WriteInt32(_id);
			writer.WriteString(_name);
			writer.WriteEnum((Int32)_color);
			writer.WriteBooleanList(_flags);
			writer.WriteSingleList(_ratio);
			writer.WriteObjectList<NestedClasss>(_nesttt);
        }
        private Int32 _id;
        /// <summary>
        /// ID
        /// </summary>
        public Int32 id
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
            }
        }
        private String _name;
        /// <summary>
        /// 名称
        /// </summary>
        public String name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }
        private Color _color;
        /// <summary>
        /// 颜色
        /// </summary>
        public Color color
        {
            get
            {
                return _color;
            }
            private set
            {
                _color = value;
            }
        }
        private List<Boolean> _flags;
        /// <summary>
        /// 标志
        /// </summary>
        public List<Boolean> flags
        {
            get
            {
                return _flags;
            }
            private set
            {
                _flags = value;
            }
        }
        private List<Single> _ratio;
        /// <summary>
        /// 比率
        /// </summary>
        public List<Single> ratio
        {
            get
            {
                return _ratio;
            }
            private set
            {
                _ratio = value;
            }
        }
        private List<NestedClasss> _nesttt;
        /// <summary>
        /// 内嵌类
        /// </summary>
        public List<NestedClasss> nesttt
        {
            get
            {
                return _nesttt;
            }
            private set
            {
                _nesttt = value;
            }
        }

    }
    /// <summary>
    /// 测试数据类
    /// </summary>
    [Serializable]
    public partial class ConfigPP2: IConfigData, IBinarySerializer, IBinaryDeserializer
    {
        [Serializable]
        public class NestedClasss: IBinarySerializer, IBinaryDeserializer
        {
            public NestedClasss()
            {
            }
            public NestedClasss(Int32 id, String name)
            {
				this._id = id;
				this._name = name;
            }
            public void Deserialize(BinaryParser reader)
            {
				_id = reader.ReadInt32();
				_name = reader.ReadString();
            }
            public void Serialize(BinaryFormatter writer)
            {
				writer.WriteInt32(_id);
				writer.WriteString(_name);
            }
            private Int32 _id;
            /// <summary>
            /// Id
            /// </summary>
            public Int32 id
            {
                get
                {
                    return _id;
                }
                private set
                {
                    _id = value;
                }
            }
            private String _name;
            /// <summary>
            /// 姓名
            /// </summary>
            public String name
            {
                get
                {
                    return _name;
                }
                private set
                {
                    _name = value;
                }
            }

        }
        public ConfigPP2()
        {
        }
        public ConfigPP2(Int32 id, String name, Color color, List<Boolean> flags, List<Single> ratio, NestedClasss nesttt)
        {
			this._id = id;
			this._name = name;
			this._color = color;
			this._flags = flags;
			this._ratio = ratio;
			this._nesttt = nesttt;
        }
        public void Deserialize(BinaryParser reader)
        {
			_id = reader.ReadInt32();
			_name = reader.ReadString();
			_color = (Color)reader.ReadEnum();
			_flags = reader.ReadBooleanList();
			_ratio = reader.ReadSingleList();
			_nesttt = reader.ReadObject<NestedClasss>();
        }
        public void Serialize(BinaryFormatter writer)
        {
			writer.WriteInt32(_id);
			writer.WriteString(_name);
			writer.WriteEnum((Int32)_color);
			writer.WriteBooleanList(_flags);
			writer.WriteSingleList(_ratio);
			writer.WriteObject<NestedClasss>(_nesttt);
        }
        private Int32 _id;
        /// <summary>
        /// ID
        /// </summary>
        public Int32 id
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
            }
        }
        private String _name;
        /// <summary>
        /// 名称
        /// </summary>
        public String name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }
        private Color _color;
        /// <summary>
        /// 颜色
        /// </summary>
        public Color color
        {
            get
            {
                return _color;
            }
            private set
            {
                _color = value;
            }
        }
        private List<Boolean> _flags;
        /// <summary>
        /// 标志
        /// </summary>
        public List<Boolean> flags
        {
            get
            {
                return _flags;
            }
            private set
            {
                _flags = value;
            }
        }
        private List<Single> _ratio;
        /// <summary>
        /// 比率
        /// </summary>
        public List<Single> ratio
        {
            get
            {
                return _ratio;
            }
            private set
            {
                _ratio = value;
            }
        }
        private NestedClasss _nesttt;
        /// <summary>
        /// 内嵌类
        /// </summary>
        public NestedClasss nesttt
        {
            get
            {
                return _nesttt;
            }
            private set
            {
                _nesttt = value;
            }
        }

    }

}