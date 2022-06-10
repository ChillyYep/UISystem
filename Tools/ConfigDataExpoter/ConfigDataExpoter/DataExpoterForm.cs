﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            directory = Path.Combine(directory, "../../../Design");
            m_parseProcess = new ExcelProcess(directory, Path.Combine(directory, "Code"), Path.Combine(directory, "Data"), "ConfigData.cs", "ConfigDataTypeEnum.cs",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../BinaryReadWrite"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_parseProcess.ParseAllExcel();
        }
        private ExcelProcess m_parseProcess;
    }
}
