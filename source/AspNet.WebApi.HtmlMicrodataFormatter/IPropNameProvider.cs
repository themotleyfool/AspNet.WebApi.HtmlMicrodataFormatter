using System.Text;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    /// <summary>
    /// Provides a facility for converting property names on .net objects
    /// to suitable names to use in HTML Microdata itemprop attributes.
    /// </summary>
    public interface IPropNameProvider
    {
        string GetItemProp(string propertyName);
    }

    /// <summary>
    /// Returns property names unmodified.
    /// </summary>
    public class IdentityPropNameProvider : IPropNameProvider
    {
        public string GetItemProp(string propertyName)
        {
            return propertyName;
        }
    }

    /// <summary>
    /// Converts property names to camel case.
    /// </summary>
    public class CamelCasePropNameProvider : IPropNameProvider
    {
        public string GetItemProp(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) return propertyName;

            var sb = new StringBuilder(propertyName);

            sb[0] = char.ToLowerInvariant(sb[0]);

            return sb.ToString();
        }
    }
}