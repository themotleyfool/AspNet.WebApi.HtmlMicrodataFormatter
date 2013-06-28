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

        [Test]
        public void RelativeUri()
        {
            var result = (XElement)serializer.Serialize("MyProp", new Uri("/some path?query=y+y#chapter2", UriKind.Relative), context).Single();

            Assert.That(result.Attribute("href").Value, Is.EqualTo("/some%20path?query=y+y#chapter2"));
        }

        [Test]
        public void Empty()
        {
            var result = serializer.Serialize("MyProp", new Uri("", UriKind.Relative), context);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Null()
        {
            var result = serializer.Serialize("MyProp", null, context);

            Assert.That(result, Is.Empty);
        }
    }
}