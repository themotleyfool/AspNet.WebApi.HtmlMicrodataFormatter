using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Provides a bridge from the generalized <see cref="HtmlDocumentation"/>
    /// to <see cref="IDocumentationProvider"/>.
    /// </summary>
    public class WebApiHtmlDocumentationProvider : IDocumentationProvider
    {
        private readonly HtmlDocumentation documentation;

        public WebApiHtmlDocumentationProvider(HtmlDocumentation documentation)
        {
            this.documentation = documentation;
        }

        public string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;

            if (reflectedActionDescriptor == null) return string.Empty;

            return documentation.GetMethodDocumentation(reflectedActionDescriptor.MethodInfo);
        }

        public string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            var reflectedActionDescriptor = parameterDescriptor.ActionDescriptor as ReflectedHttpActionDescriptor;

            if (reflectedActionDescriptor == null) return string.Empty;

            return documentation.GetParameterDocumentation(reflectedActionDescriptor.MethodInfo,
                                                           parameterDescriptor.ParameterName);
        }
    }
}