namespace apPatcherApp
{
    using Microsoft.COM;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class compressedArchiveOperator
    {
        public IInArchive ArchiveIn;
        public IOutArchive ArchiveOut;
        public static ProgressBar attachedProgressBar = null;
        public static Label attachedStatus = null;
        public static string creating_archive = "";
        public bool failure;
        public static string fileInRar = "";
        public static string fileInRarCRC = "";
        public static string outFileSet = "";
        public Unrar unrar;

        private void AttachHandlers(Unrar unrar)
        {
            unrar.ExtractionProgress += new ExtractionProgressHandler(this.unrar_ExtractionProgress);
            unrar.MissingVolume += new MissingVolumeHandler(this.unrar_MissingVolume);
            unrar.PasswordRequired += new PasswordRequiredHandler(this.unrar_PasswordRequired);
        }

        public bool extract(string archiveName, uint fileNumber, string arcType, ProgressBar progress, Label status)
        {
            Program.form.waitForFreeMemory(status);
            attachedProgressBar = progress;
            attachedStatus = status;
            status.Text = "Extracting File " + this.extracting_file;
            progress.Maximum = 1;
            progress.Value = 0;
            Application.DoEvents();
            bool flag = false;
            KnownSevenZipFormat format = this.stringToSevenZipFormat(arcType);
            switch (format)
            {
                case KnownSevenZipFormat.SevenZip:
                case KnownSevenZipFormat.Zip:
                    break;

                case KnownSevenZipFormat.Rar:
                    return this.extractRar(archiveName, progress, status, true);

                default:
                    showError("1.0");
                    break;
            }
            using (SevenZipFormat format2 = new SevenZipFormat(SevenZipDllPath))
            {
                if (this.ArchiveIn != null)
                {
                    Marshal.ReleaseComObject(this.ArchiveIn);
                    for (int i = 0; i < 5; i++)
                    {
                        Application.DoEvents();
                    }
                }
                this.ArchiveIn = format2.CreateInArchive(SevenZipFormat.GetClassIdFromKnownFormat(format));
                if (this.ArchiveIn == null)
                {
                    showError("1.1");
                    flag = false;
                }
                try
                {
                    using (InStreamWrapper wrapper = new InStreamWrapper(System.IO.File.OpenRead(archiveName)))
                    {
                        IArchiveOpenCallback openArchiveCallback = new ArchiveOpenCallback();
                        if (this.ArchiveIn.Open(wrapper, 0x20000L, openArchiveCallback) != 0)
                        {
                            showError("1.2");
                            flag = false;
                        }
                        else
                        {
                            PropVariant variant = new PropVariant();
                            this.ArchiveIn.GetProperty(fileNumber, ItemPropId.kpidPath, ref variant);
                            string fileName = (string) variant.GetObject();
                            this.ArchiveIn.Extract(new uint[] { fileNumber }, 1, 0, new ArchiveExtractCallback(fileNumber, fileName));
                            flag = true;
                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(this.ArchiveIn);
                }
            }
            Program.form.waitForFreeMemory(status);
            return flag;
        }

        public bool extractRar(string archiveName, ProgressBar progress, Label status, bool singleFile = false)
        {
            attachedProgressBar = progress;
            attachedStatus = status;
            if (singleFile && (fileInRar == ""))
            {
                return false;
            }
            this.unrar = new Unrar();
            this.AttachHandlers(this.unrar);
            for (int i = 0; i < outFileSet.Length; i++)
            {
                if (outFileSet.Substring((outFileSet.Length - 1) - i, 1) == "/")
                {
                    outFileSet = outFileSet.Substring(0, outFileSet.Length - i);
                    break;
                }
            }
            this.unrar.DestinationPath = outFileSet;
            this.unrar.Open(archiveName, Unrar.OpenMode.Extract);
            while (this.unrar.ReadHeader())
            {
                if (!singleFile)
                {
                    fileInRar = this.unrar.CurrentFile.FileName;
                }
                if (this.unrar.CurrentFile.FileName == fileInRar)
                {
                    progress.Maximum = 100;
                    progress.Value = 0;
                    status.Text = "Extracting File 0%";
                    Application.DoEvents();
                    this.unrar.Extract();
                }
                else
                {
                    this.unrar.Skip();
                }
            }
            this.unrar.Close();
            status.Text = "Extracting File Complete";
            progress.Maximum = 4;
            progress.Value = 4;
            Application.DoEvents();
            return true;
        }

        public int findFirstExtInArchive(string ext, string archiveName, string arcType, ProgressBar progress, Label status)
        {
            status.Text = "Scanning Archive " + archiveName;
            progress.Maximum = 4;
            progress.Value = 0;
            Application.DoEvents();
            int num = -1;
            using (SevenZipFormat format = new SevenZipFormat(SevenZipDllPath))
            {
                KnownSevenZipFormat sevenZip = KnownSevenZipFormat.SevenZip;
                arcType = arcType.ToLower();
                switch (arcType)
                {
                    case "rar":
                        sevenZip = KnownSevenZipFormat.Rar;
                        break;

                    case "zip":
                        sevenZip = KnownSevenZipFormat.Zip;
                        break;

                    case "7z":
                        sevenZip = KnownSevenZipFormat.SevenZip;
                        break;

                    default:
                        showError("0.1");
                        break;
                }
                IInArchive o = format.CreateInArchive(SevenZipFormat.GetClassIdFromKnownFormat(sevenZip));
                if (o == null)
                {
                    showError("0.2");
                    num = -1;
                }
                try
                {
                    using (InStreamWrapper wrapper = new InStreamWrapper(System.IO.File.OpenRead(archiveName)))
                    {
                        IArchiveOpenCallback openArchiveCallback = new ArchiveOpenCallback();
                        if (o.Open(wrapper, 0x40000L, openArchiveCallback) != 0)
                        {
                            showError("0.3");
                            num = -1;
                        }
                        uint numberOfItems = o.GetNumberOfItems();
                        fileInRar = "";
                        for (int i = 0; i < numberOfItems; i++)
                        {
                            PropVariant variant = new PropVariant();
                            o.GetProperty((uint) i, ItemPropId.kpidPath, ref variant);
                            PropVariant variant2 = new PropVariant();
                            o.GetProperty((uint) i, ItemPropId.kpidCRC, ref variant2);
                            if (Program.form.getFileExtension(variant.GetObject().ToString()).ToLower() == ext.ToLower())
                            {
                                num = i;
                                fileInRar = variant.GetObject().ToString();
                                this.crc_of_extracting_file = long.Parse(variant2.GetObject().ToString()).ToString("X8");
                                return num;
                            }
                        }
                        return num;
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return num;
        }

        public void Pack(string archiveName, ICollection<string> filePathList, string arcType, ProgressBar progress, Label status, bool delete)
        {
            Program.form.waitForFreeMemory(status);
            string outPutFolder = "";
            if (archiveName.Replace('/', '+') != archiveName)
            {
                outPutFolder = archiveName.Substring(0, archiveName.LastIndexOf('/') + 1);
            }
            else
            {
                outPutFolder = archiveName.Substring(0, archiveName.LastIndexOf('\\') + 1);
            }
            creating_archive = Program.form.origFileLocToNewFileName(archiveName, false, false, outPutFolder);
            attachedProgressBar = progress;
            attachedStatus = status;
            KnownSevenZipFormat format = this.stringToSevenZipFormat(arcType);
            using (SevenZipFormat format2 = new SevenZipFormat(SevenZipDllPath))
            {
                this.ArchiveOut = format2.CreateOutArchive(SevenZipFormat.GetClassIdFromKnownFormat(format));
                if (this.ArchiveOut == null)
                {
                    showError("-0100\n\n" + format);
                    creating_archive = "";
                    return;
                }
                List<FileInfo> list = new List<FileInfo>(filePathList.Count);
                foreach (string str2 in filePathList)
                {
                    list.Add(new FileInfo(str2));
                }
                try
                {
                    using (OutStreamWrapper wrapper = new OutStreamWrapper(System.IO.File.OpenWrite(archiveName)))
                    {
                        this.ArchiveOut.UpdateItems(wrapper, list.Count, new ArchiveUpdateCallback(list));
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(this.ArchiveOut);
                }
                if (delete)
                {
                    foreach (string str3 in filePathList)
                    {
                        status.Text = "Waiting for " + Program.form.origFileLocToNewFileName(str3, false, false, "") + " to be released";
                        Application.DoEvents();
                    Label_0187:;
                        try
                        {
                            System.IO.File.Delete(str3);
                        }
                        catch
                        {
                            goto Label_0187;
                        }
                    }
                }
            }
            creating_archive = "";
            Program.form.waitForFreeMemory(status);
        }

        public bool PackSingle(string archiveName, string fileName, string arcType, ProgressBar progress, Label status, bool delete)
        {
            List<string> filePathList = new List<string>();
            if (System.IO.File.Exists(fileName))
            {
                filePathList.Add(fileName);
                this.Pack(archiveName, filePathList, arcType, progress, status, delete);
                return true;
            }
            MessageBox.Show("File compress error, the source file does not exist\n\n" + fileName);
            return false;
        }

        public static void showError(string code)
        {
            MessageBox.Show("Invalid Archive [err code " + code + "]");
        }

        public KnownSevenZipFormat stringToSevenZipFormat(string arcType)
        {
            switch (arcType)
            {
                case "rar":
                    return KnownSevenZipFormat.Rar;

                case "zip":
                    return KnownSevenZipFormat.Zip;

                case "7z":
                    return KnownSevenZipFormat.SevenZip;
            }
            return KnownSevenZipFormat.Arj;
        }

        private void unrar_ExtractionProgress(object sender, ExtractionProgressEventArgs e)
        {
            attachedStatus.Text = string.Concat(new object[] { "Extracting ", e.FileName, " ", (int) e.PercentComplete, "%" });
            attachedProgressBar.Value = (int) e.PercentComplete;
            Application.DoEvents();
        }

        private void unrar_MissingVolume(object sender, MissingVolumeEventArgs e)
        {
            MessageBox.Show("Volume is missing.");
            e.ContinueOperation = false;
        }

        private void unrar_PasswordRequired(object sender, PasswordRequiredEventArgs e)
        {
            MessageBox.Show("Password is required for extraction.");
            e.ContinueOperation = false;
        }

        public string crc_of_extracting_file
        {
            get { return fileInRarCRC; }
            set
            {
                fileInRarCRC = value;
            }
        }

        public string extracting_file =>
            fileInRar;

        public string outFile
        {
            get { return outFileSet; }
            set
            {
                outFileSet = value;
            }
        }

        private static string SevenZipDllPath =>
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7z.dll");

        private class ArchiveExtractCallback : IProgress, IArchiveExtractCallback
        {
            private string FileName;
            private uint FileNumber;
            private OutStreamWrapper FileStream;
            private static ulong totalsize;

            public ArchiveExtractCallback(uint fileNumber, string fileName)
            {
                this.FileNumber = fileNumber;
                this.FileName = fileName;
            }

            public int GetStream(uint index, out ISequentialOutStream outStream, AskMode askExtractMode)
            {
                if ((index == this.FileNumber) && (askExtractMode == AskMode.kExtract))
                {
                    string directoryName = Path.GetDirectoryName(this.FileName);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    if (compressedArchiveOperator.outFileSet == "")
                    {
                        MessageBox.Show("Fatal Error: No outfile was sent for extraction");
                    }
                    if (compressedArchiveOperator.fileInRar.Replace('\\', '+') != compressedArchiveOperator.fileInRar)
                    {
                        compressedArchiveOperator.fileInRar = compressedArchiveOperator.fileInRar.Substring(compressedArchiveOperator.fileInRar.LastIndexOf('\\'), compressedArchiveOperator.fileInRar.Length - compressedArchiveOperator.fileInRar.LastIndexOf('\\'));
                    }
                    if (compressedArchiveOperator.fileInRar.Replace('/', '+') != compressedArchiveOperator.fileInRar)
                    {
                        compressedArchiveOperator.fileInRar = compressedArchiveOperator.fileInRar.Substring(compressedArchiveOperator.fileInRar.LastIndexOf('/'), compressedArchiveOperator.fileInRar.Length - compressedArchiveOperator.fileInRar.LastIndexOf('/'));
                    }
                    compressedArchiveOperator.fileInRar = Program.form.longFileName(compressedArchiveOperator.fileInRar, compressedArchiveOperator.outFileSet.Length);
                    if (this.FileStream != null)
                    {
                        this.FileStream.Dispose();
                        this.FileStream = null;
                    }
                    this.FileStream = new OutStreamWrapper(System.IO.File.Create(compressedArchiveOperator.outFileSet + compressedArchiveOperator.fileInRar));
                    outStream = this.FileStream;
                }
                else
                {
                    outStream = null;
                }
                return 0;
            }

            public void PrepareOperation(AskMode askExtractMode)
            {
            }

            public void SetCompleted(ref ulong completeValue)
            {
                if (completeValue < totalsize)
                {
                    decimal num = (completeValue / totalsize) * 100M;
                    compressedArchiveOperator.attachedStatus.Text = string.Concat(new object[] { "Extracting File ", Program.form.compressor.extracting_file, " ", (int) num, "%" });
                    compressedArchiveOperator.attachedProgressBar.Value = (int) num;
                    Application.DoEvents();
                }
            }

            public void SetOperationResult(OperationResult resultEOperationResult)
            {
                this.FileStream.Dispose();
                for (int i = 0; i < 5; i++)
                {
                    Application.DoEvents();
                }
                switch (resultEOperationResult)
                {
                    case OperationResult.kOK:
                        break;

                    default:
                        if (System.IO.File.Exists(compressedArchiveOperator.outFileSet))
                        {
                            System.IO.File.Delete(compressedArchiveOperator.outFileSet);
                        }
                        break;
                }
                compressedArchiveOperator.attachedStatus.Text = "Extraction Complete";
                compressedArchiveOperator.attachedProgressBar.Maximum = 4;
                compressedArchiveOperator.attachedProgressBar.Value = 4;
                Application.DoEvents();
            }

            public void SetTotal(ulong total)
            {
                compressedArchiveOperator.attachedStatus.Text = "Extracting File " + Program.form.compressor.extracting_file + " 0%";
                compressedArchiveOperator.attachedProgressBar.Maximum = 100;
                compressedArchiveOperator.attachedProgressBar.Value = 0;
                Application.DoEvents();
                totalsize = total;
            }
        }

        private class ArchiveOpenCallback : IArchiveOpenCallback
        {
            public void SetCompleted(IntPtr files, IntPtr bytes)
            {
            }

            public void SetTotal(IntPtr files, IntPtr bytes)
            {
            }
        }

        private class ArchiveUpdateCallback : IProgress, IArchiveUpdateCallback
        {
            private Stream CurrentSourceStream;
            private IList<FileInfo> FileList;
            private static ulong totalsize;

            public ArchiveUpdateCallback(IList<FileInfo> list)
            {
                this.FileList = list;
            }

            public void GetProperty(int index, ItemPropId propID, IntPtr value)
            {
                FileInfo info = this.FileList[index];
                switch (propID)
                {
                    case ItemPropId.kpidPath:
                        Marshal.GetNativeVariantForObject(Path.GetFileName(info.FullName), value);
                        return;

                    case ItemPropId.kpidIsFolder:
                    case ItemPropId.kpidIsAnti:
                        Marshal.GetNativeVariantForObject(false, value);
                        return;

                    case ItemPropId.kpidSize:
                        Marshal.GetNativeVariantForObject((ulong) info.Length, value);
                        return;

                    case ItemPropId.kpidCreationTime:
                        this.GetTimeProperty(info.CreationTime, value);
                        return;

                    case ItemPropId.kpidLastAccessTime:
                        this.GetTimeProperty(info.LastAccessTime, value);
                        return;

                    case ItemPropId.kpidLastWriteTime:
                        this.GetTimeProperty(info.LastWriteTime, value);
                        return;
                }
                Marshal.WriteInt16(value, (short) 0);
            }

            public void GetStream(int index, out ISequentialInStream inStream)
            {
                this.CurrentSourceStream = this.FileList[index].OpenRead();
                inStream = new InStreamTimedWrapper(this.CurrentSourceStream);
            }

            private void GetTimeProperty(DateTime time, IntPtr value)
            {
                Marshal.GetNativeVariantForObject(time.ToFileTime(), value);
                Marshal.WriteInt16(value, (short) 0x40);
            }

            public void GetUpdateItemInfo(int index, out int newData, out int newProperties, out uint indexInArchive)
            {
                newData = 1;
                newProperties = 1;
                indexInArchive = uint.MaxValue;
            }

            public void SetCompleted(ref ulong completeValue)
            {
                if (completeValue < totalsize)
                {
                    decimal num = (completeValue / totalsize) * 100M;
                    compressedArchiveOperator.attachedStatus.Text = string.Concat(new object[] { "Compressing File ", compressedArchiveOperator.creating_archive, " ", (int) num, "%" });
                    compressedArchiveOperator.attachedProgressBar.Value = (int) num;
                    Application.DoEvents();
                }
            }

            public void SetOperationResult(int operationResult)
            {
                this.CurrentSourceStream.Close();
            }

            public void SetTotal(ulong total)
            {
                compressedArchiveOperator.attachedStatus.Text = "Compressing File " + compressedArchiveOperator.creating_archive + " 0%";
                compressedArchiveOperator.attachedProgressBar.Maximum = 100;
                compressedArchiveOperator.attachedProgressBar.Value = 0;
                Application.DoEvents();
                totalsize = total;
            }
        }
    }
}

