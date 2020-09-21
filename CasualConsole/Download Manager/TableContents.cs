using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Download_Manager
{
    public class TableContents
    {
        private const string contentsFile = "contents.json";

        public System.Drawing.Size? Size { get; set; }
        public List<Column> Columns { get; set; }
        public List<TableContent> TableData { get; set; }

        public static TableContents GetContents()
        {
            if (File.Exists(contentsFile))
            {
                TableContents tableContents = JsonConvert.DeserializeObject<TableContents>(File.ReadAllText(contentsFile, Encoding.UTF8));

                var defaultColumns = GetColumns();

                if (tableContents.Columns.Count != defaultColumns.Count)
                {
                    for (int i = tableContents.Columns.Count; i < defaultColumns.Count; i++)
                        tableContents.Columns.Add(defaultColumns[i]);
                }
                return tableContents;
            }
            else
            {
                var tableContents = new TableContents
                {
                    Columns = GetColumns(),
                    TableData = new List<TableContent>
                    {
                    }
                };

                SaveContents(tableContents);

                return tableContents;
            }
        }

        public static List<Column> GetColumns()
        {
            var columns = new List<Column> {
                        new Column{ Name = Constants.Filename, Type = typeof(string), ColumnWidth = 100 },
                        new Column{ Name = Constants.FileSize, Type = typeof(string), ColumnWidth = 100 },
                        new Column{ Name = Constants.Status, Type = typeof(string), ColumnWidth = 100 },
                        new Column{ Name = Constants.Url, Type = typeof(string), ColumnWidth = 100 },
                        new Column{ Name = Constants.Referer, Type = typeof(string), ColumnWidth = 100 },
                        new Column{ Name = Constants.Percentage, Type = typeof(int), IsInvisible = true },
                        new Column{ Name = Constants.FileSizeLong, Type = typeof(long), IsInvisible = true },
                    };
            return columns;
        }

        public static void SaveContents(TableContents contents)
        {
            File.WriteAllText(path: contentsFile, contents: JsonConvert.SerializeObject(contents), encoding: Encoding.UTF8);
        }
    }

    public class TableContent
    {
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public DownloadStatus Status { get; set; }
        public string Url { get; set; }
        public string Referer { get; set; }

        public long? FileSizeLong { get; set; }
        public long BytesDownloadedThisSession { get; set; }
        public long TotalBytes { get; set; }
        public string TempDownloadPath { get; set; }
        public string RealDownloadPath { get; set; }
        public long BytesDownloadedBefore { get; set; }

        [JsonIgnore]
        public int? Percentage { get; set; }

        public object[] AsObjectArray()
        {
            return new object[] { this.FileName, this.FileSize, this.Status, this.Url, this.Referer, this.Percentage, this.FileSizeLong };
        }
    }

    public class Column
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public int ColumnWidth { get; set; }
        public bool IsInvisible { get; set; }
    }

    public enum DownloadStatus : int
    {
        Stopped = 0,
        Downloading = 1,
        Completed = 2,
        Paused = 3
    }

}
