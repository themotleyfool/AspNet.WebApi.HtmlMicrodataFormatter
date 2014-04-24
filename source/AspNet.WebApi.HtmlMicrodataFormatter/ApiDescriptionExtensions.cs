using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public static class ApiDescriptionExtensions
    {
        public static SimpleApiDescription Simplify(this ApiDescription apiDescription, HttpConfiguration config)
        {
            var href = config.ToAbsolute(apiDescription.Route.RouteTemplate);
            
            var controllerName = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = apiDescription.ActionDescriptor.ActionName;

            var documentationProvider = config.Services.GetDocumentationProviderEx();
            var parameters = apiDescription.ParameterDescriptions.SelectMany(pd => Flatten(apiDescription, pd, documentationProvider)).ToList();

            href = href.Replace("{action}", actionName.ToLowerInvariant())
                       .Replace("{controller}", controllerName.ToLowerInvariant())
                       .Replace("{*", "{");

            href = RemoveOptionalRouteParameters(href, parameters);

            var simpleApi = new SimpleApiDescription
                {
                    Href = href,
                    Name = actionName,
                    Method = apiDescription.HttpMethod.Method,
                    Documentation = apiDescription.Documentation,
                    Parameters = parameters
                };

            AddAuthenticationInfo(simpleApi, apiDescription);

            return simpleApi;
        }

        private static string RemoveOptionalRouteParameters(string href, IEnumerable<SimpleApiParameterDescriptor> parameters)
        {
            var paramNames = new HashSet<string>(parameters.Select(p => p.Name));
            return Regex.Replace(href, "{(?<name>[^}]+)}",
                m => (paramNames.Contains(m.Groups["name"].Value)) ? m.Value : "");
        }

        private static void AddAuthenticationInfo(SimpleApiDescription simpleApi, ApiDescription apiDescription)
        {
            var authorizeAttributes = 
                apiDescription.ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>(true)
                .Union(apiDescription.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<AuthorizeAttribute>(true));

            if (authorizeAttributes.Any())
            {
                simpleApi.RequiresAuthentication = true;
            }

            simpleApi.RequiresRoles =
                authorizeAttributes
                    .Where(attr => !string.IsNullOrWhiteSpace(attr.Roles))
                    .Select(attr => attr.Roles.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()));
        }

        /// <summary>
        /// Converts an <see cref="ApiParameterDescription"/> to one or more <see cref="SimpleApiParameterDescriptor"/>s.
        /// In simple cases, such as a string or primitive type parameter, a simple mapping will occur. When a parameter
        /// represents a complex type, one parameter will be returned for each public property on the type.
        /// </summary>
        public static IEnumerable<SimpleApiParameterDescriptor> Flatten(ApiDescription apiDescription, ApiParameterDescription parameterDescription, IDocumentationProviderEx documentationProvider)
        {
            var descriptor = parameterDescription.ParameterDescriptor;

            if (descriptor == null)
            {
                return new[]
                {
                    new SimpleApiParameterDescriptor(parameterDescription.Name, typeof(string), apiDescription.RelativePath, parameterDescription.Source, parameterDescription.Documentation)
                };
            }

            bool isMany;
            var parameterType = Unwrap(descriptor.ParameterType, out isMany);

            if (IsSimpleType(parameterType))
            {
                return new[] { new SimpleApiParameterDescriptor(descriptor, apiDescription.RelativePath, isMany, parameterDescription.Documentation) };    
            }
            
            if (UsesCustomModelBinder(descriptor))
            {
                var callingConvention = SimpleApiParameterDescriptor.GetCallingConvention(apiDescription.RelativePath, descriptor.ParameterName);

                return new[]
                {
                    new SimpleApiParameterDescriptor(descriptor.ParameterName, parameterType, callingConvention, descriptor.DefaultValue, descriptor.IsOptional, isMany, parameterDescription.Documentation)
                };
            }

            return EnumerateProperties(apiDescription, parameterType, documentationProvider);
        }

        private static Type Unwrap(Type parameterType, out bool isMany)
        {
            isMany = false;
            if (parameterType.IsArray)
            {
                isMany = true;
                return parameterType.GetElementType();
            }
            return parameterType;
        }

        private static IEnumerable<SimpleApiParameterDescriptor> EnumerateProperties(ApiDescription apiDescription, IReflect type, IDocumentationProviderEx documentationProvider, string prefix = "", IList<SimpleApiParameterDescriptor> list = null, ISet<Type> visitedTypes = null)
        {
            if (list == null) list = new List<SimpleApiParameterDescriptor>();
            if (visitedTypes == null) visitedTypes = new HashSet<Type>();

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var callingConvention = (apiDescription.HttpMethod == HttpMethod.Get || apiDescription.HttpMethod == HttpMethod.Head) ? "query-string" : "body";
            
            foreach (var p in props.Where(prop => prop.CanWrite))
            {
                bool isMany;
                var parameterType = Unwrap(p.PropertyType, out isMany);
                
                if (IsSimpleType(parameterType) || UsesCustomModelBinder(p.PropertyType))
                {
                    list.Add(new SimpleApiParameterDescriptor(prefix + p.Name, parameterType, callingConvention, null, false, isMany, documentationProvider.GetDocumentation(p)));
                }
                else
                {
                    if (visitedTypes.Contains(p.PropertyType))
                    {
                        list.Add(new SimpleApiParameterDescriptor(prefix + p.Name, parameterType, callingConvention, null, false, isMany, documentationProvider.GetDocumentation(p)));
                    }
                    else
                    {
                        visitedTypes.Add(p.PropertyType);
                        EnumerateProperties(apiDescription, parameterType, documentationProvider, prefix + p.Name + ".", list, visitedTypes);
                    }
                }
            }

            return list;
        }

        private static bool UsesCustomModelBinder(HttpParameterDescriptor descriptor)
        {
            return descriptor.GetCustomAttributes<ModelBinderAttribute>()
                .Union(descriptor.ParameterType.GetCustomAttributes(typeof (ModelBinderAttribute), true))
                .Any(attr => attr.GetType() != typeof(FromUriAttribute) && attr.GetType() != typeof(FromBodyAttribute));
        }

        private static bool UsesCustomModelBinder(Type type)
        {
            return type.GetCustomAttributes(typeof(ModelBinderAttribute), true)
                .Any(attr => attr.GetType() != typeof(FromUriAttribute) && attr.GetType() != typeof(FromBodyAttribute));
        }

        internal static readonly ISet<Type> WellKnownSimpleTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(decimal),
            };

        internal static bool IsSimpleType(Type type)
        {
            if (type.IsPrimitive || WellKnownSimpleTypes.Contains(type)) return true;

            var converter = TypeDescriptor.GetConverter(type);

            return converter.CanConvertFrom(typeof (string));
        }
    }
}