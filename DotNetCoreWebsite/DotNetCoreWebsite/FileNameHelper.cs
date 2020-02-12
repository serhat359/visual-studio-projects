using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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
                fileNames = new Dictionary<string, string>();
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

        public string GetOriginalFileName(string fileName)
        {
            CheckFileNames();

            if (fileNames.TryGetValue(fileName, out var originalFileName))
                return originalFileName;
            else
                return null;
        }

        private void SaveFileNames()
        {
            File.WriteAllText(path: repositoryFileName, contents: JsonConvert.SerializeObject(this.fileNames));
        }

        private void CheckFileNames()
        {
            if (this.fileNames == null)
            {
                this.fileNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(repositoryFileName));
            }
        }
    }
}
