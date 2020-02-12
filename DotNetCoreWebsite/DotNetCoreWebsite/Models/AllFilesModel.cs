using System.Collections.Generic;
using System.Net;

namespace DotNetCoreWebsite.Models
{
    public class AllFilesModel
    {
        public List<FileInfo> FileList { get; set; }
        public string BackFolderPath { get; set; }
        public string CurrentPath { get; set; }
    }

    public class FileInfo
    {
        public bool IsFolder { get; set; }
        public string Name { get; set; }
        public long? FileSize { get; set; }

        public string FileSizeString { get { return FileSize.ToFileSizeString(); } }
    }
}
