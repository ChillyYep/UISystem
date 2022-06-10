using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;

namespace ConfigDataExpoter
{
    class CodeExpoter : FileExporter
    {
        public void Setup(IEnumerable<ConfigSheetData> configSheetDatas)
        {
            m_configSheetDatas = configSheetDatas;
        }

        public void ExportConfigCode(string filePath)
        {
            // 生成代码
            string code = CodeNamespace;
            StringBuilder classList = new StringBuilder();
            foreach (var sheetData in m_configSheetDatas)
            {
                if (sheetData.m_sheetType == SheetType.Enum && sheetData.m_configMetaData is ConfigEnumMetaData enumMetaData)
                {
                    var enumCode = CreateEnumCode(enumMetaData);
                    classList.AppendLine(enumCode);
                }
                else if (sheetData.m_sheetType == SheetType.Class && sheetData.m_configMetaData is ConfigClassMetaData classMetaData)
                {
                    var classCode = CreateClassCode(classMetaData);
                    classList.AppendLine(classCode);
                }
            }
            if (classList.Length > 0)
            {
                var allClassCode = classList.ToString();
                code = code.Replace(CLASS_LIST, allClassCode);
            }
            else
            {
                code = string.Empty;
            }
            ExportFile(filePath, code, false);
        }

        public void ExportTypeEnumCode(string filePath)
        {
            string code = CodeNamespace;
            var typeEnumMetaData = new ConfigEnumMetaData()
            {
                m_name = "BinaryDataType",
                m_comment = "配置类枚举",
                m_visiblity = Visiblity.Both
            };
            var enumKeyValues = typeEnumMetaData.m_enumNameValue;
            int count = 0;
            for (int i = (int)DataType.Int8; i < (int)DataType.Text; ++i)
            {
                enumKeyValues[count++] = new ConfigEnumMetaData.EnumData()
                {
                    m_ID = count,
                    m_comment = "基础类型",
                    m_name = ((DataType)i).ToString()
                };
            }
            // 新生成的类
            foreach (var sheetData in m_configSheetDatas)
            {
                if (sheetData.m_sheetType == SheetType.Enum && sheetData.m_configMetaData is ConfigEnumMetaData enumMetaData)
                {
                    enumKeyValues[count++] = new ConfigEnumMetaData.EnumData()
                    {
                        m_ID = count,
                        m_comment = enumMetaData.m_comment,
                        m_name = enumMetaData.m_name
                    };
                }
                else if (sheetData.m_sheetType == SheetType.Class && sheetData.m_configMetaData is ConfigClassMetaData classMetaData)
                {
                    enumKeyValues[count++] = new ConfigEnumMetaData.EnumData()
                    {
                        m_ID = count,
                        m_comment = classMetaData.m_comment,
                        m_name = classMetaData.m_classname
                    };
                    var nestedClasses = classMetaData.m_fieldsInfo.Where(fieldInfo => fieldInfo.DataType == DataType.NestedClass);
                    foreach (var nestedClass in nestedClasses)
                    {
                        enumKeyValues[count++] = new ConfigEnumMetaData.EnumData()
                        {
                            m_ID = count,
                            m_comment = "",
                            m_name = classMetaData.m_classname + "_" + nestedClass.m_nestedClassMetaData.m_classname
                        };
                    }
                }
            }
            if (typeEnumMetaData.m_enumNameValue.Count > 0)
            {
                var enumCode = CreateEnumCode(typeEnumMetaData);
                code = code.Replace(CLASS_LIST, enumCode);
            }
            else
            {
                code = string.Empty;
            }
            ExportFile(filePath, code, false);

        }

        public void CopyBinaryTools(string srcDirectory, string dstDirectory)
        {
            if (!Directory.Exists(dstDirectory))
            {
                Directory.CreateDirectory(dstDirectory);
            }
            var srcFiles = Directory.GetFiles(srcDirectory);
            foreach (var file in srcFiles)
            {
                File.Copy(file, Path.Combine(dstDirectory, Path.GetFileName(file)), true);
            }
        }

