using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Formats arbitrary objects as html5 documents using microdata attributes
    /// to annotate properties and values.
    /// </summary>
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
        public readonly TypeDocumentationSerializer TypeDocumentationSerializer = new TypeDocumentationSerializer();

        /// <summary>
        /// Default constructor. Registers pre-defined <see cref="IHtmlMicrodataSerializer"/>
        /// instances.
        /// </summary>
        public HtmlMicrodataFormatter()
        {
            serializerRegistry.Register(UriSerializer);
            serializerRegistry.Register(LinkSerializer);
            serializerRegistry.Register(DateTimeSerializer);
            serializerRegistry.Register(TimeSpanSerializer);
            serializerRegistry.Register(ToStringSerializer);
            serializerRegistry.Register(ApiDocumentationSerializer);
            serializerRegistry.Register(ApiDescriptionSerializer);
            serializerRegistry.Register(TypeDocumentationSerializer);

            PropNameProvider = new CamelCasePropNameProvider();
        }

        /// <summary>
        /// Adds a serializer instance that provides custom rendering
        /// for a given type or list of types.
        /// </summary>
        public void RegisterSerializer(IHtmlMicrodataSerializer serializer)
        {
            serializerRegistry.Register(serializer);
        }

        /// <summary>
        /// Removes a previously registered serializer instance.
        /// Pre-defined serializers may be removed if you wish
        /// to replace with your own implementation, as in
        /// <c>htmlMicrodataFormatter.RemoveSerializer(htmlMicrodataFormatter.LinkSerializer);</c>
        /// </summary>
        public void RemoveSerializer(IHtmlMicrodataSerializer serializer)
        {
            serializerRegistry.Remove(serializer);
        }

        /// <summary>
        /// Returns <c>true</c> for all types.
        /// </summary>
        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override IEnumerable<XObject> BuildBody(object value, HttpRequestMessage request)
        {
            var context = new SerializationContext(this, PropNameProvider, request);
            return Serialize(null, value, context);
        }

        /// <summary>
        /// Controls how property names are converted to <c>itemprop</c> attribute values.
        /// Uses <see cref="CamelCasePropNameProvider"/> by default.
        /// </summary>
        public IPropNameProvider PropNameProvider { get; set; }

        /// <summary>
        /// Empty list. This <see cref="IHtmlMicrodataSerializer"/> is the root
        /// serializer and delegates all serialization to other instances found
        /// in <see cref="serializerRegistry"/>.
        /// </summary>
        public IEnumerable<Type> SupportedTypes { get { return Enumerable.Empty<Type>(); } }

        /// <summary>
        /// Finds a suitable <see cref="IHtmlMicrodataSerializer"/> in
        /// <see cref="serializerRegistry"/> and delegates to it.
        /// </summary>
        public IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var type = obj == null ? typeof (string) : obj.GetType();
            var serializer = serializerRegistry.GetSerializer(type);

            return serializer.Serialize(propertyName, obj, context);
        }
    }
}