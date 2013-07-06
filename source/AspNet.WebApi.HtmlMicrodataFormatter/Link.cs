using System.Collections.Generic;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class Link
    {
        private readonly IEnumerable<KeyValuePair<string, string>> attributes;
        private readonly string body;

        public Link(string href, string relationship, string body)
            : this(new Dictionary<string, string> { { "href", href }, { "rel", relationship } }, body)
        {
        }

        public Link(IEnumerable<KeyValuePair<string, string>> attributes, string body)
        {
            this.attributes = attributes;
            this.body = body;
        }

        public IEnumerable<KeyValuePair<string, string>> Attributes { get { return attributes; } }
        public string Body { get { return body; } }
    }
}