using System;
using System.Windows.Forms;

namespace DuplicateFinder
{
    public static class Extensions
    {
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
