using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class DefaultSerializerTests
    {
        private DefaultSerializer serializer;

        [SetUp]
        public void SetUp()
        {
            serializer = new DefaultSerializer();
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