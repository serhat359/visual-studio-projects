using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TextCompare
{
    public partial class TextCompare : Form
    {
        public TextCompare()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var tempPath = System.IO.Path.GetTempPath();

            string txt1Path = Path.Combine(tempPath, "file1.txt");
            string txt2Path = Path.Combine(tempPath, "file2.txt");

            File.WriteAllText(txt1Path, richTextBox1.Text);
            File.WriteAllText(txt2Path, richTextBox2.Text);

            Process.Start("tortoisegitmerge", $"\"{txt1Path}\" \"{txt2Path}\"");
        }
    }
}
