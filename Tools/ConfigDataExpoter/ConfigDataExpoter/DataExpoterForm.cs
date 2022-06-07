using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigDataExpoter
{
    public partial class DataExpoterForm : Form
    {
        public DataExpoterForm()
        {
            InitializeComponent();
            m_parseProcess = new ParseExcelProcess("D:/ChillyYep/UISystem/Design", "D:/ChillyYep/UISystem/Design/Code", "ConfigData.cs");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_parseProcess.ParseAllExcel();
        }
        private ParseExcelProcess m_parseProcess;
    }
}
