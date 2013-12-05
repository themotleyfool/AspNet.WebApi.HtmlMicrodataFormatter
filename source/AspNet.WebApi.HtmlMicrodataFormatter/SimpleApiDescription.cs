using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class SimpleApiDescription
    {
        public string Documentation { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string Method { get; set; }
        public IEnumerable<SimpleApiParameterDescriptor> Parameters { get; set; }
        public bool RequiresAuthentication { get; set; }
        public IEnumerable<IEnumerable<string>> RequiresRoles { get; set; }

        public bool Templated
        {
            get
            {
                return Href.Contains("{") && Href.Contains("}");
            }
        }

        public SimpleApiDescription()
        {
            this.Parameters = Enumerable.Empty<SimpleApiParameterDescriptor>();
        }

        public SimpleApiDescription(HttpRequestMessage request, string name, string appRelativePath)
        {
            this.Href = request.GetConfiguration().MapPath(appRelativePath);
            this.Name = name;
            this.Method = "GET";
            this.Parameters = Enumerable.Empty<SimpleApiParameterDescriptor>();
        }
    }
}