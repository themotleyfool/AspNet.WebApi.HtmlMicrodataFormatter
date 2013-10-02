using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class XmlCommentHtmlTransformerTests
    {
        private XmlCommentHtmlTransformer transformer;

        [SetUp]
        public void SetUp()
        {
            transformer = new XmlCommentHtmlTransformer();
        }

        [Test]
        [TestCase("example")]
        [TestCase("summary")]
        [TestCase("remarks")]
        [TestCase("returns")]
        public void TransformBlockTags(string elementName)
        {
            var xml = string.Format("<root><{0}>text</{0}></root>", elementName);

            var result = transformer.Transform(new XmlTextReader(new StringReader(xml)));

            Assert.That(ToString(result), Is.EqualTo(string.Format("<root><section class=\"{0}\">text</section></root>", elementName)));
        }

        [Test]
        [TestCase("c", "code")]
        [TestCase("para", "p")]
        public void TransformInlineTags(string elementName, string expectedName)
        {
            var xml = string.Format("<root><{0}>text</{0}></root>", elementName);

            var result = transformer.Transform(new XmlTextReader(new StringReader(xml)));

            Assert.That(ToString(result), Is.EqualTo(string.Format("<root><{0}>text</{0}></root>", expectedName)));
        }

        [Test]
        public void TransformNestedTags()
        {
            var result = transformer.Transform(new XmlTextReader(new StringReader("<root><summary><c>a</c><c>b</c></summary></root>")));

            Assert.That(ToString(result), Is.EqualTo("<root><section class=\"summary\"><code>a</code><code>b</code></section></root>"));
        }

        [Test]
        public void TransformCode()
        {
            var result = transformer.Transform(new XmlTextReader(new StringReader("<root><code>a</code></root>")));

            Assert.That(ToString(result), Is.EqualTo("<root><code class=\"pre\">a</code></root>"));
        }

        [Test]
        public void TransformEmptyElementWithCref()
        {
            var result = transformer.Transform(new XmlTextReader(new StringReader("<root><see cref=\"a\"/></root>")));

            Assert.That(ToString(result), Is.EqualTo("<root>a</root>"));
        }

        [Test]
        public void TransformEmptyElementWithName()
        {
            var result = transformer.Transform(new XmlTextReader(new StringReader("<root><paramref name=\"a\"/></root>")));

            Assert.That(ToString(result), Is.EqualTo("<root>a</root>"));
        }

        private string ToString(XPathNavigator node)
        {
            var output = new StringWriter(CultureInfo.InvariantCulture);
            var settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Auto;

            using (var writer = XmlWriter.Create(output, settings))
            {
                writer.WriteNode(node, true);
            }

            return output.ToString();
        }
    }
}
