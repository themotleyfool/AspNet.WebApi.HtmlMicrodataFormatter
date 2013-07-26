using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class ToStringSerializer : DefaultSerializer
    {
        private readonly List<Type> supportedTypes;

        public ToStringSerializer()
            :this(new Type[0])
        {
        }

        public ToStringSerializer(params Type[] supportedTypes)
        {
            this.supportedTypes = supportedTypes.ToList();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return supportedTypes; }
        }

        public void AddSupportedTypes(params Type[] type)
        {
            supportedTypes.AddRange(type);
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var elem = new XElement("span", obj.ToString());

            SetPropertyName(elem, propertyName, context);

            return new[] { elem };
        }
    }
}