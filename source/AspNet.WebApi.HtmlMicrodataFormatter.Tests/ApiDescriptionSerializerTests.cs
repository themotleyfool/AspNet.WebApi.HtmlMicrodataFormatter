using NUnit.Framework;

namespace AspNet.WebApi.HtmlMicrodataFormatter.Tests
{
    [TestFixture]
    public class ApiDescriptionSerializerTests : SerializerTestBase<ApiDescriptionSerializer>
    {
        private SimpleApiDescription desc;

        [SetUp]
        public void SetUp()
        {
            desc = new SimpleApiDescription { Href = "/path/to/action", Method = "post", Name = "AnActionName" };
        }

        [Test]
        public void FormAttributes()
        {
            var form = serializer.BuildForm(desc, context);

            Assert.That(form.Attribute("action").Value, Is.EqualTo(desc.Href));
            Assert.That(form.Attribute("method").Value, Is.EqualTo(desc.Method));
            Assert.That(form.Attribute("name").Value, Is.EqualTo(context.FormatPropertyName(desc.Name)));
            Assert.That(form.Attribute("data-templated").Value, Is.EqualTo("false"));
            Assert.That(form.Attribute("data-rel").Value, Is.EqualTo(context.FormatPropertyName(desc.Name)));
        }
    }
}