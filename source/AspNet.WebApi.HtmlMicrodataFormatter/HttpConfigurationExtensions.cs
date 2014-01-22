using System.Web.Http;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    internal static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Like VirtualPathUtility.ToAbsolute without requiring System.Web.
        /// </summary>
        internal static string ToAbsolute(this HttpConfiguration configuration, string appRelativePath)
        {
            var root = configuration.VirtualPathRoot;

            if (root == "/") return "/" + appRelativePath;

            return root + "/" + appRelativePath;
        }
    }
}