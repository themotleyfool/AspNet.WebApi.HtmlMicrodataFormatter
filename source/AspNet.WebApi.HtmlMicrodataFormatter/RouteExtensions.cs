using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http.Routing;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Extensions for customizing how routes are included.
    /// </summary>
    public static class RouteExtensions
    {
        private static readonly ISet<IHttpRoute> hiddenRoutes = new HashSet<IHttpRoute>();

        /// <summary>
        /// Add a data token that tells <see cref="DocumentationController"/>
        /// not to include any <see cref="ApiDescription"/>s that use this route.
        /// </summary>
        public static void HideFromDocumentationExplorer(this IHttpRoute route)
        {
            hiddenRoutes.Add(route);
        }

        /// <summary>
        /// Checks if a route has been hidden using <see cref="HideFromDocumentationExplorer"/>.
        /// </summary>
        public static bool IsHiddenFromApiExplorer(this IHttpRoute route)
        {
            return hiddenRoutes.Contains(route);
        }
    }
}