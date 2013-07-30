using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Routing;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class DefaultSerializer : IHtmlMicrodataSerializer
    {
        public virtual IEnumerable<Type> SupportedTypes { get { throw new InvalidOperationException(); } }

        public virtual IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            if (obj == null)
            {
                yield return new XText(string.Empty);
                yield break;
            }

            if (obj is string || obj is ValueType)
            {
                var elem = new XElement("span", obj.ToString());
                SetPropertyName(elem, propertyName, context);
                yield return elem;
                yield break;
            }

            var items = obj as IEnumerable;
            if (items != null && !(obj is IEnumerable<KeyValuePair<string, object>>))
            {
                var x = items.OfType<object>().SelectMany(i => BuildPropertyValue(obj, propertyName, i, context));
                foreach (var item in x)
                {
                    yield return item;
                }

                yield break;
            }

            var dataList = new XElement("dl",
                                      new XAttribute("itemtype", GetItemType(obj.GetType(), context)),
                                      new XAttribute("itemscope", "itemscope"),
                                      BuildProperties(obj, context));

            SetPropertyName(dataList, propertyName, context);

            yield return dataList;
        }

        protected virtual void SetPropertyName(XElement element, string propertyName, SerializationContext context)
        {
            if (string.IsNullOrEmpty(propertyName)) return;

            element.SetAttributeValue("itemprop", context.PropNameProvider.GetItemProp(propertyName));
        }

        protected virtual string GetItemType(Type type, SerializationContext context)
        {
            var request = context.Request;
            string result = null;
            if (TypeDocumentationRouteAvailable(request))
            {
                result = new UrlHelper(request).Link(RouteNames.TypeDocumentation, new { typeName = type.FullName });        
            }

            return result ?? "http://schema.org/Thing";
        }

        private static bool TypeDocumentationRouteAvailable(HttpRequestMessage request)
        {
            return request.GetConfiguration().Routes.ContainsKey(RouteNames.TypeDocumentation);
        }

        protected internal virtual IEnumerable<XObject> BuildProperties(object obj, SerializationContext context)
        {
            var dic = obj as IEnumerable<KeyValuePair<string, object>> ?? Reflect(obj);
            return BuildProperties(obj, dic, context);
        }

        protected internal virtual IEnumerable<XObject> BuildProperties(object parent, IEnumerable<KeyValuePair<string, object>> items, SerializationContext context)
        {
            foreach (var kv in items)
            {
                var o = kv.Value;
                var first = true;
                
                foreach (var propValue in BuildPropertyValue(parent, kv.Key, o, context))
                {
                    if (IsEmpty(propValue)) continue;

                    if (first)
                    {
                        first = false;
                        yield return new XElement("dt", new XText(kv.Key));
                    }


                    yield return new XElement("dd", propValue);
                }
            }
        }

        protected virtual bool IsEmpty(XObject obj)
        {
            var text = obj as XText;

            return obj == null || text != null && string.IsNullOrEmpty(text.Value);
        }

        protected internal virtual IEnumerable<XObject> BuildPropertyValue(object parent, string propertyName, object propertyValue, SerializationContext context)
        {
            return context.RootSerializer.Serialize(propertyName, propertyValue, context);
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