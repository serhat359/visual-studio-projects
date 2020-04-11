using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SubtitleFixer
{
    public partial class SubtitleFixer : Form
    {
        private string[] fileNames;

        public SubtitleFixer()
        {
            InitializeComponent();

            this.folderPathTextBox.Click += new System.EventHandler(this.folderPathTextBox_Click);
        }

        private void folderPathTextBox_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Custom Files (*.srt)|*.srt|All files (*.*)|*.*";

            var result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.fileNames = openFileDialog.FileNames;

                folderPathTextBox.Text = string.Join(",", this.fileNames);

                Check();
            }
        }

        private void Check()
        {
            var b = this.fileNames?.Length > 0 && int.TryParse(msTextbox.Text, out var millis);

            fixButton.Enabled = b;
        }

        private void msTextbox_TextChanged(object sender, EventArgs e)
        {
            Check();
        }

        private void fixButton_Click(object sender, EventArgs e)
        {
            int.TryParse(msTextbox.Text, out var millis);

            var fileName = this.fileNames[0];
            var i = fileName.LastIndexOf('.');
            var writeFileName = fileName.Substring(0, i) + "_2" + fileName.Substring(i);

            using (var fWrite = File.OpenWrite(writeFileName))
            {
                foreach (var line in File.ReadAllLines(fileName))
                {
                    var line2 = line;
                    var divider = " --> ";
                    if (line2.Contains(divider))
                    {
                        var parts = line2.Split(new[] { divider }, StringSplitOptions.None);
                        var t1 = Convert(parts[0]);
                        var t2 = Convert(parts[1]);

                        t1 += TimeSpan.FromMilliseconds(millis);
                        t2 += TimeSpan.FromMilliseconds(millis);

                        var format = "hh\\:mm\\:ss\\,fff";
                        line2 = t1.ToString(format) + divider + t2.ToString(format);
                    }

                    var bytes = Encoding.UTF8.GetBytes(line2 + "\n");
                    fWrite.Write(bytes, 0, bytes.Length);
                }
            }

            MessageBox.Show("Done!");
        }

        private static TimeSpan Convert(string s)
        {
            var parts = s.Split(',');

            var t = TimeSpan.Parse(parts[0]) + TimeSpan.FromMilliseconds(int.Parse(parts[1]));

            return t;
        }
    }
}
