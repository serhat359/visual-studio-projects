using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GenerateCRC32Checksum
{
    class Program
    {
        static Regex regex = new Regex(@"\[[a-zA-Z0-9]{8}\]");

        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory() + "\\";

            string[] filenames = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            var codelist = filenames.Select(x => new { filename = x.Replace(path, ""), code = GetCRC(x) }).Where(x => x.code != null).ToList();

            if (codelist.Count > 0)
            {
                string sfvFileName = "Checksum.sfv";
                string sfvPath = Path.Combine(path, sfvFileName);

                File.WriteAllLines(sfvPath, codelist.Select(x => string.Format("{0} {1}", x.filename, x.code)), Encoding.UTF8);
            }
        }

        static string GetCRC(string filename)
        {
            var matchResult = regex.Match(filename);

            if (matchResult != Match.Empty)
            {
                return matchResult.Value.Substring(1, 8);
            }

            return null;
        }
    }
}
