using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DotNetCoreWebsite
{
    public class FileNameHelper
    {
        private static readonly string repositoryFileName = "filenames.json";

        public FileNameHelper()
        {
            if (!File.Exists(repositoryFileName))
            {
                SaveFileNames(new Dictionary<string, string>());
            }
        }

        public string CreateAlternativeFileName(string originalFileName)
        {
            var fileNames = GetFileNames();

            var newFileName = Guid.NewGuid().ToString().Replace("-", "").ToLowerInvariant();

            fileNames[newFileName] = originalFileName;

            SaveFileNames(fileNames);

            return newFileName;
        }

        public string? GetOriginalFileName(string fileName)
        {
            var fileNames = GetFileNames();

            if (fileNames.TryGetValue(fileName, out var originalFileName))
                return originalFileName;
            else
                return null;
        }

        private static void SaveFileNames(Dictionary<string, string> fileNames)
        {
            File.WriteAllText(path: repositoryFileName, contents: JsonSerializer.Serialize(fileNames));
        }

        private Dictionary<string, string> GetFileNames()
        {
            var text = File.ReadAllText(repositoryFileName);
            if (text == "")
                return new Dictionary<string, string>();
            return JsonSerializer.Deserialize<Dictionary<string, string>>(text)!;
        }
    }
}
