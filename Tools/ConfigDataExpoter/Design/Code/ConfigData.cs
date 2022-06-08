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
    public partial class ConfigPP
    {
        [Serializable]
        public class NestedClasss
        {
            public NestedClasss()
            {
            }
            public NestedClasss(Int32 id, String name)
            {
				this.id = @id;
				this.name = @name;
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
        public ConfigPP(Int32 id, String name, Color color, List<Boolean> flags, List<Single> ratio, NestedClasss nesttt)
        {
			this.id = @id;
			this.name = @name;
			this.color = @color;
			this.flags = @flags;
			this.ratio = @ratio;
			this.nesttt = @nesttt;
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
    /// <summary>
    /// 测试数据类
    /// </summary>
    [Serializable]
    public partial class ConfigPP2
    {
        [Serializable]
        public class NestedClasss
        {
            public NestedClasss()
            {
            }
            public NestedClasss(Int32 id, String name)
            {
				this.id = @id;
				this.name = @name;
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
			this.id = @id;
			this.name = @name;
			this.color = @color;
			this.flags = @flags;
			this.ratio = @ratio;
			this.nesttt = @nesttt;
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