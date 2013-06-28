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
            if (obj == null) yield break;

            var uri = (Uri) obj;

            var href = uri.IsAbsoluteUri
                           ? uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped)
                           : Uri.EscapeUriString(uri.ToString());

            if (string.IsNullOrWhiteSpace(href))
            {
                yield break;
            }

            var element = new XElement("a",
                                       new XAttribute("href", href),
                                       new XText(uri.ToString()));

            SetPropertyName(element, propertyName, context);

            yield return element;
        }
    }
}