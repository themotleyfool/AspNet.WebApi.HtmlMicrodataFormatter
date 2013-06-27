using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class ToStringSerializerTests
    {
        private ToStringSerializer serializer;
        private SerializationContext context;

        [SetUp]
        public void SetUp()
        {
            serializer = new ToStringSerializer();
            context = new SerializationContext(serializer, new CamelCasePropNameProvider());
        }

        [Test]
        public void NullPropertyName()
        {
            var result = serializer.Serialize(null, this, context);

            Assert.That(result.Single().ToString(), Is.EqualTo("<span>" + ToString() + "</span>"));
        }

        [Test]
        public void IncludePropertyName()
        {
            var result = (XElement)serializer.Serialize("MyProp", this, context).Single();
            var attr = result.Attribute("itemprop");
            Assert.That(attr, Is.Not.Null, "Should include 'itemprop' attribute in <span> element.");
            Assert.That(attr.ToString(), Is.EqualTo(new XAttribute("itemprop", "myProp").ToString()));
        }

        public override string ToString()
        {
            return "Sample ToString Implementation";
        }
    }
}