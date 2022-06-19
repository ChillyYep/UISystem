namespace ConfigDataExpoter
{
    public enum SheetType
    {
        Invalid,
        Enum,
        Class
    }
    /// <summary>
    /// 表数据
    /// </summary>
    public class ConfigSheetData : ConfigMetaData
    {
        public const int TypeRowIndex = 0;

        #region Enum相关
        public const int EnumHeaderIndex = 1;
        public const int EnumFieldIndex = 2;
        public const int EnumBodyIndex = 3;

        public const int EnumNameCellIndex = 0;
        public const int EnumCommentCellIndex = 1;
        public const int EnumVisiblityCellIndex = 2;

        public const int EnumFlagIDCellIndex = 0;
        public const int EnumFlagNameCellIndex = 1;
        public const int EnumFlagCommentCellIndex = 2;
        #endregion

        #region Class相关
        public const int ClassHeaderIndex = 1;
        public const int ClassNameCellIndex = 0;
        public const int ClassCommentCellIndex = 1;

        #endregion
        public string m_filePath;

        public string m_fileName;

        public string m_sheetName;

        public SheetType m_sheetType;

        public ConfigMetaData m_configMetaData;
    }
}
