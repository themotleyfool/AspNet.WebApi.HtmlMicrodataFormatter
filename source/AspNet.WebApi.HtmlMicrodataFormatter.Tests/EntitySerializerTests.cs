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
            var expected = RenderName("Name", entity.Name, context).ToString();
            Assert.That(elem.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void SetsItemPropWhenMissing()
        {
            serializer.Property(e => e.Name, (p, v, ctx) => new XElement("h6"));

            var result = (XElement)serializer.Serialize(null, entity, context).Single();

            var elem = result.Descendants().Single(e => e.Name == "h6");
            Assert.That(elem.Attribute("itemprop"), Is.Not.Null, "itemprop");
            Assert.That(elem.Attribute("itemprop").Value, Is.EqualTo("name"));
        }

        [Test]
        public void CustomPropertyHandlerAlternateSignature()
        {
            context.PropNameProvider = new IdentityPropNameProvider();
            serializer.Property(e => e.Name, RenderEntity);
            var result = (XElement)serializer.Serialize(null, entity, context).Single();

            var elem = result.Descendants().Single(e => e.Attribute("itemprop") != null && e.Attribute("itemprop").Value != "Id");

            var expected = RenderName("Name", entity.Name, context);

            Assert.That(elem.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void MetaProperty()
        {
            EntityHandler<Entity> handler = (e, _) => new XElement("blockquote", new XText(e.Name));

            serializer.Property("NoSuch", handler);

            var result = (XElement)serializer.Serialize(null, entity, context).Single();
            var elem = result.Descendants().Single(e => e.Name == "blockquote");

            Assert.That(elem.ToString(), Is.EqualTo("<blockquote itemprop=\"noSuch\">enty</blockquote>"));
        }

        private XElement RenderEntity(Entity e, SerializationContext context)
        {
            return new XElement("section", new XAttribute("itemprop", "content"), new XText(e.Name));
        }

        private XElement RenderName(string propName, string name, SerializationContext context)
        {
            return new XElement("section", new XAttribute("itemprop", "content"), new XText(name));
        }

    }
}