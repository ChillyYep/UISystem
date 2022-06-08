using System;
using System.Collections.Generic;
// 程序自动生成的配置代码
namespace ConfigData
{
    //颜色
    public enum Color
    {
        //红
        Red = 1,
        //绿
        Green = 2,
        //蓝
        Blue = 3,

    }
    //测试数据类
    [Serializable]
    public partial class ConfigPP
    {
        //NestedClass
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
            //Id
            private Int32 _id;
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
            //姓名
            private String _name;
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
        //ID
        
        private Int32 _id;
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
        //名称
        
        private String _name;
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
        //颜色
        
        private Color _color;
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
        //标志
        
        private List<Boolean> _flags;
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
        //比率
        
        private List<Single> _ratio;
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
        //内嵌类
        
        private NestedClasss _nesttt;
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