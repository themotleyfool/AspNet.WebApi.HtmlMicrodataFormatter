using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class ToStringSerializer : DefaultSerializer
    {
        private readonly IEnumerable<Type> supportedTypes;

        public ToStringSerializer(params Type[] supportedTypes)
        {
            this.supportedTypes = supportedTypes;
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return supportedTypes; }
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            var elem = new XElement("span", obj.ToString());

            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                elem.SetAttributeValue("itemprop", propertyName);
            }

            return new[] { elem };
        }
    }
}