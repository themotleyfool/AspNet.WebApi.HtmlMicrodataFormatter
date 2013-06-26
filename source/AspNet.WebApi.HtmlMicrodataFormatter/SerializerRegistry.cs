using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    internal class SerializerRegistry
    {
        private readonly IDictionary<Type, IHtmlMicrodataSerializer> serializers =
            new Dictionary<Type, IHtmlMicrodataSerializer>();

        public IHtmlMicrodataSerializer DefaultSerializer { get; set; }

        public SerializerRegistry()
            :this(new DefaultSerializer())
        {
        }

        public SerializerRegistry(IHtmlMicrodataSerializer defaultSerializer)
        {
            DefaultSerializer = defaultSerializer;
        }

        public void Register(IHtmlMicrodataSerializer serializer)
        {
            foreach (var type in serializer.SupportedTypes)
            {
                serializers[type] = serializer;
            }
        }

        public IHtmlMicrodataSerializer GetSerializer(Type t)
        {
            return serializers.FirstOrDefault(kv => kv.Key.IsAssignableFrom(t)).Value ?? DefaultSerializer;
        }
    }
}