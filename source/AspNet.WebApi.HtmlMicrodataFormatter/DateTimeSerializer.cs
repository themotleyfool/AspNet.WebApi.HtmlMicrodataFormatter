using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class DateTimeSerializer : DefaultSerializer
    {
        public override IEnumerable<Type> SupportedTypes { get { return new[] {typeof(DateTime), typeof(DateTimeOffset)}; } }

        public string DataDateFormat { get; set; }
        public string TextDateFormat { get; set; }

        public DateTimeSerializer()
        {
            DataDateFormat = "s";
            TextDateFormat = "R";
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

            var element = new XElement("time",
                                       new XAttribute("datetime", date.ToString(DataDateFormat)),
                                       new XText(date.ToString(TextDateFormat)));

            SetPropertyName(element, propertyName, context);

            return new[] {element};
        }
    }
}