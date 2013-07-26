using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    public abstract class SerializerTestBase<T> where T : IHtmlMicrodataSerializer, new()
    {
        protected T serializer;
        protected SerializationContext context;

        [SetUp]
        public void SetUp()
        {
            serializer = new T();
            context = new SerializationContext(serializer, new CamelCasePropNameProvider());
        }
    }
}