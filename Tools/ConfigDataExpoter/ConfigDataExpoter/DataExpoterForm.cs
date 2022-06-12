using System;
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
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            m_settingPath = Path.Combine(directory, nameof(ExportConfigDataSettings) + ".settings");
            if (!File.Exists(m_settingPath))
            {
                using (FileStream fs = new FileStream(m_settingPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    var formatter = new ConfigData.BinaryFormatter(fs);
                    formatter.WriteObject(settings);
                    formatter.Flush();
                    formatter.Close();
                }
            }
            else
            {
                using (FileStream fs = new FileStream(m_settingPath, FileMode.Open, FileAccess.Read))
                {
                    var parser = new ConfigData.BinaryParser(fs);
                    settings = parser.ReadObject<ExportConfigDataSettings>();
                    parser.Close();
                }

            }
            InitializeComponent();
            InitializeFromSetting();
            m_parseProcess = new ExcelProcess(directory);
        }

        private void ParseAllExcel(object sender, EventArgs e)
        {
            using (FileStream fs = new FileStream(m_settingPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var formatter = new ConfigData.BinaryFormatter(fs);
                formatter.WriteObject(settings);
            }
            m_parseProcess.ParseAllExcel(settings);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void InitializeFromSetting()
        {
            rootDirectoryText.Text = settings.ExportRootDirectoryPath;
            codeDirectoryNameText.Text = settings.ExportCodeDirectoryName;
            dataDirectoryNameText.Text = settings.ExportDataDirectoryName;
            configDataNameText.Text = settings.ExportConfigDataName;
            loaderCodeNameText.Text = settings.ExportLoaderCodeName;
            typeEnumCodeNameText.Text = settings.ExportTypeEnumCodeName;
            copyFromDirectoryPathText.Text = settings.CopyFromDirectoryPath;
            unityCodeDirectoryText.Text = settings.UnityCodeDirectory;
            unityDataDirectoryText.Text = settings.UnityDataDirectory;
            m_dropDownItems = Enum.GetNames(typeof(CodeType));
            codeTypeDropDown.Items.AddRange(m_dropDownItems);
            codeTypeDropDown.SelectedItem = settings.CodeVisiblity.ToString();
        }
        private void _TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == rootDirectoryText)
            {
                settings.ExportRootDirectoryPath = textBox.Text;
            }
            else if (textBox == codeDirectoryNameText)
            {
                settings.ExportCodeDirectoryName = textBox.Text;
            }
            else if (textBox == dataDirectoryNameText)
            {
                settings.ExportDataDirectoryName = textBox.Text;
            }
            else if (textBox == configDataNameText)
            {
                settings.ExportConfigDataName = textBox.Text;
            }
            else if (textBox == loaderCodeNameText)
            {
                settings.ExportLoaderCodeName = textBox.Text;
            }
            else if (textBox == typeEnumCodeNameText)
            {
                settings.ExportTypeEnumCodeName = textBox.Text;
            }
            else if (textBox == copyFromDirectoryPathText)
            {
                settings.CopyFromDirectoryPath = textBox.Text;
            }
            else if (textBox == unityCodeDirectoryText)
            {
                settings.UnityCodeDirectory = textBox.Text;
            }
            else if (textBox == unityDataDirectoryText)
            {
                settings.UnityDataDirectory = textBox.Text;
            }
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private string m_settingPath;

        private ExportConfigDataSettings settings = new ExportConfigDataSettings();

        private ExcelProcess m_parseProcess;

        private string[] m_dropDownItems;

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dropdown = sender as ComboBox;
            if (dropdown == codeTypeDropDown)
            {
                try
                {
                    settings.CodeVisiblity = (CodeType)Enum.Parse(typeof(CodeType), m_dropDownItems[dropdown.SelectedIndex]);
                }
                catch
                {
                    settings.CodeVisiblity = CodeType.Client;
                }
            }
        }
    }
}
