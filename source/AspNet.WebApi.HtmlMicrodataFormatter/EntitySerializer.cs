using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public delegate IEnumerable<XObject> PropertyHandler<in TProperty>(string s, TProperty q);
    public delegate IEnumerable<XObject> PropertyHandlerWithoutName<in TProperty>(TProperty q);

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

        public void Property<TProperty>(Expression<Func<T, TProperty>> expression, PropertyHandlerWithoutName<TProperty> propertyHandler)
        {
            PropertyHandler<TProperty> wrapper = (name, value) => propertyHandler(value);
            Property(expression, wrapper);
        }

        public void Property<TProperty>(Expression<Func<T, TProperty>> expression, PropertyHandler<TProperty> propertyHandler)
        {
            var info = GetMemberInfo(expression.Body);
            propertyHandlers.Add(info.Name, propertyHandler);
        }

        public void Property(Expression<Func<T, object>> expression, Func<T, IEnumerable<XObject>> entityHandler)
        {
            var info = GetMemberInfo(expression.Body);

            entityHandlers.Add(info.Name, entityHandler);
        }

        protected internal override IEnumerable<XObject> BuildPropertyValue(object entity, string propertyName, object propertyValue, SerializationContext context)
        {
            Delegate handler;
            if (entityHandlers.TryGetValue(propertyName, out handler))
            {
                return (IEnumerable<XObject>)handler.DynamicInvoke(entity);
            }
            if (propertyHandlers.TryGetValue(propertyName, out handler))
            {
                return (IEnumerable<XObject>)handler.DynamicInvoke(propertyName, propertyValue);
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