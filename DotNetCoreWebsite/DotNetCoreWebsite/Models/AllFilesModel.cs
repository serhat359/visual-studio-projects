using System;
using System.Collections.Generic;

namespace DotNetCoreWebsite.Models
{
    public class AllFilesModel
    {
        public List<FileInfoModel> FileList { get; set; }
        public string BackFolderPath { get; set; }
        public string CurrentPath { get; set; }
    }

    public class FileInfoModel
    {
        public bool IsFolder { get; set; }
        public string Name { get; set; }
        public long? FileSize { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public TimeSpan? Age { get; set; }

        public string FileSizeString { get { return FileSize.ToFileSizeString(); } }
        public string ModifiedDateString { get { return ModifiedDate == null ? "":  ModifiedDate.Value.ToString("yyy-MM-dd HH:mm UTC");  } }
        public string AgeString { get { return Age.ToAgeString(); } }
    }
}
