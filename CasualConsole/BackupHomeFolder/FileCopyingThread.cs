using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace BackupHomeFolder
{
    class FileCopyingThread
    {
        public volatile bool continueCopy = false;

        ThreadStart threadAction;

        public FileCopyingThread(List<FileCopyInfo> filesToCopy, IntPtr windowHandle, long bytesToCopy, Label fileCopyLabel, List<string> filesToDelete, string destinationFolder)
        {
            threadAction = () =>
            {
                DoWork(filesToCopy, windowHandle, bytesToCopy, fileCopyLabel, filesToDelete, destinationFolder);
            };

            Thread thread = new Thread(threadAction);
            thread.Start();
        }

        private void DoWork(List<FileCopyInfo> filesToCopy, IntPtr handle, long bytesToCopy, Label fileCopyLabel, List<string> filesToDelete, string destinationFolder)
        {
            continueCopy = true;
            long bytesCopied = 0;

            filesToDelete.Each((filePathToDelete, i) =>
            {
                try
                {
                    File.SetAttributes(filePathToDelete, FileAttributes.Normal);
                    File.Delete(filePathToDelete);
                }
                catch (UnauthorizedAccessException) { }

                return continueCopy;
            });

            filesToCopy.Each((copyInfo, i) =>
            {
                UpdateLabel(fileCopyLabel, string.Format("Copying {0} of {1} files", (i + 1), filesToCopy.Count));
                Directory.CreateDirectory(Path.GetDirectoryName(copyInfo.DestinationPath));

                try
                {
                    try { File.SetAttributes(copyInfo.DestinationPath, FileAttributes.Normal); } catch (FileNotFoundException) { }
                    File.Copy(copyInfo.SourcePath, copyInfo.DestinationPath, true);
                }
                catch (UnauthorizedAccessException) { }

                TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.Normal);
                TaskbarProgress.SetValue(handle, bytesCopied, IfZero(bytesToCopy, 1));
                bytesCopied += copyInfo.FileSizeBytes;
                return continueCopy;
            });

            DeleteEmptyFolders(destinationFolder);

            TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.Normal);
            TaskbarProgress.SetValue(handle, 1, 1);

            string labelText = continueCopy ? "Copying Completed" : "Copying Stopped";

            UpdateLabel(fileCopyLabel, labelText);
            MessageBox.Show(labelText);
            TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.NoProgress);
        }

        private bool DeleteEmptyFolders(string folder)
        {
            bool containsFolder = Directory.EnumerateDirectories(folder).Select(subFolder => DeleteEmptyFolders(subFolder)).Any(x => x == true);
            bool containsFile = Directory.EnumerateFiles(folder).Any();

            if (!containsFolder && !containsFile)
            {
                Directory.Delete(folder);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void UpdateLabel(Label fileCopyLabel, string text)
        {
            fileCopyLabel.ThreadSafe(x =>
            {
                x.Text = text;
                x.Refresh();
            });
        }

        private static long IfZero(long num, long ifZeroVal)
        {
            return num != 0 ? num : ifZeroVal;
        }
    }
}
