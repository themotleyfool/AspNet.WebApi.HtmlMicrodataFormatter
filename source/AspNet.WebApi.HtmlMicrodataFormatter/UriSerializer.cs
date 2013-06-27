using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class UriSerializer : DefaultSerializer
    {
        public override IEnumerable<Type> SupportedTypes { get { return new[] {typeof(Uri)}; } }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var uri = (Uri) obj;

            var element = new XElement("a",
                                       new XAttribute("href", uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped)),
                                       new XText(uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.Unescaped)));

            SetPropertyName(element, propertyName, context);

            return new[] {element};
        }
    }
}