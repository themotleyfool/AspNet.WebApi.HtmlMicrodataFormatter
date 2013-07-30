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
            var binPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;

            var xmlFiles = Directory.GetFiles(binPath, "*.xml", SearchOption.TopDirectoryOnly);

            Load(xmlFiles);
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
            return GetNodeText(MemberXPathExpression, "M:" + name);
        }

        public string GetParameterDocumentation(MethodInfo method, string parameterName)
        {
            var methodName = GetMethodName(method);
            return GetNodeText(ParameterXPathExpression, "M:" + methodName, parameterName);
        }

        private string GetNodeText(string queryFormat, params object[] args)
        {
            var selectExpression = string.Format(queryFormat, args);

            return documentNavigators
                       .Select(n => n.SelectSingleNode(selectExpression))
                       .Where(r => r != null)
                       .Select(n => n.InnerXml)
                       .FirstOrDefault() ?? "";
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
