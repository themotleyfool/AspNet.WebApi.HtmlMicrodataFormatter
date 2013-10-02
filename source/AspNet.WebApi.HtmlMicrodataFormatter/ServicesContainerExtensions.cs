using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public static class ServicesContainerExtensions
    {
        public static IDocumentationProviderEx GetDocumentationProviderEx(this ServicesContainer container)
        {
            var result = container.GetDocumentationProvider() as IDocumentationProviderEx;

            if (result == null)
            {
                var msg = string.Format("{0} is not registered as {1} service provider.",
                    typeof(WebApiHtmlDocumentationProvider),
                    typeof(IDocumentationProvider));
                throw new InvalidOperationException(msg);
            }

            return result;
        }
    }
}