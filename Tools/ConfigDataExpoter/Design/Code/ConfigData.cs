using System;
using System.Collections.Generic;
// 程序自动生成的配置代码
namespace ConfigData
{
    //颜色
    public enum Color
    {
        //红
        Red=1,
        //绿
        Green=2,
        //蓝
        Blue=3,

    }
    //测试数据类
    public partial class ConfigPP
    {
        //NestedClass
        public partial class NestedClasss
        {
            public NestedClasss(Int32 @id, String @name)
            {
				id = @id;
				name = @name;
            }
            //Id
            public Int32 id
            {
                get;
                private set;
            }
            //姓名
            public String name
            {
                get;
                private set;
            }

        }

        public ConfigPP(Int32 @id, String @name, Color[] @color, List<Single> @ratio, NestedClasss @nesttt)
        {
			id = @id;
			name = @name;
			color = @color;
			ratio = @ratio;
			nesttt = @nesttt;
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
        private Color[] _color;
        public Color[] color
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