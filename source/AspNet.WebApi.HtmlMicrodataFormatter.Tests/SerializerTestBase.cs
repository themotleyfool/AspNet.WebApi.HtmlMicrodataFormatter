using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    public abstract class SerializerTestBase<T> where T : IHtmlMicrodataSerializer, new()
    {
        protected T serializer;
        protected SerializationContext context;
        protected HttpRequestMessage request;
        protected HttpConfiguration configuration;

        [SetUp]
        public void SetUp()
        {
            configuration = new HttpConfiguration(new HttpRouteCollection("/"));

            serializer = new T();
            request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute("doc"));
            context = new SerializationContext(serializer, new CamelCasePropNameProvider(), request, "Fake Title");
        }
    }
}