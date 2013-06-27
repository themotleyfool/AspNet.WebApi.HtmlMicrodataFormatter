using System;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class DateTimeSerializerTests
    {
        private DateTimeSerializer serializer;
        private SerializationContext context;

        [SetUp]
        public void SetUp()
        {
            serializer = new DateTimeSerializer();
            context = new SerializationContext(serializer, new CamelCasePropNameProvider());
        }

        [Test]
        public void DefaultFormats()
        {
            Assert.That(serializer.DataDateFormat, Is.EqualTo("s"));
            Assert.That(serializer.TextDateFormat, Is.EqualTo("R"));
        }

        [Test]
        public void DateTimeOffset()
        {
            var date = new DateTimeOffset(2012, 5, 3, 17, 59, 43, 0, TimeSpan.Zero);

            var result = (XElement)serializer.Serialize("MyProp", date, context).Single();

            var expected = new XElement("time",
                new XAttribute("datetime", date.ToString(serializer.DataDateFormat)),
                new XAttribute("itemprop", "myProp"),
                new XText(date.ToString(serializer.TextDateFormat)));

            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void LocalDateTime()
        {
            var date = new DateTimeOffset(2012, 5, 3, 17, 59, 43, 0, TimeSpan.FromHours(5)).LocalDateTime;

            var result = (XElement)serializer.Serialize("MyProp", date, context).Single();

            var expected = new XElement("time",
                new XAttribute("datetime", date.ToString(serializer.DataDateFormat)),
                new XAttribute("itemprop", "myProp"),
                new XText(date.ToString(serializer.TextDateFormat)));

            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void UtcDateTime()
        {
            var date = new DateTime(2012, 5, 3, 17, 59, 43, 0, DateTimeKind.Utc);

            var result = (XElement)serializer.Serialize("MyProp", date, context).Single();

            var expected = new XElement("time",
                new XAttribute("datetime", date.ToString(serializer.DataDateFormat)),
                new XAttribute("itemprop", "myProp"),
                new XText(date.ToString(serializer.TextDateFormat)));

            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
        }
    }
}