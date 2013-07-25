using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class HtmlMicrodataFormatter : HtmlMediaTypeFormatter, IHtmlMicrodataSerializer
    {
        private readonly SerializerRegistry serializerRegistry = new SerializerRegistry();

        public readonly UriSerializer UriSerializer = new UriSerializer();
        public readonly LinkSerializer LinkSerializer = new LinkSerializer();
        public readonly DateTimeSerializer DateTimeSerializer = new DateTimeSerializer();
        public readonly TimeSpanSerializer TimeSpanSerializer = new TimeSpanSerializer();
        public readonly ToStringSerializer ToStringSerializer = new ToStringSerializer();
        public readonly ApiDocumentationSerializer ApiDocumentationSerializer = new ApiDocumentationSerializer();
        public readonly ApiDescriptionSerializer ApiDescriptionSerializer = new ApiDescriptionSerializer();

        public HtmlMicrodataFormatter()
        {
            serializerRegistry.Register(UriSerializer);
            serializerRegistry.Register(LinkSerializer);
            serializerRegistry.Register(DateTimeSerializer);
            serializerRegistry.Register(TimeSpanSerializer);
            serializerRegistry.Register(ToStringSerializer);
            serializerRegistry.Register(ApiDocumentationSerializer);
            serializerRegistry.Register(ApiDescriptionSerializer);

            PropNameProvider = new CamelCasePropNameProvider();
        }

        public void RegisterSerializer(IHtmlMicrodataSerializer serializer)
        {
            serializerRegistry.Register(serializer);
        }

        public void RemoveSerializer(IHtmlMicrodataSerializer serializer)
        {
            serializerRegistry.Remove(serializer);
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