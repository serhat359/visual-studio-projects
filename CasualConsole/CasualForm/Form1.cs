using System;
using System.Windows.Forms;
using System.Linq;

namespace CasualForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Button1_Click(object sender, EventArgs e)
        {
            var text = richTextBox1.Text;
            text.Replace("\r\n", "\n");
            text = text.ToLowerInvariant();

            var lines = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var grouped = lines
                .GroupBy(x => x)
                .Select(x => new {
                    text = x.Key,
                    count = x.Count()
                })
                .OrderByDescending(x => x.count).ThenBy(x => x.text);

            MessageBox.Show(string.Join("\n", grouped.Select(x => $"count: {x.count}, name: {x.text}")));
        }
    }
}
