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
            m_baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            m_settingPath = Path.Combine(m_baseDirectory, nameof(ExportConfigDataSettings) + ".settings");
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
        }

        private void ParseAllExcel(object sender, EventArgs e)
        {
            m_parseProcess = new ExcelProcess(m_baseDirectory);
            m_parseProcess.ParseAllExcel(settings);
        }

        private void ParseLanguageExcel(object sender, EventArgs e)
        {
            string srcDir = Path.Combine(m_baseDirectory, settings.ExportRootDirectoryPath, settings.ExportLanguageDirectoryName);
            string dstDir = Path.Combine(m_baseDirectory, settings.UnityLanaguageDirectory);
            SerializedMultiLanguageExporter expoter = new SerializedMultiLanguageExporter(srcDir);
            expoter.ExportData(dstDir);
        }

        private void InitializeFromSetting()
        {
            rootDirectoryText.Text = settings.ExportRootDirectoryPath;
            codeDirectoryNameText.Text = settings.ExportCodeDirectoryName;
            dataDirectoryNameText.Text = settings.ExportDataDirectoryName;
            languageDirectoryText.Text = settings.ExportLanguageDirectoryName;
            configDataNameText.Text = settings.ExportConfigDataName;
            loaderCodeNameText.Text = settings.ExportLoaderCodeName;
            typeEnumCodeNameText.Text = settings.ExportTypeEnumCodeName;
            copyFromDirectoryPathText.Text = settings.CopyFromDirectoryPath;
            unityCodeDirectoryText.Text = settings.UnityCodeDirectory;
            unityDataDirectoryText.Text = settings.UnityDataDirectory;
            unityLanguageDirectoryText.Text = settings.UnityLanaguageDirectory;
            m_dropDownItems = Enum.GetNames(typeof(CodeType));
            codeTypeDropDown.Items.AddRange(m_dropDownItems);
            codeTypeDropDown.SelectedItem = settings.CodeVisiblity.ToString();

            removeExpiredLanguageItemChecked.Checked = settings.RemoveExpiredLanguageItem;
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
            else if (textBox == languageDirectoryText)
            {
                settings.ExportLanguageDirectoryName = textBox.Text;
            }
            else if (textBox == unityLanguageDirectoryText)
            {
                settings.UnityLanaguageDirectory = textBox.Text;
            }
            SaveSettings();
        }

        private void SaveSettings()
        {
            using (FileStream fs = new FileStream(m_settingPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var formatter = new ConfigData.BinaryFormatter(fs);
                formatter.WriteObject(settings);
                formatter.Flush();
                formatter.Close();
            }
        }

        private void _SelectedIndexChanged(object sender, EventArgs e)
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

        private void _CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (removeExpiredLanguageItemChecked == checkBox)
            {
                settings.RemoveExpiredLanguageItem = checkBox.Checked;
            }
            SaveSettings();
        }

        private string m_settingPath;

        private ExportConfigDataSettings settings = new ExportConfigDataSettings();

        private ExcelProcess m_parseProcess;

        private string[] m_dropDownItems;

        private string m_baseDirectory;
    }
}
