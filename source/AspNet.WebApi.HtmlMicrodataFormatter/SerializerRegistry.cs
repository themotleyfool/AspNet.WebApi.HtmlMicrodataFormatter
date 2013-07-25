using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    internal class SerializerRegistry
    {
        private readonly IList<IHtmlMicrodataSerializer> serializers =
            new List<IHtmlMicrodataSerializer>();

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
            serializers.Add(serializer);
        }

        public IHtmlMicrodataSerializer GetSerializer(Type type)
        {
            return serializers.FirstOrDefault(s => s.SupportedTypes.Any(t => t.IsAssignableFrom(type))) ?? DefaultSerializer;
        }

        public bool Remove(IHtmlMicrodataSerializer serializer)
        {
            return serializers.Remove(serializer);
        }
    }
}