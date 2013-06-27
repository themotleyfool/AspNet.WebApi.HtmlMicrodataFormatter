using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public interface IHtmlMicrodataSerializer
    {
        IEnumerable<Type> SupportedTypes { get; }
        IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context);
    }
}