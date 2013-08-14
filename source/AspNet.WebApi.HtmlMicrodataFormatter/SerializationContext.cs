using System.Net.Http;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class SerializationContext
    {
        public SerializationContext()
        {
        }

        public SerializationContext(IHtmlMicrodataSerializer rootSerializer, IPropNameProvider propNameProvider, HttpRequestMessage request, string title)
        {
            RootSerializer = rootSerializer;
            PropNameProvider = propNameProvider;
            Request = request;
            Title = title;
        }

        public IHtmlMicrodataSerializer RootSerializer { get; set; }
        public IPropNameProvider PropNameProvider { get; set; }
        public HttpRequestMessage Request { get; set; }
        public string Title { get; set; }
    }
}