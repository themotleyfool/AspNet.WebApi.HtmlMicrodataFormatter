using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public interface IHtmlMicrodataSerializer
    {
        IEnumerable<Type> SupportedTypes { get; }
        IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer);
    }

    public class DefaultSerializer : IHtmlMicrodataSerializer
    {
        public virtual IEnumerable<Type> SupportedTypes { get { throw new InvalidOperationException(); } }

        public virtual IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            Console.WriteLine("serialize {0} / {1}", propertyName, obj);

            if (obj == null)
            {
                yield return new XText(string.Empty);
                yield break;
            }

            if (obj is string || obj is ValueType)
            {
                var elem = new XElement("span", obj.ToString());
                if (!string.IsNullOrEmpty(propertyName))
                {
                    elem.SetAttributeValue("itemprop", propertyName);
                }
                yield return elem;
                yield break;
            }

            var items = obj as IEnumerable;
            if (items != null)
            {
                var x = items.OfType<object>().SelectMany(i => Serialize(propertyName, i, rootSerializer));
                foreach (var item in x)
                {
                    Console.WriteLine(item);
                    yield return item;
                }

                yield break;
            }

            yield return new XElement("dl",
                new XAttribute("itemtype", GetItemType(obj.GetType())),
                new XAttribute("itemscope", "itemscope"),
                BuildProperties(obj, rootSerializer));
        }

        protected virtual string GetItemType(Type type)
        {
            return "http://schema.org/Thing";
        }

        protected virtual IEnumerable<XObject> BuildProperties(object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            var dic = obj as IEnumerable<KeyValuePair<string, object>> ?? Reflect(obj);

            foreach (var kv in dic)
            {
                var o = kv.Value;

                yield return new XElement("dt",
                                          new XText(kv.Key));

                foreach (var propValue in BuildPropertyValue(kv.Key, o, rootSerializer))
                {
                    yield return new XElement("dd",
                                              propValue);
                }
            }
        }

        protected internal virtual IEnumerable<object> BuildPropertyValue(string propertyName, object propertyValue, IHtmlMicrodataSerializer rootSerializer)
        {
           
                yield return rootSerializer.Serialize(propertyName, propertyValue, rootSerializer);
           
        }

        protected internal virtual IEnumerable<KeyValuePair<string, object>> Reflect(object value)
        {
            var type = value.GetType();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                yield return new KeyValuePair<string, object>(prop.Name, prop.GetValue(value));
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                yield return new KeyValuePair<string, object>(field.Name, field.GetValue(value));
            }
        }
    }

    public class ToStringSerializer : DefaultSerializer
    {
        private readonly IEnumerable<Type> supportedTypes;

        public ToStringSerializer(params Type[] supportedTypes)
        {
            this.supportedTypes = supportedTypes;
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return supportedTypes; }
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            return new[]
                {
                    new XElement("span", obj.ToString())
                };
        }
    }

    public class UriSerializer : IHtmlMicrodataSerializer
    {
        public IEnumerable<Type> SupportedTypes { get { return new[] {typeof(Uri)}; } }

        public IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            var uri = (Uri) obj;

            var element = new XElement("a",
                new XAttribute("href", uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped)),
                new XText(uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.Unescaped)));

            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                element.SetAttributeValue("itemprop", propertyName);
            }

            return new[] {element};
        }
    }

    public class HtmlMicrodataFormatter : HyperMediaHtmlMediaTypeFormatter, IHtmlMicrodataSerializer
    {
        private readonly SerializerRegistry serializerRegistry = new SerializerRegistry();

        public HtmlMicrodataFormatter()
        {
            serializerRegistry.Register(new UriSerializer());
            serializerRegistry.Register(new ToStringSerializer(typeof(Version)));
        }

        public void RegisterSerializer(IHtmlMicrodataSerializer serializer)
        {
            serializerRegistry.Register(serializer);
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override IEnumerable<XObject> BuildBody(object value)
        {
            return Serialize(null, value, this);
        }

        public IEnumerable<Type> SupportedTypes { get { throw new InvalidOperationException(); } }

        public IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
            var type = obj == null ? typeof (string) : obj.GetType();
            var serializer = serializerRegistry.GetSerializer(type);

            return serializer.Serialize(propertyName, obj, rootSerializer);
        }
    }
}