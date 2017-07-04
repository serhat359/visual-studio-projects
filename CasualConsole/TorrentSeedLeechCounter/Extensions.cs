using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TorrentSeedLeechCounter
{
    public static class Extensions
    {
        public static IEnumerable<T> AsIterable<T>(this IEnumerable values)
        {
            foreach (object item in values)
            {
                yield return (T)item;
            }
        }

        public static void AppendTextLine(this TextBoxBase textbox, string text)
        {
            textbox.AppendText(text);
            textbox.AppendText("\n");
        }

        public static void AppendTextLine(this TextBoxBase textbox)
        {
            textbox.AppendText("\n");
        }
    }
}
