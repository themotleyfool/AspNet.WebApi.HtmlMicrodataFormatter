using System.Collections.Generic;
using System.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Top level class that aggregates api methods into groups of resources
    /// and includes general documentation about the application.
    /// </summary>
    public class SimpleApiDocumentation
    {
        private readonly IList<KeyValuePair<string, SimpleApiGroup>> resources = new List<KeyValuePair<string, SimpleApiGroup>>();

        /// <summary>
        /// Documentation text to be shown at the top of the html document.
        /// May include html markup, inline styles and scripts. Must be well-formed.
        /// </summary>
        public string TopLevelDocumentation { get; set; }

        /// <summary>
        /// Groupings of <see cref="SimpleApiDescription"/> by resource (ApiController).
        /// </summary>
        public IEnumerable<SimpleApiGroup> Resources { get { return resources.Select(res => res.Value); } }

        /// <summary>
        /// Retrieves a <see cref="SimpleApiGroup"/> by resource name.
        /// </summary>
        public SimpleApiGroup this[string resource]
        {
            get { return resources.First(r => r.Key == resource).Value; }
        }

        /// <summary>
        /// Add a <see cref="SimpleApiDescription"/> to a list grouped by resource name.
        /// </summary>
        public void Add(string resourceName, SimpleApiDescription api)
        {
            var resource = resources.FirstOrDefault(r => r.Key == resourceName);
            if (resource.Key == null)
            {
                var apiGroup = new SimpleApiGroup {Name = resourceName};

                resource = new KeyValuePair<string, SimpleApiGroup>(resourceName, apiGroup);
                resources.Add(resource);
            }

            resource.Value.AddAction(api);
        }
    }
}