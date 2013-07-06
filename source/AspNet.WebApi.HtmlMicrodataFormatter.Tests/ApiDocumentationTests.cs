using System.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class ApiDocumentationTests
    {
        private SimpleApiDocumentation doc;

        [SetUp]
        public void SetUp()
        {
            doc = new SimpleApiDocumentation();
        }

        [Test]
        public void PreservesOrder()
        {
            doc.Add("Users", Create());
            doc.Add("Accounts", Create());

            Assert.That(doc.Resources.Select(s => s.Name).ToArray(), Is.EqualTo(new[] {"Users", "Accounts"}));
        }

        [Test]
        public void GroupsByResourceName()
        {
            doc.Add("a", Create());
            doc.Add("a", Create());
            doc.Add("a", Create());

            Assert.That(doc.Resources.Single().Actions.Count(), Is.EqualTo(3));
        }

        public SimpleApiDescription Create()
        {
            return new SimpleApiDescription { Href = "/sample/action", Method = "get", Name = "Users", Parameters = new SimpleApiParameterDescriptor[0]};
        }

    }
}