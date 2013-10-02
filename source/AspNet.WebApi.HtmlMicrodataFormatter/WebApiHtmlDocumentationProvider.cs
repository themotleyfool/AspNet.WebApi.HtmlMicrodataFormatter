using System;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public interface IDocumentationProviderEx : IDocumentationProvider
    {
        /// <summary>
        /// Gets documentation provided for a type.
        /// </summary>
        string GetDocumentation(Type type);

        /// <summary>
        /// Gets documentation provided for a type name that includes the namespace
        /// but not the assembly name, similar to <see cref="Type.FullName"/>.
        /// </summary>
        string GetDocumentation(string fullTypeName);

        /// <summary>
        /// Gets documentation provided for a property.
        /// </summary>
        string GetDocumentation(PropertyInfo propertyInfo);
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
            return GetDocumentation(type.FullName);
        }

        public string GetDocumentation(string fullTypeName)
        {
            return documentation.GetTypeDocumentation(fullTypeName);
        }

        public string GetDocumentation(PropertyInfo propertyInfo)
        {
            return documentation.GetPropertyDocumentation(propertyInfo);
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