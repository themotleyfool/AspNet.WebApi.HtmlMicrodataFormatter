using System.Collections.Generic;
using System.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Extension methods to help manage links and lists of links.
    /// </summary>
    public static class LinkExtensions
    {
        /// <summary>
        /// Given a list of links, set <c>rel=prev</c>, <c>rel=self</c> and <c>rel=next</c>
        /// attributes.
        /// </summary>
        public static void SetRelationships(this IEnumerable<Link> links, int indexOfSelf)
        {
            var i = 0;
            foreach (var link in links)
            {
                if (i == 0)
                {
                    link.AddRelationship("start");
                }

                if (i == indexOfSelf)
                {
                    link.AddRelationship("self");
                }
                else if (i == indexOfSelf - 1)
                {
                    link.AddRelationship("prev");
                }
                else if (i == indexOfSelf + 1)
                {
                    link.AddRelationship("next");
                }
                i++;
            }

        }
    }
}