        public Assembly Compile(string directory)
        {
            var files = Directory.GetFiles(directory);
            var codeProvider = CodeDomProvider.CreateProvider("CSharp");

            string[] codes = new string[files.Length];
            for (int i = 0; i < files.Length; ++i)
            {
                codes[i] = File.ReadAllText(files[i]);
            }
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = "ConfigData.dll";
            parameters.GenerateInMemory = true;
            parameters.IncludeDebugInformation = false;
            parameters.TreatWarningsAsErrors = true;
            //AppDomain.CurrentDomain.GetAssemblies()
            parameters.ReferencedAssemblies.Add("BinaryReaderWriter.dll");

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, codes);
            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var err in results.Errors)
                {
                    sb.AppendLine(err.ToString());
                }
                throw new ParseExcelException($"编译ConfigData代码出错，出错信息:{sb.ToString()}");
            }
            return results.CompiledAssembly;
        }

        private string CreateEnumCode(ConfigEnumMetaData enumMetaData)
        {
            StringBuilder codeSB = new StringBuilder();
            string enumCode = CodeEnum;
            // 1、注释
            if (!string.IsNullOrEmpty(enumMetaData.m_comment))
            {
                enumCode = enumCode.Replace(COMMENT, enumMetaData.m_comment);
            }
            else
            {
                enumCode = enumCode.Replace(COMMENT, "");
            }
            // 2、枚举类型名
            enumCode = enumCode.Replace(ENUM_NAME, enumMetaData.m_name);
            // 3、枚举字段
            foreach (var equality in enumMetaData.m_enumNameValue)
            {
                // 字段注释
                string withComment = CodeEnumEquality.Replace(COMMENT, string.IsNullOrEmpty(equality.Value.m_comment) ? "" : equality.Value.m_comment);
                // 等式
                codeSB.AppendLine(string.Format(withComment, equality.Value.m_name, equality.Value.m_ID));
            }
            enumCode = enumCode.Replace(ENUM_EQUALITY_LIST, codeSB.ToString());
            return enumCode;
        }

        private string CreateClassCode(ConfigClassMetaData classMetaData)
        {
            string className = classMetaData.m_classname;
            string comment = classMetaData.m_comment;
            List<ConfigFieldMetaData> fieldsInfo = classMetaData.m_fieldsInfo;
            StringBuilder codeSB = new StringBuilder();
            string classCode = CodeClass;
            // 1、注释
            if (!string.IsNullOrEmpty(comment))
            {
                classCode = classCode.Replace(COMMENT, comment);
            }
            else
            {
                classCode = classCode.Replace(COMMENT, "");
            }

            // 2、类型名
            classCode = classCode.Replace(CLASS_NAME, className);

            // 3、构造函数
            classCode = classCode.Replace(CONSTRUCTOR_PARAMS, string.Join(", ", fieldsInfo.Select(fieldInfo =>
              {
                  return string.Format(CodeParam, fieldInfo.RealTypeName, fieldInfo.FieldName);
              })));
            var codeEquality = "\t\t\t" + CodeEquality;
            classCode = classCode.Replace(EQAULITY_LIST, string.Join(Environment.NewLine, fieldsInfo.Select(fieldInfo =>
              {
                  return string.Format(codeEquality, fieldInfo.PrivateFieldName, fieldInfo.FieldName);
              })));

            // 4、内嵌类
            StringBuilder nestedClass = new StringBuilder();
            foreach (var fieldInfo in fieldsInfo)
            {
                if (fieldInfo.DataType == DataType.NestedClass && fieldInfo.m_nestedClassMetaData.FieldNum > 0)
                {
                    var nestedClassCode = CreateNestedCode(fieldInfo.m_nestedClassMetaData.m_classname,
                        fieldInfo.m_nestedClassMetaData.m_comment, fieldInfo.m_nestedClassMetaData.m_fieldsInfo);
                    nestedClass.Append(nestedClassCode);
                }
            }
            classCode = classCode.Replace(NESTEDCLASS_LIST, nestedClass.ToString());
            // 5、方法
            classCode = classCode.Replace(SERIALIZE_FUNC, CreateSerializeFunc(fieldsInfo, CodeSerializeFunc, "\t\t\t" + WriteCodeFormat));
            classCode = classCode.Replace(DESERIALIZE_FUNC, CreateDeserializeFunc(fieldsInfo, CodeDeserializeFunc, "\t\t\t" + ReadCodeFormat));

            // 6、属性列表
            foreach (var fieldInfo in fieldsInfo)
            {
                // 字段注释
                string withComment = CodeProperty.Replace(COMMENT, string.IsNullOrEmpty(fieldInfo.Comment) ? "" : fieldInfo.Comment);
                // 替换类型名和变量名
                withComment = withComment.Replace(TYPE_NAME, fieldInfo.RealTypeName).Replace(VARAINT_NAME, fieldInfo.FieldName);
                codeSB.AppendLine(withComment);
            }
            classCode = classCode.Replace(PROPERTY_LIST, codeSB.ToString());
            return classCode;
        }

        private string CreateNestedCode(string className, string comment, IEnumerable<ConfigFieldMetaData.NestClassFieldInfo> fieldsInfo)
        {
            StringBuilder codeSB = new StringBuilder();
            string classCode = CodeNestedClass;
            // 1、注释
            if (!string.IsNullOrEmpty(comment))
            {
                classCode = classCode.Replace(COMMENT, comment);
            }
            else
            {
                classCode = classCode.Replace(COMMENT, "");
            }

            // 2、类型名
            classCode = classCode.Replace(CLASS_NAME, className);

            // 3、构造函数
            classCode = classCode.Replace(CONSTRUCTOR_PARAMS, string.Join(", ", fieldsInfo.Select(fieldInfo =>
              {
                  return string.Format(CodeParam, fieldInfo.RealTypeName, fieldInfo.FieldName);
              })));
            string codeEquality = "\t\t\t\t" + CodeEquality;
            classCode = classCode.Replace(EQAULITY_LIST, string.Join(Environment.NewLine, fieldsInfo.Select(fieldInfo =>
              {
                  return string.Format(codeEquality, fieldInfo.PrivateFieldName, fieldInfo.FieldName);
              })));

            // 4、方法
            classCode = classCode.Replace(SERIALIZE_FUNC, CreateSerializeFunc(fieldsInfo, CodeNestedClassSerializeFunc, "\t\t\t\t" + WriteCodeFormat));
            classCode = classCode.Replace(DESERIALIZE_FUNC, CreateDeserializeFunc(fieldsInfo, CodeNestedClassDeserializeFunc, "\t\t\t\t" + ReadCodeFormat));

            // 5、属性列表
            foreach (var fieldInfo in fieldsInfo)
            {
                // 字段注释
                string withComment = CodeNestedClassProperty.Replace(COMMENT, string.IsNullOrEmpty(fieldInfo.Comment) ? "" : fieldInfo.Comment);
                // 替换类型名和变量名
                withComment = withComment.Replace(TYPE_NAME, fieldInfo.RealTypeName).Replace(VARAINT_NAME, fieldInfo.FieldName);
                codeSB.AppendLine(withComment);
            }
            classCode = classCode.Replace(PROPERTY_LIST, codeSB.ToString());
            return classCode;
        }

        private string CreateSerializeFunc(IEnumerable<ConfigFieldMetaDataBase> fieldsInfo, string serializeFuncCode, string writeCodeFormat)
        {
            StringBuilder sb = new StringBuilder();
            int count = fieldsInfo.Count();
            foreach (var fieldInfo in fieldsInfo)
            {
                --count;
                if (ConfigFieldMetaData.GetListType(fieldInfo.ListType) == ListType.None)
                {
                    var classType = ConfigFieldMetaData.GetTypeName(fieldInfo, fieldInfo.DataType, ConfigFieldMetaData.None/*, fieldInfo.m_dataType == DataType.NestedClass*/);
                    if (fieldInfo.DataType >= DataType.Int8 && fieldInfo.DataType <= DataType.Text)
                    {
                        sb.Append(string.Format(writeCodeFormat, classType, fieldInfo.PrivateFieldName));
                    }
                    else if (fieldInfo.DataType == DataType.Enum)
                    {
                        //var enumTypeName = ConfigFieldMetaData.GetTypeName(fieldInfo, DataType.Enum, fieldInfo.m_listType);
                        sb.Append(string.Format(writeCodeFormat, "Enum", $"(Int32){fieldInfo.PrivateFieldName}"));
                    }
                    else if (fieldInfo.DataType == DataType.NestedClass)
                    {
                        //var nestedClassTypeName = ConfigFieldMetaData.GetTypeName(fieldInfo, DataType.NestedClass, fieldInfo.m_listType, true);
                        sb.Append(string.Format(writeCodeFormat, $"Object<{classType}>", fieldInfo.PrivateFieldName));
                    }
                }
                else
                {
                    var classType = ConfigFieldMetaData.GetTypeName(fieldInfo, fieldInfo.DataType, ConfigFieldMetaData.None/*, fieldInfo.m_dataType == DataType.NestedClass*/);
                    if (fieldInfo.DataType >= DataType.Int8 && fieldInfo.DataType <= DataType.Text)
                    {
                        sb.Append(string.Format(writeCodeFormat, $"{classType}List", fieldInfo.PrivateFieldName));
                    }
                    else if (fieldInfo.DataType == DataType.Enum)
                    {
                        throw new ParseExcelException("不支持枚举数组");
                    }
                    else if (fieldInfo.DataType == DataType.NestedClass)
                    {
                        //var nestedClassTypeName = ConfigFieldMetaData.GetTypeName(fieldInfo, DataType.NestedClass, ConfigFieldMetaData.None, true);
                        sb.Append(string.Format(writeCodeFormat, $"ObjectList<{classType}>", fieldInfo.PrivateFieldName));
                    }
                }
                if (count > 0)
                {
                    sb.AppendLine();
                }
            }
            serializeFuncCode = serializeFuncCode.Replace(SERIALIZE_EQUALITY, sb.ToString());
            return serializeFuncCode;
        }

        private string CreateDeserializeFunc(IEnumerable<ConfigFieldMetaDataBase> fieldsInfo, string deSerializeFuncCode, string readCodeFormat)
        {
            StringBuilder sb = new StringBuilder();
            int count = fieldsInfo.Count();
            foreach (var fieldInfo in fieldsInfo)
            {
                --count;
                if (ConfigFieldMetaData.GetListType(fieldInfo.ListType) == ListType.None)
                {
                    var classType = ConfigFieldMetaData.GetTypeName(fieldInfo, fieldInfo.DataType, ConfigFieldMetaData.None/*, fieldInfo.m_dataType == DataType.NestedClass*/);
                    if (fieldInfo.DataType >= DataType.Int8 && fieldInfo.DataType <= DataType.Text)
                    {
                        sb.Append(string.Format(readCodeFormat, fieldInfo.PrivateFieldName, "", classType));
                    }
                    else if (fieldInfo.DataType == DataType.Enum)
                    {
                        sb.Append(string.Format(readCodeFormat, fieldInfo.PrivateFieldName, $"({classType})", "Enum"));
                    }
                    else if (fieldInfo.DataType == DataType.NestedClass)
                    {
                        sb.Append(string.Format(readCodeFormat, fieldInfo.PrivateFieldName, "", $"Object<{classType}>"));
                    }
                }
                else
                {
                    var classType = ConfigFieldMetaData.GetTypeName(fieldInfo, fieldInfo.DataType, ConfigFieldMetaData.None/*, fieldInfo.m_dataType == DataType.NestedClass*/);
                    if (fieldInfo.DataType >= DataType.Int8 && fieldInfo.DataType <= DataType.Text)
                    {
                        sb.Append(string.Format(readCodeFormat, fieldInfo.PrivateFieldName, "", $"{classType}List"));
                    }
                    else if (fieldInfo.DataType == DataType.Enum)
                    {
                        throw new ParseExcelException("不支持枚举数组");
                    }
                    else if (fieldInfo.DataType == DataType.NestedClass)
                    {
                        sb.Append(string.Format(readCodeFormat, fieldInfo.PrivateFieldName, "", $"ObjectList<{classType}>"));
                    }
                }
                if (count > 0)
                {
                    sb.AppendLine();
                }
            }
            deSerializeFuncCode = deSerializeFuncCode.Replace(DESERIALIZE_EQUALITY, sb.ToString());
            return deSerializeFuncCode;
        }

        private const string CLASS_LIST = "#CLASS_LIST#";
        private const string CLASS_NAME = "#CLASS_NAME#";
        private const string ENUM_NAME = "#ENUM_NAME#";
        private const string ENUM_EQUALITY_LIST = "#ENUM_EQUALITY_LIST#";
        private const string TYPE_NAME = "#TYPE_NAME#";
        private const string VARAINT_NAME = "#VARAINT_NAME#";
        private const string CONSTRUCTOR_PARAMS = "#CONSTRUCTOR_PARAMS#";
        private const string EQAULITY_LIST = "#EQAULITY_LIST#";
        private const string EQUALITY = "#EQUALITY#";
        private const string PROPERTY_LIST = "#PROPERTY_LIST#";
        private const string COMMENT = "#COMMENT#";
        private const string DESERIALIZE_FUNC = "#DESERIALIZE_FUNC#";
        private const string SERIALIZE_FUNC = "#SERIALIZE_FUNC#";
        private const string NESTEDCLASS_LIST = "#NESTEDCLASS_LIST#";
        private const string DESERIALIZE_EQUALITY = "#DESERIALIZE_EQUALITY#";
        private const string SERIALIZE_EQUALITY = "#SERIALIZE_EQUALITY#";

        private const string NameSpace = "ConfigData";
        private const string ReadCodeFormat = "{0} = {1}reader.Read{2}();";
        private const string WriteCodeFormat = "writer.Write{0}({1});";
        private const string CodeParam = "{0} {1}";
        private const string CodeEquality = "this.{0} = {1};";

        private const string CodeNamespace =
