using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class HtmlMicrodataFormatter : HyperMediaHtmlMediaTypeFormatter, IHtmlMicrodataSerializer
    {
        private readonly SerializerRegistry serializerRegistry = new SerializerRegistry();

        public HtmlMicrodataFormatter()
        {
            serializerRegistry.Register(new UriSerializer());
            serializerRegistry.Register(new ToStringSerializer(typeof(Version)));
            serializerRegistry.Register(new ApiGroupSerializer());
            serializerRegistry.Register(new ApiDescriptionSerializer());
        }

        public void RegisterSerializer(IHtmlMicrodataSerializer serializer)
        {
            serializerRegistry.Register(serializer);
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override IEnumerable<XObject> BuildBody(object value)
        {
            return Serialize(null, value, this);
        }

        public IEnumerable<Type> SupportedTypes { get { throw new InvalidOperationException(); } }

        public IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            var type = obj == null ? typeof (string) : obj.GetType();
            var serializer = serializerRegistry.GetSerializer(type);

            return serializer.Serialize(propertyName, obj, rootSerializer);
        }
    }
}