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

            DocumentationProvider = Configuration.Services.GetDocumentationProviderEx();
            
        }

        public virtual SimpleApiDocumentation GetApiDocumentation()
        {
            var apiExplorer = Configuration.Services.GetApiExplorer();

            var documentation = new SimpleApiDocumentation();

            foreach (var api in apiExplorer.ApiDescriptions)
            {
                if (api.Route.IsHiddenFromApiExplorer()) continue;

                var controllerDescriptor = api.ActionDescriptor.ControllerDescriptor;
                documentation.Add(controllerDescriptor.ControllerName, ConvertApiDescription(api));
                documentation[controllerDescriptor.ControllerName].Documentation =
                    DocumentationProvider.GetDocumentation(controllerDescriptor.ControllerType);
            }

            return documentation;
        }

        /// <summary>
        /// Converts a complex <see cref="ApiDescription"/> into a simpler representation.
        /// </summary>
        public virtual SimpleApiDescription ConvertApiDescription(ApiDescription api)
        {
            return api.Simplify(Configuration);
        }

        /// <summary>
        /// Gets information about a given .NET type.
        /// </summary>
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