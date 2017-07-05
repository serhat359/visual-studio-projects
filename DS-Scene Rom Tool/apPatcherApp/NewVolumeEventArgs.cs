namespace apPatcherApp
{
    using System;

    public class NewVolumeEventArgs
    {
        public bool ContinueOperation = true;
        public string VolumeName;

        public NewVolumeEventArgs(string volumeName)
        {
            this.VolumeName = volumeName;
        }
    }
}

