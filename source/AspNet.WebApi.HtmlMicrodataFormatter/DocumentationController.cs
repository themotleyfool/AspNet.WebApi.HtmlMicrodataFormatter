using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class DocumentationController : ApiController
    {
        public IEnumerable<ApiGroup> GetApiGroups()
        {
            return GroupApiDescriptions();
        }

        public IEnumerable<ApiGroup> GroupApiDescriptions()
        {
            var apiExplorer = Configuration.Services.GetApiExplorer();

            var grouping = from api in apiExplorer.ApiDescriptions
                           group api by api.ActionDescriptor.ControllerDescriptor.ControllerType into g
                           let cd = g.First().ActionDescriptor.ControllerDescriptor
                           select new ApiGroup
                               {
                                   Name = cd.ControllerName,
                                   Documentation = doc.GetTypeDocumentation(cd.ControllerType),
                                   Actions = g
                               };

            return grouping;
        }

        public static readonly HtmlDocumentation doc = new HtmlDocumentation();

        static DocumentationController()
        {
            doc.Load();
        }
    }
}