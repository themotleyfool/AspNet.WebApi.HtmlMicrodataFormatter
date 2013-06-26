using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class HyperMediaHtmlMediaTypeFormatter : MediaTypeFormatter
    {
        public HyperMediaHtmlMediaTypeFormatter()
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

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            return this;
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

        public virtual IEnumerable<XObject> BuildBody(object value)
        {
            var descriptions = (IEnumerable<ApiGroup>)value;

            foreach (var g in descriptions)
            {
                yield return new XElement("section",
                                              new XAttribute("class", "api-group"),
                                              BuildApiGroupSummary(g),
                                              g.Actions.Select(BuildApi));
            }
        }

        private XObject BuildApiGroupSummary(ApiGroup apiGroup)
        {
            return new XElement("header",
                new XElement("h1", new XText(apiGroup.Name)),
                ParseDocumentation(apiGroup.Documentation));
        }

        private XElement BuildApi(ApiDescription api)
        {
            var children = new List<object>();

            children.Add(new XAttribute("class", "api"));
            children.Add(new XElement("header", new XText(api.ID)));

            if (api.Documentation != null)
            {
                children.AddRange(ParseDocumentation(api.Documentation));
            }

            if (api.ParameterDescriptions.Any() || api.HttpMethod != HttpMethod.Get)
            {
                children.Add(
                    new XElement("form",
                        new XAttribute("action", api.Route.RouteTemplate),
                        new XAttribute("method", api.HttpMethod.Method),
                        new XAttribute("class", "ns:todo"),
                        BuildFormInputs(api)));
            }
            else
            {
                children.Add(
                    new XElement("a",
                        new XAttribute("href", api.Route.RouteTemplate),
                        new XText(api.ActionDescriptor.ActionName)));
            }

            return new XElement("section", children);
        }

        private IEnumerable<XElement> ParseDocumentation(string html)
        {
            return XElement.Parse("<r>" + html + "</r>").Elements();
        }

        private IEnumerable<XNode> BuildFormInputs(ApiDescription api)
        {
            foreach (var p in api.ParameterDescriptions)
            {
                var id = "ns:todo";
                var type = GetHtmlInputType(p);

                yield return new XElement("label", new XAttribute("for", id), new XText(p.Name));
                yield return new XElement("input",
                    new XAttribute("name", p.Name),
                    new XAttribute("type", type),
                    new XAttribute("value", p.ParameterDescriptor.DefaultValue ?? ""),
                    new XAttribute("placeholder", p.ParameterDescriptor.DefaultValue ?? "")
                    );
            }

            yield return new XElement("input", new XAttribute("type", "submit"));
        }

        private string GetHtmlInputType(ApiParameterDescription apiParam)
        {
            if (apiParam.ParameterDescriptor.ParameterType == typeof(bool))
            {
                return "checkbox";
            }

            return "text";
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("text/html");
        }
    }
}