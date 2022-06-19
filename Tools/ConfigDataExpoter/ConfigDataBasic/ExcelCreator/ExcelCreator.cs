using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigDataExpoter
{
    public class ExcelCreator
    {
        public ExcelCreator(string filePath)
        {
            m_fileExcel = filePath;
        }
        public ISheet CreateSheet(ConfigSheetData configSheetData)
        {
            if (configSheetData.m_sheetType == SheetType.Enum)
            {
                return CreateEnumSheet(configSheetData.m_configMetaData as ConfigEnumMetaData);
            }
            else if (configSheetData.m_sheetType == SheetType.Class)
            {
                return CreateClassSheet(configSheetData.m_configMetaData as ConfigClassMetaData);
            }
            return null;
        }
        /// <summary>
        /// 创建Enum Sheet
        /// </summary>
        /// <param name="configEnumMetaData"></param>
        /// <returns></returns>
        public ISheet CreateEnumSheet(ConfigEnumMetaData configEnumMetaData)
        {
            var enumName = configEnumMetaData.m_name;
            var comment = configEnumMetaData.m_comment;
            var visiblity = configEnumMetaData.m_visiblity;
            var values = configEnumMetaData.m_enumNameValue;
            var sheet = workBook.CreateSheet(enumName);
            // Sheet类型
            sheet.CreateRow(0).CreateCell(0).SetCellValue(SheetType.Enum.ToString());
            // 枚举元数据
            var enumHeaderRow = sheet.CreateRow(1);
            enumHeaderRow.CreateCell(0).SetCellValue(enumName);
            enumHeaderRow.CreateCell(1).SetCellValue(comment);
            enumHeaderRow.CreateCell(2).SetCellValue(visiblity.ToString());
            // FieldName
            var fieldNameRow = sheet.CreateRow(2);
            fieldNameRow.CreateCell(0).SetCellValue(ConfigEnumMetaData.IDPrimaryKey);
            fieldNameRow.CreateCell(1).SetCellValue(ConfigEnumMetaData.ValuePrimaryKey);
            fieldNameRow.CreateCell(2).SetCellValue(ConfigEnumMetaData.CommentCell);
            // Value
            List<ConfigEnumMetaData.EnumData> enumDatas = new List<ConfigEnumMetaData.EnumData>(values.Values);
            enumDatas.Sort((a, b) =>
            {
                return a.m_ID.CompareTo(b.m_ID);
            });
            int curRow = 2;
            foreach (var enumData in enumDatas)
            {
                ++curRow;
                var valueRow = sheet.CreateRow(curRow);
                valueRow.CreateCell(0).SetCellValue(enumData.m_ID.ToString());
                valueRow.CreateCell(1).SetCellValue(enumData.m_name);
                valueRow.CreateCell(2).SetCellValue(enumData.m_comment);
            }
            return sheet;
        }
        /// <summary>
        /// 创建Class
        /// </summary>
        /// <param name="configClassMetaData"></param>
        public ISheet CreateClassSheet(ConfigClassMetaData configClassMetaData)
        {
            var className = configClassMetaData.m_classname;
            var classComment = configClassMetaData.m_comment;
            var fieldInfos = configClassMetaData.m_fieldsInfo;
            var sheet = workBook.CreateSheet(className);
            // Sheet类型
            sheet.CreateRow(0).CreateCell(0).SetCellValue(SheetType.Class.ToString());
            // 类型和注释
            var classRow = sheet.CreateRow(1);
            classRow.CreateCell(0).SetCellValue(className);
            classRow.CreateCell(1).SetCellValue(classComment);
            // 创建行
            for (int i = (int)ConfigClassFieldHeader.ClassFieldName; i <= (int)ConfigClassFieldHeader.ClassNestedClassFieldIsList; ++i)
            {
                sheet.CreateRow(i);
            }
            // 创建FieldInfo
            var idFieldInfoIndex = fieldInfos.FindIndex(fieldinfo => fieldinfo.FieldName.Equals(ConfigClassMetaData.IDPrimaryKey, StringComparison.OrdinalIgnoreCase));
            CreateFieldInfo(fieldInfos[idFieldInfoIndex], sheet, idFieldInfoIndex);
            for (int i = 0; i < fieldInfos.Count; ++i)
            {
                if (i == idFieldInfoIndex)
                {
                    continue;
                }
                CreateFieldInfo(fieldInfos[i], sheet, i);
            }
            return sheet;
        }
        /// <summary>
        /// 创建域
        /// </summary>
        /// <param name="fieldMetaData"></param>
        /// <param name="sheet"></param>
        /// <param name="index"></param>
        private void CreateFieldInfo(ConfigFieldMetaData fieldMetaData, ISheet sheet, int index)
        {
            sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldName).CreateCell(index).SetCellValue(fieldMetaData.FieldName);
            sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldComment).CreateCell(index).SetCellValue(fieldMetaData.Comment);
            sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldVisiblity).CreateCell(index).SetCellValue(fieldMetaData.OwnVisiblity.ToString());
            sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldForeignKey).CreateCell(index).SetCellValue(fieldMetaData.ForeignKey);
            sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldIsList).CreateCell(index).SetCellValue(fieldMetaData.ListType);
            if (fieldMetaData.OwnDataType == DataType.NestedClass)
            {
                var nestedClassMetaData = fieldMetaData.OwnNestedClassMetaData;
                sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldDataType).CreateCell(index).SetCellValue(nestedClassMetaData.m_classname);
                string[] fieldNames = new string[nestedClassMetaData.FieldNum];
                string[] fieldComments = new string[nestedClassMetaData.FieldNum];
                string[] fieldTypes = new string[nestedClassMetaData.FieldNum];
                string[] fieldIslists = new string[nestedClassMetaData.FieldNum];
                for (int i = 0; i < nestedClassMetaData.m_fieldsInfo.Count; ++i)
                {
                    fieldNames[i] = nestedClassMetaData.m_fieldsInfo[i].FieldName;
                    fieldComments[i] = nestedClassMetaData.m_fieldsInfo[i].Comment;
                    fieldTypes[i] = nestedClassMetaData.m_fieldsInfo[i].OwnDataType.ToString();
                    fieldIslists[i] = nestedClassMetaData.m_fieldsInfo[i].ListType;
                }
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldNames).CreateCell(index).SetCellValue(string.Join(",", fieldNames));
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldComments).CreateCell(index).SetCellValue(string.Join(",", fieldComments));
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldTypes).CreateCell(index).SetCellValue(string.Join(",", fieldTypes));
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldIsList).CreateCell(index).SetCellValue(string.Join(",", fieldIslists));
            }
            else
            {
                sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldDataType).CreateCell(index).SetCellValue(fieldMetaData.OwnDataType.ToString());
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldNames).CreateCell(index).SetCellValue(ConfigFieldMetaData.None);
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldComments).CreateCell(index).SetCellValue(ConfigFieldMetaData.None);
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldTypes).CreateCell(index).SetCellValue(ConfigFieldMetaData.None);
                sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldIsList).CreateCell(index).SetCellValue(ConfigFieldMetaData.None);
            }
        }
        public void Save()
        {
            using (FileStream fs = new FileStream(m_fileExcel, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workBook.Write(fs);
            }
        }
        private string m_fileExcel;

        public XSSFWorkbook workBook = new XSSFWorkbook();

    }
}
