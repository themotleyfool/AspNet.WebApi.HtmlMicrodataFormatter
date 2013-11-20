using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using Moq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class DocumentationControllerTests
    {
        private TestableDocumentationController controller;
        private HttpConfiguration config;
        private IDocumentationProvider defaultProvider;
        private Mock<IDocumentationProviderEx> provider;
        private IApiExplorer explorer;

        [SetUp]
        public void SetUp()
        {
            provider = new Mock<IDocumentationProviderEx>();
            config = new HttpConfiguration(new HttpRouteCollection("/"));
            defaultProvider = config.Services.GetDocumentationProvider();
            config.Services.Replace(typeof(IDocumentationProvider), provider.Object);
            explorer = config.Services.GetApiExplorer();
            controller = new TestableDocumentationController
                {
                    Configuration = config,
                    DocumentationProvider = provider.Object
                };
        }

        [Test]
        public void InitializeResolvesDocumentationProvider()
        {
            try
            {
                controller.DocumentationProvider = null;
                config.Services.Replace(typeof (IDocumentationProvider), provider.Object);

                controller.Initialize(new HttpControllerContext(config, new HttpRouteData(new HttpRoute()),
                                                                new HttpRequestMessage(HttpMethod.Get, "/doc")));

                Assert.That(controller.DocumentationProvider, Is.SameAs(provider.Object));
            }
            catch (Exception)
            {
                
            }
        }

        [Test]
        public void InitializeSkipsResolutionWhenInjected()
        {
            controller.Initialize(new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), new HttpRequestMessage(HttpMethod.Get, "/doc")));

            Assert.That(controller.DocumentationProvider, Is.SameAs(provider.Object));
        }

        [Test]
        public void InitializeThrowsOnIncompatibleProvider()
        {
            config.Services.Replace(typeof(IDocumentationProvider), defaultProvider);
            controller.DocumentationProvider = null;

            TestDelegate call = () => controller.Initialize(new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), new HttpRequestMessage(HttpMethod.Get, "/doc")));

            Assert.That(call, Throws.InvalidOperationException);
        }

        [Test]
        public void GetsDocumentationFromApiExplorer()
        {
            var apiDescription = ApiDescriptionExtensionTests.CreateApiDescription(config);
            var controllerDescriptor = apiDescription.ActionDescriptor.ControllerDescriptor;
            const string docsForController = "docs for controller";

            explorer.ApiDescriptions.Add(apiDescription);
            provider.Setup(p => p.GetDocumentation(controllerDescriptor)).Returns(docsForController);

            var result = controller.GetApiDocumentation();

            Assert.That(result.Resources.Single().Documentation, Is.EqualTo(docsForController));
        }

        [Test]
        public void HideDocumentationWithRouteToken()
        {
            var apiDescription = ApiDescriptionExtensionTests.CreateApiDescription(config);
            apiDescription.Route.HideFromDocumentationExplorer();

            explorer.ApiDescriptions.Add(apiDescription);

            var result = controller.GetApiDocumentation();

            Assert.That(result.Resources, Is.Empty);
        }

        public class TestableDocumentationController : DocumentationController
        {
            public new void Initialize(HttpControllerContext controllerContext)
            {
                base.Initialize(controllerContext);
            }
        }
    }
}