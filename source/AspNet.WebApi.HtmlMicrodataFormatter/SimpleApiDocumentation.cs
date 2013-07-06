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

        public IEnumerable<SimpleApiGroup> Resources { get { return resources.Select(res => res.Value); } }

        public SimpleApiGroup this[string resource]
        {
            get { return resources.First(r => r.Key == resource).Value; }
        }

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