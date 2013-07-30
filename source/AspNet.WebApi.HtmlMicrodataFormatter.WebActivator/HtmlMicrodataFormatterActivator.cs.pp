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
            GlobalConfiguration.Configuration.Formatters.Insert(0, CreateHtmlMicrodataFormatter());
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

        private static MediaTypeFormatter CreateHtmlMicrodataFormatter()
        {
            var formatter = new HtmlMicrodataFormatter();

            // optional: insert css and javascript
            formatter.AddHeadContent(new XElement("title", "$AssemblyName$"));
            formatter.AddHeadContent(new XElement("link",
                                                  new XAttribute("rel", "stylesheet"),
                                                  new XAttribute("href",
                                                                 "//netdna.bootstrapcdn.com/twitter-bootstrap/2.3.2/css/bootstrap-combined.min.css")));

            // optional: addd custom serializers to control how a specific Type is rendered as html:
            //formatter.RegisterSerializer(new ToStringSerializer(typeof (Version)));

            return formatter;
        }


        private static void MapDocumentationRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(RouteNames.ApiDocumentation,
                                "api",
                                new {controller = "Documentation", action = "GetApiDocumentation"});

            routes.MapHttpRoute(RouteNames.TypeDocumentation,
                                "api/doc/{typeName}",
                                new {controller = "Documentation", action = "GetTypeDocumentation"});
        }
    }
}
