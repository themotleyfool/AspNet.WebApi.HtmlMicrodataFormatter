using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class ApiDescriptionSerializer : DefaultSerializer
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new[] { typeof(SimpleApiDescription) }; }
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            yield return BuildApi((SimpleApiDescription)obj);
        }

        private XElement BuildApi(SimpleApiDescription api)
        {
            var children = new List<object>();

            children.Add(new XAttribute("class", "api"));
            children.Add(new XElement("h2", new XText(api.Name)));
            children.Add(new XElement("p", new XText(api.Method + " " + api.Href)));

            if (api.Documentation != null)
            {
                children.AddRange(ParseDocumentation(api.Documentation));
            }
            
            if (api.Parameters.Any() || !string.Equals(api.Method, "get", StringComparison.InvariantCultureIgnoreCase))
            {
                children.Add(
                    new XElement("form",
                                 new XAttribute("action", api.Href),
                                 new XAttribute("method", api.Method),
                                 new XAttribute("data-templated", api.Templated),
                                 BuildFormInputs(api)));
            }
            else
            {
                children.Add(
                    new XElement("a",
                                 new XAttribute("href", api.Href),
                                 new XText(api.Name)));
            }

            return new XElement("section", children);
        }

        public static IEnumerable<XElement> ParseDocumentation(string html)
        {
            return XElement.Parse("<r>" + html + "</r>").Elements();
        }

        private IEnumerable<XNode> BuildFormInputs(SimpleApiDescription api)
        {
            foreach (var p in api.Parameters)
            {
                var type = GetHtmlInputType(p);
                
                yield return new XElement("label", 
                    new XText(p.Name),
                    new XElement("input",
                            new XAttribute("name", p.Name),
                            new XAttribute("type", type),
                            new XAttribute("value", p.DefaultValue ?? ""),
                            new XAttribute("data-required", !p.IsOptional),
                            new XAttribute("data-calling-convention", p.CallingConvention)));
            }

            yield return new XElement("input", new XAttribute("type", "submit"));
        }

        private string GetHtmlInputType(SimpleApiParameterDescriptor apiParam)
        {
         //   if (apiParam.ParameterType == typeof(bool))
            {
            //    return "checkbox";
            }

            return "text";
        }
    }
}