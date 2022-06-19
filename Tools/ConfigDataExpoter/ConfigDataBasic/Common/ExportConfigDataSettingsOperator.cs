using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    public class ExportConfigDataSettingsOperator
    {
        public ExportConfigDataSettingsOperator(string settingPath)
        {
            m_settingPath = settingPath;
        }
        public ExportConfigDataSettings Load()
        {
            if (!File.Exists(m_settingPath))
            {
                Save();
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
            return settings;
        }
        public void Save()
        {
            using (FileStream fs = new FileStream(m_settingPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var formatter = new ConfigData.BinaryFormatter(fs);
                formatter.WriteObject(settings);
                formatter.Flush();
                formatter.Close();
            }
        }

        private ExportConfigDataSettings settings = new ExportConfigDataSettings();

        private string m_settingPath;
    }
}
