using System.Web.Http;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    internal static class HttpConfigurationExtensions
    {
        internal static string MapPath(this HttpConfiguration configuration, string appRelativePath)
        {
            var root = configuration.VirtualPathRoot;

            if (root == "/") return "/" + appRelativePath;

            return root + "/" + appRelativePath;
        }
    }
}