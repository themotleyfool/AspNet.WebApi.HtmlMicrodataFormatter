using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
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
            Settings = new XmlWriterSettings();
            Settings.OmitXmlDeclaration = true;
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public override sealed Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                                                       TransportContext transportContext)
        {
            throw new NotSupportedException();
        }

        public XmlWriterSettings Settings { get; set; }


        public abstract IEnumerable<XObject> BuildBody(object value, HttpRequestMessage request);

        public virtual void AddHeadContent(XObject content)
        {
            headElements.Add(content);
        }

        public virtual IEnumerable<XObject> BuildHeadElements(object value, HttpRequestMessage request)
        {
            return headElements;
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request,
                                                                          MediaTypeHeaderValue mediaType)
        {
            return new PerRequestHtmlMediaTypeFormatter(this, request);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers,
                                                      MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("text/html");
        }

        /// <summary>
        /// Wraps HtmlMediaTypeFormatter with an instance that keeps track of request.
        /// </summary>
        class PerRequestHtmlMediaTypeFormatter : MediaTypeFormatter
        {
            private readonly HtmlMediaTypeFormatter outer;
            private readonly HttpRequestMessage request;

            public PerRequestHtmlMediaTypeFormatter(HtmlMediaTypeFormatter htmlMediaTypeFormatter,
                                                    HttpRequestMessage request)
            {
                this.outer = htmlMediaTypeFormatter;
                this.request = request;
            }

            public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream,
                                                          HttpContent content,
                                                          TransportContext transportContext)
            {
                var buffer = new StringBuilder();
                var xmlWriter = XmlWriter.Create(buffer, outer.Settings);

                using (xmlWriter)
                {
                    var head = new XElement("head", outer.BuildHeadElements(value, request));
                    var body = new XElement("body", outer.BuildBody(value, request));
                    var html = new XElement("html", head, body);
                    var doc = new XDocument(new XDocumentType("html", null, null, null), html);

                    doc.WriteTo(xmlWriter);
                }

                // le sigh
                buffer.Replace("<!DOCTYPE html >", "<!DOCTYPE html>");

                using (var writer = new StreamWriter(writeStream, Encoding.UTF8))
                {
                    await writer.WriteAsync(buffer.ToString());
                    await writer.FlushAsync();
                }
            }

            public override bool CanReadType(Type type)
            {
                return outer.CanReadType(type);
            }

            public override bool CanWriteType(Type type)
            {
                return outer.CanWriteType(type);
            }
        }
    }
}