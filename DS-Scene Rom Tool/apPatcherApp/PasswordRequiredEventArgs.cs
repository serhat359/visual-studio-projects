namespace apPatcherApp
{
    using System;

    public class PasswordRequiredEventArgs
    {
        public bool ContinueOperation = true;
        public string Password = string.Empty;
    }
}

