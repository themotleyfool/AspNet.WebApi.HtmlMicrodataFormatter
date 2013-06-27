namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class SerializationContext
    {
        public SerializationContext()
        {
        }

        public SerializationContext(IHtmlMicrodataSerializer rootSerializer, IPropNameProvider propNameProvider)
        {
            RootSerializer = rootSerializer;
            PropNameProvider = propNameProvider;
        }

        public IHtmlMicrodataSerializer RootSerializer { get; set; }
        public IPropNameProvider PropNameProvider { get; set; }
    }
}