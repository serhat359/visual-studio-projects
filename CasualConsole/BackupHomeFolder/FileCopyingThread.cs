using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace BackupHomeFolder
{
    class FileCopyingThread
    {
        private readonly CancellationTokenSource stopCopySource;

        public FileCopyingThread(IReadOnlyList<FileCopyInfo> filesToCopy, IntPtr windowHandle, long bytesToCopy, Label fileCopyLabel, IReadOnlyList<string> filesToDelete, string destinationFolder)
        {
            this.stopCopySource = new CancellationTokenSource();

            Task task = Task.Run(() =>
            {
                DoWork(filesToCopy, windowHandle, bytesToCopy, fileCopyLabel, filesToDelete, destinationFolder, stopCopySource.Token);
            });
        }

        public void StopCopy()
        {
            stopCopySource.Cancel();
        }

        private void DoWork(IReadOnlyList<FileCopyInfo> filesToCopy, IntPtr handle, long bytesToCopy, Label fileCopyLabel, IReadOnlyList<string> filesToDelete, string destinationFolder, CancellationToken stopCopy)
        {
            long bytesCopied = 0;

            for (int i = 0; i < filesToDelete.Count; i++)
            {
                if (stopCopy.IsCancellationRequested)
                    break;

                string filePathToDelete = filesToDelete[i];
                UpdateLabel(fileCopyLabel, string.Format("Deleting {0} of {1} files", (i + 1), filesToDelete.Count));

                try
                {
                    File.SetAttributes(filePathToDelete, FileAttributes.Normal);
                    File.Delete(filePathToDelete);
                }
                catch (UnauthorizedAccessException) { }
            }

            for (int i = 0; i < filesToCopy.Count; i++)
            {
                if (stopCopy.IsCancellationRequested)
                    break;

                var copyInfo = filesToCopy[i];
                UpdateLabel(fileCopyLabel, string.Format("Copying {0} of {1} files", (i + 1), filesToCopy.Count));
                string directoryPath = Delimon.Win32.IO.Path.GetDirectoryName(copyInfo.DestinationPath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                try
                {
                    try { Delimon.Win32.IO.File.SetAttributes(copyInfo.DestinationPath, Delimon.Win32.IO.FileAttributes.Normal); }
                    catch (System.IO.FileNotFoundException) { }
                    catch (System.Exception e) { if (!e.Message.StartsWith("The system cannot find the file specified")) throw; }
                    Delimon.Win32.IO.File.Copy(copyInfo.SourcePath, copyInfo.DestinationPath, true);
                }
                catch (UnauthorizedAccessException) { }

                TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.Normal);
                TaskbarProgress.SetValue(handle, bytesCopied, IfZero(bytesToCopy, 1));
                bytesCopied += copyInfo.FileSizeBytes;
            };

            DeleteEmptyFolders(destinationFolder);

            TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.Normal);
            TaskbarProgress.SetValue(handle, 1, 1);

            string labelText = !stopCopy.IsCancellationRequested ? "Copying Completed" : "Copying Stopped";

            UpdateLabel(fileCopyLabel, labelText);
            MessageBox.Show(labelText);
            TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.NoProgress);
        }

        private bool DeleteEmptyFolders(string folder)
        {
            bool containsFolder = Directory.EnumerateDirectories(folder)
                .Select(subFolder => DeleteEmptyFolders(subFolder))
                .ToList()
                .Any(x => x == true);
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
