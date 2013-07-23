using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class ApiDocumentationSerializer : ApiDescriptionSerializer
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new[] { typeof(SimpleApiDocumentation) }; }
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var descriptions = (SimpleApiDocumentation)obj;

            var toc = new XElement("ul", new XAttribute("class", "nav"));

            foreach (var group in descriptions.Resources)
            {
                toc.Add(new XElement("li",
                    new XElement("a",
                        new XAttribute("href", "#" + group.Name),
                        new XText(group.Name))));
            }

            yield return toc;

            foreach (var group in descriptions.Resources)
            {
                yield return new XElement("section",
                                          new XAttribute("class", "api-group"),
                                          new XAttribute("id", group.Name),
                                          BuildApiGroupSummary(group),
                                          group.Actions.Select(api => base.Serialize(propertyName, api, context)));
            }
        }

        private XObject BuildApiGroupSummary(SimpleApiGroup apiGroup)
        {
            return new XElement("header",
                                new XElement("h1", new XText(apiGroup.Name)),
                                ParseDocumentation(apiGroup.Documentation));
        }

    }
}