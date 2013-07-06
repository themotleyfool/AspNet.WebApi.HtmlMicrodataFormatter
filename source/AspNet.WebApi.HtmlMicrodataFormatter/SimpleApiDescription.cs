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

        public bool Templated
        {
            get
            {
                return Href.Contains("{") && Href.Contains("}");
            }
        }

        public SimpleApiDescription()
        {
        }

        private static string GetAbsoluteUri(HttpRequestMessage request, string relativePath)
        {
            return new Uri(request.RequestUri,
                           request.GetConfiguration().VirtualPathRoot + relativePath)
                .GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
        }

        public SimpleApiDescription(HttpRequestMessage request, string name, string relativePath)
        {
            this.Href = GetAbsoluteUri(request, relativePath);
            this.Name = name;
            this.Method = "GET";
            this.Parameters = Enumerable.Empty<SimpleApiParameterDescriptor>();
        }
    }
}