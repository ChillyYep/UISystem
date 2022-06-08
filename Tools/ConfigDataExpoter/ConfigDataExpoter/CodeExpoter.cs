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
    //public enum VisitFlag
    //{
    //    Public,
    //    Private,
    //    Protected,
    //    Default
    //}

    //public enum VisitorFlag
    //{
    //    Getter,
    //    Setter,
    //    Both
    //}

    //public class ClassDesc
    //{
    //    public string m_name;
    //    public VisitFlag m_flags;
    //    public List<FieldDesc> m_fields;
    //}

    //public class FieldDesc
    //{
    //    public string m_name;
    //    public VisitFlag m_flags;
    //    public string m_defaultValue;
    //}

    //public class PropertyDesc
    //{
    //    public string m_name;
    //    public VisitorFlag m_visitorFlag;
    //    public FieldDesc m_relativeField;
    //}

    class CodeExpoter : FileExporter
    {
        public void Setup(IEnumerable<ConfigSheetData> configSheetDatas)
        {
            m_configSheetDatas = configSheetDatas;
        }

        public void ExportCode(string filePath)
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
            ExportFile(filePath, code);
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
                enumCode = enumCode.Replace(COMMENT, COMMENT_PREFIX + enumMetaData.m_comment);
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
                string withComment = CodeEnumEquality.Replace(COMMENT, string.IsNullOrEmpty(equality.Value.m_comment) ? "" : COMMENT_PREFIX + equality.Value.m_comment);
                // 等式
                codeSB.AppendLine(string.Format(withComment, equality.Value.m_name, equality.Value.m_ID));
            }
            enumCode = enumCode.Replace(ENUM_EQUALITY_LIST, codeSB.ToString());
            return enumCode;
        }

        private string CreateClassCode(ConfigClassMetaData classMetaData)
        {
            string className = classMetaData.m_name;
            string comment = classMetaData.m_comment;
            List<ConfigFieldMetaData> fieldsInfo = classMetaData.m_fieldsInfo;
            StringBuilder codeSB = new StringBuilder();
            string classCode = CodeClass;
            // 1、注释
            if (!string.IsNullOrEmpty(comment))
            {
                classCode = classCode.Replace(COMMENT, COMMENT_PREFIX + comment);
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
                  return string.Format(CodeParam, fieldInfo.m_realTypeName, fieldInfo.m_name);
              })));
            var codeEquality = "\t\t\t" + CodeEquality;
            classCode = classCode.Replace(EQAULITY_LIST, string.Join(Environment.NewLine, fieldsInfo.Select(fieldInfo =>
              {
                  return string.Format(codeEquality, fieldInfo.m_name, string.Format(CodeParamName, fieldInfo.m_name));
              })));

            // 4、内嵌类
            StringBuilder nestedClass = new StringBuilder();
            foreach (var fieldInfo in fieldsInfo)
            {
                if (fieldInfo.m_dataType == DataType.NestedClass && fieldInfo.m_nestedClassMetaData.m_fieldsInfo.Count > 0)
                {
                    var nestedClassCode = CreateNestedCode(fieldInfo.m_nestedClassMetaData.m_className,
                        ConfigFieldMetaData.NestedClassMetaData.Comment,
                        fieldInfo.m_nestedClassMetaData.m_fieldsInfo.Values);
                    nestedClass.AppendLine(nestedClassCode);
                }
            }
            classCode = classCode.Replace(NESTEDCLASS_LIST, nestedClass.ToString());
            // 5、属性列表
            foreach (var fieldInfo in fieldsInfo)
            {
                // 字段注释
                string withComment = CodeProperty.Replace(COMMENT, string.IsNullOrEmpty(fieldInfo.m_comment) ? "" : COMMENT_PREFIX + fieldInfo.m_comment);
                // 替换类型名和变量名
                withComment = withComment.Replace(TYPE_NAME, fieldInfo.m_realTypeName).Replace(VARAINT_NAME, fieldInfo.m_name);
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
                classCode = classCode.Replace(COMMENT, COMMENT_PREFIX + comment);
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
                  return string.Format(CodeParam, fieldInfo.m_realTypeName, fieldInfo.m_fieldName);
              })));
            string codeEquality = "\t\t\t\t" + CodeEquality;
            classCode = classCode.Replace(EQAULITY_LIST, string.Join(Environment.NewLine, fieldsInfo.Select(fieldInfo =>
              {
                  return string.Format(codeEquality, fieldInfo.m_fieldName, string.Format(CodeParamName, fieldInfo.m_fieldName));
              })));

            // 4、属性列表
            foreach (var fieldInfo in fieldsInfo)
            {
                // 字段注释
                string withComment = CodeNestedClassProperty.Replace(COMMENT, string.IsNullOrEmpty(fieldInfo.m_comment) ? "" : COMMENT_PREFIX + fieldInfo.m_comment);
                // 替换类型名和变量名
                withComment = withComment.Replace(TYPE_NAME, fieldInfo.m_realTypeName).Replace(VARAINT_NAME, fieldInfo.m_fieldName);
                codeSB.AppendLine(withComment);
            }
            classCode = classCode.Replace(PROPERTY_LIST, codeSB.ToString());
            return classCode;
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
        private const string COMMENT_PREFIX = "//";
        private const string NESTEDCLASS_LIST = "#NESTEDCLASS_LIST#";
        private const string NameSpace = "ConfigData";
        private const string CodeNamespace =
@"using System;
using System.Collections.Generic;
// 程序自动生成的配置代码
namespace " + NameSpace + @"
{
#CLASS_LIST#
}";

        private const string CodeClass =
@"    #COMMENT#
    [Serializable]
    public partial class #CLASS_NAME#
    {
#NESTEDCLASS_LIST#
        public #CLASS_NAME#()
        {
        }
        public #CLASS_NAME#(#CONSTRUCTOR_PARAMS#)
        {
#EQAULITY_LIST#
        }
#PROPERTY_LIST#
    }";
        /// <summary>
        /// 只支持内嵌一层
        /// </summary>
        private const string CodeNestedClass =
    @"        #COMMENT#
        [Serializable]
        public class #CLASS_NAME#
        {
            public #CLASS_NAME#()
            {
            }
            public #CLASS_NAME#(#CONSTRUCTOR_PARAMS#)
            {
#EQAULITY_LIST#
            }
#PROPERTY_LIST#
        }";
        private const string CodeParamName = "@{0}";
        private const string CodeParam = "{0} {1}";

        private const string CodeEquality = "this.{0} = {1};";

        private const string CodeEnumEquality =
    @"        #COMMENT#
        {0} = {1},";

        private const string CodeProperty =
    @"        #COMMENT#
        
        private #TYPE_NAME# _#VARAINT_NAME#;
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
@"            #COMMENT#
            private #TYPE_NAME# _#VARAINT_NAME#;
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
    @"    #COMMENT#
    public enum #ENUM_NAME#
    {
#ENUM_EQUALITY_LIST#
    }";
        private IEnumerable<ConfigSheetData> m_configSheetDatas;
    }
}
