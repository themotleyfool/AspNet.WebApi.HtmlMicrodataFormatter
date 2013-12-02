using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;

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
            var href = config.MapPath(apiDescription.Route.RouteTemplate);
            
            var controllerName = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = apiDescription.ActionDescriptor.ActionName;

            href = href.Replace("{action}", actionName.ToLowerInvariant())
                       .Replace("{controller}", controllerName.ToLowerInvariant())
                       .Replace("{*", "{");

            var documentationProvider = config.Services.GetDocumentationProviderEx();

            var simpleApi = new SimpleApiDescription
                {
                    Href = href,
                    Name = actionName,
                    Method = apiDescription.HttpMethod.Method,
                    Documentation = apiDescription.Documentation,
                    Parameters = apiDescription.ParameterDescriptions.SelectMany(pd => Flatten(apiDescription, pd, documentationProvider)).ToList()
                };

            return simpleApi;
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
                    new SimpleApiParameterDescriptor(parameterDescription.Name, apiDescription.RelativePath, parameterDescription.Source, parameterDescription.Documentation)
                };
            }

            if (IsSimpleType(descriptor.ParameterType))
            {
                return new[] { new SimpleApiParameterDescriptor(descriptor, apiDescription.RelativePath, parameterDescription.Documentation) };    
            }
            
            if (UsesCustomModelBinder(descriptor))
            {
                var callingConvention = SimpleApiParameterDescriptor.GetCallingConvention(apiDescription.RelativePath, descriptor.ParameterName);

                return new[]
                {
                    
                    new SimpleApiParameterDescriptor(descriptor.ParameterName, callingConvention, descriptor.DefaultValue, descriptor.IsOptional, parameterDescription.Documentation)
                };
            }

            return EnumerateProperties(apiDescription, descriptor.ParameterType, documentationProvider);
        }

        private static IEnumerable<SimpleApiParameterDescriptor> EnumerateProperties(ApiDescription apiDescription, IReflect type, IDocumentationProviderEx documentationProvider, string prefix = "", IList<SimpleApiParameterDescriptor> list = null, ISet<Type> visitedTypes = null)
        {
            if (list == null) list = new List<SimpleApiParameterDescriptor>();
            if (visitedTypes == null) visitedTypes = new HashSet<Type>();

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var callingConvention = (apiDescription.HttpMethod == HttpMethod.Get || apiDescription.HttpMethod == HttpMethod.Head) ? "query-string" : "body";

            foreach (var p in props.Where(prop => prop.CanWrite))
            {
                if (IsSimpleType(p.PropertyType))
                {
                    list.Add(new SimpleApiParameterDescriptor(prefix + p.Name, callingConvention, null, false, documentationProvider.GetDocumentation(p)));
                }
                else
                {
                    if (visitedTypes.Contains(p.PropertyType))
                    {
                        list.Add(new SimpleApiParameterDescriptor(prefix + p.Name, callingConvention, null, false, documentationProvider.GetDocumentation(p)));
                    }
                    else
                    {
                        visitedTypes.Add(p.PropertyType);
                        EnumerateProperties(apiDescription, p.PropertyType, documentationProvider, prefix + p.Name + ".", list, visitedTypes);
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