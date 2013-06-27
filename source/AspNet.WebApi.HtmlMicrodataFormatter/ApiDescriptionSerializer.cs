using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Description;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class ApiGroupSerializer : ApiDescriptionSerializer
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new[] { typeof(IEnumerable<ApiGroup>) }; }
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var descriptions = (IEnumerable<ApiGroup>)obj;

            foreach (var g in descriptions)
            {
                yield return new XElement("section",
                                              new XAttribute("class", "api-group"),
                                              BuildApiGroupSummary(g),
                                              g.Actions.Select(api => base.Serialize(propertyName, api, context)));
            }
        }

        private XObject BuildApiGroupSummary(ApiGroup apiGroup)
        {
            return new XElement("header",
                new XElement("h1", new XText(apiGroup.Name)),
                ParseDocumentation(apiGroup.Documentation));
        }

    }

    public class ApiDescriptionSerializer : DefaultSerializer
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new[] {typeof (ApiDescription)}; }
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            yield return BuildApi((ApiDescription) obj);
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

        protected IEnumerable<XElement> ParseDocumentation(string html)
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
                                          new XAttribute("obj", p.ParameterDescriptor.DefaultValue ?? ""),
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
    }
}