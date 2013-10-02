using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class TypeDocumentationSerializer : EntitySerializer<SimpleTypeDocumentation>
    {
        public TypeDocumentationSerializer()
        {
            Property(doc => doc.Documentation, GetDocumentation);
        }

        private XElement GetDocumentation(string propertyName, string value, SerializationContext context)
        {
            var div = new XElement("div", ApiDescriptionSerializer.ParseDocumentation(value));

            SetPropertyName(div, propertyName, context);

            return div;
        }
    }
}