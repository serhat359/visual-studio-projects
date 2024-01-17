using System;
using System.Windows.Forms;

namespace BackupHomeFolder
{
    public static class Ext
    {
        // An extension to access UI elements from another thread safely
        public static void ThreadSafe<T>(this T control, Action<T> action) where T : Control
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke((MethodInvoker)delegate ()
                {
                    action(control);
                });
            }
            else
            {
                action(control);
            }
        }
    }
}
