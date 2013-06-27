using System;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class UriSerializerTests
    {
        private UriSerializer serializer;
        private SerializationContext context;

        [SetUp]
        public void SetUp()
        {
            serializer = new UriSerializer();
            context = new SerializationContext(serializer, new CamelCasePropNameProvider());
        }

        [Test]
        public void NullPropertyName()
        {
            var result = serializer.Serialize(null, new Uri("http://example.com/some%20path"), context);

            Assert.That(result.Single().ToString(), Is.EqualTo("<a href=\"http://example.com/some%20path\">http://example.com/some path</a>"));
        }

        [Test]
        public void IncludePropertyName()
        {
            var result = (XElement)serializer.Serialize("MyProp", new Uri("http://example.com/some%20path"), context).Single();
            var attr = result.Attribute("itemprop");
            Assert.That(attr, Is.Not.Null, "Should include 'itemprop' attribute in <a> element.");
            Assert.That(attr.ToString(), Is.EqualTo(new XAttribute("itemprop", "myProp").ToString()));
        }
    }
}