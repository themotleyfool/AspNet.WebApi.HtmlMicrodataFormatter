using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public delegate IEnumerable<XObject> EntityHandler<in TEntity>(TEntity entity, SerializationContext context);
    public delegate IEnumerable<XObject> PropertyHandler<in TProperty>(string propertyName, TProperty value, SerializationContext context);

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

        protected override string GetItemType(Type type)
        {
            return ItemType ?? base.GetItemType(type);
        }

        public void Property<TProperty>(Expression<Func<T, TProperty>> expression, PropertyHandler<TProperty> propertyHandler)
        {
            var info = GetMemberInfo(expression.Body);
            propertyHandlers.Add(info.Name, propertyHandler);
        }

        public void Property(Expression<Func<T, object>> expression, EntityHandler<T> entityHandler)
        {
            var info = GetMemberInfo(expression.Body);

            entityHandlers.Add(info.Name, entityHandler);
        }

        public void Property<TProperty>(string propertyName, PropertyHandler<TProperty> propertyHandler)
        {
            propertyHandlers.Add(propertyName, propertyHandler);
        }

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
            if (entityHandlers.TryGetValue(propertyName, out handler))
            {
                return (IEnumerable<XObject>)handler.DynamicInvoke(entity, context);
            }
            if (propertyHandlers.TryGetValue(propertyName, out handler))
            {
                return (IEnumerable<XObject>)handler.DynamicInvoke(propertyName, propertyValue, context);
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

    }
}