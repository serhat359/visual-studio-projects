using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DotNetCoreWebsite
{
    public class FileNameHelper
    {
        private Dictionary<string, string> fileNames = null;
        private readonly string repositoryFileName = "filenames.json";

        public FileNameHelper()
        {
            if (!File.Exists(repositoryFileName))
            {
                fileNames = new();
                SaveFileNames();
            }
        }

        public string CreateAlternativeFileName(string originalFileName)
        {
            CheckFileNames();

            var newFileName = Guid.NewGuid().ToString().Replace("-", "").ToLowerInvariant();

            fileNames[newFileName] = originalFileName;

            SaveFileNames();

            return newFileName;
        }

        public string? GetOriginalFileName(string fileName)
        {
            CheckFileNames();

            if (fileNames.TryGetValue(fileName, out var originalFileName))
                return originalFileName;
            else
                return null;
        }

        private void SaveFileNames()
        {
            File.WriteAllText(path: repositoryFileName, contents: JsonSerializer.Serialize(this.fileNames));
        }

        private void CheckFileNames()
        {
            this.fileNames ??= JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(repositoryFileName));
        }
    }
}
