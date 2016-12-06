﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BackupHomeFolder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            AppSetting setting = Settings.Get();
            sourceTextBox.Text = setting.SourceFolder;
            destinationTextBox.Text = setting.DestinationFolder;
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            string sourceFolder = sourceTextBox.Text;
            string destinationFolder = destinationTextBox.Text;

            if (IsNotEmpty(sourceFolder) && IsNotEmpty(destinationFolder))
            {
                AppSetting setting = Settings.Get();
                setting.SourceFolder = sourceFolder;
                setting.DestinationFolder = destinationFolder;
                Settings.Set(setting);

                string[] subfolders = new string[] { "Pictures", "Videos", "Desktop", "Documents" };

                long fileCountToCopy = 0;
                long bytesToCopy = 0;

                List<FileCopyInfo> filesToCopy = new List<FileCopyInfo>();

                foreach (var subfolderName in subfolders)
                {
                    List<String> allFiles = DirSearch(Path.Combine(sourceFolder, subfolderName));

                    foreach (String oldFilePath in allFiles)
                    {
                        string newFilePath = oldFilePath.Replace(sourceFolder, destinationFolder);

                        FileInfo newFileInfo = new FileInfo(newFilePath);
                        FileInfo oldFileInfo = new FileInfo(oldFilePath);

                        if (!newFileInfo.Exists || oldFileInfo.LastWriteTime > newFileInfo.LastWriteTime)
                        {
                            fileCountToCopy++;
                            bytesToCopy += oldFileInfo.Length;
                            filesToCopy.Add(new FileCopyInfo { SourcePath = oldFilePath, DestinationPath = newFilePath, FileSize = oldFileInfo.Length });
                        }
                    }
                }

                string dialogtext = string.Format("{0} files and {1} will be copied, continue?", fileCountToCopy, ByteSize.SizeSuffix(bytesToCopy));

                DialogResult dialogResult = MessageBox.Show(dialogtext, "Copy Files", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    long bytesCopied = 0;
                    filesToCopy.Each((copyInfo, i) =>
                    {
                        fileCopyLabel.Text = string.Format("Copying {0} of {1} files", (i + 1), filesToCopy.Count);
                        fileCopyLabel.Refresh();
                        Directory.CreateDirectory(Path.GetDirectoryName(copyInfo.DestinationPath));
                        File.Copy(copyInfo.SourcePath, copyInfo.DestinationPath, true);
                        TaskbarProgress.SetState(this.Handle, TaskbarProgress.TaskbarStates.Normal);
                        TaskbarProgress.SetValue(this.Handle, bytesCopied, IfZero(bytesToCopy, 1));
                        bytesCopied += copyInfo.FileSize;
                    });

                    TaskbarProgress.SetState(this.Handle, TaskbarProgress.TaskbarStates.Normal);
                    TaskbarProgress.SetValue(this.Handle, 1, 1);

                    fileCopyLabel.Text = "Copying Completed";
                    fileCopyLabel.Refresh();
                    MessageBox.Show("Copying Completed");
                    TaskbarProgress.SetState(this.Handle, TaskbarProgress.TaskbarStates.NoProgress);
                }
            }
            else
            {
                MessageBox.Show("Choose both folders");
            }
        }

        private static bool IsNotEmpty(string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        private List<String> DirSearch(string sDir)
        {
            List<String> files = new List<String>();

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (System.Exception excpt)
            {
                //MessageBox.Show(excpt.Message);
            }

            return files;
        }

        private long IfZero(long num, long ifZeroVal)
        {
            return num != 0 ? num : ifZeroVal;
        }
    }

    class FileCopyInfo
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long FileSize { get; set; }
    }
}
