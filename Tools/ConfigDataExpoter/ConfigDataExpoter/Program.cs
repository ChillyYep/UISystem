using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigDataExpoter
{
    static class Program
    {
        /// <summary>
        /// 功能要求：
        /// 1、创建/修改表头；
        /// 2、自动生成代码；
        /// 3、数据导出；
        /// 4、文件比对；
        /// 5、多语言
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DataExpoterForm());
        }
    }
}
