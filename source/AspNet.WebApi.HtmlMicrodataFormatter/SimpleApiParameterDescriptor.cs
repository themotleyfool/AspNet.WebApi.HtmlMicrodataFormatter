using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class SimpleApiParameterDescriptor
    {
        public string Name { get; private set; }
        public Type ParameterType { get; private set; }
        public string CallingConvention { get; private set; }
        public string Documentation { get; private set; }
        public object DefaultValue { get; private set; }
        public bool IsOptional { get; private set; }
        public bool IsMany { get; private set; }

        public SimpleApiParameterDescriptor(string name, Type type, string callingConvention, object defaultValue, bool isOptional, bool isMany = false, string documentation = "")
        {
            Name = name;
            ParameterType = type;
            CallingConvention = callingConvention;
            DefaultValue = defaultValue;
            IsOptional = isOptional;
            IsMany = isMany;
            Documentation = documentation;
        }

        public SimpleApiParameterDescriptor(string name, Type type, string routePath, ApiParameterSource callingConvention, string documentation)
            : this(name, type, GetCallingConvention(name, routePath, callingConvention), null, false, false, documentation)
        {
        }

        private static string GetCallingConvention(string name, string routePath, ApiParameterSource callingConvention)
        {
            switch (callingConvention)
            {
                case ApiParameterSource.FromBody:
                    return "body";
                default:
                    return GetCallingConvention(routePath, name);
            }
        }

        public SimpleApiParameterDescriptor(HttpParameterDescriptor arg, string routePath, bool isMany, string documentation = "")
        {
            this.Name = arg.ParameterName;
            this.IsOptional = arg.IsOptional;
            this.IsMany = isMany;
            this.Documentation = documentation;
            this.ParameterType = arg.ParameterType;

            if (this.IsOptional)
            {
                this.DefaultValue = arg.DefaultValue;
            }

            if (arg.ParameterBinderAttribute is FromBodyAttribute)
            {
                this.CallingConvention = "body";
            }
            else
            {
                this.CallingConvention = GetCallingConvention(routePath, arg.ParameterName);
            }
        }

        public static string GetCallingConvention(string routePath, string parameterName)
        {
            var indexOfQueryString = routePath.IndexOf('?');

            var indexOfParameter = routePath.IndexOf("{" + parameterName + "}", StringComparison.InvariantCultureIgnoreCase);

            if (indexOfQueryString > 0 && indexOfParameter > indexOfQueryString)
            {
                return "query-string";
            }

            if (indexOfParameter > 0)
            {
                return "uri";
            }

            return "unknown";
        }
    }
}