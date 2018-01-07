using Ionic.Zip;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CasualConsole;
using System.Linq;

namespace HeroesMapManipulator
{
    public partial class Form1 : Form
    {
        const string tempFolderName = "temp";

        readonly string[] mapNameFiles = { "mapname-text-0.txt", "caption-text-0.txt", "caption-text-1.txt", "caption-text-2.txt", "caption-text-3.txt",
            "caption-text-4.txt", "caption-text-5.txt", "caption-text-6.txt", "caption-text-7.txt" };

        string mapFilePath;
        string xdbPath;
        string xdbDirectory;

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);

            beginButton.Visible = false;
            checkBoxPanel.Visible = false;

            this.FormClosed += Form1_FormClosed;
        }

        private void EmptyTempFolder()
        {
            DirectoryInfo tempDir = new DirectoryInfo(tempFolderName);

            foreach (FileInfo file in tempDir.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in tempDir.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            EmptyTempFolder();
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 1)
                MessageBox.Show("One file at a time!");
            else
            {
                mapFilePath = files[0];

                FileInfo fileInfo = new FileInfo(mapFilePath);

                string fileExtension = fileInfo.Extension;

                if (fileExtension != ".h5m")
                {
                    MessageBox.Show("This is not a map file!");
                }
                else
                {
                    string mapFileDirectory = fileInfo.DirectoryName;

                    if (!Directory.Exists(tempFolderName))
                    {
                        Directory.CreateDirectory(tempFolderName);
                    }

                    fileDragLabel.Visible = false;
                    beginButton.Visible = true;
                    checkBoxPanel.Visible = true;
                    HeroesMapAction.allMonstersWithPos = null;
                }
            }
        }

        private void beginButton_Click(object sender, EventArgs e)
        {
            Button buttonClicked = (Button)sender;

            buttonClicked.Enabled = false;

            XmlDocument document = GetXDBFile();

            if (randomizeChestCheckBox.Checked)
                HeroesMapAction.RandomizeTreasureChest(document);

            if (deleteAdditionalMonsterCheckBox.Checked)
                HeroesMapAction.DeleteAdditionalStacks(document);

            if (deleteMonolithCheckBox.Checked)
                HeroesMapAction.DeleteTwoWayMonoliths(document);

            if (weakenShipyardsCheckBox.Checked)
                HeroesMapAction.WeakenShipyardMonsters(document);

            RepackFile(document);

            buttonClicked.Enabled = true;

            MessageBox.Show("Done!");
        }

        private void RepackFile(XmlDocument document)
        {
            XmlNode mapNameNode = document.GetElementsByTagName("MapName").AsEnumerable<XmlNode>().First();

            var oldMapName = mapNameNode.FirstChild.Value;

            //var newMapName = oldMapName + "_edited";

            //mapNameNode.FirstChild.Value = newMapName;

            document.Save(xdbPath);

            foreach (string mapNameFile in mapNameFiles)
            {
                string filePath = Path.Combine(xdbDirectory, mapNameFile);
                string oldText = File.ReadAllText(filePath);

                if (oldText == oldMapName)
                {
                    //File.WriteAllText(filePath, newMapName);
                }
                else
                {
                    throw new Exception("Wasn't expecting that");
                }
            }

            int lastDotIndex = mapFilePath.LastIndexOf(".");
            string newFilePath = mapFilePath.Substring(0, lastDotIndex) + "_edited" + mapFilePath.Substring(lastDotIndex);

            File.Delete(newFilePath);
            ZipFile newZip = new ZipFile(newFilePath);
            newZip.AddDirectory(tempFolderName);
            newZip.Save(newFilePath);

            EmptyTempFolder();
        }

        private XmlDocument GetXDBFile()
        {
            ZipFile zip = ZipFile.Read(mapFilePath);

            EmptyTempFolder();

            zip.ExtractAll(tempFolderName);

            string rmgDirectory = tempFolderName + "/Maps/RMG/";

            string subFolderName = new DirectoryInfo(rmgDirectory).GetDirectories()[0].Name;

            xdbDirectory = Path.Combine(rmgDirectory, subFolderName);

            xdbPath = Path.Combine(rmgDirectory, subFolderName, "map.xdb");

            XmlDocument document = new XmlDocument();

            document.Load(xdbPath);

            return document;
        }
    }
}
