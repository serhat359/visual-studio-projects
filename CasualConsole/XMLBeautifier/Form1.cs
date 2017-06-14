using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace XMLBeautifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            string oldStr = ((RichTextBox)sender).Text;

            string newStr = PrintXML(oldStr);

            richTextBox2.Text = newStr;
        }

        public static string PrintXML(string XML)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(XML);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXML = sReader.ReadToEnd();

                result = formattedXML.Replace("  ","\t");

                mStream.Close();
                writer.Close();
            }
            catch (XmlException)
            {
            }

            return result;
        }
    }
}
