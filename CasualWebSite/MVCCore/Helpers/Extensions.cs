using System.Text;
using System.Xml;

namespace MVCCore.Helpers
{
    public static class Extensions
    {
        public static string Beautify(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

        public static async Task<IEnumerable<T>> AwaitAllAsync<T>(this IEnumerable<Task<T>> source)
        {
            var res = new List<T>();
            foreach (var item in source)
            {
                res.Add(await item);
            }
            return res;
        }

        public static IEnumerable<XmlNode> GetAllNodesRecursive(this XmlNode node)
        {
            yield return node;

            foreach (XmlNode item in node.ChildNodes)
            {
                var allNodes = item.GetAllNodesRecursive();

                foreach (var newNode in allNodes)
                {
                    yield return newNode;
                }
            }
        }

        public static XmlNode? SearchByTag(this XmlNode node, string name, string? @class = null)
        {
            var allNodes = node.GetAllNodesRecursive();
            if (@class == null)
                return allNodes.FirstOrDefault(c => c.TagName == name);
            else
                return allNodes.FirstOrDefault(c => c.TagName == name && c.Attributes?["class"] == @class);
        }

        public static string BeforeFirst(this string s, string needle)
        {
            var index = s.IndexOf(needle);

            if (index >= 0)
                return s.Substring(0, index);
            else
                return s;
        }
    }
}
