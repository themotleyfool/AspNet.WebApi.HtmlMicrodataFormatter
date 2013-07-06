using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public static class ApiDescriptionExtensions
    {
        public static SimpleApiDescription Simplify(this ApiDescription apiDescription)
        {
            return apiDescription.Simplify(GlobalConfiguration.Configuration);
        }

        public static SimpleApiDescription Simplify(this ApiDescription apiDescription, HttpConfiguration config)
        {
            var href = config.VirtualPathRoot + apiDescription.Route.RouteTemplate;

            var controllerName = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = apiDescription.ActionDescriptor.ActionName;

            href = href.Replace("{action}", actionName.ToLowerInvariant())
                       .Replace("{controller}", controllerName.ToLowerInvariant());

            var simpleApi = new SimpleApiDescription
                {
                    Href = href,
                    Name = actionName,
                    Method = apiDescription.HttpMethod.Method,
                    Documentation = apiDescription.Documentation,
                    Parameters =
                        apiDescription.ParameterDescriptions.Select(
                            pd => new SimpleApiParameterDescriptor(pd.ParameterDescriptor, apiDescription.RelativePath))
                                      .ToList()
                };

            return simpleApi;
        }
    }
}