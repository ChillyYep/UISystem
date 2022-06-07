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
            public NestedClasss(Int32 @id,String @name)
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

        public ConfigPP(Int32 @id,String @name,List<Single> @ratio,Color[] @color,NestedClasss @nesttt)
        {
			id = @id;
			name = @name;
			ratio = @ratio;
			color = @color;
			nesttt = @nesttt;
        }
        //ID
        public Int32 id
        {
            get;
            private set;
        }
        //名称
        public String name
        {
            get;
            private set;
        }
        //比率
        public List<Single> ratio
        {
            get;
            private set;
        }
        //颜色
        public Color[] color
        {
            get;
            private set;
        }
        //内嵌类
        public NestedClasss nesttt
        {
            get;
            private set;
        }

    }

}