@"using System;
using System.Collections.Generic;
/// <summary>
/// 程序自动生成的配置代码
/// </summary>
namespace " + NameSpace + @"
{
#CLASS_LIST#
}";

        private const string CodeClass =
@"    /// <summary>
    /// #COMMENT#
    /// </summary>
    [Serializable]
    public partial class #CLASS_NAME#: IBinarySerializer, IBinaryDeserializer
    {
#NESTEDCLASS_LIST#
        public #CLASS_NAME#()
        {
        }
        public #CLASS_NAME#(#CONSTRUCTOR_PARAMS#)
        {
#EQAULITY_LIST#
        }
#DESERIALIZE_FUNC#
#SERIALIZE_FUNC#
#PROPERTY_LIST#
    }";
        /// <summary>
        /// 只支持内嵌一层
        /// </summary>
        private const string CodeNestedClass =
@"        [Serializable]
        public class #CLASS_NAME#: IBinarySerializer, IBinaryDeserializer
        {
            public #CLASS_NAME#()
            {
            }
            public #CLASS_NAME#(#CONSTRUCTOR_PARAMS#)
            {
#EQAULITY_LIST#
            }
#DESERIALIZE_FUNC#
#SERIALIZE_FUNC#
#PROPERTY_LIST#
        }";


        private const string CodeEnumEquality =
