namespace apPatcherApp
{
    using System;

    public class RARFileInfo
    {
        public long BytesExtracted;
        public bool ContinuedFromPrevious;
        public bool ContinuedOnNext;
        public int FileAttributes;
        public long FileCRC;
        public string FileName;
        public DateTime FileTime;
        public int HostOS;
        public bool IsDirectory;
        public int Method;
        public long PackedSize;
        public long UnpackedSize;
        public int VersionToUnpack;

        public double PercentComplete
        {
            get
            {
                if (this.UnpackedSize != 0L)
                {
                    return ((((double) this.BytesExtracted) / ((double) this.UnpackedSize)) * 100.0);
                }
                return 0.0;
            }
        }
    }
}

