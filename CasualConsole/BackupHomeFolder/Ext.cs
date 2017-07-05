using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackupHomeFolder
{
    public static class Ext
    {
        public static void Each<T>(this IEnumerable<T> elements, Func<T, int, bool> action)
        {
            int i = 0;

            foreach (T t in elements)
            {
                bool continueLoop = action(t, i++);

                if (continueLoop)
                    continue;
                else
                    break;
            }
        }

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
