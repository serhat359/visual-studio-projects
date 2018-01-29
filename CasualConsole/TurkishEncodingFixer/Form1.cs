using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TurkishEncodingFixer
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> replacements = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();

            // Upper Case
            replacements["Ý"] = "İ";
            replacements["Þ"] = "Ş";
            replacements["Ð"] = "Ğ";

            // Lower Case
            replacements["ý"] = "ı";
            replacements["þ"] = "ş";
            replacements["ð"] = "ğ";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            string oldStr = ((RichTextBox)sender).Text;

            string newStr = TurkishReplace(oldStr);

            richTextBox2.Text = newStr;
        }

        private string TurkishReplace(string originalText)
        {
            foreach (var item in replacements)
            {
                originalText = originalText.Replace(item.Key, item.Value);
            }

            return originalText;
        }
    }
}
