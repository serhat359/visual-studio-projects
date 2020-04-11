using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Download_Manager
{
    public partial class AddButtonWindow : Form
    {
        Lazy<char[]> chars = new Lazy<char[]>(() =>
        {
            var chars = Path.GetInvalidFileNameChars();

            chars = chars.Concat(new char[] { '=', '&', '/' }).ToArray();

            return chars;
        });

        public AddButtonWindow()
        {
            InitializeComponent();
        }

        private void urltextBox_TextChanged(object sender, EventArgs e)
        {
            var text = urltextBox.Text;

            var i = text.LastIndexOfAny(chars.Value);
            var fileName = i > 0 ? text.Substring(i + 1) : text;

            TryCatch(ref fileName, x => WebUtility.UrlDecode(x));
            
            i = fileName.LastIndexOfAny(chars.Value);
            fileName = i > 0 ? fileName.Substring(i + 1) : fileName;
            
            fileNameTextBox.Text = fileName;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var content = new TableContent
            {
                FileName = this.fileNameTextBox.Text ?? "",
                Referer = this.refererTextBox.Text ?? "",
                Url = this.urltextBox.Text ?? ""
            };

            if (content.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errorLabel.Text = "File name invalid";
            }
            else
            {
                MainWindow.contents.TableData.Add(content);
                TableContents.SaveContents(MainWindow.contents);

                this.Close();
            }
        }

        private void TryCatch(ref string s, Func<string, string> method)
        {
            try
            {
                s = method(s);
            }
            catch (Exception) { }
        }
    }
}
