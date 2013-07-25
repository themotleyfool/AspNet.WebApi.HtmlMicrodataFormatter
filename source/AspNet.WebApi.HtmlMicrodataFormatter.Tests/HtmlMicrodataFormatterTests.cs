using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class HtmlMicrodataFormatterTests
    {
        private HtmlMicrodataFormatter formatter;

        [SetUp]
        public void SetUp()
        {
            formatter = new HtmlMicrodataFormatter();
        }
        
        public class CtrTests : HtmlMicrodataFormatterTests
        {
            [Test]
            public void UsesCamelCasePropNameProvider()
            {
                Assert.That(formatter.PropNameProvider, Is.InstanceOf<CamelCasePropNameProvider>());
            }
        }

        public class SerializeTests : HtmlMicrodataFormatterTests
        {
            public class Sample
            {
                public Uri Image { get; set; }
            }

            public class SampleSerializer : DefaultSerializer
            {
                public override IEnumerable<Type> SupportedTypes
                {
                    get { return new[] {typeof (Sample)}; }
                }

                protected override string GetItemType(Type type)
                {
                    return "http://example.com/schema/MyCustomThing";
                }

                protected internal override IEnumerable<XObject> BuildPropertyValue(object obj, string propertyName, object propertyValue, SerializationContext context)
                {
                    if (propertyName == "Image")
                    {
                        return new[] {new XElement("img", new XAttribute("itemprop", propertyName))};
                    }

                    return base.BuildPropertyValue(obj, propertyName, propertyValue, context);
                }
            }

            [SetUp]
            public void RegisterSerializer()
            {
                formatter.RegisterSerializer(new SampleSerializer());
            }

            [Test]
            public void SerializeUriAsImg()
            {
                var result = formatter.BuildBody(new Sample {Image = new Uri("http://example.com/favicon.ico")});

                var actual = (XElement)result.Single();

                var elem = actual.Descendants().Where(n => n.Attribute("itemprop") != null).Single(n => n.Attribute("itemprop").Value == "Image");

                Assert.That(elem.Name, Is.EqualTo((XName)"img"));
            }

            [Test]
            public void SerializeWithCustomItemType()
            {
                var result = formatter.BuildBody(new Sample { Image = new Uri("http://example.com/favicon.ico") });
                
                var actual = result.OfType<XElement>().Single();

                Assert.That(actual.Attributes("itemtype").Select(a => a.Value), Is.EqualTo(new[] {"http://example.com/schema/MyCustomThing"}));
            }

            [Test]
            public void RemoveSerializer()
            {
                formatter.RemoveSerializer(formatter.DateTimeSerializer);
                var result = formatter.BuildBody(new DateTime()).Single();
                Assert.That(result.ToString(), Is.EqualTo("<span>1/1/0001 12:00:00 AM</span>"));
            }
        }

        public class BuildPropertyValueTests : HtmlMicrodataFormatterTests
        {
            [Test]
            public void String()
            {
                AssertResultEquals(formatter.BuildBody("string"), "<span>string</span>");
            }

            [Test]
            public void Int()
            {
                AssertResultEquals(formatter.BuildBody(32), "<span>32</span>");
            }

            [Test]
            public void Bool()
            {
                AssertResultEquals(formatter.BuildBody(true), "<span>True</span>");
            }

            [Test]
            public void Uri()
            {
                AssertResultEquals(formatter.BuildBody(new Uri("http://example.com/some%20path")), "<a href=\"http://example.com/some%20path\">http://example.com/some path</a>");
            }

            [Test]
            public void Null()
            {
                AssertResultEquals(formatter.BuildBody(null), "");
            }

            [Test]
            public void ArrayOfString()
            {
                AssertResultEquals(formatter.BuildBody(new[] { "s1", "s2" }), "<span>s1</span>", "<span>s2</span>");
            }

            [Test]
            public void Expando()
            {
                dynamic expando = new ExpandoObject();

                expando.Name = "Fred";

                var expected = new XElement("dl",
                    new XAttribute("itemtype", "http://schema.org/Thing"),
                    new XAttribute("itemscope", "itemscope"),
                    new XElement("dt", new XText("Name")),
                    new XElement("dd",
                        new XElement("span",
                            new XAttribute("itemprop", "name"),
                            new XText("Fred"))));

                AssertResultEquals(formatter.BuildBody(expando), expected.ToString());
            }
            
            [Test]
            public void OneDataDefinitionPerItem()
            {
                dynamic expando = new ExpandoObject();

                expando.Names = new[] {"Fred", "Ted"};

                var result = (XElement) Enumerable.Single(formatter.BuildBody(expando));

                Assert.That(result.Descendants().Count(e => e.Name == "dd"), Is.EqualTo(2));
            }

            [Test]
            public void SetItemPropOnDataList()
            {
                dynamic expando = new ExpandoObject();

                expando.Names = new[] { "Fred", "Ted" };

                var result = (XElement)Enumerable.Single(formatter.Serialize("PropName", expando, new SerializationContext { RootSerializer = formatter, PropNameProvider = new CamelCasePropNameProvider()}));

                Assert.That(result.Attribute("itemprop"), Is.Not.Null, "Should set itemprop attribute on <dl>");
                Assert.That(result.Attribute("itemprop").Value, Is.EqualTo("propName"));
            }

            private static void AssertResultEquals(IEnumerable<XObject> result, params object[] expectedValues)
            {
                Assert.That(result.Select(x => x.ToString()), Is.EqualTo(expectedValues));
            }
        }
    }
}