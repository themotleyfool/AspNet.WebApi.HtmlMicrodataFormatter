using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Generates a document containing a table of contents (navigation menu)
    /// and sections of forms and links grouped by resource/controller.
    /// </summary>
    public class ApiDocumentationSerializer : ApiDescriptionSerializer
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new[] { typeof(SimpleApiDocumentation) }; }
        }

        /// <summary>
        /// Entry point for building the body from a <see cref="SimpleApiDocumentation"/>.
        /// </summary>
        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var descriptions = (SimpleApiDocumentation)obj;
            var content = new List<XObject>();

            content.AddRange(BuildTopLevelDocumentation(descriptions, context));
            content.Add(BuildNav(descriptions));

            foreach (var group in descriptions.Resources)
            {
                content.Add(new XElement("section",
                                          new XAttribute("class", "api-group"),
                                          new XAttribute("id", group.Name),
                                          BuildApiGroupSummary(group),
                                          group.Actions.Select(api => context.Serialize(propertyName, api))));
            }

            var container = new XElement("div",
                                         new XAttribute("class", "container"),
                                         content);

            return new[] {container};
        }

        /// <summary>
        /// If <see cref="SimpleApiDocumentation.TopLevelDocumentation"/> has been provided,
        /// simply returns that markup. Otherwise builds an h1 element containing the
        /// text from <see cref="HtmlMediaTypeFormatter.Title"/> if available, or placeholder
        /// text otherwise.
        /// </summary>
        public virtual IEnumerable<XNode> BuildTopLevelDocumentation(SimpleApiDocumentation descriptions, SerializationContext context)
        {
            if (!string.IsNullOrWhiteSpace(descriptions.TopLevelDocumentation))
            {
                return ParseDocumentation(descriptions.TopLevelDocumentation);    
            }

            var title = context.Title;
            if (string.IsNullOrEmpty(title))
            {
                title = "Api Documentation";
            }

            return new[]
                {
                    new XElement("h1", new XText(title))
                };
        }

        /// <summary>
        /// Creates a ul tag with list items for each resource, linking
        /// to the section containing documentation for each resource
        /// using the resource name as an id/fragment.
        /// </summary>
        /// <param name="descriptions"></param>
        /// <returns></returns>
        public virtual XElement BuildNav(SimpleApiDocumentation descriptions)
        {
            var toc = new XElement("ul", new XAttribute("class", "nav nav-list span4"));

            toc.Add(new XElement("li",
                new XAttribute("class", "nav-header"),
                new XText("Resources")));

            foreach (var group in descriptions.Resources)
            {
                toc.Add(new XElement("li",
                                     new XElement("a",
                                                  new XAttribute("href", "#" + @group.Name),
                                                  new XText(@group.Name))));
            }

            return new XElement("div", new XAttribute("class", "row"), toc);
        }

        /// <summary>
        /// Builds a header for a list of API methods grouped by resource.
        /// By default creates a header tag containing the resource name
        /// and documentation from comments on the <see cref="ApiController"/>.
        /// </summary>
        public virtual XObject BuildApiGroupSummary(SimpleApiGroup apiGroup)
        {
            return new XElement("header",
                                new XElement("h1", new XText(apiGroup.Name)),
                                ParseDocumentation(apiGroup.Documentation));
        }

    }
}