using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace ExcelScriptGenerator
{
    public partial class ExcelScriptGenerator : Form
    {
        private string tableName;

        public ExcelScriptGenerator()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.tableName = ShowDialog("Enter table name", "Table Name");

            var text = richTextBox1.Text;

            var columnsWithQuotes = text
                                    .Replace("\r\n", "")
                                    .Replace("\n", "")
                                    .Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var columns = columnsWithQuotes.Select(x => x.Replace("'", "")).ToArray();

            var quote = "\"";
            string columnsMerged = string.Join(",", columns.Select(x => "[" + x + "]"));
            string valuesFormula = string.Join(",", columnsWithQuotes.Select((x, i) =>
            {
                bool hasQuote = x.Contains("'");
                string singleQuote = hasQuote ? "'" : "";
                string nLetter = hasQuote ? "N" : "";
                string cellNumber = (char)('A' + i) + "2";
                return $"{nLetter}{singleQuote}\"&{cellNumber}&\"{singleQuote}";
            }));

            string fullFormula = $"={quote}Insert into {tableName}({columnsMerged}) values({valuesFormula}){quote}";

            richTextBox2.Text = fullFormula;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            var text = richTextBox1.Text;
            richTextBox1.Text = text;
        }

        public string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            textBox.Text = tableName ?? "";
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
