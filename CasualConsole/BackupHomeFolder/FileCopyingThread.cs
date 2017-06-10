using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace BackupHomeFolder
{
    class FileCopyingThread
    {
        public volatile bool continueCopy = false;

        ThreadStart threadAction;

        public FileCopyingThread(List<FileCopyInfo> filesToCopy, IntPtr windowHandle, long bytesToCopy, Label fileCopyLabel, List<string> filesToDelete)
        {
            threadAction = () =>
            {
                DoWork(filesToCopy, windowHandle, bytesToCopy, fileCopyLabel, filesToDelete);
            };

            Thread thread = new Thread(threadAction);
            thread.Start();
        }

        private void DoWork(List<FileCopyInfo> filesToCopy, IntPtr handle, long bytesToCopy, Label fileCopyLabel, List<string> filesToDelete)
        {
            continueCopy = true;
            long bytesCopied = 0;

            filesToDelete.Each((filePathToDelete, i) =>
            {
                try
                {
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
                    File.Copy(copyInfo.SourcePath, copyInfo.DestinationPath, true);
                }
                catch (UnauthorizedAccessException) { }

                TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.Normal);
                TaskbarProgress.SetValue(handle, bytesCopied, IfZero(bytesToCopy, 1));
                bytesCopied += copyInfo.FileSizeBytes;
                return continueCopy;
            });

            TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.Normal);
            TaskbarProgress.SetValue(handle, 1, 1);

            string labelText = continueCopy ? "Copying Completed" : "Copying Stopped";

            UpdateLabel(fileCopyLabel, labelText);
            MessageBox.Show(labelText);
            TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.NoProgress);
        }

        private void UpdateLabel(Label fileCopyLabel, string text)
        {
            if (fileCopyLabel.InvokeRequired)
            {
                fileCopyLabel.BeginInvoke((MethodInvoker)delegate ()
                {
                    fileCopyLabel.Text = text;
                    fileCopyLabel.Refresh();
                });
            }
            else
            {
                fileCopyLabel.Text = text;
            }
        }

        private static long IfZero(long num, long ifZeroVal)
        {
            return num != 0 ? num : ifZeroVal;
        }
    }
}
