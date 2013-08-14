using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class HtmlMediaTypeFormatterTests
    {
        private TestableHtmlMediaTypeFormatter formatter;

        [SetUp]
        public void SetUp()
        {
            formatter = new TestableHtmlMediaTypeFormatter();
        }

        [Test]
        public void NoTitle()
        {
            var head = formatter.BuildHeadElements(null, null);
            Assert.That(head.OfType<XElement>().Where(e => e.Name == "title"), Is.Empty);
            Assert.That(formatter.Title, Is.Empty);
        }

        [Test]
        public void GetTitle()
        {
            formatter.AddHeadContent(new XElement("title", new XText("a title")));
            Assert.That(formatter.Title, Is.EqualTo("a title"));
        }

        [Test]
        public void SetTitle()
        {
            formatter.Title = "some title";
            var head = formatter.BuildHeadElements(null, null);
            Assert.That(head.OfType<XElement>().Single(e => e.Name == "title").ToString(), Is.EqualTo("<title>some title</title>"));
        }

        [Test]
        public void ReplaceTitle()
        {
            formatter.AddHeadContent(new XElement("title", new XText("default")));
            formatter.Title = "some title";
            var head = formatter.BuildHeadElements(null, null);
            Assert.That(head.OfType<XElement>().Single(e => e.Name == "title").ToString(), Is.EqualTo("<title>some title</title>"));
        }

        class TestableHtmlMediaTypeFormatter : HtmlMediaTypeFormatter
        {
            public override IEnumerable<XObject> BuildBody(object value, HttpRequestMessage request)
            {
                throw new NotImplementedException();
            }

            
        }
    }
}