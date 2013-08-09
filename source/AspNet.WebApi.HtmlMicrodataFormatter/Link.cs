using System.Collections.Generic;
using System.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Represents an html anchor tag.
    /// </summary>
    public class Link
    {
        private readonly Dictionary<string, string> attributes;

        public Link(string href, string rel, string text)
            : this(href, text)
        {
            SetAttribute("rel", rel);
        }

        public Link(string href, string text)
            : this(Enumerable.Empty<KeyValuePair<string,string>>(), text)
        {
            SetAttribute("href", href);
        }

        public Link(IEnumerable<KeyValuePair<string, string>> attributes, string text)
        {
            this.attributes = attributes.ToDictionary(kv => kv.Key, kv => kv.Value);
            Text = text;
        }

        public void AddRelationship(string relationship)
        {
            string rel;
            if (attributes.TryGetValue("rel", out rel) && !string.IsNullOrWhiteSpace(rel))
            {
                relationship = rel + " " + relationship;
            }
            SetAttribute("rel", relationship);
        }

        public void SetAttribute(string name, string value)
        {
            attributes[name] = value;
        }

        public IEnumerable<KeyValuePair<string, string>> Attributes { get { return attributes; } }

        public string Text { get; set; }
    }
}