@"        /// <summary>
        /// #COMMENT#
        /// </summary>
        {0} = {1},";

        private const string CodeProperty =
@"        private #TYPE_NAME# _#VARAINT_NAME#;
        /// <summary>
        /// #COMMENT#
        /// </summary>
        public #TYPE_NAME# #VARAINT_NAME#
        {
            get
            {
                return _#VARAINT_NAME#;
            }
            private set
            {
                _#VARAINT_NAME# = value;
            }
        }";
        private const string CodeNestedClassProperty =
@"            private #TYPE_NAME# _#VARAINT_NAME#;
            /// <summary>
            /// #COMMENT#
            /// </summary>
            public #TYPE_NAME# #VARAINT_NAME#
            {
                get
                {
                    return _#VARAINT_NAME#;
                }
                private set
                {
                    _#VARAINT_NAME# = value;
                }
            }";

        private const string CodeEnum =
@"    /// <summary>
    /// #COMMENT#
    /// </summary>
    public enum #ENUM_NAME#
    {
#ENUM_EQUALITY_LIST#
    }";

        private const string CodeDeserializeFunc =
@"        public void Deserialize(BinaryParser reader)
        {
#DESERIALIZE_EQUALITY#
        }";
        private const string CodeSerializeFunc =
@"        public void Serialize(BinaryFormatter writer)
        {
#SERIALIZE_EQUALITY#
        }";

        private const string CodeNestedClassDeserializeFunc =
@"            public void Deserialize(BinaryParser reader)
            {
#DESERIALIZE_EQUALITY#
            }";
        private const string CodeNestedClassSerializeFunc =
@"            public void Serialize(BinaryFormatter writer)
            {
#SERIALIZE_EQUALITY#
            }";

        private IEnumerable<ConfigSheetData> m_configSheetDatas;
    }
}
