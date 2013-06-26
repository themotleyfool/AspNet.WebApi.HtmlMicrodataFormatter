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

        [SetUp]
        public void SetUp()
        {
            serializer = new UriSerializer();
        }

        [Test]
        public void NullPropertyName()
        {
            var result = serializer.Serialize(null, new Uri("http://example.com/some%20path"), new DefaultSerializer());

            Assert.That(result.Single().ToString(), Is.EqualTo("<a href=\"http://example.com/some%20path\">http://example.com/some path</a>"));
        }

        [Test]
        public void IncludePropertyName()
        {
            var result = (XElement)serializer.Serialize("MyProp", new Uri("http://example.com/some%20path"), new DefaultSerializer()).Single();
            var attr = result.Attribute("itemprop");
            Assert.That(attr, Is.Not.Null, "Should include 'itemprop' attribute in <a> element.");
            Assert.That(attr.ToString(), Is.EqualTo(new XAttribute("itemprop", "MyProp").ToString()));
        }

        public override string ToString()
        {
            return "Sample ToString Implementation";
        }
    }
}