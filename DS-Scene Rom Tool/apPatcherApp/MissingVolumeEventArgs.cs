namespace apPatcherApp
{
    using System;

    public class MissingVolumeEventArgs
    {
        public bool ContinueOperation;
        public string VolumeName;

        public MissingVolumeEventArgs(string volumeName)
        {
            this.VolumeName = volumeName;
        }
    }
}

