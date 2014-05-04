using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml+xml"));
            Settings = new XmlWriterSettings {OmitXmlDeclaration = true};
        }

        /// <summary>
        /// Returns <c>false</c> because accepting requests using html
        /// is not supported.
        /// </summary>
        public override bool CanReadType(Type type)
        {
            return false;
        }

        /// <summary>
        /// Returns <c>false</c> because <see cref="GetPerRequestFormatterInstance"/>
        /// will return a different implementation that will be used for writing.
        /// </summary>
        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public override sealed Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                                                       TransportContext transportContext)
        {
            throw new NotSupportedException(typeof(PerRequestHtmlMediaTypeFormatter) + " must be used for writing.");
        }

        /// <summary>
        /// Settings that control how <see cref="XDocument"/> is serialized
        /// as text in the response. By default, <see cref="XmlWriterSettings.OmitXmlDeclaration"/>
        /// is set to <c>true</c> and <see cref="XmlWriterSettings.Indent"/> is disabled.
        /// </summary>
        public XmlWriterSettings Settings { get; set; }
        
        /// <summary>
        /// Builds the list of objects to include in the <c>&lt;body&gt;</c> section.
        /// May include <see cref="XAttribute"/> instances to specify attributes
        /// on the body tag, such as class or style attributes.
        /// </summary>
        /// <param name="value">The object returned from an ApiController action, to be serialized in the body.</param>
        /// <param name="request">The current request.</param>
        /// <returns></returns>
        public abstract IEnumerable<XObject> BuildBody(object value, HttpRequestMessage request);

        /// <summary>
        /// Adds content to the <c>&lt;head&gt;</c> section, such as title, meta,
        /// link and script elements.
        /// </summary>
        public virtual void AddHeadContent(XObject content)
        {
            headElements.Add(content);
        }

        /// <summary>
        /// Sets the text to show in the <c>title</c> element.
        /// </summary>
        public string Title
        {
            get
            {
                var title = headElements.OfType<XElement>().FirstOrDefault(e => e.Name == "title");
                if (title == null) return string.Empty;
                return title.DescendantNodes().Single().ToString();
            }
            set
            {
                var title = headElements.OfType<XElement>().FirstOrDefault(e => e.Name == "title");
                if (title == null)
                {
                    title = new XElement("title");
                    AddHeadContent(title);
                }
                title.ReplaceAll(new XText(value));
            }
        }

        /// <summary>
        /// Builds the list of elements to be included in the <c>&lt;head&gt;</c> section.
        /// Be default, returns the list of content previously added by <see cref="AddHeadContent"/>.
        /// </summary>
        /// <param name="value">The object returned from an ApiController action, to be serialized later in the body.</param>
        /// <param name="request">The current request.</param>
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

                var bytes = Encoding.UTF8.GetBytes(buffer.ToString());
                await writeStream.WriteAsync(bytes, 0, bytes.Length);
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