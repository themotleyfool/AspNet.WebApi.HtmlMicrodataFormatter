using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class HtmlDocumentation
    {
        private const string MemberXPathExpression = "/doc/members/member[@name='{0}']";
        private const string ParameterXPathExpression = "/doc/members/member[@name='{0}']/param[@name='{1}']";

        private static readonly Regex NullableTypeNameRegex = new Regex(@"(.*\.Nullable)" + Regex.Escape("`1[[") + "([^,]*),.*");

        private readonly IList<XPathNavigator> documentNavigators = new List<XPathNavigator>();

        public void Load()
        {
            var dllFiles = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic)
                .Select(asm => asm.Location)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToList();

            var xmlFiles = dllFiles
                .Select(ProbeAssemblyXml)
                .Where(i => i != null)
                .ToList();

            Load(xmlFiles);
        }

        private static string ProbeAssemblyXml(string assemblyPath)
        {
            var basePath = Path.Combine(Path.GetDirectoryName(assemblyPath), Path.GetFileNameWithoutExtension(assemblyPath));

            if (File.Exists(basePath + ".xml"))
            {
                return basePath + ".xml";
            }

            if (File.Exists(basePath + ".XML"))
            {
                return basePath + ".XML";
            }

            return null;
        }

        public void Load(IEnumerable<string> xmlFiles)
        {
            var transformer = new XmlCommentHtmlTransformer();
            foreach (var xmlFile in xmlFiles)
            {
                using (var stream = new FileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var doc = transformer.Transform(new XmlTextReader(stream));
                    this.documentNavigators.Add(doc);
                }
            }
        }

        public string GetTypeDocumentation(string fullTypeName)
        {
            return GetNodeText(MemberXPathExpression, "T:" + fullTypeName);
        }

        public string GetMethodDocumentation(MethodInfo method)
        {
            var name = GetMethodName(method);
            var expression = string.Format(MemberXPathExpression, "M:" + name);

            var node = GetFirstMatch(expression);

            if (node == null) return string.Empty;

            var nonParamElements = node.Select("./*[name() != 'param']").Cast<XPathNavigator>().Select(n => n.InnerXml);
            return string.Join("", nonParamElements);
        }

        public string GetParameterDocumentation(MethodInfo method, string parameterName)
        {
            var methodName = GetMethodName(method);
            return GetNodeText(ParameterXPathExpression, "M:" + methodName, parameterName);
        }

        public string GetPropertyDocumentation(PropertyInfo propertyInfo)
        {
            var propertyName = string.Format("{0}.{1}", propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            return GetNodeText(MemberXPathExpression, "P:" + propertyName);
        }

        private string GetNodeText(string queryFormat, params object[] args)
        {
            var selectExpression = string.Format(queryFormat, args);

            var node = GetFirstMatch(selectExpression);
            return node != null ? node.InnerXml : string.Empty;
        }

        private XPathNavigator GetFirstMatch(string selectExpression)
        {
            return documentNavigators
                .Select(n => n.SelectSingleNode(selectExpression)).FirstOrDefault(r => r != null);
        }


        private string GetMethodName(MethodInfo method)
        {
            var name = string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name);
            var parameters = method.GetParameters();

            if (parameters.Length != 0)
            {
                var parameterTypeNames = parameters.Select(param => ProcessTypeName(param.ParameterType.FullName)).ToArray();
                name += string.Format("({0})", string.Join(",", parameterTypeNames));
            }

            return name;
        }

        private static string ProcessTypeName(string typeName)
        {
            // Handle nullable
            var result = NullableTypeNameRegex.Match(typeName);

            if (result.Success)
            {
                return string.Format("{0}{{{1}}}", result.Groups[1].Value, result.Groups[2].Value);
            }

            return typeName;
        }
    }
}
