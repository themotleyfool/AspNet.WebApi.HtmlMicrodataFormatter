using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class HtmlMicrodataFormatter : HtmlMediaTypeFormatter, IHtmlMicrodataSerializer
    {
        private readonly SerializerRegistry serializerRegistry = new SerializerRegistry();

        public HtmlMicrodataFormatter()
        {
            serializerRegistry.Register(new UriSerializer());
            serializerRegistry.Register(new ToStringSerializer(typeof(Version)));
            serializerRegistry.Register(new ApiGroupSerializer());
            serializerRegistry.Register(new ApiDescriptionSerializer());

            PropNameProvider = new CamelCasePropNameProvider();
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
            var context = new SerializationContext {RootSerializer = this, PropNameProvider = PropNameProvider};
            return Serialize(null, value, context);
        }

        public IPropNameProvider PropNameProvider { get; set; }

        public IEnumerable<Type> SupportedTypes { get { return Enumerable.Empty<Type>(); } }

        public IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var type = obj == null ? typeof (string) : obj.GetType();
            var serializer = serializerRegistry.GetSerializer(type);

            return serializer.Serialize(propertyName, obj, context);
        }
    }
}