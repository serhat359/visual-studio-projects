namespace apPatcherApp
{
    using System;

    public class DataAvailableEventArgs
    {
        public bool ContinueOperation = true;
        public readonly byte[] Data;

        public DataAvailableEventArgs(byte[] data)
        {
            this.Data = data;
        }
    }
}

