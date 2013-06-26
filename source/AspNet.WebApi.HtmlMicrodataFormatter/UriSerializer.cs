using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class UriSerializer : IHtmlMicrodataSerializer
    {
        public IEnumerable<Type> SupportedTypes { get { return new[] {typeof(Uri)}; } }

        public IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            var uri = (Uri) obj;

            var element = new XElement("a",
                                       new XAttribute("href", uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped)),
                                       new XText(uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.Unescaped)));

            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                element.SetAttributeValue("itemprop", propertyName);
            }

            return new[] {element};
        }
    }
}