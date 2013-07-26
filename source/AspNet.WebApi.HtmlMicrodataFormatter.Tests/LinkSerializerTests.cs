using System;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class LinkSerializerTests : SerializerTestBase<LinkSerializer>
    {
        [Test]
        public void NullPropertyName()
        {
            var result = serializer.Serialize(null, new Link("http://example.com/some%20path", "self", "my self"), context);

            Assert.That(result.Single().ToString(), Is.EqualTo("<a href=\"http://example.com/some%20path\" rel=\"self\">my self</a>"));
        }

        [Test]
        public void IncludePropertyName()
        {
            var result = (XElement)serializer.Serialize("MyProp", new Link("http://example.com/some%20path", "self", "my self"), context).Single();
            var attr = result.Attribute("itemprop");
            Assert.That(attr, Is.Not.Null, "Should include 'itemprop' attribute in <a> element.");
            Assert.That(attr.ToString(), Is.EqualTo(new XAttribute("itemprop", "myProp").ToString()));
        }

        [Test]
        public void Null()
        {
            var result = serializer.Serialize("MyProp", null, context);

            Assert.That(result, Is.Empty);
        }
    }
}