using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Base class for formatting objects as well-formed HTML.
    /// Subclasses must implement <see cref="BuildBody"/>.
    /// </summary>
    public abstract class HtmlMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly IList<XObject> headElements = new List<XObject>(); 

        protected HtmlMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml+xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            var target = typeof(IEnumerable<SimpleApiGroup>);
            return target.IsAssignableFrom(type);
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                                                      TransportContext transportContext)
        {
            var buffer = new MemoryStream();
            var xmlWriter = XmlWriter.Create(buffer, new XmlWriterSettings { Indent = false });

            using (xmlWriter)
            {
                xmlWriter.WriteDocType("html", null, null, null);

                var head = new XElement("head", BuildHeadElements(value));
                var body = new XElement("body", BuildBody(value));
                var html = new XElement("html", head, body);
                
                html.WriteTo(xmlWriter);
            }

            buffer.Seek(0, SeekOrigin.Begin);

            await buffer.CopyToAsync(writeStream);
        }

        public abstract IEnumerable<XObject> BuildBody(object value);

        public virtual void AddHeadContent(XObject content)
        {
            headElements.Add(content);
        }

        public virtual IEnumerable<XObject> BuildHeadElements(object value)
        {
            return headElements;
        }
        
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("text/html");
        }
    }
}