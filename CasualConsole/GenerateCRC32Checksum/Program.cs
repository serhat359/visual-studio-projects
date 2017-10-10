using System;
using System.Linq;
using System.Text;
using System.IO;

namespace GenerateCRC32Checksum
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory() + "\\";

            string[] filenames = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            var codelist = filenames.Select(x => new { filename = x.Replace(path,""), code = GetCRC(x) }).Where(x => x.code != null).ToList();

            if (codelist.Count > 0)
            {
                string sfvFileName = "Checksum.sfv";
                string sfvPath = Path.Combine(path, sfvFileName);

                File.WriteAllLines(sfvPath, codelist.Select(x => string.Format("{0} {1}", x.filename, x.code)), Encoding.UTF8);
            }
        }

        static string GetCRC(string filename)
        {
            int length = filename.Length;

            for (int i = 0; i < length; i++)
            {
                int bracIndex = filename.IndexOf('[', i);

                if (bracIndex >= 0)
                {
                    int bracEndIndex = filename.IndexOf(']', bracIndex);

                    if (bracEndIndex - bracIndex == 9)
                    {
                        string code = filename.Substring(bracIndex + 1, 8);
                        if (code.All(c => IsHexadecimal(c)))
                            return code;
                    }
                }
                else
                    break;

                i = bracIndex + 1;
            }

            return null;
        }

        private static bool IsHexadecimal(char c)
        {
            Func<char, char, char, bool> between = (ch, start, end) => ch >= start && ch <= end;

            return between(c, '0', '9') || between(c, 'a', 'f') || between(c, 'A', 'F');
        }
    }
}
