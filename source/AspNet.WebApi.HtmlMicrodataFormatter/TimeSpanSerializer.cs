using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// <para>
    /// Serializes <see cref="TimeSpan"/> instances using the
    /// html5 <c>time</c> tag, encoding the timespan as a duration
    /// in a format such as <c>"PT4H18M3S"</c>.
    /// </para>
    /// 
    /// <para>
    /// For more information on this format, see
    /// http://www.w3.org/TR/html-markup/time.html
    /// </para>
    /// </summary>
    public class TimeSpanSerializer : DefaultSerializer
    {
        public override IEnumerable<Type> SupportedTypes { get { return new[] {typeof(TimeSpan)}; } }

        public string TextFormat { get; set; }

        public TimeSpanSerializer()
        {
            TextFormat = "c";
        }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            var duration = (TimeSpan) obj;

            var element = new XElement("time",
                                       new XAttribute("datetime", XmlConvert.ToString(duration)),
                                       new XText(duration.ToString(TextFormat)));

            SetPropertyName(element, propertyName, context);

            return new[] {element};
        }
    }
}