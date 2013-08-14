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

        /// <summary>
        /// Creates a section tag with nested elements containing
        /// either a link or a form that points to the <paramref name="api"/>
        /// URL.
        /// </summary>
        public virtual XElement BuildApi(SimpleApiDescription api)
        {
            var children = new List<object>();

            children.Add(new XAttribute("class", "api"));
            children.Add(new XElement("h2", new XText(api.Name)));
            children.Add(new XElement("p",
                                      new XAttribute("class", "api-endpoint"),
                                      new XElement("code", new XText(api.Method + " " + api.Href))));

            if (api.Documentation != null)
            {
                children.AddRange(ParseDocumentation(api.Documentation));
            }
            
            if (api.Parameters.Any() || !string.Equals(api.Method, "get", StringComparison.InvariantCultureIgnoreCase))
            {
                children.Add(BuildForm(api));
            }
            else
            {
                children.Add(
                    new XElement("div",
                                 new XElement("a",
                                              new XAttribute("href", api.Href),
                                              new XAttribute("rel", api.Name),
                                              new XText(api.Name))));
            }

            return new XElement("section", children);
        }

        /// <summary>
        /// Build a form tag with nested labels and inputs.
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public virtual XElement BuildForm(SimpleApiDescription api)
        {
            return new XElement("form",
                                new XAttribute("name", api.Name),
                                new XAttribute("action", api.Href),
                                new XAttribute("method", api.Method),
                                new XAttribute("data-templated", api.Templated),
                                new XAttribute("data-rel", api.Name),
                                BuildFormInputs(api));
        }

        /// <summary>
        /// Converts a well-formed snippet of html into one or more <see cref="XElement"/>s.
        /// </summary>
        public static IEnumerable<XElement> ParseDocumentation(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return Enumerable.Empty<XElement>();
            }

            return XElement.Parse("<r>" + html + "</r>").Elements();
        }

        /// <summary>
        /// Creates an label tag with a nested input tag for each parameter
        /// on <paramref name="api"/>.
        /// </summary>
        public virtual IEnumerable<XNode> BuildFormInputs(SimpleApiDescription api)
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

        /// <summary>
        /// Determine which type attribute value to use for a <see cref="SimpleApiParameterDescriptor"/>.
        /// Currently this method only returns <c>"text"</c> though support for
        /// other types such as <c>"checkbox"</c>, <c>"radio"</c>, <c>"select"</c> or <c>"textarea"</c>
        /// may be added later.
        /// </summary>
        public virtual string GetHtmlInputType(SimpleApiParameterDescriptor apiParam)
        {
            return "text";
        }
    }
}