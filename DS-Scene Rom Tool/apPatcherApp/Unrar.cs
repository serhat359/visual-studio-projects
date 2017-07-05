namespace apPatcherApp
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class Unrar : IDisposable
    {
        private ArchiveFlags archiveFlags;
        private IntPtr archiveHandle;
        private string archivePathName;
        private UNRARCallback callback;
        private string comment;
        private RARFileInfo currentFile;
        private string destinationPath;
        private RARHeaderDataEx header;
        private string password;
        private bool retrieveComment;

        public event DataAvailableHandler DataAvailable;

        public event ExtractionProgressHandler ExtractionProgress;

        public event MissingVolumeHandler MissingVolume;

        public event NewFileHandler NewFile;

        public event NewVolumeHandler NewVolume;

        public event PasswordRequiredHandler PasswordRequired;

        public Unrar()
        {
            this.archivePathName = string.Empty;
            this.archiveHandle = new IntPtr(0);
            this.retrieveComment = true;
            this.password = string.Empty;
            this.comment = string.Empty;
            this.header = new RARHeaderDataEx();
            this.destinationPath = string.Empty;
            this.callback = new UNRARCallback(this.RARCallback);
        }

        public Unrar(string archivePathName) : this()
        {
            this.archivePathName = archivePathName;
        }

        public void Close()
        {
            if (this.archiveHandle != IntPtr.Zero)
            {
                int result = RARCloseArchive(this.archiveHandle);
                if (result != 0)
                {
                    this.ProcessFileError(result);
                }
                else
                {
                    this.archiveHandle = IntPtr.Zero;
                }
            }
        }

        public void Dispose()
        {
            if (this.archiveHandle != IntPtr.Zero)
            {
                RARCloseArchive(this.archiveHandle);
                this.archiveHandle = IntPtr.Zero;
            }
        }

        public void Extract()
        {
            this.Extract(this.destinationPath, string.Empty);
        }

        public void Extract(string destinationName)
        {
            this.Extract(string.Empty, destinationName);
        }

        private void Extract(string destinationPath, string destinationName)
        {
            int result = RARProcessFile(this.archiveHandle, 2, destinationPath, destinationName);
            if (result != 0)
            {
                this.ProcessFileError(result);
            }
        }

        public void ExtractToDirectory(string destinationPath)
        {
            this.Extract(destinationPath, string.Empty);
        }

        ~Unrar()
        {
            if (this.archiveHandle != IntPtr.Zero)
            {
                RARCloseArchive(this.archiveHandle);
                this.archiveHandle = IntPtr.Zero;
            }
        }

        private DateTime FromMSDOSTime(uint dosTime)
        {
            int day = 0;
            int month = 0;
            int year = 0;
            int hour = 0;
            int minute = 0;
            ushort num7 = (ushort) ((dosTime & -65536) >> 0x10);
            ushort num8 = (ushort) (dosTime & 0xffff);
            year = ((num7 & 0xfe00) >> 9) + 0x7bc;
            month = (num7 & 480) >> 5;
            day = num7 & 0x1f;
            hour = (num8 & 0xf800) >> 11;
            minute = (num8 & 0x7e0) >> 5;
            return new DateTime(year, month, day, hour, minute, (num8 & 0x1f) << 1);
        }

        public string[] ListFiles()
        {
            ArrayList list = new ArrayList();
            while (this.ReadHeader())
            {
                if (!this.currentFile.IsDirectory)
                {
                    list.Add(this.currentFile.FileName);
                }
                this.Skip();
            }
            string[] array = new string[list.Count];
            list.CopyTo(array);
            return array;
        }

        protected virtual int OnDataAvailable(IntPtr p1, int p2)
        {
            int num = 1;
            if (this.currentFile != null)
            {
                this.currentFile.BytesExtracted += p2;
            }
            if (this.DataAvailable != null)
            {
                byte[] destination = new byte[p2];
                Marshal.Copy(p1, destination, 0, p2);
                DataAvailableEventArgs e = new DataAvailableEventArgs(destination);
                this.DataAvailable(this, e);
                if (!e.ContinueOperation)
                {
                    num = -1;
                }
            }
            if ((this.ExtractionProgress != null) && (this.currentFile != null))
            {
                ExtractionProgressEventArgs args2 = new ExtractionProgressEventArgs {
                    FileName = this.currentFile.FileName,
                    FileSize = this.currentFile.UnpackedSize,
                    BytesExtracted = this.currentFile.BytesExtracted,
                    PercentComplete = this.currentFile.PercentComplete
                };
                this.ExtractionProgress(this, args2);
                if (!args2.ContinueOperation)
                {
                    num = -1;
                }
            }
            return num;
        }

        protected virtual string OnMissingVolume(string volume)
        {
            string volumeName = string.Empty;
            if (this.MissingVolume != null)
            {
                MissingVolumeEventArgs e = new MissingVolumeEventArgs(volume);
                this.MissingVolume(this, e);
                if (e.ContinueOperation)
                {
                    volumeName = e.VolumeName;
                }
            }
            return volumeName;
        }

        protected virtual void OnNewFile()
        {
            if (this.NewFile != null)
            {
                NewFileEventArgs e = new NewFileEventArgs(this.currentFile);
                this.NewFile(this, e);
            }
        }

        protected virtual int OnNewVolume(string volume)
        {
            int num = 1;
            if (this.NewVolume != null)
            {
                NewVolumeEventArgs e = new NewVolumeEventArgs(volume);
                this.NewVolume(this, e);
                if (!e.ContinueOperation)
                {
                    num = -1;
                }
            }
            return num;
        }

        protected virtual int OnPasswordRequired(IntPtr p1, int p2)
        {
            int num = -1;
            if (this.PasswordRequired == null)
            {
                throw new IOException("Password is required for extraction.");
            }
            PasswordRequiredEventArgs e = new PasswordRequiredEventArgs();
            this.PasswordRequired(this, e);
            if (!e.ContinueOperation || (e.Password.Length <= 0))
            {
                return num;
            }
            for (int i = 0; (i < e.Password.Length) && (i < p2); i++)
            {
                Marshal.WriteByte(p1, i, (byte) e.Password[i]);
            }
            Marshal.WriteByte(p1, e.Password.Length, 0);
            return 1;
        }

        public void Open()
        {
            if (this.ArchivePathName.Length == 0)
            {
                throw new IOException("Archive name has not been set.");
            }
            this.Open(this.ArchivePathName, OpenMode.Extract);
        }

        public void Open(OpenMode openMode)
        {
            if (this.ArchivePathName.Length == 0)
            {
                throw new IOException("Archive name has not been set.");
            }
            this.Open(this.ArchivePathName, openMode);
        }

        public void Open(string archivePathName, OpenMode openMode)
        {
            IntPtr zero = IntPtr.Zero;
            if (this.archiveHandle != IntPtr.Zero)
            {
                this.Close();
            }
            this.ArchivePathName = archivePathName;
            RAROpenArchiveDataEx archiveData = new RAROpenArchiveDataEx();
            archiveData.Initialize();
            archiveData.ArcName = this.archivePathName + "\0";
            archiveData.ArcNameW = this.archivePathName + "\0";
            archiveData.OpenMode = (uint) openMode;
            if (this.retrieveComment)
            {
                archiveData.CmtBuf = new string('\0', 0x10000);
                archiveData.CmtBufSize = 0x10000;
            }
            else
            {
                archiveData.CmtBuf = null;
                archiveData.CmtBufSize = 0;
            }
            zero = RAROpenArchiveEx(ref archiveData);
            switch (archiveData.OpenResult)
            {
                case 11:
                    throw new OutOfMemoryException("Insufficient memory to perform operation.");

                case 12:
                    throw new IOException("Archive header broken");

                case 13:
                    throw new IOException("File is not a valid archive.");

                case 15:
                    throw new IOException("File could not be opened.");
            }
            this.archiveHandle = zero;
            this.archiveFlags = (ArchiveFlags) archiveData.Flags;
            RARSetCallback(this.archiveHandle, this.callback, this.GetHashCode());
            if (archiveData.CmtState == 1)
            {
                this.comment = archiveData.CmtBuf.ToString();
            }
            if (this.password.Length != 0)
            {
                RARSetPassword(this.archiveHandle, this.password);
            }
            this.OnNewVolume(this.archivePathName);
        }

        private void ProcessFileError(int result)
        {
            switch (((RarError) result))
            {
                case RarError.BadData:
                    throw new IOException("File CRC Error");

                case RarError.BadArchive:
                    throw new IOException("File is not a valid archive.");

                case RarError.UnknownFormat:
                    throw new OutOfMemoryException("Unknown archive format.");

                case RarError.OpenError:
                    throw new IOException("File could not be opened.");

                case RarError.CreateError:
                    throw new IOException("File could not be created.");

                case RarError.CloseError:
                    throw new IOException("File close error.");

                case RarError.ReadError:
                    throw new IOException("File read error.");

                case RarError.WriteError:
                    throw new IOException("File write error.");
            }
        }

        private int RARCallback(uint msg, int UserData, IntPtr p1, int p2)
        {
            string volume = string.Empty;
            string str2 = string.Empty;
            int num = -1;
            switch (((CallbackMessages) msg))
            {
                case CallbackMessages.VolumeChange:
                    volume = Marshal.PtrToStringAnsi(p1);
                    if (p2 != 1)
                    {
                        if (p2 != 0)
                        {
                            return num;
                        }
                        str2 = this.OnMissingVolume(volume);
                        if (str2.Length == 0)
                        {
                            return -1;
                        }
                        if (str2 != volume)
                        {
                            for (int i = 0; i < str2.Length; i++)
                            {
                                Marshal.WriteByte(p1, i, (byte) str2[i]);
                            }
                            Marshal.WriteByte(p1, str2.Length, 0);
                        }
                        return 1;
                    }
                    return this.OnNewVolume(volume);

                case CallbackMessages.ProcessData:
                    return this.OnDataAvailable(p1, p2);

                case CallbackMessages.NeedPassword:
                    return this.OnPasswordRequired(p1, p2);
            }
            return num;
        }

        [DllImport("unrar.dll")]
        private static extern int RARCloseArchive(IntPtr hArcData);
        [DllImport("unrar.dll")]
        private static extern IntPtr RAROpenArchive(ref RAROpenArchiveData archiveData);
        [DllImport("UNRAR.DLL")]
        private static extern IntPtr RAROpenArchiveEx(ref RAROpenArchiveDataEx archiveData);
        [DllImport("unrar.dll")]
        private static extern int RARProcessFile(IntPtr hArcData, int operation, [MarshalAs(UnmanagedType.LPStr)] string destPath, [MarshalAs(UnmanagedType.LPStr)] string destName);
        [DllImport("unrar.dll")]
        private static extern int RARReadHeader(IntPtr hArcData, ref RARHeaderData headerData);
        [DllImport("unrar.dll")]
        private static extern int RARReadHeaderEx(IntPtr hArcData, ref RARHeaderDataEx headerData);
        [DllImport("unrar.dll")]
        private static extern void RARSetCallback(IntPtr hArcData, UNRARCallback callback, int userData);
        [DllImport("unrar.dll")]
        private static extern void RARSetPassword(IntPtr hArcData, [MarshalAs(UnmanagedType.LPStr)] string password);
        public bool ReadHeader()
        {
            if (this.archiveHandle == IntPtr.Zero)
            {
                throw new IOException("Archive is not open.");
            }
            this.header = new RARHeaderDataEx();
            this.header.Initialize();
            this.currentFile = null;
            switch (RARReadHeaderEx(this.archiveHandle, ref this.header))
            {
                case 10:
                    return false;

                case 12:
                    throw new IOException("Archive data is corrupt.");
            }
            if (((this.header.Flags & 1) != 0) && (this.currentFile != null))
            {
                this.currentFile.ContinuedFromPrevious = true;
            }
            else
            {
                this.currentFile = new RARFileInfo();
                this.currentFile.FileName = this.header.FileNameW.ToString();
                if ((this.header.Flags & 2) != 0)
                {
                    this.currentFile.ContinuedOnNext = true;
                }
                if (this.header.PackSizeHigh != 0)
                {
                    this.currentFile.PackedSize = (this.header.PackSizeHigh * 0x100000000L) + this.header.PackSize;
                }
                else
                {
                    this.currentFile.PackedSize = this.header.PackSize;
                }
                if (this.header.UnpSizeHigh != 0)
                {
                    this.currentFile.UnpackedSize = (this.header.UnpSizeHigh * 0x100000000L) + this.header.UnpSize;
                }
                else
                {
                    this.currentFile.UnpackedSize = this.header.UnpSize;
                }
                this.currentFile.HostOS = (int) this.header.HostOS;
                this.currentFile.FileCRC = this.header.FileCRC;
                this.currentFile.FileTime = this.FromMSDOSTime(this.header.FileTime);
                this.currentFile.VersionToUnpack = (int) this.header.UnpVer;
                this.currentFile.Method = (int) this.header.Method;
                this.currentFile.FileAttributes = (int) this.header.FileAttr;
                this.currentFile.BytesExtracted = 0L;
                if ((this.header.Flags & 0xe0) == 0xe0)
                {
                    this.currentFile.IsDirectory = true;
                }
                this.OnNewFile();
            }
            return true;
        }

        public void Skip()
        {
            int result = RARProcessFile(this.archiveHandle, 0, string.Empty, string.Empty);
            if (result != 0)
            {
                this.ProcessFileError(result);
            }
        }

        public void Test()
        {
            int result = RARProcessFile(this.archiveHandle, 1, string.Empty, string.Empty);
            if (result != 0)
            {
                this.ProcessFileError(result);
            }
        }

        public string ArchivePathName
        {
            get { return this.archivePathName; }
            set
            {
                this.archivePathName = value;
            }
        }

        public string Comment =>
            this.comment;

        public RARFileInfo CurrentFile =>
            this.currentFile;

        public string DestinationPath
        {
            get { return this.destinationPath; }
            set
            {
                this.destinationPath = value;
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                if (this.archiveHandle != IntPtr.Zero)
                {
                    RARSetPassword(this.archiveHandle, value);
                }
            }
        }

        [Flags]
        private enum ArchiveFlags : uint
        {
            AuthenticityPresent = 0x20,
            CommentPresent = 2,
            EncryptedHeaders = 0x80,
            FirstVolume = 0x100,
            Lock = 4,
            NewNamingScheme = 0x10,
            RecoveryRecordPresent = 0x40,
            SolidArchive = 8,
            Volume = 1
        }

        private enum CallbackMessages : uint
        {
            NeedPassword = 2,
            ProcessData = 1,
            VolumeChange = 0
        }

        public enum OpenMode
        {
            List,
            Extract
        }

        private enum Operation : uint
        {
            Extract = 2,
            Skip = 0,
            Test = 1
        }

        private enum RarError : uint
        {
            BadArchive = 13,
            BadData = 12,
            BufferTooSmall = 20,
            CloseError = 0x11,
            CreateError = 0x10,
            EndOfArchive = 10,
            InsufficientMemory = 11,
            OpenError = 15,
            ReadError = 0x12,
            UnknownError = 0x15,
            UnknownFormat = 14,
            WriteError = 0x13
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RARHeaderData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string FileName;
            public uint Flags;
            public uint PackSize;
            public uint UnpSize;
            public uint HostOS;
            public uint FileCRC;
            public uint FileTime;
            public uint UnpVer;
            public uint Method;
            public uint FileAttr;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;
            public void Initialize()
            {
                this.CmtBuf = new string('\0', 0x10000);
                this.CmtBufSize = 0x10000;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct RARHeaderDataEx
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x200)]
            public string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x400)]
            public string ArcNameW;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x200)]
            public string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x400)]
            public string FileNameW;
            public uint Flags;
            public uint PackSize;
            public uint PackSizeHigh;
            public uint UnpSize;
            public uint UnpSizeHigh;
            public uint HostOS;
            public uint FileCRC;
            public uint FileTime;
            public uint UnpVer;
            public uint Method;
            public uint FileAttr;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x400)]
            public uint[] Reserved;
            public void Initialize()
            {
                this.CmtBuf = new string('\0', 0x10000);
                this.CmtBufSize = 0x10000;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAROpenArchiveData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string ArcName;
            public uint OpenMode;
            public uint OpenResult;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;
            public void Initialize()
            {
                this.CmtBuf = new string('\0', 0x10000);
                this.CmtBufSize = 0x10000;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAROpenArchiveDataEx
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string ArcName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ArcNameW;
            public uint OpenMode;
            public uint OpenResult;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            public uint[] Reserved;
            public void Initialize()
            {
                this.CmtBuf = new string('\0', 0x10000);
                this.CmtBufSize = 0x10000;
                this.Reserved = new uint[0x20];
            }
        }

        private delegate int UNRARCallback(uint msg, int UserData, IntPtr p1, int p2);

        private enum VolumeMessage : uint
        {
            Ask = 0,
            Notify = 1
        }
    }
}

