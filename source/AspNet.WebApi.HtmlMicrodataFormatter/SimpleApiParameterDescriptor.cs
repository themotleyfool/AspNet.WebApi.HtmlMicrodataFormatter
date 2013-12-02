using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class SimpleApiParameterDescriptor
    {
        public string Name { get; private set; }
        public string CallingConvention { get; private set; }
        public string Documentation { get; private set; }
        public object DefaultValue { get; private set; }
        public bool IsOptional { get; private set; }

        public SimpleApiParameterDescriptor(string name, string callingConvention, object defaultValue, bool isOptional, string documentation = "")
        {
            Name = name;
            CallingConvention = callingConvention;
            DefaultValue = defaultValue;
            IsOptional = isOptional;
            Documentation = documentation;
        }

        public SimpleApiParameterDescriptor(string name, string routePath, ApiParameterSource callingConvention, string documentation)
            : this(name, GetCallingConvention(name, routePath, callingConvention), null, false, documentation)
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

        public SimpleApiParameterDescriptor(HttpParameterDescriptor arg, string routePath, string documentation="")
        {
            this.Name = arg.ParameterName;
            this.IsOptional = arg.IsOptional;
            this.Documentation = documentation;

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