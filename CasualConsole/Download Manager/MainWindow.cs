﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Download_Manager
{
    public partial class MainWindow : Form
    {
        public static TableContents contents;

        public volatile Dictionary<WebClient, DataRowView> rowLookupByWebclient = new Dictionary<WebClient, DataRowView>();
        public volatile Dictionary<DataRow, TableContent> contentLookupByRow = new Dictionary<DataRow, TableContent>();
        BetterTimer timer = new BetterTimer();

        public MainWindow()
        {
            InitializeComponent();

            contents = TableContents.GetContents();

            SetupWindow();

            RefreshTableContents();

            ClearSelectedRows();

            timer.Tick += Timer_Tick;
            timer.Interval = 200; // millis
        }

        private void SetupWindow()
        {
            this.ResizeEnd += MainWindow_ResizeEnd;

            if (contents.Size.HasValue) this.Size = contents.Size.Value;
        }

        private void MainWindow_ResizeEnd(object sender, EventArgs e)
        {
            contents.Size = this.ClientSize;
            TableContents.SaveContents(contents);
        }

        private void DataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

            }
        }

        private void RefreshTableContents()
        {
            var table = new DataTable();
            foreach (var item in contents.Columns)
            {
                var column = table.Columns.Add(item.Name, item.Type);
            }

            foreach (var item in contents.TableData)
            {
                var newRow = table.Rows.Add(item.AsObjectArray());
                contentLookupByRow[newRow] = item;
            }

            dataGridView1.DataSource = table;
            dataGridView1.MouseDown += DataGridView1_MouseDown;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.ColumnWidthChanged += DataGridView1_ColumnWidthChanged;

            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                var column = dataGridView1.Columns[i];
                column.Width = contents.Columns[i].ColumnWidth;

                if (contents.Columns[i].IsInvisible)
                    column.Visible = false;
            }

            ClearSelectedRows();
        }

        private void ClearSelectedRows()
        {
            var rows = dataGridView1.SelectedRows;
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].Selected = false;
            }
        }

        private void addButton_Click(object sender, System.EventArgs e)
        {
            var window = new AddButtonWindow().ShowDialog();

            RefreshTableContents();
        }

        private void resumeButton_Click(object sender, System.EventArgs e)
        {
            var rows = dataGridView1.SelectedRows;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = (DataRowView)rows[i].DataBoundItem;
                if (row != null)
                {
                    var status = row.Row[Constants.Status];
                    if (GetStatus(status) == DownloadStatus.Stopped)
                    {
                        StartDownload(row);
                    }
                }
            }
        }

        public DownloadStatus GetStatus(object o)
        {
            var s = (string)o;
            var i = s.IndexOf(" ");
            s = i >= 0 ? s.Substring(i + 1) : s;

            if (Enum.TryParse<DownloadStatus>(s, out var status))
                return status;
            else
                return DownloadStatus.Stopped;
        }

        private void DataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                var column = dataGridView1.Columns[i];
                contents.Columns[i].ColumnWidth = column.Width;
            }

            TableContents.SaveContents(contents);
        }

        private void StartDownload(DataRowView row)
        {
            var url = row.Row[Constants.Url].ToString();
            var downloadPath = GetDownloadPath(row);

            Thread thread = new Thread(() =>
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var bytesDownloaded = contentLookupByRow[row.Row].BytesDownloaded;
                var client = new MyWebClient(bytesDownloaded);
                client.Headers["Referer"] = row.Row[Constants.Referer]?.ToString() ?? "";
                client.Headers["Accept"] = "video/webm,video/ogg,video/*;q=0.9,application/ogg;q=0.7,audio/*;q=0.6,*/*;q=0.5";
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(url), downloadPath);

                rowLookupByWebclient[client] = row;

                CheckTimer();
            });
            row.Row[Constants.Status] = DownloadStatus.Downloading;
            row.Row[Constants.Percentage] = 0;
            thread.Start();

            CheckTimer();
        }

        private void CheckTimer()
        {
            if (rowLookupByWebclient.Count == 0)
                timer.Stop();
            else if (timer.Enabled == false)
                timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (var row in rowLookupByWebclient.Values)
            {
                row.Row[Constants.Status] = row.Row[Constants.Percentage] + "% " + DownloadStatus.Downloading;
            }
        }

        private static string GetDownloadPath(DataRowView row)
        {
            var filename = row.Row[Constants.Filename].ToString();
            var downloadPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "Downloads", filename) + ".part";
            return downloadPath;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                //label2.Text = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                //progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());

                var row = rowLookupByWebclient[(WebClient)sender];
                row.Row[Constants.Percentage] = (int)percentage;
                if (string.IsNullOrEmpty(row.Row[Constants.FileSize]?.ToString()))
                {
                    row.Row[Constants.FileSize] = ((long?)totalBytes).ToFileSizeString();
                }

                var content = contentLookupByRow[row.Row];
                content.BytesDownloaded = (long)bytesIn;
                content.TotalBytes = (long)totalBytes;
            });
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                var row = rowLookupByWebclient[(WebClient)sender];

                var content = contentLookupByRow[row.Row];
                var isDownloadedCompletely = content.BytesDownloaded == content.TotalBytes;

                if (isDownloadedCompletely)
                {
                    row.Row[Constants.Status] = DownloadStatus.Completed;

                    var path = GetDownloadPath(row);
                    var newPath = path.Substring(0, path.IndexOf(".part"));

                    Lazy<(string restOfPath, string extension)> fileParts = new Lazy<(string, string)>(() =>
                    {
                        var filename = newPath.Substring(newPath.LastIndexOf('/') + 1);
                        int dotIndex = filename.IndexOf('.');
                        bool hasDot = dotIndex >= 0;

                        if (hasDot)
                            return (newPath.Substring(0, dotIndex), newPath.Substring(dotIndex));
                        else
                            return (newPath, "");
                    });

                    for (int i = 2; File.Exists(newPath); i++)
                    {
                        newPath = fileParts.Value.restOfPath + $" ({i})" + fileParts.Value.extension;
                    }

                    File.Move(path, newPath);

                    rowLookupByWebclient.Remove((WebClient)sender);
                }
                else
                {
                    StartDownload(row);
                }
            });

            CheckTimer();
        }
    }
}
