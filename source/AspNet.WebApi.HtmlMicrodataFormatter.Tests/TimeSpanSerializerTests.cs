using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class TimeSpanSerializerTests : SerializerTestBase<TimeSpanSerializer>
    {
        [Test]
        public void TimeSpand()
        {
            var duration = TimeSpan.FromHours(4).Add(TimeSpan.FromMinutes(9)).Add(TimeSpan.FromSeconds(5));

            var result = (XElement)serializer.Serialize("MyProp", duration, context).Single();

            var expected = new XElement("time",
                                        new XAttribute("datetime", XmlConvert.ToString(duration)),
                                        new XAttribute("itemprop", "myProp"),
                                        new XText(duration.ToString("c")));

            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void UsesChosenTextFormat()
        {
            serializer.TextFormat = "h' hours, 'm' minutes, and 's' seconds'";

            var duration = TimeSpan.FromHours(4).Add(TimeSpan.FromMinutes(9)).Add(TimeSpan.FromSeconds(5));

            var result = (XElement) serializer.Serialize("MyProp", duration, context).Single();
            var text = result.Nodes().OfType<XText>().Single();

            Assert.That(text.ToString(), Is.EqualTo(duration.ToString(serializer.TextFormat)));
        }
    }
}