using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class DateTimeSerializer : DefaultSerializer
    {
        /// <summary>
        /// XML dateTime canonical representation. See http://www.w3.org/TR/xmlschema-2/#dateTime-canonical-representation.
        /// </summary>
        public const string SortableUtcFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        public override IEnumerable<Type> SupportedTypes { get { return new[] {typeof(DateTime), typeof(DateTimeOffset)}; } }

        /// <summary>
        /// The format to use when rendering the <c>datetime</c> attribute of the <c>time</c> element.
        /// Defaults to <see cref="SortableUtcFormat"/>
        /// </summary>
        public string DataFormat { get; set; }

        /// <summary>
        /// The format to use when rendering a DateTime into the body of the <c>time</c> element.
        /// Defaults to <c>"R"</c>, the RCF1123 format.
        /// </summary>
        public string TextFormat { get; set; }

        public DateTimeSerializer()
        {
            DataFormat = SortableUtcFormat;
            TextFormat = "R";
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            DateTime date;

            if (obj is DateTimeOffset)
            {
                date = ((DateTimeOffset) obj).DateTime;
            }
            else
            {
                date = (DateTime) obj;
            }

            if (date.Kind == DateTimeKind.Local)
            {
                date = date.ToUniversalTime();
            }

            var element = new XElement("time",
                                       new XAttribute("datetime", date.ToString(DataFormat)),
                                       new XText(date.ToString(TextFormat)));

            SetPropertyName(element, propertyName, context);

            return new[] {element};
        }
    }
}