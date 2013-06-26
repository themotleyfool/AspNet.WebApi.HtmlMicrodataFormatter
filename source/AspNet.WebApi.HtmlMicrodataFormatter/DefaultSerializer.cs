using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class DefaultSerializer : IHtmlMicrodataSerializer
    {
        public virtual IEnumerable<Type> SupportedTypes { get { throw new InvalidOperationException(); } }

        public virtual IEnumerable<XObject> Serialize(string propertyName, object obj, IHtmlMicrodataSerializer rootSerializer)
        {
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
            if (items != null && !(obj is IEnumerable<KeyValuePair<string, object>>))
            {
                var x = items.OfType<object>().SelectMany(i => BuildPropertyValue(obj, propertyName, i, rootSerializer));
                foreach (var item in x)
                {
                    yield return item;
                }

                yield break;
            }

            var dataList = new XElement("dl",
                                      new XAttribute("itemtype", GetItemType(obj.GetType())),
                                      new XAttribute("itemscope", "itemscope"),
                                      BuildProperties(obj, rootSerializer));

            if (!string.IsNullOrEmpty(propertyName))
            {
                dataList.SetAttributeValue("itemprop", propertyName);
            }

            yield return dataList;
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

                yield return new XElement("dt", new XText(kv.Key));

                foreach (var propValue in BuildPropertyValue(obj, kv.Key, o, rootSerializer))
                {
                    yield return new XElement("dd", propValue);
                }
            }
        }

        protected internal virtual IEnumerable<XObject> BuildPropertyValue(object obj, string propertyName, object propertyValue, IHtmlMicrodataSerializer rootSerializer)
        {
            return rootSerializer.Serialize(propertyName, propertyValue, rootSerializer);
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
}