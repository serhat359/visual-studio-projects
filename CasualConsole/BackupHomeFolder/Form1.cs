﻿using MyThreadProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BackupHomeFolder
{
    public partial class Form1 : Form
    {
        FileCopyingThread thread = null;

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
                if (!Directory.Exists(sourceFolder))
                {
                    MessageBox.Show("Source folder doesn't exist");
                }
                else if (!Directory.Exists(destinationFolder))
                {
                    MessageBox.Show("Destination folder doesn't exist");
                }
                else
                {
                    checkButton.Enabled = false;

                    AppSetting setting = Settings.Get();
                    setting.SourceFolder = sourceFolder;
                    setting.DestinationFolder = destinationFolder;
                    Settings.Set(setting);

                    IntPtr windowHandle = this.Handle;

                    MyThread<int> actionthread = MyThread.DoInThread(true, () =>
                    {
                        CheckResult checkResult = CheckDifferences(sourceFolder, destinationFolder);
                        
                        checkButton.ThreadSafe(x => x.Enabled = true);

                        string dialogtext = string.Format("{0} files and {1} will be copied, {2} files and {3} will be deleted, continue?", checkResult.FileCountToCopy, ByteSize.SizeSuffix(checkResult.BytesToCopy), checkResult.FileCountToDelete, ByteSize.SizeSuffix(checkResult.BytesToDelete));

                        DialogResult dialogResult = MessageBox.Show(dialogtext, "Copy Files", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            thread = new FileCopyingThread(checkResult.FilesToCopy, windowHandle, checkResult.BytesToCopy, fileCopyLabel, checkResult.FilesToDelete, destinationFolder);
                        }

                        return 0;
                    });
                }
            }
            else
            {
                MessageBox.Show("Choose both folders");
            }
        }

        private CheckResult CheckDifferences(string sourceFolder, string destinationFolder)
        {
            string[] subfolders = new string[] { "Pictures", "Videos", "Desktop", "Documents", "Downloads", "Music", "workspace" };

            // Copying files to hard drive
            int fileCountToCopy = 0;
            long bytesToCopy = 0;

            var threadToCopy = MyThread.DoInThread(true, () =>
            {
                List<FileCopyInfo> result = GetFilesToCopy(sourceFolder, destinationFolder, subfolders, ref fileCountToCopy, ref bytesToCopy);

                return result;
            });

            // Deleting files from hard drive
            int fileCountToDelete = 0;
            long bytesToDelete = 0;

            var threadToDelete = MyThread.DoInThread(true, () =>
            {
                List<string> result = GetFilesToDelete(sourceFolder, destinationFolder, subfolders, ref fileCountToDelete, ref bytesToDelete);

                return result;
            });

            List<FileCopyInfo> filesToCopy = threadToCopy.Await();
            List<string> filesToDelete = threadToDelete.Await();

            return new CheckResult
            {
                FilesToCopy = filesToCopy,
                FileCountToCopy = fileCountToCopy,
                BytesToCopy = bytesToCopy,
                FilesToDelete = filesToDelete,
                FileCountToDelete = fileCountToDelete,
                BytesToDelete = bytesToDelete
            };
        }

        private List<string> GetFilesToDelete(string sourceFolder, string destinationFolder, string[] subfolders, ref int fileCountToDelete, ref long bytesToDelete)
        {
            List<string> filesToDelete = new List<string>();

            foreach (var subfolderName in subfolders)
            {
                List<string> allFiles = DirSearch(Path.Combine(destinationFolder, subfolderName), true);

                foreach (string destFilePath in allFiles)
                {
                    string srcFilePath = destFilePath.Replace(destinationFolder, sourceFolder);

                    var destFileInfo = new Delimon.Win32.IO.FileInfo(destFilePath);

                    if (!File.Exists(srcFilePath))
                    {
                        fileCountToDelete++;
                        bytesToDelete += destFileInfo.Length;
                        filesToDelete.Add(destFilePath);
                    }
                }
            }

            return filesToDelete;
        }

        private List<FileCopyInfo> GetFilesToCopy(string sourceFolder, string destinationFolder, string[] subfolders, ref int fileCountToCopy, ref long bytesToCopy)
        {
            List<FileCopyInfo> filesToCopy = new List<FileCopyInfo>();

            foreach (var subfolderName in subfolders)
            {
                List<string> allFiles = DirSearch(Path.Combine(sourceFolder, subfolderName), false);

                foreach (string oldFilePath in allFiles)
                {
                    string newFilePath = oldFilePath.Replace(sourceFolder, destinationFolder);

                    var newFileInfo = new Delimon.Win32.IO.FileInfo(newFilePath);
                    var oldFileInfo = new Delimon.Win32.IO.FileInfo(oldFilePath);
                    
                    if (!newFileInfo.Exists || oldFileInfo.LastWriteTime > newFileInfo.LastWriteTime)
                    {
                        fileCountToCopy++;
                        bytesToCopy += oldFileInfo.Length;
                        filesToCopy.Add(new FileCopyInfo { SourcePath = oldFilePath, DestinationPath = newFilePath, FileSizeBytes = oldFileInfo.Length });
                    }
                }
            }

            return filesToCopy;
        }

        private static bool IsNotEmpty(string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        private List<string> DirSearch(string sDir, bool suppressExceptions)
        {
            List<string> files = new List<string>();

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d, suppressExceptions));
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception excpt)
            {
                if (!suppressExceptions)
                    MessageBox.Show(excpt.Message);
            }

            return files;
        }

        private void stopCopybutton_Click(object sender, EventArgs e)
        {
            if (thread != null)
                thread.continueCopy = false;
        }
    }

    class FileCopyInfo
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long FileSizeBytes { get; set; }
    }

    class CheckResult
    {
        public long BytesToCopy { get; set; }
        public long BytesToDelete { get; set; }
        public int FileCountToCopy { get; set; }
        public int FileCountToDelete { get; set; }
        public List<FileCopyInfo> FilesToCopy { get; set; }
        public List<string> FilesToDelete { get; set; }
    }
}
