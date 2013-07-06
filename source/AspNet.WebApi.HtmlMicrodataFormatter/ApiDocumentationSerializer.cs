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

            foreach (var group in descriptions.Resources)
            {
                yield return new XElement("section",
                                          new XAttribute("class", "api-group"),
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