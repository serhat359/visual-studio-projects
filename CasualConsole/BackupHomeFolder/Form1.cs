using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BackupHomeFolder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            AppSetting setting = Settings.Get();
            sourceTextBox.Text = setting.SourceFolder;
            destinationTextBox.Text = setting.DestinationFolder;
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            string sourceFolder = sourceTextBox.Text;
            string destinationFolder = destinationTextBox.Text;

            if (IsNotEmpty(sourceFolder) && IsNotEmpty(destinationFolder))
            {
                AppSetting setting = Settings.Get();
                setting.SourceFolder = sourceFolder;
                setting.DestinationFolder = destinationFolder;
                Settings.Set(setting);

                string[] subfolders = new string[] { "Pictures" };

                foreach (var subfolderName in subfolders)
                {

                }
            }
            else
            {
                MessageBox.Show("Choose both folders");
            }
        }

        private static bool IsNotEmpty(string str)
        {
            return !string.IsNullOrEmpty(str);
        }
    }
}
