﻿using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigDataExpoter
{
    /// <summary>
    /// 代码解析类
    /// </summary>
    class CodeParser : ExcelParserBase
    {
        /// <summary>
        /// 解析一个目录下所有Excel
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public Dictionary<string, List<ConfigSheetData>> ParseAllMetaData(string directory)
        {
            var sheetsDataDict = new Dictionary<string, List<ConfigSheetData>>();
            var files = Directory.GetFiles(directory, "*.xlsx", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                sheetsDataDict[file] = ParseMetaData(file);
            }
            return sheetsDataDict;
        }

        /// <summary>
        /// 解析一个Excel
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<ConfigSheetData> ParseMetaData(string path)
        {
            var sheetDatas = new List<ConfigSheetData>();
            var sheets = GetSheets(path);
            if (sheets.Count <= 0)
            {
                return sheetDatas;
            }

            foreach (var sheet in sheets)
            {
                var sheetData = ParseSheet(sheet);
                sheetData.m_filePath = path;
                sheetData.m_fileName = Path.GetFileNameWithoutExtension(path);
                sheetData.m_sheetName = sheet.SheetName;
                sheetDatas.Add(sheetData);
            }
            return sheetDatas;
        }

        /// <summary>
        /// 解析表格数据
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private ConfigSheetData ParseSheet(ISheet sheet)
        {
            int rowOffset;
            var sheetType = ParseSheetType(sheet, out rowOffset);
            switch (sheetType)
            {
                case SheetType.Enum:
                    {
                        var enumMetaData = ParseEnumHeader(sheet);
                        ParseEnumValues(sheet, enumMetaData);
                        return new ConfigSheetData()
                        {
                            m_sheetType = sheetType,
                            m_configMetaData = enumMetaData
                        };
                    }
                case SheetType.Class:
                    {
                        var classMetaData = ParseClassHeader(sheet);
                        ParseClassFields(sheet, classMetaData);
                        return new ConfigSheetData()
                        {
                            m_sheetType = sheetType,
                            m_configMetaData = classMetaData
                        };
                    }
            }
            return new ConfigSheetData()
            {
                m_sheetType = SheetType.Invalid,
            };

        }

        /// <summary>
        /// 解析表类型
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowOffset"></param>
        /// <returns></returns>
        private SheetType ParseSheetType(ISheet sheet, out int rowOffset)
        {
            rowOffset = 1;
            var row = sheet.GetRow(ConfigSheetData.TypeRowIndex);
            var cell = row.GetCell(0);
            var sheetType = cell.StringCellValue;

            if (Enum.TryParse<SheetType>(sheetType, true, out var result))
            {
                return result;
            }
            else
            {
                throw new ParseExcelException($"解析表类型失败,Sheet:{sheet.SheetName}");
            }
        }

        /// <summary>
        /// 解析表头
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="beginOffset"></param>
        /// <param name="rowOffset"></param>
        /// <returns></returns>
        private ConfigEnumMetaData ParseEnumHeader(ISheet sheet)
        {
            ConfigEnumMetaData enumMetaData = new ConfigEnumMetaData();

            var row = sheet.GetRow(ConfigSheetData.EnumHeaderIndex);
            var nameCell = row.GetCell(ConfigSheetData.EnumNameCellIndex);
            var commentCell = row.GetCell(ConfigSheetData.EnumCommentCellIndex);
            var visiblityCell = row.GetCell(ConfigSheetData.EnumVisiblityCellIndex);

            enumMetaData.m_name = nameCell.StringCellValue;
            enumMetaData.m_comment = commentCell.StringCellValue;
            var visiblity = enumMetaData.ParseEnum(visiblityCell.StringCellValue, Visiblity.Invalid);
            if (visiblity != Visiblity.Invalid)
            {
                enumMetaData.m_visiblity = visiblity;
            }
            else
            {
                throw new ParseExcelException($"可见性解析失败，Sheet:{sheet.SheetName}");
            }
            return enumMetaData;
        }

        private void ParseEnumValues(ISheet sheet, ConfigEnumMetaData enumMetaData)
        {
            for (int i = ConfigSheetData.EnumBodyIndex; i <= sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                var idCell = row.GetCell(ConfigSheetData.EnumFlagIDCellIndex);
                var nameCell = row.GetCell(ConfigSheetData.EnumFlagNameCellIndex);
                var commentCell = row.GetCell(ConfigSheetData.EnumFlagCommentCellIndex);
                if (idCell == null)
                {
                    throw new ParseExcelException("请填写Enum行数据");
                }

                var id = (int)idCell.NumericCellValue;

                if (!enumMetaData.m_enumNameValue.TryGetValue(id, out var flagData))
                {
                    if (nameCell != null && !string.IsNullOrEmpty(nameCell.StringCellValue))
                    {
                        // 同名检测
                        foreach (var enumData in enumMetaData.m_enumNameValue.Values)
                        {
                            if (enumData.m_name.Equals(nameCell.StringCellValue))
                            {
                                throw new ParseExcelException($"Enum 不能有同名枚举 {nameCell.StringCellValue}");
                            }
                        }

                        flagData = new ConfigEnumMetaData.EnumData()
                        {
                            m_ID = id,
                            m_name = nameCell.StringCellValue,
                            m_comment = commentCell == null ? "" : commentCell.StringCellValue // 可空
                        };
                        enumMetaData.m_enumNameValue[id] = flagData;
                    }
                    else
                    {
                        throw new ParseExcelException("Enum Name不能为Empty Or Null");
                    }
                }
                else
                {
                    throw new ParseExcelException("Enum ID重复");
                }
            }
        }

        /// <summary>
        /// 解析类头
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private ConfigClassMetaData ParseClassHeader(ISheet sheet)
        {
            var classMetaData = new ConfigClassMetaData();
            var headerRow = sheet.GetRow(ConfigSheetData.ClassHeaderIndex);
            var nameCell = headerRow.GetCell(ConfigSheetData.ClassNameCellIndex);
            var commentCell = headerRow.GetCell(ConfigSheetData.ClassCommentCellIndex);
            if (nameCell != null && !string.IsNullOrEmpty(nameCell.StringCellValue))
            {
                classMetaData.m_classname = nameCell.StringCellValue;
                classMetaData.m_comment = commentCell == null ? "" : commentCell.StringCellValue;
            }
            else
            {
                throw new ParseExcelException($"类名异常,Sheet:{sheet.SheetName}");
            }
            return classMetaData;
        }

        /// <summary>
        /// 解析类的域名
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="classMetaData"></param>
        /// <returns></returns>
        private void ParseClassFields(ISheet sheet, ConfigClassMetaData classMetaData)
        {
            var fieldsDict = new Dictionary<string, ConfigFieldMetaData>();
            var fieldNameRow = sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldName);
            var fieldTypeRow = sheet.GetRow((int)ConfigClassFieldHeader.ClassFieldDataType);
            // 排除了特殊处理的列表
            var headers = (Enum.GetValues(typeof(ConfigClassFieldHeader)) as ConfigClassFieldHeader[]).Except(new ConfigClassFieldHeader[]{ConfigClassFieldHeader.ClassFieldName,
                                ConfigClassFieldHeader.ClassFieldDataType ,
                                ConfigClassFieldHeader.ClassNestedClassFieldComments ,
                                ConfigClassFieldHeader.ClassNestedClassFieldIsList ,
                                ConfigClassFieldHeader.ClassNestedClassFieldNames ,
                                ConfigClassFieldHeader.ClassNestedClassFieldTypes});

            // 首先获得域名
            for (int i = 0; i < fieldNameRow.LastCellNum; ++i)
            {
                var fieldNameCell = fieldNameRow.GetCell(i);
                var fieldTypeCell = fieldTypeRow.GetCell(i);
                var tempFieldDataType = classMetaData.ParseEnum(fieldTypeCell.StringCellValue, DataType.None);

                if (fieldNameCell != null && !string.IsNullOrEmpty(fieldNameCell.StringCellValue))
                {
                    if (!fieldsDict.ContainsKey(fieldNameCell.StringCellValue))
                    {
                        var fieldMetaData = new ConfigFieldMetaData()
                        {
                            BelongClassName = classMetaData.m_classname,
                            FieldName = fieldNameCell.StringCellValue.ToLower(),
                        };
                        fieldsDict[fieldMetaData.FieldName] = fieldMetaData;
                        // 解析内嵌类，如果不是内嵌类，则这四个单元格的数据应当也是None的
                        var nestedClassNamesCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldNames).GetCell(i);
                        var nestedClassTypesCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldTypes).GetCell(i);
                        var nestedClassCommentsCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldComments).GetCell(i);
                        var nestedClassIsListCell = sheet.GetRow((int)ConfigClassFieldHeader.ClassNestedClassFieldIsList).GetCell(i);

                        if (nestedClassNamesCell != null && !string.IsNullOrEmpty(nestedClassNamesCell.StringCellValue) &&
                            nestedClassTypesCell != null && !string.IsNullOrEmpty(nestedClassTypesCell.StringCellValue) &&
                            nestedClassCommentsCell != null && !string.IsNullOrEmpty(nestedClassCommentsCell.StringCellValue) &&
                            nestedClassIsListCell != null && !string.IsNullOrEmpty(nestedClassIsListCell.StringCellValue))
                        {
                            bool isNestedClass = fieldMetaData.ParseNestedClass(tempFieldDataType, nestedClassNamesCell.StringCellValue, nestedClassTypesCell.StringCellValue,
                                nestedClassCommentsCell.StringCellValue, nestedClassIsListCell.StringCellValue);
                            if (isNestedClass)
                            {
                                fieldMetaData.DataType = DataType.NestedClass;
                                fieldMetaData.m_nestedClassMetaData.m_classname = fieldTypeCell.StringCellValue;
                                //fieldMetaData.m_nestedClassMetaData.m_comment = fieldMetaData.m_comment;
                            }
                            else
                            {
                                fieldMetaData.DataType = tempFieldDataType;
                            }
                        }
                        else
                        {
                            throw new ParseExcelException($"内嵌类配置错误，请检查[{fieldNameCell.StringCellValue}]");
                        }
                        // 通常处理
                        foreach (var headerInfo in headers)
                        {
                            ConfigClassFieldHeader header = headerInfo;
                            int rowIndex = (int)headerInfo;
                            var row = sheet.GetRow(rowIndex);
                            var headerCell = row.GetCell(i);
                            // 枚举或字符串
                            if (!fieldMetaData.SetValue(header, headerCell.StringCellValue))
                            {
                                throw new ParseExcelException($"域信息解析异常，域名:{fieldNameCell.StringCellValue}，域信息类型：{header}");
                            }
                        }
                        // 域的实际类名
                        fieldMetaData.RealTypeName = ConfigFieldMetaData.GetTypeName(fieldMetaData, fieldMetaData.DataType, fieldMetaData.ListType);
                        // 表中第几列
                        fieldMetaData.m_columnIndex = i;
                    }
                    else
                    {
                        throw new ParseExcelException($"一个配置类里不能有重名的字段名,域名:{fieldNameCell.StringCellValue}");
                    }
                }
                else
                {
                    throw new ParseExcelException($"域名不得为空，Sheet:{sheet.SheetName}");
                }
            }

            var fieldList = fieldsDict.Values.ToList();
            fieldList.Sort((a, b) =>
            {
                if (a.FieldName.Length != b.FieldName.Length)
                {
                    return a.FieldName.Length.CompareTo(b.FieldName.Length);
                }
                return a.FieldName.CompareTo(b.FieldName);
            });
            classMetaData.m_fieldsInfo.Clear();
            classMetaData.m_fieldsInfo.AddRange(fieldList);
        }
    }
}
