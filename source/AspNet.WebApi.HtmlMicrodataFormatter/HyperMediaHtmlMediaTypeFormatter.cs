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
    public abstract class HyperMediaHtmlMediaTypeFormatter : MediaTypeFormatter
    {
        protected HyperMediaHtmlMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml+xml"));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            var target = typeof(IEnumerable<ApiGroup>);
            return target.IsAssignableFrom(type);
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                                                      TransportContext transportContext)
        {
            var buffer = new MemoryStream();
            var xmlWriter = XmlWriter.Create(buffer, new XmlWriterSettings { Indent = true });

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

        public virtual IEnumerable<XObject> BuildHeadElements(object value)
        {
            yield return new XElement("link",
                                      new XAttribute("rel", "stylesheet"),
                                      new XAttribute("href", "/css/flat-ui.css"),
                                      new XAttribute("media", "all"),
                                      new XAttribute("type", "text/css"));
            yield return new XElement("link",
                                      new XAttribute("rel", "stylesheet"),
                                      new XAttribute("href", "/css/app.css"),
                                      new XAttribute("media", "all"),
                                      new XAttribute("type", "text/css"));
        }
        
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("text/html");
        }
    }
}