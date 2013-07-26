using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class EntitySerializerTests : SerializerTestBase<EntitySerializer<EntitySerializerTests.Entity>>
    {
        private Entity entity;
        private DefaultSerializer defaultSerializer;

        [SetUp]
        public void SetUp()
        {
            entity = new Entity {Id = 2, Name = "enty"};
            defaultSerializer = new DefaultSerializer();
        }

        public class Entity
        {
            public string Name { get; set; }
            public int Id { get; set; }
        }

        [Test]
        public void BehavesLikeDefaultSerializerByDefault()
        {
            var result = serializer.Serialize(null, entity, context).Single();
            var expected = defaultSerializer.Serialize(null, entity, context).Single();

            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void SetItemType()
        {
            serializer.ItemType = "example";
            var result = (XElement)serializer.Serialize(null, entity, context).Single();
            Assert.That(result.Attribute("itemtype").Value, Is.EqualTo("example"));
        }

        [Test]
        public void CustomPropertyHandler()
        {
            context.PropNameProvider = new IdentityPropNameProvider();
            serializer.Property(e => e.Name, RenderName);
            var result = (XElement)serializer.Serialize(null, entity, context).Single();

            var elem = result.Descendants().Single(e => e.Attribute("itemprop") != null && e.Attribute("itemprop").Value != "Id");
            
            var expected = RenderName(entity.Name).Single();

            Assert.That(elem.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void CustomPropertyHandlerWithName()
        {
            serializer.Property(e => e.Name, RenderProperty);
            var result = (XElement)serializer.Serialize(null, entity, context).Single();

            var elem = result.Descendants().Single(e => e.Attribute("itemprop") != null && e.Attribute("itemprop").Value != "Id");

            var expected = RenderProperty("Name", entity.Name).Single();

            Assert.That(elem.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void CustomPropertyHandlerAlternateSignature()
        {
            context.PropNameProvider = new IdentityPropNameProvider();
            serializer.Property(e => e.Name, RenderEntity);
            var result = (XElement)serializer.Serialize(null, entity, context).Single();

            var elem = result.Descendants().Single(e => e.Attribute("itemprop") != null && e.Attribute("itemprop").Value != "Id");

            var expected = RenderName(entity.Name).Single();

            Assert.That(elem.ToString(), Is.EqualTo(expected.ToString()));
        }

        private IEnumerable<XObject> RenderEntity(Entity e)
        {
            yield return new XElement("section", new XAttribute("itemprop", "content"), new XText(e.Name));
        }

        private IEnumerable<XObject> RenderName(string name)
        {
            var elem = new XElement("section", new XAttribute("itemprop", "content"), new XText(name));
            return new[] {elem};
        }

        private IEnumerable<XObject> RenderProperty(string propertyName, string value)
        {
            var elem = new XElement("section", new XAttribute("itemprop", propertyName), new XText(value));
            return new[] { elem };
        }


    }
}