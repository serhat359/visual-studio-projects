using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SharePointMvc.Helpers
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

        public static Stream CreateCopy(this Stream stream)
        {
            var newStream = new MemoryStream();
            stream.CopyTo(newStream);
            newStream.Position = 0;
            return newStream;
        }
    }
}