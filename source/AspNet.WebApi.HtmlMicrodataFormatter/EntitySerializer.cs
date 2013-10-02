using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.Routing;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    

    /// <summary>
    /// Creates an XElement from a property on a given entity.
    /// </summary>
    public delegate XElement PropertyHandler<in TProperty>(string propertyName, TProperty value, SerializationContext context);

    /// <summary>
    /// Creates an XElement from a given entity. This delegate may be used
    /// to generate computed elements that combine multiple properties from
    /// a given entity.
    /// </summary>
    public delegate XElement EntityHandler<in TEntity>(TEntity entity, SerializationContext context);

    public class EntitySerializer<T> : DefaultSerializer
    {
        private readonly IDictionary<string, Delegate> entityHandlers =
            new Dictionary<string, Delegate>();
        private readonly IDictionary<string, Delegate> propertyHandlers =
            new Dictionary<string, Delegate>();

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new[] {typeof(T)}; }
        }

        public string ItemType { get; set; }

        protected override string GetItemType(Type type, SerializationContext context)
        {
            return ItemType ?? base.GetItemType(type, context);
        }

        public PropertyPart<TProperty> Property<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return new PropertyPart<TProperty>(this, expression);
        }

        /// <summary>
        /// Adds a <see cref="PropertyHandler{TProperty}"/> to convert a property to an <see cref="XElement"/>.
        /// If the handler does not set the <c>itemprop</c> attribute on the returned element, it will be
        /// set automatically.
        /// </summary>
        public void Property<TProperty>(Expression<Func<T, TProperty>> expression, PropertyHandler<TProperty> propertyHandler)
        {
            var info = GetMemberInfo(expression.Body);
            propertyHandlers.Add(info.Name, propertyHandler);
        }

        /// <summary>
        /// Adds a <see cref="EntityHandler{TEntity}"/> to convert a property to an <see cref="XElement"/>.
        /// If the handler does not set the <c>itemprop</c> attribute on the returned element, it will be
        /// set automatically.
        /// </summary>
        public void Property(Expression<Func<T, object>> expression, EntityHandler<T> entityHandler)
        {
            var info = GetMemberInfo(expression.Body);

            entityHandlers.Add(info.Name, entityHandler);
        }

        /// <summary>
        /// Adds a <see cref="EntityHandler{TEntity}"/> to create an <see cref="XElement"/>. This overload
        /// can be used to generate a computed property when such a property is not present on the entity.
        /// If the handler does not set the <c>itemprop</c> attribute on the returned element, it will be
        /// set automatically.
        /// </summary>
        public void Property<TProperty>(string propertyName, PropertyHandler<TProperty> propertyHandler)
        {
            propertyHandlers.Add(propertyName, propertyHandler);
        }

        /// <summary>
        /// Adds a <see cref="EntityHandler{TEntity}"/> to create an <see cref="XElement"/>. This overload
        /// can be used to generate a computed property when such a property is not present on <typeparamref name="T"/>.
        /// If the handler does not set the <c>itemprop</c> attribute on the returned element, it will be
        /// set automatically.
        /// </summary>
        public void Property(string propertyName, EntityHandler<T> entityHandler)
        {
            entityHandlers.Add(propertyName, entityHandler);
        }

        protected internal override IEnumerable<KeyValuePair<string, object>> Reflect(object value)
        {
            var dic = base.Reflect(value).ToDictionary(kv => kv.Key, kv => kv.Value);

            var missing = entityHandlers.Keys.Union(propertyHandlers.Keys).Except(dic.Keys);

            foreach (var k in missing)
            {
                dic.Add(k, null);
            }

            return dic;
        }

        protected internal override IEnumerable<XObject> BuildPropertyValue(object entity, string propertyName, object propertyValue, SerializationContext context)
        {
            Delegate handler;
            XElement result = null;
            bool handled = false;

            if (entityHandlers.TryGetValue(propertyName, out handler))
            {
                result = (XElement)handler.DynamicInvoke(entity, context);
                handled = true;
            }
            if (propertyHandlers.TryGetValue(propertyName, out handler))
            {
                result = (XElement)handler.DynamicInvoke(propertyName, propertyValue, context);
                handled = true;
            }

            if (handled && result == null) return Enumerable.Empty<XObject>();

            if (handled)
            {
                if (result.Attribute("itemprop") == null)
                {
                    SetPropertyName(result, propertyName, context);    
                }
                return new[] {result};
            }
            
            return base.BuildPropertyValue(entity, propertyName, propertyValue, context);
        }

        private MemberInfo GetMemberInfo(Expression expression)
        {
            MemberExpression memberExpression;

            if (expression.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression;
                memberExpression = (MemberExpression)body.Operand;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = (MemberExpression)expression;
            }
            else
            {
                throw new InvalidOperationException("Unsupported expression " + expression.NodeType);
            }

            return memberExpression.Member;
        }

        public class PropertyPart<TProperty>
        {
            private readonly EntitySerializer<T> outer;
            private readonly Expression<Func<T, TProperty>> expression;

            internal PropertyPart(EntitySerializer<T> outer, Expression<Func<T, TProperty>> expression)
            {
                this.outer = outer;
                this.expression = expression;
            }

            public void AsLink(string routeName, Func<TProperty, object> getRouteData)
            {
                outer.Property(expression, (propName, value, context) =>
                {
                    var urlHelper = new UrlHelper(context.Request);
                    var link = urlHelper.Link(routeName, getRouteData(value));
                    if (link == null)
                    {
                        throw new InvalidOperationException("No route matched '" + routeName + "' with the supplied route data.");
                    }
                    
                    return new XElement("a",
                        new XAttribute("href", link),
                        new XText(value.ToString()));
                });
            }

            public void AsImage(Func<TProperty, string> getImageUrl, Func<TProperty, string> getAltText)
            {
                outer.Property(expression, (propName, value, context) => new XElement("img",
                    new XAttribute("src", getImageUrl(value)),
                    new XAttribute("alt", getAltText(value))));
            }
        }
    }
}