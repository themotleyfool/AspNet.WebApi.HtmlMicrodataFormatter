using System.Collections.Generic;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Groups one or more api methods into logical collections.
    /// </summary>
    public class SimpleApiGroup
    {
        private readonly List<SimpleApiDescription> actions = new List<SimpleApiDescription>();

        public string Name { get; set; }
        public string Documentation { get; set; }

        public IEnumerable<SimpleApiDescription> Actions
        {
            get
            {
                return actions;
            }
            set
            {
                actions.Clear();
                actions.AddRange(value);
            }
        }

        public void AddAction(SimpleApiDescription api)
        {
            actions.Add(api);
        }
    }
}