using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.XPath;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class XmlCommentHtmlTransformer
    {
        private static readonly ISet<string> SectionTags = new HashSet<string>(new[] { "example", "exception", "permission", "remarks", "returns", "summary", "typeparam" });

        private static readonly IDictionary<string, string> ReplaceTags = new Dictionary<string, string>
            {
                {"c", "code"},
                {"para", "p"}
            };

        public XPathNavigator Transform(XmlReader reader)
        {
            var doc = new XmlDocument();

            doc.Load(reader);

            Transform(doc, new[] {doc.DocumentElement});

            return doc.CreateNavigator();
        }

        private void Transform(XmlDocument doc, IEnumerable<XmlElement> nodes)
        {
            foreach (var node in nodes)
            {
                Transform(doc, node.ChildNodes.OfType<XmlElement>().ToArray());

                XmlNode newChild = null;

                string replacement;

                if (node.Name == "code")
                {
                    node.SetAttribute("class", "pre");
                }
                else if (ReplaceTags.TryGetValue(node.Name, out replacement))
                {
                    newChild = doc.CreateElement(replacement);
                    newChild.InnerXml = node.InnerXml;
                }
                else if (SectionTags.Contains(node.Name))
                {
                    var elem = doc.CreateElement("section");
                    elem.SetAttribute("class", node.Name);
                    elem.InnerXml = node.InnerXml;
                    newChild = elem;
                }
                else if (node.IsEmpty)
                {
                    var text = node.GetAttribute("cref");
                    if (string.IsNullOrEmpty(text))
                    {
                        text = node.GetAttribute("name");
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        newChild = doc.CreateTextNode(text);    
                    }
                }

                if (newChild != null)
                {
                    node.ParentNode.ReplaceChild(newChild, node);
                }
            }
        }
    }
}