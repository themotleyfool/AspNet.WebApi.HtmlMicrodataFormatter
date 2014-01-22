using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Linq;
using AspNet.WebApi.HtmlMicrodataFormatter;

// This part configures HtmlMicrodataFormatter to format responses as html: 
[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.HtmlMicrodataFormatterActivator), "ConfigureHtmlMicrodataFormatter")]

// This part configures routes that generate documentation, forms and links for your project at ~/api and ~/api/doc/{typeName}:
[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.HtmlMicrodataFormatterActivator), "ConfigureDocumentationProvider")]

namespace $rootnamespace$.App_Start
{
    public static class HtmlMicrodataFormatterActivator
    {
        public static void ConfigureHtmlMicrodataFormatter()
        {
            var config = GlobalConfiguration.Configuration;
            config.Formatters.Insert(0, CreateHtmlMicrodataFormatter(config));
        }
        
        public static void ConfigureDocumentationProvider()
        {
            var config = GlobalConfiguration.Configuration;
            var documentation = new HtmlDocumentation();
            documentation.Load();

            config.Services.Replace(typeof(IDocumentationProvider), new WebApiHtmlDocumentationProvider(documentation));

            var routes = config.Routes;

            MapDocumentationRoutes(routes);
        }

        private static MediaTypeFormatter CreateHtmlMicrodataFormatter(HttpConfiguration config)
        {
            var formatter = new HtmlMicrodataFormatter();

			// optional: use HtmlMicrodataFormatter for clients that request xml
			//formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
			//formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));

            // optional: insert css and javascript
            const string bootstrapUrl = "//netdna.bootstrapcdn.com/twitter-bootstrap/2.3.2/css/bootstrap-combined.min.css";
            const string formTemplateRelativeUrl = "/Scripts/formtemplate.min.js";
            var formTemplateUrl = (config.VirtualPathRoot.Length == 1 ? "" : config.VirtualPathRoot) + formTemplateRelativeUrl;
            
            formatter.AddHeadContent(new XElement("title", "$AssemblyName$"));
            formatter.AddHeadContent(
				new XElement("link",
                             new XAttribute("rel", "stylesheet"),
                             new XAttribute("href", bootstrapUrl)));
             formatter.AddHeadContent(
				new XElement("script",
                             new XAttribute("src",  formTemplateUrl),
                             new XText("")));

            // optional: addd custom serializers to control how a specific Type is rendered as html:
            //formatter.RegisterSerializer(new ToStringSerializer(typeof (Version)));

            return formatter;
        }


        private static void MapDocumentationRoutes(HttpRouteCollection routes)
        {
            var route = routes.MapHttpRoute(global::AspNet.WebApi.HtmlMicrodataFormatter.RouteNames.ApiDocumentation,
                                "api",
                                new {controller = "Documentation", action = "GetApiDocumentation"});

			// optional: remove this line to include Documentation api:
			route.HideFromDocumentationExplorer();
        }
    }
}
