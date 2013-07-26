using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class DefaultSerializerTests : SerializerTestBase<DefaultSerializer>
    {
        public class BuildPropertiesTests : DefaultSerializerTests
        {
            [Test]
            public void OmitEmptyProperty()
            {
                var props = new[] {new KeyValuePair<string, object>("empty", null)};

                var result = serializer.BuildProperties(this, props, context);

                Assert.That(result, Is.Empty);
            }

            [Test]
            public void IncludeDataDefinitionPerElement()
            {
                var props = new[] { new KeyValuePair<string, object>("ListOfThings", new[] {"a", "b"}) };

                var result = serializer.BuildProperties(this, props, context);

                var expected = new[]
                    {
                        new XElement("dt", new XText("ListOfThings")),
                        new XElement("dd", new XElement("span", new XAttribute("itemprop", "listOfThings"), new XText("a"))),
                        new XElement("dd", new XElement("span", new XAttribute("itemprop", "listOfThings"), new XText("b")))
                    }.Select(i => i.ToString()).ToArray();

                Assert.That(result.Select(r => r.ToString()), Is.EqualTo(expected));
            }
        }

        public class ReflectPropertyTests : DefaultSerializerTests
        {
            public class Sample
            {
                public string MyProp { get; set; }
            }

            [Test]
            public void ReflectPublicProperties()
            {
                var result = serializer.Reflect(new Sample { MyProp = "hello" })
                                       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                Assert.That(result, Is.EquivalentTo(new Dictionary<string, object> { { "MyProp", "hello" } }));
            }
        }

        public class ReflectFieldTests : DefaultSerializerTests
        {
            public class Sample
            {
                public string MyField;
            }

            [Test]
            public void ReflectPublicFields()
            {
                var result = serializer.Reflect(new Sample { MyField = "world" })
                                       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                Assert.That(result, Is.EquivalentTo(new Dictionary<string, object> { { "MyField", "world" } }));
            }
        }
    }
}