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
            yield return BuildApi((SimpleApiDescription)obj, context);
        }

        /// <summary>
        /// Creates a section tag with nested elements containing
        /// either a link or a form that points to the <paramref name="api"/>
        /// URL.
        /// </summary>
        public virtual XElement BuildApi(SimpleApiDescription api, SerializationContext context)
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
                children.Add(BuildForm(api, context));
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
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual XElement BuildForm(SimpleApiDescription api, SerializationContext context)
        {
            var formName = context.FormatPropertyName(api.Name);

            return new XElement("form",
                                new XAttribute("name", formName),
                                new XAttribute("action", api.Href),
                                new XAttribute("method", api.Method),
                                new XAttribute("data-templated", api.Templated),
                                new XAttribute("data-rel", formName),
                                BuildFormInputs(api, context));
        }

        /// <summary>
        /// Converts a well-formed snippet of html into one or more <see cref="XElement"/>s.
        /// </summary>
        public static IEnumerable<XNode> ParseDocumentation(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return Enumerable.Empty<XElement>();
            }

            return XElement.Parse("<r>" + html + "</r>").Nodes();
        }

        /// <summary>
        /// Creates an label tag with a nested input tag for each parameter
        /// on <paramref name="api"/>.
        /// </summary>
        public virtual IEnumerable<XNode> BuildFormInputs(SimpleApiDescription api, SerializationContext context)
        {
            foreach (var p in api.Parameters)
            {
                var type = GetHtmlInputType(p);

                var name = context.FormatPropertyName(p.Name);

                if (!string.IsNullOrEmpty(p.Documentation))
                {
                    yield return new XElement("p", new XAttribute("class", "api-documentation"), ParseDocumentation(p.Documentation));
                }

                if (p.CallingConvention != "unknown")
                {
                    yield return new XElement("label",
                        new XText(name),
                        new XElement("input",
                            new XAttribute("name", name),
                            new XAttribute("type", type),
                            new XAttribute("value", p.DefaultValue ?? ""),
                            new XAttribute("data-required", !p.IsOptional),
                            new XAttribute("data-calling-convention", p.CallingConvention)));
                }
                else
                {
                    yield return new XElement("p", new XAttribute("class", "unknown-calling-convention"),
                        new XText("The parameter "),
                        new XElement("code", new XText(name)),
                        new XText(" cannot be sumbitted through this form because it has an unrecognized calling convention."));
                }
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