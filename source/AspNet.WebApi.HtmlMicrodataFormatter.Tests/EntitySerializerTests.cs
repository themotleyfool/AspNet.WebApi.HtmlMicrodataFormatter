using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class EntitySerializerTests
    {
        private EntitySerializer<Entity> serializer;
        private Entity entity;

        [SetUp]
        public void SetUp()
        {
            serializer = new EntitySerializer<Entity>();
            entity = new Entity {Id = 2, Name = "enty"};
        }

        public class Entity
        {
            public string Name { get; set; }
            public int Id { get; set; }
        }

        [Test]
        public void BehavesLikeDefaultSerializerByDefault()
        {
            var defaultSerializer = new DefaultSerializer();
            
            var result = serializer.Serialize(null, entity, defaultSerializer).Single();
            var expected = defaultSerializer.Serialize(null, entity, defaultSerializer).Single();

            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void SetItemType()
        {
            serializer.ItemType = "example";
            var result = (XElement) serializer.Serialize(null, entity, new DefaultSerializer()).Single();
            Assert.That(result.Attribute("itemtype").Value, Is.EqualTo("example"));
        }

        [Test]
        public void CustomPropertyHandler()
        {
            serializer.Property(e => e.Name, RenderName);
            var result = (XElement)serializer.Serialize(null, entity, new DefaultSerializer()).Single();

            var elem = result.Descendants().Single(e => e.Attribute("itemprop") != null && e.Attribute("itemprop").Value != "Id");
            
            var expected = RenderName(entity.Name).Single();

            Assert.That(elem.ToString(), Is.EqualTo(expected.ToString()));
        }

        private IEnumerable<XObject> RenderName(string name)
        {
            var elem = new XElement("section", new XAttribute("itemprop", "content"), new XText(name));
            return new[] {elem};
        }
    }
}