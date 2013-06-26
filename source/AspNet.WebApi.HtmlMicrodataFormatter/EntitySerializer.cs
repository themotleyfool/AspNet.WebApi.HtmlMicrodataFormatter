using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class EntitySerializer<T> : DefaultSerializer
    {
        private readonly IDictionary<string, Delegate> handlers =
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

        public void Property<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, IEnumerable<XObject>> handler)
        {
            var info = GetMemberInfo(expression.Body);

            handlers.Add(info.Name, handler);
        }

        protected internal override IEnumerable<XObject> BuildPropertyValue(object entity, string propertyName, object propertyValue, IHtmlMicrodataSerializer rootSerializer)
        {
            Delegate handler;
            if (handlers.TryGetValue(propertyName, out handler))
            {
                return (IEnumerable<XObject>)handler.DynamicInvoke(propertyValue);
            }

            return base.BuildPropertyValue(entity, propertyName, propertyValue, rootSerializer);
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