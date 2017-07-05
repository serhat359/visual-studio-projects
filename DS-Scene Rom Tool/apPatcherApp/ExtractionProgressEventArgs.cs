namespace apPatcherApp
{
    using System;

    public class ExtractionProgressEventArgs
    {
        public long BytesExtracted;
        public bool ContinueOperation = true;
        public string FileName;
        public long FileSize;
        public double PercentComplete;
    }
}

