using System;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public interface IDocumentationProviderEx : IDocumentationProvider
    {
        /// <summary>
        /// Gets documentation provided for a type enclosing a given method.
        /// </summary>
        string GetDocumentation(Type type);
    }

    /// <summary>
    /// Provides a bridge from the generalized <see cref="HtmlDocumentation"/>
    /// to <see cref="IDocumentationProvider"/>.
    /// </summary>
    public class WebApiHtmlDocumentationProvider : IDocumentationProviderEx
    {
        private readonly HtmlDocumentation documentation;

        public WebApiHtmlDocumentationProvider(HtmlDocumentation documentation)
        {
            this.documentation = documentation;
        }

        public string GetDocumentation(Type type)
        {
            return documentation.GetTypeDocumentation(type);
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