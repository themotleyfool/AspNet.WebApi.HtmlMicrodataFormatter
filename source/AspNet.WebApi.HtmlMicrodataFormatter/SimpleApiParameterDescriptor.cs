using System;
using System.Web.Http;
using System.Web.Http.Controllers;

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

        public SimpleApiParameterDescriptor(HttpParameterDescriptor arg, string routePath, string documentation="")
        {
            this.Name = arg.ParameterName;
            this.IsOptional = arg.IsOptional;
            this.Documentation = documentation;

            if (this.IsOptional)
            {
                this.DefaultValue = arg.DefaultValue;
            }

            var indexOfQueryString = routePath.IndexOf('?');

            if (arg.ParameterBinderAttribute is FromBodyAttribute)
            {
                this.CallingConvention = "body";
            }
            else
            {
                var indexOfParameter = routePath.IndexOf("{" + arg.ParameterName + "}", StringComparison.InvariantCultureIgnoreCase);

                if (indexOfQueryString > 0 && indexOfParameter > indexOfQueryString)
                {
                    this.CallingConvention = "query-string";
                }
                else if (indexOfParameter > 0)
                {
                    this.CallingConvention = "uri";
                }
                else
                {
                    this.CallingConvention = "unknown";
                }
            }
        }
    }
}