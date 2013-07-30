using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class DocumentationController : ApiController
    {
        public IDocumentationProviderEx DocumentationProvider { get; set; }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            if (DocumentationProvider != null) return;

            DocumentationProvider = Configuration.Services.GetService(typeof (IDocumentationProvider))
                                    as IDocumentationProviderEx;

            if (DocumentationProvider == null)
            {
                var msg = string.Format("{0} can only be used when {1} is registered as {2} service provider.",
                    typeof(DocumentationController),
                    typeof(WebApiHtmlDocumentationProvider),
                    typeof(IDocumentationProvider));
                throw new InvalidOperationException(msg);
            }
        }

        public virtual SimpleApiDocumentation GetApiDocumentation()
        {
            var apiExplorer = Configuration.Services.GetApiExplorer();

            var documentation = new SimpleApiDocumentation();

            foreach (var api in apiExplorer.ApiDescriptions)
            {
                var controllerDescriptor = api.ActionDescriptor.ControllerDescriptor;
                documentation.Add(controllerDescriptor.ControllerName, api.Simplify(Configuration));
                documentation[controllerDescriptor.ControllerName].Documentation =
                    DocumentationProvider.GetDocumentation(controllerDescriptor.ControllerType);
            }

            return documentation;
        }

        public virtual object GetTypeDocumentation(string typeName)
        {
            var documentation = DocumentationProvider.GetDocumentation(typeName);
            if (string.IsNullOrWhiteSpace(documentation))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Documentation for the type '" + typeName + "' was not found.");
            }

            return new SimpleTypeDocumentation {Name = typeName, Documentation = documentation};
        }
    }
}