using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Provides a context for a request that initiates serialization.
    /// </summary>
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

        /// <summary>
        /// Initial serializer that handles or delegates serialization
        /// to other handlers.
        /// </summary>
        public IHtmlMicrodataSerializer RootSerializer { get; set; }

        /// <summary>
        /// Controls how properties are formatted in the response,
        /// such as converting them to camel-case.
        /// </summary>
        public IPropNameProvider PropNameProvider { get; set; }

        /// <summary>
        /// The request that initiated serialization. Useful for constructing
        /// <see cref="UrlHelper"/> and accessing information about the request
        /// and hosting configuration.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Document title, as set by <see cref="HtmlMediaTypeFormatter.Title"/>.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Uses <see cref="PropNameProvider"/> to format a property name,
        /// such as converting it to camel-case.
        /// </summary>
        public string FormatPropertyName(string propertyName)
        {
            return PropNameProvider.GetItemProp(propertyName);
        }

        /// <summary>
        /// Uses the <see cref="RootSerializer"/> to serialize a value.
        /// </summary>
        public IEnumerable<XObject> Serialize(string propertyName, object value)
        {
            return RootSerializer.Serialize(propertyName, value, this);
        }
    }
}