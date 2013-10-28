using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    public class ApiDescriptionExtensionTests
    {
        private HttpConfiguration httpConfiguration;

        [SetUp]
        public void SetUp()
        {
            httpConfiguration = new HttpConfiguration(new HttpRouteCollection("/myApp"));
            httpConfiguration.Services.Replace(typeof(IDocumentationProvider), new WebApiHtmlDocumentationProvider(new HtmlDocumentation()));
        }

        [TestFixture]
        public class RouteTemplateTests : ApiDescriptionExtensionTests
        {
            [Test]
            public void CombineRoutePathWithAppRoot()
            {
                var api = CreateApiDescription();

                var simple = api.Simplify(httpConfiguration);

                Assert.That(simple.Href, Is.EqualTo("/myApp/foo/bar"));
            }

            [Test]
            public void ReplaceActionParameterInTemplate()
            {
                var api = CreateApiDescription("foo/{action}");

                var simple = api.Simplify(httpConfiguration);

                Assert.That(simple.Href, Is.EqualTo("/myApp/foo/put"));
            }

            [Test]
            public void RemoveAsteriskFromCatchAllParameter()
            {
                var api = CreateApiDescription("users/{*name}");

                var simple = api.Simplify(httpConfiguration);

                Assert.That(simple.Href, Is.EqualTo("/myApp/users/{name}"));
            }

            [Test]
            public void ReplaceControllerInTemplate()
            {
                var api = CreateApiDescription("{controller}");

                var simple = api.Simplify(httpConfiguration);

                Assert.That(simple.Href, Is.EqualTo("/myApp/sample"));
            }
        }

        [TestFixture]
        public class Properties : ApiDescriptionExtensionTests
        {
            [Test]
            public void SetNameFromControllerAndAction()
            {
                var api = CreateApiDescription("{controller}");

                var simple = api.Simplify(httpConfiguration);

                Assert.That(simple.Name, Is.EqualTo("Put"));
            }

            [Test]
            public void CopiesDocumentation()
            {
                var api = CreateApiDescription("{controller}");

                var simple = api.Simplify(httpConfiguration);

                Assert.That(simple.Documentation, Is.EqualTo(api.Documentation));
            }
        }

        [TestFixture]
        public class PropertyConversion : ApiDescriptionExtensionTests
        {
            [Test]
            [TestCase(typeof(bool))]
            [TestCase(typeof(string))]
            [TestCase(typeof(decimal))]
            [TestCase(typeof(UInt64))]
            [TestCase(typeof(DateTime))]
            public void SimpleTypes(Type type)
            {
                Assert.That(ApiDescriptionExtensions.IsSimpleType(type), Is.True, "For type " + type);
            }

            public class Model
            {
                public string Name { get; set; }
                public bool Active { get; set; }
            }

            [Test]
            public void NonSimpleType()
            {
                Assert.That(ApiDescriptionExtensions.IsSimpleType(typeof(Model)), Is.False, "For type " + typeof(Model));
            }
        }

        private ApiDescription CreateApiDescription(string routeTemplate = "foo/bar")
        {
            return CreateApiDescription(httpConfiguration, routeTemplate);
        }

        public static ApiDescription CreateApiDescription(HttpConfiguration httpConfiguration, string routeTemplate = "foo/bar")
        {
            var api = new ApiDescription();

            api.Route = new HttpRoute(routeTemplate, new HttpRouteValueDictionary());

            var controllerDescriptor = new HttpControllerDescriptor(httpConfiguration, "Sample", typeof(SampleController));
            api.ActionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, controllerDescriptor.ControllerType.GetMethod("Put"));
            api.HttpMethod = HttpMethod.Put;
            api.Documentation = "Docs for " + routeTemplate;

            return api;
        }

        public class SampleController : ApiController
        {
            public object Put()
            {
                return "OK";
            }
        }
    }
}