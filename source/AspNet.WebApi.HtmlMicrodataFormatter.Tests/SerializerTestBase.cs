using System.Net.Http;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    public abstract class SerializerTestBase<T> where T : IHtmlMicrodataSerializer, new()
    {
        protected T serializer;
        protected SerializationContext context;
        protected HttpRequestMessage request;

        [SetUp]
        public void SetUp()
        {
            serializer = new T();
            request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            context = new SerializationContext(serializer, new CamelCasePropNameProvider(), request);
        }
    }
}